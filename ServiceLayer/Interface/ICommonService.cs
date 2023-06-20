using Microsoft.AspNetCore.Http;
using Model;


namespace ServiceLayer.Interface
{
    public interface ICommonService
    {
        List<StateModel> GetStateList();
        List<CityModel> GetCityList(int StateId);
        List<string> GetImageUrl(int PropertyId);
        public string GetOwnerName(int UserId);
        PropertyModel GetPropertyDetails(int PropertyId);
        List<CustomerModel> GetUnverifiedUserList();
        void UpdateVerificationState(int UserId, int state);
        public bool SendQuerry(PropertyModel query);

        public bool InsertQuery(ChatModel query);
        public int isVerifiedController(int UserId);
        public FormResponse SubmitFlag(FormModel form);

        Task<bool> UploadProfile(string RootPath, IFormFile file, int UserId);

        public List<CustomerModel> GetBlockedUserList();
        bool FlagUser(FormModel model);
        bool UnFlagUser(int UserId, int FlaggedBy);
        List<FlagUserModel> GetFlaggedUserList();


    }
}
