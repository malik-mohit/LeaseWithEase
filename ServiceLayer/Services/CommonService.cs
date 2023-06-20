using Azure;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Model;
using ServiceLayer.Interface;
using System.Data;

namespace ServiceLayer.Services
{
    public class CommonService : ConnectionManager, ICommonService
    {
        public CommonService(string dbConnection) : base(dbConnection)
        {

        }
        #region GetStateList
        public List<StateModel> GetStateList()
        {
            List<StateModel> states = new();
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    string storedProcName = "dbo.GetStateList";
                    connection.Open();
                    var reader = connection.ExecuteReader(storedProcName, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        states.Add(new StateModel
                        {
                            StateId = Convert.ToInt32(reader["StateId"]),
                            StateName = reader["StateName"].ToString()

                        });
                    }

                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                //handle exception
            }

            return states;
        }
        #endregion


        #region GetCityList
        public List<CityModel> GetCityList(int StateId)
        {
            List<CityModel> city = new();
            var parameter = new DynamicParameters();
            parameter.Add("@StateId", StateId, DbType.Int32, ParameterDirection.Input);
            string storedProcName = "dbo.GetCityList";
            using (SqlConnection connection = GetSqlConnection())
            {
                connection.Open();
                var reader = connection.ExecuteReader(storedProcName, parameter, commandType: CommandType.StoredProcedure);

                while (reader.Read())
                {
                    city.Add(new CityModel
                    {
                        CityId = Convert.ToInt32(reader["CityId"]),
                        CityName = reader["CityName"].ToString(),
                        StateId = Convert.ToInt32(reader["StateId"])

                    });
                }

                connection.Close();

            }

            return city;
        }
        #endregion

