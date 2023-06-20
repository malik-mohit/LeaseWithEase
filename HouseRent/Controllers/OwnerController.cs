using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json.Linq;
using ServiceLayer.Interface;
using ServiceLayer.Services;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace HouseRent.Controllers
{
    public class OwnerController : Controller
    {
        private readonly IOwnerService OwnerService;
        private readonly ICommonService CommonService;
        private readonly IUserService UserService;
        
        public OwnerController(IOwnerService OwnerService, ICommonService CommonService, IUserService UserService)
        {
            this.OwnerService = OwnerService;
            this.CommonService = CommonService;
            this.UserService = UserService;
            
        }
        public IActionResult LikedProperties()
        {
            var CustomerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(x => x.Type == "CustomerId").FirstOrDefault()?.Value);
            List<int> LikePropIds = UserService.GetLikedId(CustomerId);
            List<PropertyModel> data = UserService.GetLikedPropertiesList(LikePropIds);
            return View(data);
        }
        public IActionResult ListedProperties()
        {
            var UserId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            var data = OwnerService.GetListedPropertyList(UserId);
            return View(data);
        }
        public IActionResult ReceivedRequests()
        {
            return View();
        }
        public IActionResult SentRequests()
        {

            var customerid = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<Model.PropertyModel> listOfPropertyInquery = UserService.ListOfPropertyInCustomerDashboard(customerid);

           
            return View(listOfPropertyInquery);
        }
        public IActionResult BlockedProperties()
        {
            var UserId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            var data = OwnerService.GetBlockedPropertyList(UserId);
            return View(data);
        }

        public IActionResult HiddenProperties()
        {
            var UserId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);

            var data = UserService.ShowHiddenProperties(UserId);
            return View(data);
        }

        public IActionResult HideProperty(int UserId, int id)
        {
            UserService.HideProperty(UserId, id);

            return RedirectToAction("Detail", "Property", new { id });
        }
        public IActionResult UnHideProperty(int UserId, int id)
        {
            UserService.UnHideProperty(UserId, id);

            return RedirectToAction("Detail", "Property", new { id });
        }
        public IActionResult Enquries()
        {
            var OwnerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<PropertyModel> list = OwnerService.ListOfPropertyRequest(OwnerId);
            return View(list);
        }

        public IActionResult InqueryCustomerList(int PropertyId)
        {

            List<CustomerModel> customerListToSpecificProperty = OwnerService.ListOfCustomerToSpecificProperty(PropertyId);
            var model = new Tuple<List<CustomerModel>, int>(customerListToSpecificProperty, PropertyId);
            return View(model);
        }

        public IActionResult InqueryofCustomer(int propertyid, int customerId)
        {
            var OwnerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<QueryViewModel> InqueriesListOfSpecificProperty = OwnerService.ListOfInqueriesByCustomerToOwnerSpecificProperty(propertyid, customerId, OwnerId);
            CustomerModel customer = OwnerService.GetUserDetails(customerId);

            PropertyModel chat = new PropertyModel();
            chat.Customer = customer;
            chat.QueryList = InqueriesListOfSpecificProperty;
            chat.SenderId = InqueriesListOfSpecificProperty[0].ReceiverId;
            chat.ReceiverId = customerId;

            return View(chat);
        }
        public IActionResult ListOfCustomerInqueries(int propertyid, int ownerid)
        {

            var customerid = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<QueryViewModel> ListModel = UserService.ListOfInqueriesByProperty(propertyid, customerid, ownerid);
            CustomerModel customer = OwnerService.GetUserDetails(ownerid);

            PropertyModel chat = new PropertyModel();
            chat.Customer = customer;
            chat.SenderId = ListModel[0].SenderId;
            chat.ReceiverId = ListModel[0].ReceiverId;
            chat.PropertyId = ListModel[0].PropertyId;
            chat.QueryList = ListModel;

            return View(chat);
        }


        [HttpPost]
        public IActionResult SendResponse(ChatModel model,string url)
        {

            CommonService.InsertQuery(model);
            return Redirect(url);
        }

    }
}
