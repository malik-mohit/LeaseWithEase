using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Model;
using ServiceLayer.Interface;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web.Helpers;


namespace ServiceLayer.Services
{
    public class UserService : ConnectionManager, IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly ICommonService CommonService;
        public UserService(string dbConnection, ICommonService CommonService) : base(dbConnection)
        {
            this.CommonService = CommonService;

        }

        #region UserAlreadyExists
        public bool UserAlreadyExists(string Email)
        {

            string storedProcName = "dbo.UserAlreadyExists";
            int result = -1;
            var parameter = new DynamicParameters();

            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                if (!string.IsNullOrEmpty(storedProcName))
                {
                    using (SqlConnection connection = GetSqlConnection())
                    {
                        connection.Open();
                        result = connection.ExecuteScalar<int>(storedProcName, parameter, commandType: CommandType.StoredProcedure);
                        connection.Close();


                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }


            if (result > 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region GetUserPassword

        public bool GetUserPassword(string Email, string UserEnteredPassword)
        {
            bool IsPasswordCorrect = false;
            //string query = "Select Password from customer where Email=@Email";
            var Parameter = new DynamicParameters();
            Parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("GetUserPassword", Parameter, commandType: CommandType.StoredProcedure);
                    string Password = "";
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            Password = reader["Password"].ToString();
                        }

                        if (UserEnteredPassword == Password)
                        {
                            IsPasswordCorrect = true;
                        }


                    }
                }
            }catch(Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return IsPasswordCorrect;

        }

        #endregion

