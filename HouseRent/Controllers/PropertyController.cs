using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Model;
using ServiceLayer.Interface;
using System.Data;
using System.Security.Claims;

namespace HouseRent.Controllers
{
    public class PropertyController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IOwnerService OwnerService;
        private readonly IAdminService AdminService;
        private readonly IUserService UserService;
        private readonly ICommonService CommonService;
        private readonly IWebHostEnvironment WebHostEnvironment;

        public PropertyController(IConfiguration Configuration, IOwnerService OwnerService, ICommonService CommonService, IWebHostEnvironment WebHostEnvironment, IAdminService AdminService, IUserService UserService)
        {
            this.Configuration = Configuration;
            this.OwnerService = OwnerService;
            this.CommonService = CommonService;
            this.WebHostEnvironment = WebHostEnvironment;
            this.AdminService = AdminService;
            this.UserService = UserService;

        }

        public IActionResult Detail(int id,string? msg)
        {
            var data = CommonService.GetPropertyDetails(id);
            ViewBag.Message = msg;
            return View(data);
        }

        public IActionResult List()
        {
            var Role = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (Role.Count() == 0)
            {
                return RedirectToAction("Login", "Account");

            }
            List<StateModel> stateList = CommonService.GetStateList();
            ViewBag.State = stateList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> List(Model.PropertyModel Property)
        {
            PropertyResponse response = new();
            List<StateModel> stateList = CommonService.GetStateList();
            ViewBag.State = stateList;
            if (ModelState.IsValid)
            {
                
                //Adding UserId
                Property.UserId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);


                //store Property Details into Database
                response = await OwnerService.UploadProperty(Property, WebHostEnvironment.WebRootPath);
                if (response.IsSuccess)
                {
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignOutAsync();
                    return RedirectToAction("Index", "HouseRent");

                }

                return View();
            }
            else
            {
                return View();
            }
        }
        

        public IActionResult Partial(int id, string url)
        {
            FormModel data = new();
            data.PropertyId = id;
            data.CustomerId = Int32.Parse(((ClaimsIdentity)User.Identity).Claims.Where(x => x.Type == "CustomerId").FirstOrDefault()?.Value);

            data.Url = url.Replace("amp;", "");

            return PartialView("Components/_FlagForm", data);
        }

        public IActionResult Search(int StateId, int CityId, string[] Type, int[] Rooms, string[] furnishing, int? minPrice, int? maxPrice, int? minArea, int? maxArea)
        {
            SearchModel data = new()
            {
                Property = new List<PropertyModel>(),
            };
            List<StateModel> stateList = CommonService.GetStateList();
            List<CityModel> cityList = CommonService.GetCityList(StateId);
            ViewBag.State = stateList;
            ViewBag.City = cityList;
            data.Property = UserService.GetSearchedProperty(StateId, CityId, Type, Rooms, furnishing, minPrice, maxPrice, minArea, maxArea);
            return View(data);

        }

        public IActionResult LikeProperty(int PropertyId, string url)
        {
            var CustomerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(x => x.Type == "CustomerId").FirstOrDefault()?.Value);



            if (UserService.HasLiked(CustomerId, PropertyId))
            {
                UserService.RemoveLike(CustomerId, PropertyId);
            }
            else
            {
                UserService.AddLike(CustomerId, PropertyId);
            }

            return Redirect(url);
        }


        [HttpPost]
        public IActionResult SubmitQuery(PropertyModel model, int Id, int ownerid)
        {
            if (model.Message!=null)
            {
                model.SenderId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value);
                model.PropertyId = Id;
                model.ReceiverId = ownerid;
                CommonService.SendQuerry(model);
            }
            else
            {
                return RedirectToAction("Detail", "Property", new { id = Id, msg = "Please enter a message" });
            }

            return RedirectToAction("Detail", "Property", new { id = Id });
        }

        [HttpPost]
        public IActionResult FlagProperty(string url, FormModel model)
        {
            FormResponse response = CommonService.SubmitFlag(model);
            return Redirect(url);
        }

        public IActionResult UnflagProperty(string url, int customerid, int propertyid)
        {
            UserService.RemoveFlag(customerid, propertyid);
            return Redirect(url);
        }




    }
}
