using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Model;
using ServiceLayer.Interface;





namespace ServiceLayer.Services
{
    public class OwnerService : ConnectionManager, IOwnerService
    {
        private readonly ICommonService CommonService;
        public OwnerService(string dbconnection, ICommonService? CommonService) : base(dbconnection)
        {
            this.CommonService = CommonService;
        }


        #region UploadProperty
        public async Task<PropertyResponse> UploadProperty(PropertyModel Property, string RootPath)
        {
            var response = new PropertyResponse()
            {
                IsSuccess = true,
                Message = "Successful"
            };
            var propertyId = AddPropertyDetails(Property, response);
            response = await PropertyImagesUpload(Property.ImageUrl, propertyId, RootPath, response);
            response = await PropertyPaperUpload(Property.PropertyPaper, propertyId, RootPath, response);
            response = UpdateRoleId(Property.UserId, response);

            return response;
        }
        #endregion

        #region AddPropertyDetails
        //method to add property 
        public int AddPropertyDetails(PropertyModel Property, PropertyResponse response)
        {

            int PropertyId = -1;
            string query1 = "insert into property(UserId,Price,Deposit,Type,Rooms,Furnishing,Area,PropertyAddress," +
                                    "StateId,CityId,AdminAction,PropertyRented,CreatedAt,Blocked) OUTPUT INSERTED.PropertyId  " +
                                    "values(@UserId,@Price,@Deposit,@Type,@Rooms,@Furnishing,@Area,@PropertyAddress,@StateId,@CityId,0,0,CURRENT_TIMESTAMP,0)";
            var parameter = new DynamicParameters();
            parameter.Add("@UserId", Property.UserId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@Price", Property.Price, DbType.String, ParameterDirection.Input);
            parameter.Add("@Deposit", Property.Deposit, DbType.String, ParameterDirection.Input);
            parameter.Add("@Type", Property.Type, DbType.String, ParameterDirection.Input);
            parameter.Add("@Rooms", Property.Rooms, DbType.String, ParameterDirection.Input);
            parameter.Add("@Furnishing", Property.Furnishing, DbType.String, ParameterDirection.Input);
            parameter.Add("@Area", Property.Area, DbType.String, ParameterDirection.Input);
            parameter.Add("@PropertyAddress", Property.PropertyAddress, DbType.String, ParameterDirection.Input);
            parameter.Add("@StateId", Property.StateId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@CityId", Property.CityId, DbType.Int32, ParameterDirection.Input);

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var obj = connection.ExecuteScalar(query1, parameter, commandType: CommandType.Text);
                    PropertyId = (int)obj;
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return PropertyId;
        }

        #endregion

        #region UpdateRoleId
        public PropertyResponse UpdateRoleId(int UserId, PropertyResponse response)
        {

            //string query = "update customer set RoleId=3 where CustomerId=@UserId";
            var parameter = new DynamicParameters();
            parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            try
            {
                //update the RoleId of the customer from 2 to 3 (Customer to Owner)
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object obj = connection.ExecuteScalar("UpdateRoleId", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion


        
        #region PropertyImagesUpload
        //method to save url and store url of that into database
        public async Task<PropertyResponse> PropertyImagesUpload(List<IFormFile> PropertyImages, int PropertyId, string RootPath, PropertyResponse response)
        {


            if (PropertyImages != null && PropertyImages.Count > 0)
            {

                foreach (var image in PropertyImages)
                {
                    //Get the File Name
                    string fileName = Path.GetFileName(image.FileName);
                    string path = "";
                    string path1 = "";
                    //string query = "Select Max(ImageId) from images";
                    int lastId = -1;
                    try
                    {
                        using (SqlConnection connection = GetSqlConnection())
                        {
                            connection.Open();

                            var result = connection.ExecuteScalar("GetMaxImageId", null, commandType: CommandType.StoredProcedure);
                            if (result != null && result != DBNull.Value)
                            {
                                lastId = (int)result;
                            }
                            else
                            {
                                lastId = 0;
                            }

                            connection.Close();
                        }

                        //splitting Image name
                        string[] str = fileName.Split('.');
                        fileName = str[0] + lastId.ToString() + "." + str[1];

                        //combine the file name with images folder path
                        path = Path.Combine(RootPath, "assets\\images\\", fileName);

                        //path to upload into image table
                        path1 = Path.Combine("/", "assets/images/", fileName);



                        //save the file to the server
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }



                    }
                    catch (Exception ex)
                    {

                        response.IsSuccess = false;
                        response.Message = ex.Message;

                    }


                    //here storing images url into image table
                    //string query2 = "insert into images (PropertyId,Url,CreatedAt) values(@PropertyId,@path,CURRENT_TIMESTAMP)";
                    try
                    {
                        SqlConnection connection;
                        using (connection = ConnectionManager.GetSqlConnection())
                        {
                            connection.Open();

                            var parameters = new DynamicParameters();
                            parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
                            parameters.Add("@path", path1, DbType.String, ParameterDirection.Input);

                            connection.ExecuteScalar("InsertImageUrl", parameters, commandType: CommandType.StoredProcedure);

                        }
                        connection.Close();

                    }
                    catch (Exception ex)
                    {
                        response.IsSuccess = false;
                        response.Message = ex.Message;
                    }


                }



            }
            else
            {
                response.IsSuccess = false;
                response.Message = "No Property";
            }

            return response;

        }
        #endregion

        #region PropertyPaperUpload
        //method to save propertypaper url into the property
        public async Task<PropertyResponse> PropertyPaperUpload(IFormFile PropertyPaper, int PropertyId, string RootPath, PropertyResponse response)
        {

            //Get the File Name
            string fileName = Path.GetFileName(PropertyPaper.FileName);
            string path;
            string path1 = null;

            int lastId = -1;
            if (PropertyPaper != null)
            {

                try
                {

                    //splitting Image name
                    string[] str = fileName.Split('.');
                    fileName = str[0] + PropertyId.ToString() + "." + str[1];


                    //combine the file name with images folder path
                    path = Path.Combine(RootPath, "assets\\propertypaper\\", fileName);

                    //path to upload into image table
                    path1 = Path.Combine("/", "assets/propertypaper/", fileName);

                    //save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await PropertyPaper.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }

                //here storing propertypaper url into property table
                //string query2 = "update property set PropertyPaperUrl=@PropertyPaperUrl where PropertyId=" + PropertyId;

                try
                {

                    using (SqlConnection connection = ConnectionManager.GetSqlConnection())
                    {
                        connection.Open();

                        var parameter = new DynamicParameters();
                        parameter.Add("@PropertyPaperUrl", path1, DbType.String, ParameterDirection.Input);
                        parameter.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);


                        connection.ExecuteScalar("UpdatePropertyPaperUrl", parameter, commandType: CommandType.StoredProcedure);


                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }

            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Null property Paper";
            }
            return response;

        }

        #endregion

        #region  GetListedPropertyList
        //Getting List of Listed Property
        public List<PropertyModel> GetListedPropertyList(int UserId)
        {
            var ListedProperty = new List<PropertyModel>();
            //string query = "select * from property where AdminAction=1 and Blocked=0 and UserId=@UserId";
            //string query = "select * from property where Blocked=0 and UserId=@UserId";
            DynamicParameters parameter = new();
            parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetOwnerListedProperty", parameter, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        var Property = new PropertyModel
                        {
                            PropertyId = (int)reader["PropertyId"],
                            ImageUrlString = CommonService.GetImageUrl((int)reader["PropertyId"]),
                            OwnerName = CommonService.GetOwnerName((int)reader["UserId"]),
                            PropertyAddress = (string)reader["PropertyAddress"],
                            UserId = (int)reader["UserId"],
                            Price = reader["Price"].ToString(),
                            Deposit = reader["Deposit"].ToString(),
                            Type = reader["Type"].ToString(),
                            Rooms = reader["Rooms"].ToString(),
                            Furnishing = reader["Furnishing"].ToString(),
                            Area = reader["Area"].ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            StateId = (int)reader["StateId"],
                            CityId = (int)reader["CityId"]

                        };
                        ListedProperty.Add(Property);
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ListedProperty;
        }
        #endregion


        #region GetBlockedPropertyList
        public List<PropertyModel> GetBlockedPropertyList(int UserId)
        {
            var ListedProperty = new List<PropertyModel>();
            //string query = "select * from property where AdminAction=1 and Blocked=0 and UserId=@UserId";
            //string query = "select * from property where Blocked=1 and UserId=@UserId";
            DynamicParameters parameter = new();
            parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetOwnerBlockedListedProperty", parameter, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        var Property = new PropertyModel
                        {
                            PropertyId = (int)reader["PropertyId"],
                            ImageUrlString = CommonService.GetImageUrl((int)reader["PropertyId"]),
                            OwnerName = CommonService.GetOwnerName((int)reader["UserId"]),
                            PropertyAddress = (string)reader["PropertyAddress"],
                            UserId = (int)reader["UserId"],
                            Price = reader["Price"].ToString(),
                            Deposit = reader["Deposit"].ToString(),
                            Type = reader["Type"].ToString(),
                            Rooms = reader["Rooms"].ToString(),
                            Furnishing = reader["Furnishing"].ToString(),
                            Area = reader["Area"].ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            StateId = (int)reader["StateId"],
                            CityId = (int)reader["CityId"]

                        };
                        ListedProperty.Add(Property);
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ListedProperty;
        }
        #endregion



        #region ListOfPropertyRequest
        public List<PropertyModel> ListOfPropertyRequest(int OwnerId)
        {
            //string query = "select DISTINCT PropertyId from queries where ReceiverId=@OwnerId";
            List<PropertyModel> list = new();
            var parameter = new DynamicParameters();
            parameter.Add("@OwnerId", OwnerId, DbType.Int64, ParameterDirection.Input);
            try
            {
                using (SqlConnection con = GetSqlConnection())
                {
                    con.Open();
                    var reader = con.ExecuteReader("ListOfPropertyRequest", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            var model = CommonService.GetPropertyDetails((int)reader["PropertyId"]);
                            list.Add(model);
                        }



                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return list;
        }

        #endregion



        #region GetCustomerData
        //method to retrieve customer data
        public CustomerModel GetCustomerData(int CustomerId)
        {
            //string query = "select * from customer where CustomerId=@CustomerId";
            CustomerModel customer = new CustomerModel();
            DynamicParameters parameter = new();
            parameter.Add("@CustomerId", CustomerId, DbType.Int64, ParameterDirection.Input);
            try
            {
                using (SqlConnection con = GetSqlConnection())
                {
                    con.Open();
                    var reader = con.ExecuteReader("GetCustomerData", parameter, commandType: CommandType.StoredProcedure);

                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            customer = new CustomerModel()
                            {
                                CustomerId = (int)reader["CustomerId"],
                                Name = reader["Name"].ToString(),
                                Address = reader["Address"].ToString(),
                                Mobile = reader["Mobile"].ToString(),
                                Email = reader["Email"].ToString()

                            };
                        }

                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return customer;


        }
        #endregion



        #region ListOfCustomerToSpecificProperty
        public List<CustomerModel> ListOfCustomerToSpecificProperty(int PropertyId)
        {
            List<CustomerModel> CustomerListToSpecificProperty = new();
            CustomerModel customer = new CustomerModel();
            //string query = "select Distinct SenderId from Queries where PropertyId=@PropertyId";
            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("ListOfCustomerToSpecificProperty", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            int customerId = (int)reader["SenderId"];
                            customer = GetCustomerData(customerId);



                            CustomerListToSpecificProperty.Add(customer);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return CustomerListToSpecificProperty;
        }
        #endregion


        #region ListOfInqueriesByCustomerToOwnerSpecificProperty
        public List<QueryViewModel> ListOfInqueriesByCustomerToOwnerSpecificProperty(int PropertyId, int CustomerId, int OwnerId)
        {
            List<QueryViewModel> InqueriesListToSpecificProperty = new();
            //string query = "select * from queries where PropertyId=@PropertyId and ((SenderId=@CustomerId or SenderId=@OwnerId)  and (ReceiverId=@OwnerId or ReceiverId=@CustomerId))";

            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@OwnerId", OwnerId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("ListOfInqueriesByCustomerToOwnerSpecificProperty", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            var InqueryModel = new QueryViewModel()
                            {
                                InqueryId = (int)reader["Id"],
                                SenderId = (int)reader["SenderId"],
                                PropertyId = (int)reader["PropertyId"],
                                Message = reader["Message"].ToString(),
                                SentOn = (DateTime)reader["SentOn"],
                                ReceiverId = (int)reader["ReceiverId"]


                            };
                            InqueriesListToSpecificProperty.Add(InqueryModel);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return InqueriesListToSpecificProperty;
        }
        #endregion


        #region GetUserDetails
        public CustomerModel GetUserDetails(int CustomerId)
        {
            var customer = new CustomerModel();
            //string query = "select * from customer where CustomerId=@CustomerId";
            DynamicParameters parameter = new();
            parameter.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetUserDetails", parameter, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        customer = new CustomerModel
                        {
                            CustomerId = (int)reader["CustomerId"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],


                        };

                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return customer;
        }
        #endregion

    }
}