using HxAntenna.Lib;
using HxAntenna.Models;
using HxAntenna.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HxAntenna.Controllers
{
    public class TestResultPimController : Controller
    {
        List<string> path = new List<string> { };
        private UnitOfWork unitOfWork = new UnitOfWork();
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "TestResultPim";
        public string ViewPathBase = "TestResultPim";
        public string ViewPath2 = "/";

        public TestResultPimController()
        {
            path.Add("质量管理");
            path.Add("PIM测试记录");
            ViewBag.path = path;
            ViewBag.Name = "PIM测试记录";
            ViewBag.Title = "PIM测试记录";
        }

        public ActionResult Index(int page = 1, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "filter", filter } };
            return View();
        }

        public ActionResult Get(string returnRoot, string actionAjax = "", int page = 1, string filter = null)
        {
            var results = Common<TestResultPim>.GetQuery(unitOfWork, filter)
                .Where(a => a.IsLatest == true);

            results = results.OrderByDescending(a => a.TestTime);
            
            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<TestResultPim>.Page(this, rv, results));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Details(int Id = 0, string returnUrl = "Index")
        {
            var result = unitOfWork.TestResultPimRepository.Get(a => a.Id == Id).SingleOrDefault();
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Details.cshtml", result);
        }
	}
}