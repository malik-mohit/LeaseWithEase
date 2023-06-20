using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using ServiceLayer.Interface;
using System.Security.Cryptography.X509Certificates;

namespace HouseRent.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICommonService CommonService;
        private readonly IAdminService AdminService;

        public AdminController(ICommonService CommonService, IAdminService AdminService)
        {

            this.CommonService = CommonService;
            this.AdminService = AdminService;
        }
        public IActionResult ApprovalRequests()
        {
            var data = AdminService.GetApprovedListData();
            return View(data);
        }

        public IActionResult ListedProperties()
        {

            var data = AdminService.GetListedPropertyList();
            return View(data);
        }

        public IActionResult BlockedProperties()
        {
            var data = AdminService.GetBlockedPropertyList();
            return View(data);
        }
        public IActionResult VerificationRequests()
        {
            List<CustomerModel> verificationProgressList = CommonService.GetUnverifiedUserList();
            return View(verificationProgressList);
        }
        public IActionResult FlaggedUsers()
        {
            List<FlagUserModel> flagUserList = CommonService.GetFlaggedUserList();
            return View(flagUserList);
        }
        public IActionResult BlockedUsers()
        {
            List<CustomerModel> verificationProgressList = CommonService.GetBlockedUserList();
            return View(verificationProgressList);
           
        }
        public IActionResult VerifyUser(int userid)
        {
            CommonService.UpdateVerificationState(userid, 2);
            return RedirectToAction("VerificationRequests", "Admin");
        }

        public IActionResult ApproveProperty(int propertyid)
        {
            bool result = AdminService.ApproveOwnerProperty(propertyid);
            return RedirectToAction("ApprovalRequests", "Admin");
        }

        public IActionResult BlockProperty(FormModel data)
        {
           FormResponse response = AdminService.BlockProperty(data);

            return RedirectToAction("ApprovalRequests", "Admin");
        }
        public IActionResult UnBlockProperty(int PropertyId)
        {
           AdminService.UnBlockProperty(PropertyId);

            return RedirectToAction("ApprovalRequests", "Admin");
        }

        public IActionResult BlockForm(int id)
        {
            FormModel data = new FormModel()
            {
                PropertyId = id
            };
        
            return PartialView("Components/_BlockForm", data);
        }


        public IActionResult ViewDocument(int UserId)
        {
            string url = AdminService.GetDocument(UserId);


            return PartialView("Components/_ViewDocumentPopUp", url);
        }
        public IActionResult ViewPropertyPaper(int PropertyId)
        {
            string pdfurl = AdminService.GetPropertyPaper(PropertyId);
            return PartialView("Components/_ViewPropertyPaperPopUp", pdfurl);
        }

        public IActionResult BlockUser(int userid)
        {
            CommonService.UpdateVerificationState(userid, 3);
            return RedirectToAction("VerificationRequests", "Admin");
        }
        
        public IActionResult UnBlockUser(int userid)
        {
            CommonService.UpdateVerificationState(userid, 1);
            return RedirectToAction("BlockedUsers", "Admin");
        }  
        public IActionResult ViewFlaggedComment(int UserId)
        {
           List<string> FlaggedCommentList= AdminService.GetFlaggedCommentList(UserId);
            return PartialView("Components/_ViewFlaggedCommentList",FlaggedCommentList);
        }


       
    }
}
