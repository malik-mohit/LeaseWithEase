using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Model;


namespace ServiceLayer.Interface
{
    public interface IUserService
    {
        bool UserAlreadyExists(string Email);
        Task<RegisterationResponse> AddCustomerDetails(RegisterModel customerModel);
        Task<LoginResponse> AuthenticateUser(LoginModel loginDetails);
        Task<DocumentResponse> UploadDocument(IFormFile AadharImage, int UserId, string RootPath);
        public List<PropertyModel> GetSearchedProperty(int StateId, int CityId, string[] Type, int[] Rooms, string[] furnishing, int? minPrice, int? maxPrice, int? minArea, int? maxArea);
        public bool HasLiked(int CustomerId, int PropertyId);
        public void AddLike(int CustomerId, int PropertyId);
        public bool HasFlaged(int CustomerId, int PropertyId);
        public void RemoveFlag(int CustomerId, int PropertyId);
        public void RemoveLike(int CustomerId, int PropertyId);
        public List<int> GetLikedId(int CustomerId);
        public List<PropertyModel> GetLikedPropertiesList(List<int> LikedPropertiesList);
        public List<QueryViewModel> ListOfInqueriesByProperty(int PropertyId, int CustomerId, int OwnerId);
        public List<PropertyModel> ListOfPropertyInCustomerDashboard(int CustomerId);
        bool StoreOTP(string Email);
        public Task<bool> SendOtpAsync(string email, string OTP, IConfiguration _configuration, string state);
        bool VerifyOTP(string OTP, string Email);
        bool UpdatePassword(string Email, string NewPassword);
        string GetOTP(string Email);
        string GenerateOTP();
        public string GetRegistrationOTP(string Email);
        public bool StoreRegistrationOtp(string email, string OTP);
        public bool VerifyOtpAction(string Email);
        public bool IsOtpVerified(string Email);
        bool GetUserPassword(string Email, string UserEnteredPassword);
        public void UnHideProperty(int UserId, int PropertyId);
        public List<PropertyModel> ShowHiddenProperties(int UserId);
        public void HideProperty(int UserId, int PropertyId);
        public bool RegisterOtpVerified(string Email);

    }
}
