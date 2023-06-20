
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using MimeKit;
using Model;
using ServiceLayer.Interface;

namespace ServiceLayer.Services
{
    public class AdminService : ConnectionManager, IAdminService
    {
        private readonly ICommonService CommonService;
        public AdminService(string dbConnection, ICommonService? CommonService) : base(dbConnection)
        {
            this.CommonService = CommonService;
        }

        #region UnBlockProperty
        public void UnBlockProperty(int PropertyId)
        {
            DynamicParameters parameters= new DynamicParameters();
            parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using SqlConnection connection = GetSqlConnection();
                connection.Open();
                connection.Execute("UnBlockProperty",parameters,commandType:CommandType.StoredProcedure);
                connection.Close();
            }
            catch(Exception ex) { }
            

        }
        #endregion
        # region IsBlocked
        public static bool IsBlocked(int PropertyId)
        {
            var obj = 0;
            
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using SqlConnection connection = GetSqlConnection();
                connection.Open();
                 obj= (int)connection.ExecuteScalar("IsBlocked", parameters, commandType: CommandType.StoredProcedure);
                connection.Close();
            }
            catch (Exception ex) {
              
            }

            return (obj>0);
        }
#endregion

        #region GetApprovedListData
        public List<PropertyModel> GetApprovedListData()
        {
            List<PropertyModel> data = new List<PropertyModel>();
            try
            {
                using (SqlConnection connection = ConnectionManager.GetSqlConnection())
                {
                    connection.Open();

                    var reader = connection.ExecuteReader("GetApprovedList", null, commandType: CommandType.StoredProcedure);

                    while (reader.Read())
                    {
                        var model = new PropertyModel
                        {
                            PropertyId = (int)reader["PropertyId"],
                            Price = reader["Price"].ToString(),
                            PropertyAddress = reader["PropertyAddress"].ToString(),
                            ImageUrlString = CommonService.GetImageUrl((int)reader["PropertyId"]),
                            Rooms = reader["Rooms"].ToString(),
                            Area = reader["Area"].ToString(),
                            Deposit = reader["Deposit"].ToString(),
                            Furnishing = reader["Furnishing"].ToString(),
                            Type = reader["Type"].ToString()

                        };
                        data.Add(model);
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return data;
        }
        #endregion


        #region ApproveOwnerProperty
        //function to Approve Owner Property
        public bool ApproveOwnerProperty(int PropertyId)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
                using (SqlConnection con = GetSqlConnection())
                {
                    con.Open();

                    //string query = "UPDATE property SET AdminAction = 1 WHERE PropertyId =" + PropertyId;

                    var obj = con.ExecuteReader("ApproveOwnerProperty", parameter, commandType: CommandType.StoredProcedure);
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;


        }

        #endregion

        #region GetListedPropertyList
        //Getting List of Listed Property
        public List<PropertyModel> GetListedPropertyList()
        {
            var ListedProperty = new List<PropertyModel>();
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    //string query = "select * from property where AdminAction=1 and Blocked=0";

                    var reader = connection.ExecuteReader("GetListedPropertyList", null, commandType: CommandType.StoredProcedure);

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


        #region BlockProperty
        //Fuction to blocked Owner Property
        public FormResponse BlockProperty(FormModel data)
        {
            FormResponse response = new()
            {
                IsSuccess = true,
                Message = "Successful"
            };
            //string query = "UPDATE property SET AdminAction = 1 , Blocked=1, ActionComment=@ActionComment WHERE PropertyId = @PropertyId";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@propertyId", data.PropertyId, DbType.Int32, ParameterDirection.Input);
            parameters.Add("@ActionComment", data.Comment, DbType.String, ParameterDirection.Input);
            try
            {
                using SqlConnection connection = GetSqlConnection();
                connection.Open();
                connection.ExecuteScalar("BlockProperty", parameters, commandType: CommandType.StoredProcedure);
                connection.Close();



            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;


        }

        #endregion

        #region GetBlockedPropertyList
        //Getting List of Listed Property
        public List<PropertyModel> GetBlockedPropertyList()
        {
            var BlockedPropertyList = new List<PropertyModel>();
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    //string query = "select * from property where Blocked=1";

                    var reader = connection.ExecuteReader("GetBlockedPropertyList", null, commandType: CommandType.StoredProcedure);

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
                        BlockedPropertyList.Add(Property);
                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return BlockedPropertyList;
        }

        #endregion

        #region GetDocument
        public string GetDocument(int UserId)
        {
            string DocumentUrl = "";
              string query = "select AadharUrl from document where UserId=@UserId";
                var Parameter = new DynamicParameters();
                Parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
                try
                {
                    using(SqlConnection connection = GetSqlConnection())
                    {
                        var reader = connection.ExecuteScalar(query, Parameter, commandType: CommandType.Text);
                        if (reader != null)
                        {
                            DocumentUrl = reader.ToString();
                        }
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            return DocumentUrl;
        }

        #endregion

        #region GetPropertyPaper
        public string GetPropertyPaper(int PropertyId)
        {
            string PaperUrl = "";
        
            var Parameter = new DynamicParameters();
            Parameter.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetPropertyPaper", Parameter, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        PaperUrl = reader["PropertyPaperUrl"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return PaperUrl;
        }
        #endregion

        #region GetFlaggedCommentList
        public List<string> GetFlaggedCommentList(int CustomerId)
        {
            List<string> FlaggedCommentList = new();
            //string query = "select Comment from flagged_user where UserId=@CustomerId";
            var Parameter = new DynamicParameters();
            Parameter.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetFlaggedCommentList", Parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            FlaggedCommentList.Add(reader["Comment"].ToString());
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return FlaggedCommentList;
        }
        #endregion

    }
}