        #region GetImageUrl
        //method to reterive the URL of Image on the basis of PropertyId
        public List<string> GetImageUrl(int PropertyId)
        {
            List<string> ImageUrl = new();
            //String querry = "select Url from images where PropertyId=@PropertyId";
            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId", PropertyId, DbType.String, ParameterDirection.Input);

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetImageUrl", parameter, commandType: CommandType.StoredProcedure);
                    while (reader.Read())
                    {
                        string img = reader["Url"].ToString();
                        ImageUrl.Add(img);
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            return ImageUrl;
        }
        #endregion


        #region GetOwnerName
        public string GetOwnerName(int UserId)
        {
            string OwnerName = "";
            SqlConnection? con = null;
            try
            {
                using (con = GetSqlConnection())
                {
                    con.Open();
                    {
                        using (SqlCommand cmd = new SqlCommand("GetOwnerName", con))
                        {
                            cmd.CommandType= CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserId", UserId);
                            var obj = cmd.ExecuteScalar();
                            OwnerName = (string)obj;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                con.Close();
            }
            return OwnerName;
        }

        #endregion


        #region  GetPropertyDetails
        //Getting PropertyDetails on the basis of PropertyId
        public PropertyModel GetPropertyDetails(int PropertyId)
        {
            //now get data from database

            PropertyModel PropertyDetail = new();
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("GetPropertyDetails", connection))
                    {
                        cmd.CommandType= CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PropertyId", PropertyId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PropertyDetail = new PropertyModel
                                {
                                    PropertyId = (int)reader["PropertyId"],
                                    ImageUrlString = GetImageUrl((int)reader["PropertyId"]),
                                    OwnerName = GetOwnerName((int)reader["UserId"]),
                                    PropertyAddress = (string)reader["PropertyAddress"],
                                    UserId = (int)reader["UserId"],
                                    Price = reader["Price"].ToString(),
                                    Deposit = reader["Deposit"].ToString(),
                                    Type = reader["Type"].ToString(),
                                    Rooms = reader["Rooms"].ToString(),
                                    Furnishing = reader["Furnishing"].ToString(),
                                    Area = reader["Area"].ToString(),
                                    CreatedAt = (DateTime)reader["CreatedAt"],
                                    StateName = GetStateName((int)reader["StateId"]),
                                    CityName = GetCityName((int)reader["CityId"])
                                };

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return PropertyDetail;
        }
        #endregion

        #region GetStateName
        //method to return StateName
        public string GetStateName(int StateId)
        {
            string? StateName = null;
            //string query = "select StateName from state where StateId=@StateId";
            var parameter = new DynamicParameters();
            parameter.Add("@StateId", StateId, DbType.Int32, ParameterDirection.Input);

            try
            {
                using(SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object obj = connection.ExecuteScalar("GetStateName", parameter, commandType: CommandType.StoredProcedure);
                      
                    if (obj != null)
                    {
                        StateName = obj.ToString();
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return StateName;
        }
        #endregion

        #region GetCityName
        //method to return CityId
        public string GetCityName(int CityId)
        {
            string? CityName = null;
            //string query = "select CityName from city where CityId=@CityId";
            var parameter = new DynamicParameters();
            parameter.Add("@CityId", CityId, DbType.Int32, ParameterDirection.Input);
          
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var obj = connection.ExecuteScalar("GetCityName", parameter, commandType: CommandType.StoredProcedure);
                    if (obj != null)
                    {
                        CityName = obj.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CityName;
        }
        #endregion


        #region GetUnverifiedUserList
        //function to get all unverified user
        public List<CustomerModel> GetUnverifiedUserList()
        {
            List<CustomerModel> UnverifiedUserList = new();
            //string query = "select * from customer where Verified=1";
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetUnverifiedUserList", commandType: CommandType.StoredProcedure);

                    while (reader.Read())
                    {
                        var customer = new CustomerModel()
                        {
                            CustomerId=(int)reader["CustomerId"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Password = reader["Password"].ToString(),
                            Mobile = reader["Mobile"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Address = reader["Address"].ToString(),
                            RoleId = (int)reader["RoleId"],
                            StateName = GetStateName((int)reader["StateId"]),
                            CityName = GetStateName((int)reader["CityId"]),
                            UserStatus = (int)reader["UserStatus"],
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            Verified = (int)reader["Verified"]
                        };
                        UnverifiedUserList.Add(customer);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return UnverifiedUserList;
        }
        #endregion


        #region GetBlockedUserList
        public List<CustomerModel> GetBlockedUserList()
        {
            List<CustomerModel> UnverifiedUserList = new();
            //string query = "select * from customer where Verified=3";
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetBlockedUserList", commandType: CommandType.StoredProcedure);

                    while (reader.Read())
                    {
                        var customer = new CustomerModel()
                        {
                            CustomerId=(int)reader["CustomerId"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Password = reader["Password"].ToString(),
                            Mobile = reader["Mobile"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Address = reader["Address"].ToString(),
                            RoleId = (int)reader["RoleId"],
                            StateName = GetStateName((int)reader["StateId"]),
                            CityName = GetStateName((int)reader["CityId"]),
                            UserStatus = (int)reader["UserStatus"],
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            Verified = (int)reader["Verified"]
                        };
                        UnverifiedUserList.Add(customer);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return UnverifiedUserList;

        }

        #endregion

         #region isVerifiedController
       
        public int isVerifiedController(int UserId)
        {
            int isVerified = 0;
            //string query = "select Verified from customer where CustomerId=@UserId";
            var parameter = new DynamicParameters();
            parameter.Add("@UserId",UserId,DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object reader = connection.ExecuteScalar("isVerifiedController", parameter, commandType: CommandType.StoredProcedure);
                    if ( reader!=null && (int)(reader) == 0)
                    {
                        isVerified = 0;
                    }else if(reader!=null && (int)reader == 1)
                    {
                        isVerified = 1;
                    }else if(reader!=null && (int)reader == 2)
                    {
                        isVerified = 2;
                    }


                }
            }catch (Exception ex)
            {
                       Console.WriteLine(ex.ToString());
            }
            return isVerified;
        }
        #endregion


        #region sVerified
        public static int isVerified(int UserId)
        {
            int isVerified = 0;
            //string query = "select Verified from customer where CustomerId=@UserId";
            var parameter = new DynamicParameters();
            parameter.Add("@UserId",UserId,DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object reader = connection.ExecuteScalar("isVerified", parameter, commandType: CommandType.StoredProcedure);
                    if ( reader!=null && (int)(reader) == 0)
                    {
                        isVerified = 0;
                    }else if(reader!=null && (int)reader == 1)
                    {
                        isVerified = 1;
                    }else if(reader!=null && (int)reader == 2)
                    {
                        isVerified = 2;
                    }


                }
            }catch (Exception ex)
            {
                       Console.WriteLine(ex.ToString());
            }
            return isVerified;
        }
        #endregion


        #region sListed
        public static bool isListed(int PropertyId)
        {
            var isListed = false;
            //string query = "select AdminAction from property where PropertyId=@PropertyId";
            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId",PropertyId,DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object reader = connection.ExecuteScalar("isListed", parameter, commandType: CommandType.StoredProcedure);
                    if ( reader!=null && (int)(reader) == 1)
                    {
                        isListed=true;
                    }


                }
            }catch (Exception ex)
            {
                       Console.WriteLine(ex.ToString());
            }
            return isListed;
        }
        #endregion


        #region UpdateVerificationState
        public void UpdateVerificationState(int UserId, int state)
        {

            //string query = "update customer set Verified=@State where CustomerId=@UserId";
            var parameter = new DynamicParameters();
            parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@State", state, DbType.Int32, ParameterDirection.Input);
            try
            {
                //update the RoleId of the customer from 2 to 3 (Customer to Owner)
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    Object obj = connection.ExecuteScalar("UpdateVerificationState", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
        #endregion


        #region SendQuerry
        public bool SendQuerry(PropertyModel query)
        {
            bool inserted = true;
            try
            {
                // Insert the query into the database using ADO.NET
                using (SqlConnection connection = GetSqlConnection())
                {
                    //string queryText = "INSERT INTO queries (SenderId, Message, PropertyId, SentOn,ReceiverId) VALUES (@CustomerId, @Message, @PropertyId, CURRENT_TIMESTAMP,@OwnerId)";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SendQuerry", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CustomerId", query.SenderId);
                        command.Parameters.AddWithValue("@Message", query.Message);
                        command.Parameters.AddWithValue("@PropertyId", query.PropertyId);
                        command.Parameters.AddWithValue("@OwnerId", query.ReceiverId);
                        command.ExecuteScalar();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                inserted = false;
                Console.WriteLine(ex.ToString());
            }

            return inserted;
        }
        #endregion


        #region  InsertQuery
        public bool InsertQuery(ChatModel query)
        {
            bool isInsert = true;            
            try
            {
                // Insert the query into the database using ADO.NET
                using (SqlConnection connection = GetSqlConnection())
                {
                    //string queryText = "INSERT INTO Queries (SenderId, Message, PropertyId, SentOn,ReceiverId) VALUES (@CustomerId, @Message, @PropertyId, CURRENT_TIMESTAMP,@OwnerId)";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("InsertQuery", connection))
                    {
                        command.CommandType=CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CustomerId", query.SenderId);
                        //command.Parameters.AddWithValue("@CustomerEmail", query.CustomerEmail);

                        command.Parameters.AddWithValue("@Message", query.Message);
                        command.Parameters.AddWithValue("@PropertyId", query.PropertyId);
                        command.Parameters.AddWithValue("@OwnerId", query.ReceiverId);
                        command.ExecuteScalar();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                isInsert = false;
                Console.WriteLine(ex.ToString());
            }

            return isInsert;
        }
        #endregion


        #region UploadProfile
        public async Task<bool> UploadProfile(string RootPath,IFormFile file, int UserId)
        {
            bool IsUpdated = false;
            var Imageurlpath = "";
            if (file != null && file.Length > 0)
            {

                var fileName = file.FileName;

                //manipulating file name
                //splitting Image name
                string[] str = fileName.Split('.');
                fileName = UserId.ToString()  + "." + str[1];





                var filePath = Path.Combine(RootPath, "assets\\profilepic", fileName);
                 Imageurlpath = Path.Combine("/", "assets/profilepic/", fileName);
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                //here storing images url into image table
                //string query = "update customer SET ProfilePic=@Imageurlpath where CustomerId=@CustomerId";
                try
                {
                    SqlConnection connection;
                    using (connection = GetSqlConnection())
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("UploadProfile", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@CustomerId", UserId);
                            command.Parameters.AddWithValue("@Imageurlpath", Imageurlpath);

                            command.ExecuteScalar();

                        }
                        connection.Close();
                    }


                    IsUpdated = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return IsUpdated;

        }
        #endregion


        #region  SubmitFlag
        public FormResponse SubmitFlag(FormModel form)
        {
            FormResponse response = new()
            {
                IsSuccess = true,
                Message = "Successful"
            };
            //string query = "INSERT INTO flags (CustomerId, PropertyId, comment, TimeStamp, FlaggedStatus) VALUES (@CustomerId, @PropertyId, @Comment, CURRENT_TIMESTAMP, 0)";
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@CustomerId", form.CustomerId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@PropertyId", form.PropertyId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@Comment", form.Comment, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    connection.ExecuteScalar("SubmitFlag", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
            }
            catch (Exception ex)
            {

                response.IsSuccess = false;
                response.Message = ex.ToString();
            }
            return response;

        }

        #endregion

        #region GetProfilePicUrl
        public static string GetProfilePicUrl(int UserId)
        {
            string? ImageUrl = null;
            //string query = "select ProfilePic from customer where CustomerId=@CustomerId";
            var parameter = new DynamicParameters();
            parameter.Add("@CustomerId", UserId, DbType.Int32, ParameterDirection.Input);

            try
            {
                using(SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("GetProfilePicUrl", parameter, commandType: CommandType.StoredProcedure);
                    if(reader!= null)
                    {
                        ImageUrl= reader.ToString();
                    }
                    connection.Close();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message );
            }
            return ImageUrl;
        }

        #endregion


        #region FlagUser
        public bool FlagUser(FormModel model)
        {
            bool flaguser = false;
            string query = "Insert into flagged_user (UserId,FlaggedUserStatus,FlaggedBy,Comment,TimeStamp) values(@UserId,1,@FlaggedBy,@Comment,CURRENT_TIMESTAMP)";
            var Parameters = new DynamicParameters();
            Parameters.Add("@UserId",model.CustomerId, DbType.Int32,ParameterDirection.Input);
            Parameters.Add("@Comment",model.Comment, DbType.String,ParameterDirection.Input);
            Parameters.Add("@FlaggedBy",model.FlaggedBy, DbType.Int32,ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.Execute(query, Parameters, commandType: CommandType.Text);
                    if ( (int)reader>0)
                    {
                        flaguser = true;
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           return flaguser;

        }
        #endregion


        #region IsUserFlag
        public static bool IsUserFlag(int UserId, int FlaggedBy)
        {
            bool IsFlag = false;
            string query = "select FlaggedUserStatus from flagged_user where UserId=@UserId and FlaggedBy=@FlaggedBy";
            var Parameters = new DynamicParameters();
            Parameters.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            Parameters.Add("@FlaggedBy",FlaggedBy, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar<int>(query, Parameters, commandType: CommandType.Text);
                    if (reader!=null&& (int)reader>0)
                    {
                        IsFlag = true;
                    }

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return IsFlag;
        }

        #endregion


        #region UnFlagUser
        public bool UnFlagUser(int UserId, int FlaggedBy)
        {
            bool IsFlagged = false;
            //string query = "delete from flagged_user where UserId=@UserId and FlaggedBy=@FlaggedBy";
            var Parameters = new DynamicParameters();
            Parameters.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            Parameters.Add("@FlaggedBy", FlaggedBy, DbType.Int32, ParameterDirection.Input);

            try
            {
                using(SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar<int>("UnFlagUser", Parameters, commandType: CommandType.StoredProcedure);
                    if (reader > 0)
                    {
                        IsFlagged = true;
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return IsFlagged;
        }

        #endregion


        #region GetFlaggedUserList

      
        public List<FlagUserModel> GetFlaggedUserList()
        {
            List<FlagUserModel> list = new();
            //string query = "select Distinct UserId from flagged_user";
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetFlaggedUserList", commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            FlagUserModel model = new FlagUserModel();
                            var UserId = (int)reader["UserId"];
                            model.CustomerId = UserId;
                            model.CustomerName = GetOwnerName(UserId);
                            model.FlagCount = CountFlagOfUser(UserId);
                            list.Add(model);

                        }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        #endregion

        #region CountFlagOfUser
        public int CountFlagOfUser(int UserId)
        {
            int count = 0;
            //string query = "SELECT COUNT(*) FROM flagged_user WHERE UserId =@UserId";
            var Parameter = new DynamicParameters();
            Parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using(SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader =(int) connection.ExecuteScalar("CountFlagOfUser", Parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        count = (int)reader;
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return count;
        }
        #endregion
    }
}
