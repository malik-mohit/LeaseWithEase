using Microsoft.AspNetCore.Mvc;
using Model;
using ServiceLayer.Interface;
using ServiceLayer.Services;
using System.Reflection;
using System.Security.Claims;

namespace HouseRent.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IUserService UserService;
        private readonly IAdminService AdminService;
        private readonly IOwnerService OwnerService;
        private readonly ICommonService CommonService;
        private readonly IWebHostEnvironment WebHostEnvironment;

        public CustomerController(IConfiguration Configuration, IUserService UserService, ICommonService CommonService, IWebHostEnvironment WebHostEnvironment, IAdminService AdminService, IOwnerService ownerService)
        {
            this.Configuration = Configuration;
            this.UserService = UserService;
            this.CommonService = CommonService;
            this.WebHostEnvironment = WebHostEnvironment;
            this.AdminService = AdminService;
            this.OwnerService = ownerService;

        }
        public IActionResult LikedProperties()
        {
            var CustomerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(x => x.Type == "CustomerId").FirstOrDefault()?.Value);
            List<int> LikePropIds = UserService.GetLikedId(CustomerId);
            List<PropertyModel> data = UserService.GetLikedPropertiesList(LikePropIds);
            return View(data);
        }
        public IActionResult SentRequests()
        {
            var customerid = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<Model.PropertyModel> listOfPropertyInquery = UserService.ListOfPropertyInCustomerDashboard(customerid);
            return View(listOfPropertyInquery);
            
        }

        public IActionResult Verify()
        {
            var customerid = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            if (CommonService.isVerifiedController(customerid) == 0)
            {
                return View();
            }
            return RedirectToAction("Index", "HouseRent");
            
        }

        [HttpPost]
        public async Task<IActionResult> Verify(DocumentModel document)
        {
            var UserId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            DocumentResponse response = await UserService.UploadDocument(document.Aadhar, UserId, WebHostEnvironment.WebRootPath);
            if (response.IsSuccess)
            {
                // 1 for progress
                CommonService.UpdateVerificationState(UserId, 1);
                return RedirectToAction("LikedProperties", "Customer");
            }
            return View();
        }

        public IActionResult ListOfCustomerInqueries(int propertyid, int ownerid)
        {

            var customerid = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            List<QueryViewModel> ListModel = UserService.ListOfInqueriesByProperty(propertyid, customerid, ownerid);
            CustomerModel customer = OwnerService.GetUserDetails(ownerid);

            Model.PropertyModel chat = new();
            chat.Customer = customer;
            chat.SenderId = ListModel[0].SenderId;
            chat.ReceiverId = ListModel[0].ReceiverId;
            chat.PropertyId = ListModel[0].PropertyId;
            chat.QueryList = ListModel;

            return View(chat);
        }

        [HttpPost]
        public IActionResult SendResponse(ChatModel model)
        {

            CommonService.InsertQuery(model);

            return RedirectToAction("ListOfCustomerInqueries", "Customer", new { propertyid = model.PropertyId, ownerid = model.ReceiverId });
        }


        public IActionResult FlagUserPartial(string url,int CustomerId) 
        {
            FormModel formModel= new FormModel();
            formModel.CustomerId = CustomerId;
            formModel.Url = url;
            formModel.FlaggedBy = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);

            return PartialView("Components/_FlaggedUserPopUp",formModel);
        }



        public IActionResult FlagUser(string url,FormModel model)
        {
            bool result=CommonService.FlagUser(model);
            return Redirect(url);
        }

        public IActionResult UnFlagUser(string url, int CustomerId)
        {
            var FlaggedBy = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
            bool result = CommonService.UnFlagUser(CustomerId, FlaggedBy);
            return Redirect(url);
        }

    }
}
