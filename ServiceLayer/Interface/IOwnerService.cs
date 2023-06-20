using Microsoft.AspNetCore.Http;
using Model;
namespace ServiceLayer.Interface
{
    public interface IOwnerService
    {
        
        Task<PropertyResponse> UploadProperty(PropertyModel Property, string RootPath);
        List<PropertyModel> GetListedPropertyList(int UserId);
        List<PropertyModel> GetBlockedPropertyList(int UserId);
        List<PropertyModel> ListOfPropertyRequest(int OwnerId);
        List<CustomerModel> ListOfCustomerToSpecificProperty(int PropertyId);
        List<QueryViewModel> ListOfInqueriesByCustomerToOwnerSpecificProperty(int PropertyId, int CustomerId, int OwnerId);
        public CustomerModel GetUserDetails(int CustomerId);



    }
}