        #region AddCustomerDetails
        public async Task<RegisterationResponse> AddCustomerDetails(RegisterModel customer)
        {
            RegisterationResponse response = new RegisterationResponse();
            response.IsSuccess = true;
            response.Message = "Successful";
            try
            {
                //if (customer.Email != null)
                //{
                //    var userExists = UserAlreadyExists(customer.Email);
                //    var otpVerified = IsOtpVerified(customer.Email);

                //    if (userExists)
                //    {
                //        response.IsSuccess = false;
                //        response.Message = "User Already Exists";
                //        return response;
                //    }

                //    if (!otpVerified)
                //    {
                //        response.IsSuccess = false;
                //        response.Message = "Please Verify Your Email";
                //        return response;
                //    }
                //}

                string storedProcName = "dbo.AddCustomer";
                var parameter = new DynamicParameters();
                parameter.Add("@Name", customer.Name, DbType.String, ParameterDirection.Input);
                parameter.Add("@Email", customer.Email, DbType.String, ParameterDirection.Input);
                parameter.Add("@Password", customer.Password, DbType.String, ParameterDirection.Input);
                parameter.Add("@Mobile", customer.Mobile, DbType.Int64, ParameterDirection.Input);
                parameter.Add("@Gender", customer.Gender, DbType.String, ParameterDirection.Input);
                parameter.Add("@Address", customer.Address, DbType.String, ParameterDirection.Input);
                parameter.Add("@StateId", customer.StateId, DbType.Int32, ParameterDirection.Input);
                parameter.Add("@CityId", customer.CityId, DbType.Int32, ParameterDirection.Input);

                if (!string.IsNullOrEmpty(storedProcName))
                {
                    using (SqlConnection connection = GetSqlConnection())
                    {
                        connection.Open();
                        int status = await connection.ExecuteAsync(storedProcName, parameter, commandType: CommandType.StoredProcedure);
                        connection.Close();
                        if (status <= 0)
                        {
                            response.IsSuccess = false;
                            response.Message = "Register Query Not Executed";
                            return response;
                        }

                    }
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

        #region AuthenticateUser
        public async Task<LoginResponse> AuthenticateUser(LoginModel loginDetails)
        {
            LoginResponse response = new LoginResponse();
            response.IsSuccess = true;
            response.Message = "Successful";

            var parameter = new DynamicParameters();
            parameter.Add("@Email", loginDetails.Email, DbType.String, ParameterDirection.Input);
            parameter.Add("@Password", loginDetails.Password, DbType.String, ParameterDirection.Input);

            string storedProcName = "dbo.AuthenticateUser";

            try
            {
                if (!string.IsNullOrEmpty(storedProcName))
                {
                    using (SqlConnection connection = GetSqlConnection())
                    {
                        connection.Open();
                        var obj = (await connection.QueryAsync(storedProcName, parameter, commandType: CommandType.StoredProcedure)).FirstOrDefault();
                        if (obj != null)
                        {
                            response.data.CustomerId = obj.CustomerId;
                            response.data.Name = obj.Name;
                            response.data.Email = obj.Email;
                            response.data.RoleId = obj.RoleId;
                        }

                    }
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

        #region UploadDocument
        public async Task<DocumentResponse> UploadDocument(IFormFile AadharImage, int UserId, string RootPath)
        {
            DocumentResponse response = new()
            {
                IsSuccess = true,
                Message = "Successful"
            };


            if (AadharImage != null)
            {
                string fileName = Path.GetFileName(AadharImage.FileName);
                string path = "";
                string path1 = "";
                try
                {
                    //splitting Image name
                    string[] str = fileName.Split('.');
                    fileName = str[0] + UserId.ToString() + "." + str[1];

                    //combine the file name with images folder path
                    path = Path.Combine(RootPath, "assets\\documents\\", fileName);

                    //path to upload into image table
                    path1 = Path.Combine("/", "assets/documents/", fileName);



                    //save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await AadharImage.CopyToAsync(stream);
                    }


                }


                catch (Exception ex)
                {

                    response.IsSuccess = false;
                    response.Message = ex.Message;

                }


                //here storing images url into image table
                //string query = "insert into document (UserId,AadharUrl,CreatedAt) values(@UserId,@AadharUrl,CURRENT_TIMESTAMP)";
                var parameter = new DynamicParameters();
                parameter.Add("@UserId", UserId, DbType.Int32, ParameterDirection.Input);
                parameter.Add("@AadharUrl", path1, DbType.String, ParameterDirection.Input);
                try
                {

                    using (SqlConnection connection = ConnectionManager.GetSqlConnection())
                    {
                        connection.Open();
                        connection.ExecuteScalar("UploadDocument", parameter, commandType: CommandType.StoredProcedure);
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
                response.Message = "Please Upload Document";
            }

            return response;

        }

        #endregion

        #region GetSearchedProperty
        public List<PropertyModel> GetSearchedProperty(int StateId, int CityId, string[] Type, int[] Rooms, string[] furnishing, int? minPrice, int? maxPrice, int? minArea, int? maxArea)
        {
            var ListedProperty = new List<PropertyModel>();
            string query = "select * from property where StateId=@StateId and CityId=@CityId and AdminAction=1 and Blocked=0 and PropertyRented=0";
            DynamicParameters parameter = new();
            if (Type != null && Type.Length > 0)
            {
                if (!Type.Contains("all"))
                {
                    query += " and Type IN ('" + string.Join("','", Type) + "')";
                    parameter.Add("@Type", string.Join(",", Type), DbType.String, ParameterDirection.Input);
                }
                
            }
            if (Rooms != null && Rooms.Length > 0)
            {

                query += " and Rooms IN ('" + string.Join("','", Rooms) + "')";
                parameter.Add("@Roomd", string.Join(",", Rooms), DbType.String, ParameterDirection.Input);


            }
            if (furnishing != null && furnishing.Length > 0)
            {
                query += " and Furnishing IN ('" + string.Join("','", furnishing) + "')";
                parameter.Add("@Furnishing", string.Join(",", furnishing), DbType.String, ParameterDirection.Input);
            }

            if (minPrice.HasValue)
            {
                query += " and Price>=@MinPrice";
                parameter.Add("@MinPrice", minPrice.ToString(), DbType.String, ParameterDirection.Input);

            }
            if (maxPrice.HasValue)
            {
                query += " and Price<=@MaxPrice";
                parameter.Add("@MaxPrice", maxPrice.ToString(), DbType.String, ParameterDirection.Input);

            }
            if (minArea.HasValue)
            {
                query += " and Area>=@MinArea";
                parameter.Add("@MinArea", minArea.ToString(), DbType.String, ParameterDirection.Input);

            }
            if (maxArea.HasValue)
            {
                query += " and Area<=@MaxArea";
                parameter.Add("@MaxArea", maxArea.ToString(), DbType.String, ParameterDirection.Input);

            }

            parameter.Add("@StateId", StateId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@CityId", CityId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader(query, parameter, commandType: CommandType.Text);
                    {
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

        #region HasLiked
        public bool HasLiked(int CustomerId, int PropertyId)
        {
            bool isTrue=false;
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var Parameters = new DynamicParameters();
                    Parameters.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
                    Parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
                    isTrue= ((int)connection.ExecuteScalar("HasLiked", Parameters, commandType: CommandType.StoredProcedure)) > 0;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);  
            }
            return isTrue;
        }
        #endregion

        #region IsLiked
        public static bool IsLiked(int CustomerId, int PropertyId)
        {
            bool  IsLiked=false;
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[likes] WHERE CustomerId = @CustomerId AND PropertyId = @PropertyId", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        IsLiked= ((int)command.ExecuteScalar()) > 0;
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return IsLiked;
        }
        #endregion

        #region HasFlaged
        public bool HasFlaged(int CustomerId, int PropertyId)
        {
            bool HasFlaged = false;
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[flags] WHERE CustomerId = @CustomerId AND PropertyId = @PropertyId", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        HasFlaged= ((int)command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return HasFlaged;
        }


        #endregion

        #region IsFlaged
        public static bool IsFlaged(int CustomerId, int PropertyId)
        {
            bool IsFlaged = false;
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[flags] WHERE CustomerId = @CustomerId AND PropertyId = @PropertyId", connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        IsFlaged= ((int)command.ExecuteScalar()) > 0;
                    }
                }
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
            return IsFlaged;
        }
        #endregion


        #region AddLike
        public void AddLike(int CustomerId, int PropertyId)
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    var Parameters = new DynamicParameters();
                    Parameters.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
                    Parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);

                    connection.ExecuteScalar("AddLike", Parameters, commandType: CommandType.StoredProcedure);

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region RemoveLike
        public void RemoveLike(int CustomerId, int PropertyId)
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    
                    var Parameters = new DynamicParameters();
                    Parameters.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
                    Parameters.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
                    connection.ExecuteScalar("RemoveLike", Parameters, commandType: CommandType.StoredProcedure);
                   
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion


        #region RemoveFlag
        public void RemoveFlag(int CustomerId, int PropertyId)
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    using (var command = new SqlCommand("RemoveFlag", connection))

                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CustomerId", CustomerId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);

                        command.ExecuteScalar();
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
        #region GetLikedId
        public List<int> GetLikedId(int CustomerId)
        {
            List<int> LikedPropId = new List<int>();
            try
            {

                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    //using (var command = new SqlCommand("Select PropertyId from [dbo].[likes] where CustomerId = @CustomerId", connection))
                    //{
                    var Parameters = new DynamicParameters();
                    Parameters.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);

                    var reader = connection.ExecuteReader("GetLikedId", Parameters, commandType: CommandType.StoredProcedure);

                    while (reader.Read())
                    {
                        int PropertyId = (int)reader["PropertyId"];
                        LikedPropId.Add(PropertyId);

                    }


                    //}
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return LikedPropId;
        }
        #endregion


        #region GetLikedPropertiesList
        public List<PropertyModel> GetLikedPropertiesList(List<int> LikedPropertiesList)
        {
            List<PropertyModel> PropertiesList = new();

            foreach (var LP in LikedPropertiesList)
            {
                //string query = "select * from [dbo].[property] where PropertyId=" + LP;
                var Parameter = new DynamicParameters();
                Parameter.Add("@PropertyId", LP, DbType.Int32, ParameterDirection.Input);

                try
                {
                    using (SqlConnection connection = GetSqlConnection())
                    {
                        connection.Open();


                        var reader = connection.ExecuteReader("GetLikedPropertiesList", Parameter, commandType: CommandType.StoredProcedure);

                        while (reader.Read())
                        {
                            PropertiesList.Add(new PropertyModel
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

                            });
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            return PropertiesList;

        }

        #endregion




        #region  ListOfInqueriesByProperty
        public List<QueryViewModel> ListOfInqueriesByProperty(int PropertyId, int CustomerId, int OwnerId)
        {
            List<QueryViewModel> InqueriesListToSpecificProperty = new();
            //string query = "select * from Queries where PropertyId=@PropertyId and ((SenderId=@CustomerId or SenderId=@OwnerId)  and (ReceiverId=@OwnerId or ReceiverId=@CustomerId))";
            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId", PropertyId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
            parameter.Add("@OwnerId", OwnerId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteReader("ListOfInqueriesByProperty", parameter, commandType: CommandType.StoredProcedure);
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

        #region  ListOfPropertyInCustomerDashboard
        public List<PropertyModel> ListOfPropertyInCustomerDashboard(int CustomerId)
        {
            //string query = "select DISTINCT PropertyId,ReceiverId from Queries where SenderId=@CustomerId";
            List<PropertyModel> list = new();
            List<PropertyModel> templist = new();
            var parameter = new DynamicParameters();
            parameter.Add("@CustomerId", CustomerId, DbType.Int32, ParameterDirection.Input);
            try
            {
                using (SqlConnection con = GetSqlConnection())
                {
                    con.Open();
                    var reader = con.ExecuteReader("ListOfPropertyInCustomerDashboard", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            var model = CommonService.GetPropertyDetails((int)reader["PropertyId"]);
                            model.ReceiverId = (int)reader["ReceiverId"];

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



        #region StoreOTP
        public bool StoreOTP(string Email)
        {
            var OTP = GenerateOTP();
            bool IsStored = false;
            //string query = "update customer SET OTP=@OTP where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            parameter.Add("@OTP", OTP, DbType.String, ParameterDirection.Input);

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("StoreOTP", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        IsStored = true;
                    }
                    connection.Close();
                    connection.Dispose();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IsStored = false;
            }

            return IsStored;
        }
        #endregion

        #region GenerateOTP
        public string GenerateOTP()
        {
            // Generate a random OTP code
            Random random = new Random();
            int otpCode = random.Next(100000, 999999);
            return otpCode.ToString();
        }

        #endregion


        #region GetOTP
        //Get OTP 
        public string GetOTP(string Email)
        {
            string OTP = null;
            //string query = "select OTP from customer where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("GetOTP", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        OTP = reader.ToString();
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return OTP;
        }
        #endregion


        #region GetRegistrationOTP
        public string GetRegistrationOTP(string Email)
        {
            string OTP = null;
            //string query = "select OTP from otp where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("GetRegistrationOTP", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        OTP = reader.ToString();
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return OTP;
        }
        #endregion

        #region SendOtpAsync
        //sendOTP TO Email
        public async Task<bool> SendOtpAsync(string email, string OTP, IConfiguration _configuration, string state)
        {



            // Create a new MimeMessage object
            var message = new MimeMessage();

            // Set the sender and recipient email addresses
            message.From.Add(new MailboxAddress("HouseRent", _configuration["Smtp:FromEmail"]));
            message.To.Add(new MailboxAddress("HouseRent", email));

            // Set the subject and body of the email
            if (state == "forget")
            {
                message.Subject = "OTP Code for Password Reset";
                message.Body = new TextPart("plain")
                {
                    Text = $"Your OTP code is {OTP}. Please enter this code on the verification page to reset your password."
                };
            }
            else
            {
                message.Subject = "OTP Code for Verify Email";
                message.Body = new TextPart("plain")
                {
                    Text = $"Your OTP code is {OTP}. Please enter this OTP for email verification."
                };
            }

            try
            {
                // Create a new SmtpClient object
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Connect to the SMTP server
                    await client.ConnectAsync(_configuration["Smtp:Server"], int.Parse(_configuration["Smtp:Port"]), true);

                    // Authenticate with the SMTP server
                    client.Authenticate(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);

                    // Send the email message
                    await client.SendAsync(message);

                    // Disconnect from the SMTP server
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }


            return true;
        }
        #endregion

        #region RegisterOtpVerified
        public bool RegisterOtpVerified(string Email)
        {
            var result = false;
            try
            {
               
                var Parameter = new DynamicParameters();
                Parameter.Add("Email", Email, DbType.String, ParameterDirection.Input);
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("RegisterOtpVerified", Parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        result = true;
                    }
                    connection.Close();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
        #endregion


        #region StoreRegistrationOtp
        public bool StoreRegistrationOtp(string email, string OTP)
        {

            bool success = true;

            bool EmailIsAlreadyPresent = false;
            try
            {
                //string query = "select OtpId from otp where Email=@Email";
                var Parameter = new DynamicParameters();
                Parameter.Add("Email", email, DbType.String, ParameterDirection.Input);
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("RegisterOtpVerified", Parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        EmailIsAlreadyPresent = true;
                    }
                    connection.Close();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            if (!EmailIsAlreadyPresent)
            {
                try
                {
                    //string querry = "INSERT INTO otp (OTP, Email, IsVerified, Timestamp) VALUES (@OTP, @Email, 0, CURRENT_TIMESTAMP)";
                    DynamicParameters parameter = new();
                    parameter.Add("@OTP", OTP, DbType.Int32, ParameterDirection.Input);
                    parameter.Add("@Email", email, DbType.String, ParameterDirection.Input);

                    using SqlConnection connection = GetSqlConnection();
                    connection.Open();
                    connection.ExecuteScalar("UpdatingOtp", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    success = false;
                }
            }
            else
            {
                try
                {
                    //string query = "update otp set OTP=@OTP,IsVerified=0,TimeStamp=CURRENT_TIMESTAMP where Email=@Email";
                    var Parameters = new DynamicParameters();
                    Parameters.Add("Email", email, DbType.String, ParameterDirection.Input);
                    Parameters.Add("@OTP", OTP, DbType.Int32, ParameterDirection.Input);
                    using (SqlConnection connection = GetSqlConnection())
                    {
                        connection.Open();
                        var reader = connection.ExecuteReader("UpdatingOtpIfAlreadyExists", Parameters, commandType: CommandType.StoredProcedure);
                        connection.Close();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return success;
        }

        #endregion

        #region VerifyOTP
        //to Verify OTP
        public bool VerifyOTP(string OTP, string Email)
        {

            bool IsVerified = false;
            var userOTP = GetOTP(Email);

            if (userOTP != null && userOTP == OTP)
            {
                IsVerified = true;


            }

            return IsVerified;

        }

        #endregion


        #region VerifyOtpAction
        public bool VerifyOtpAction(string Email)
        {
            bool IsUpdated = true;
            //string query = "update otp SET IsVerified=1 where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("VerifyOtpAction", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IsUpdated = false;

            }

            return IsUpdated;

        }
        #endregion
   

        #region IsOtpVerified
        public bool IsOtpVerified(string Email)
        {
            var Isverified = 0;
            //string query = "select IsVerified from otp where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("IsOtpVerified", parameter, commandType: CommandType.StoredProcedure);
                    if (reader != null)
                    {
                        Isverified = (int)reader;
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Isverified == 1;
        }
        #endregion

        #region HideProperty
        public void HideProperty(int UserId, int PropertyId)
        {
            //string query = "update property set PropertyRented=1 where UserId=@UserId and PropertyId=@PropertyId";

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("HideProperty", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);

                        command.ExecuteNonQuery();
                    }

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        #endregion

        #region UpdatePassword
        //method to Updated password 
        public bool UpdatePassword(string Email, string NewPassword)
        {
            bool IsUpdated = true;
            //string query = "update customer SET Password=@Password where Email=@Email";
            var parameter = new DynamicParameters();
            parameter.Add("@Password", NewPassword, DbType.String, ParameterDirection.Input);
            parameter.Add("@Email", Email, DbType.String, ParameterDirection.Input);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    var reader = connection.ExecuteScalar("UpdatePassword", parameter, commandType: CommandType.StoredProcedure);
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IsUpdated = false;

            }

            return IsUpdated;

        }
        #endregion


        #region ShowHiddenProperties
        public List<PropertyModel> ShowHiddenProperties(int UserId)
        {
            List<PropertyModel> list = new List<PropertyModel>();

            //string query = "select * from property where UserId=@UserId and PropertyRented=1";

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("ShowHiddenProperties", connection))
                    {
                        command.CommandType= CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", UserId);

                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var property = new PropertyModel
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

                            list.Add(property);


                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return list;
        }
        #endregion

        #region UnHideProperty
        public void UnHideProperty(int UserId, int PropertyId)
        {
            //string query = "update property set PropertyRented=0 where UserId=@UserId and PropertyId=@PropertyId";

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UnHideProperty", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);

                        command.ExecuteNonQuery();
                    }

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        #endregion


        #region IsHidden
        public static bool IsHidden(int UserId, int PropertyId)
        {
            var result = false;
            //string query = "select PropertyRented from property WHERE PropertyId = @PropertyId and UserId = @UserId and PropertyRented=1";

            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("IsHidden", connection))
                    {
                        command.CommandType= CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }

                    }

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return result;


        }


        #endregion


    }
}


