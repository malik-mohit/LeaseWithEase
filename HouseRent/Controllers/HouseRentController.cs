using HouseRent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Model;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using ServiceLayer.Interface;
using ServiceLayer.Services;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;

namespace HouseRent.Controllers
{
    public class HouseRentController : Controller
    {
        private readonly ILogger<HouseRentController> _logger;
        private readonly IAdminService AdminService;
        private readonly ICommonService CommonService;

        public HouseRentController(ILogger<HouseRentController> _logger ,IAdminService AdminService, ICommonService CommonService)
        {

           this._logger = _logger;
            this.AdminService = AdminService;
            this.CommonService = CommonService;
        }


        public IActionResult Index()
        {
            List<StateModel> stateList = CommonService.GetStateList();
            ViewBag.State = stateList;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}