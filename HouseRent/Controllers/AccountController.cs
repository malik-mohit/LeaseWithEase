using Azure;
using HouseRent.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json.Linq;
using ServiceLayer.Interface;
using ServiceLayer.Services;
using System.Reflection;
using System.Security.Claims;



namespace HouseRent.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IUserService UserService;
        private readonly ICommonService CommonService;
        private readonly IWebHostEnvironment WebHostEnvironment;

        public AccountController(IConfiguration Configuration, IUserService UserService, ICommonService CommonService, IWebHostEnvironment webHostEnvironment)
        {
            this.Configuration = Configuration;
            this.UserService = UserService;
            this.CommonService = CommonService;
            this.WebHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public List<CityModel> GetCityList(int StateId)
        {
            return CommonService.GetCityList(StateId);
        }

        // Register

        public IActionResult Register()
        {

            if (User.Identity.IsAuthenticated)
            {
                var userRoles = ((ClaimsIdentity)User.Identity).Claims
                                                               .Where(c => c.Type == ClaimTypes.Role)
                                                               .Select(c => c.Value).ToList();
                if (userRoles.Contains(Enums.UserRoles.Admin.GetEnumDescription()))
                {
                    return RedirectToAction("ApprovalRequests", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "HouseRent");
                }
            }
            List<StateModel> stateList = CommonService.GetStateList();
            ViewBag.State = stateList;


            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel customer)
        {
            List<StateModel> stateList = CommonService.GetStateList();
            ViewBag.State = stateList;

            if (ModelState.IsValid)
            {

                if (UserService.UserAlreadyExists(customer.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(customer);
                }

                if (!UserService.IsOtpVerified(customer.Email))
                {
                    ModelState.AddModelError("Email", "Please Verify Your Email.");
                    return View(customer);
                }

                var response = await UserService.AddCustomerDetails(customer);
                if (response != null)
                {
                    if (response.IsSuccess)
                    {
                        var successMessage = "You have successfully registered!";
                        HttpContext.Session.SetString("SuccessMessage", successMessage);
                        return RedirectToAction("Login", "Account");
                    }

                }

            }

            return View(customer);

        }


        //Login
        public IActionResult Login()
        {
            if (HttpContext.Session.TryGetValue("SuccessMessage", out byte[] messageBytes))
            {
                var successMessage = System.Text.Encoding.UTF8.GetString(messageBytes);
                ViewBag.SuccessMessage = successMessage;
                HttpContext.Session.Remove("SuccessMessage");
            }

            if (User.Identity.IsAuthenticated)
            {
                var userRoles = ((ClaimsIdentity)User.Identity).Claims
                                                               .Where(c => c.Type == ClaimTypes.Role)
                                                               .Select(c => c.Value).ToList();


                if (userRoles.Contains(Enums.UserRoles.Admin.GetEnumDescription()))
                {
                    return RedirectToAction("ApprovalRequests", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "HouseRent");
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                if (!UserService.UserAlreadyExists(loginModel.Email))
                {
                    ModelState.AddModelError("Email", "No Such User Exists");
                    return View(loginModel);
                }

                if (!UserService.GetUserPassword(loginModel.Email, loginModel.Password))
                {
                    ModelState.AddModelError("Password", "Incorrect Password! Please Try Again");
                    return View(loginModel);
                }

                    LoginResponse response = new LoginResponse();

                try
                {
                    response = await UserService.AuthenticateUser(loginModel);
                    if (response != null)
                    {
                        if (response.IsSuccess)
                        {
                            
                            var data = response.data;
                            if (data != null)
                            {
                                //here checking Password of User

                                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                                identity.AddClaim(new Claim("CustomerId", data.CustomerId.ToString()));
                                identity.AddClaim(new Claim("Email", data.Email));
                                identity.AddClaim(new Claim("Name", data.Name));
                                switch (data.RoleId)
                                {
                                    case 1:
                                        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                                        break;
                                    case 2:
                                        identity.AddClaim(new Claim(ClaimTypes.Role, "Customer"));
                                        break;
                                    case 3:
                                        identity.AddClaim(new Claim(ClaimTypes.Role, "Owner"));
                                        break;

                                }

                                var principal = new ClaimsPrincipal(identity);
                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    principal,
                                    new AuthenticationProperties
                                    {
                                        IsPersistent = false,
                                        ExpiresUtc = DateTime.UtcNow.AddDays(1),
                                    });

                                HttpContext.Session.SetString("UserId", Convert.ToString(data.CustomerId));
                                HttpContext.Session.SetString("UserType", Convert.ToString(data.RoleId));

                                if (!string.IsNullOrEmpty(loginModel.returnUrl) && Url.IsLocalUrl(loginModel.returnUrl))
                                    return Redirect(loginModel.returnUrl);
                                else
                                {

                                    if (data.RoleId == 1)
                                        return RedirectToAction("ApprovalRequests", "Admin");
                                    if (data.RoleId == 2 || data.RoleId == 3)
                                        return RedirectToAction("Index", "HouseRent");

                                }
                            }

                        }
                        else
                        {
                            //ViewBag.EmailVal = "User does not exist";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
           

            
            return View();
        }

        


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "HouseRent");
        }



        public IActionResult ForgetPassword(string Email)
        {
            if (UserService.UserAlreadyExists(Email))
            {
                UserService.StoreOTP(Email);
                UserService.SendOtpAsync(Email, UserService.GetOTP(Email), Configuration, "forget");
                return PartialView("Components/_VerifyOTP_PopUp", Email);
            }
            else
            {
                ViewBag.Emailnotexists = "We don't have any account by this email. Please check your Email";
                return PartialView("Components/_ForgetPasswordPopUp");
            }
        }

        public IActionResult VerifyOTP(string OTP, string Email)
        {
            var realOtp = UserService.GetOTP(Email);

            if (realOtp == OTP)
            {

                return PartialView("Components/_ResetPasswordPopUp", Email);
            }
            else
            {
                ViewBag.Message = "Otp not Correct";
                return PartialView("Components/_VerifyOTP_PopUp", Email); ;
            }

        }

        public bool Validate(string Email, string OTP)
        {
            var realOtp = UserService.GetRegistrationOTP(Email);
            if (realOtp == OTP)
            {
                UserService.VerifyOtpAction(Email);
                return true;
            }
            else
            {
                return false;
            }

        }
        public IActionResult ShowError(string Email, string OTP)
        {


            ViewBag.Message = "OTP is not Correct";
            return PartialView("Components/_VerifyEmailPopUp", Email);



        }


        public IActionResult Partial()
        {
            return PartialView("Components/_ForgetPasswordPopUp");
        }

        [HttpPost]
        public IActionResult UpdatePassword(string Email, string NewPassword)
        {

            bool IsUpdated = UserService.UpdatePassword(Email, NewPassword);
            return RedirectToAction("Login", "Account");
        }



        [HttpPost]
        public IActionResult VerifyEmail(string Email)
        {


            var OTP = UserService.GenerateOTP();
            UserService.SendOtpAsync(Email, OTP, Configuration,"Register");
            bool success = UserService.StoreRegistrationOtp(Email, OTP);
            return PartialView("Components/_VerifyEmailPopUp", Email);

        }

        [HttpPost]
        public IActionResult UploadImage(IFormFile file)
        {

            var CustomerId = Convert.ToInt32(((ClaimsIdentity)User.Identity).Claims.Where(x => x.Type == "CustomerId").FirstOrDefault()?.Value);

            Task<bool> IsUploaded = CommonService.UploadProfile(WebHostEnvironment.WebRootPath, file, CustomerId);



            return RedirectToAction("LikedProperties", "Owner");
        }


    }
}



