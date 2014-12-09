using HxAntenna.Lib;
using HxAntenna.Models;
using HxAntenna.Models.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HxAntenna.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        List<string> path = new List<string> { };
        private UnitOfWork unitOfWork = new UnitOfWork();
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "UserProfile";
        public string ViewPathBase = "UserProfile";
        public string ViewPath2 = "/";

        public UserProfileController() 
        {
            path.Add("系统管理");
            path.Add("用户管理");
            ViewBag.path = path;
            ViewBag.Name = "用户管理";
            ViewBag.Title = "用户管理";
        }
        public ActionResult Index(int page = 1, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "filter", filter } };
            return View();
        }
        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null) 
        {
            var results = Common<AntennaUser>.GetQuery(unitOfWork, filter);
            
            results = results.OrderBy(a => a.UserName);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<AntennaUser>.Page(this, rv, results));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateSave(RegisterViewModel model, string returnUrl = "Index") 
        {
            var userManager = new UserManager<AntennaUser>(new UserStore<AntennaUser>(unitOfWork.context));
            //允许用户名包含非字母、数字
            userManager.UserValidator = new UserValidator<AntennaUser>(userManager) { AllowOnlyAlphanumericUserNames = false };

            if(ModelState.IsValid)
            {
                try 
                {
                    var user = new AntennaUser();
                    user.UserName = model.UserName;
                    user.JobNumber = model.JobNumber;
                    user.AntennaRoleId = model.AntennaRoleId;
                    var userResult = userManager.Create(user, model.Password);
                    unitOfWork.DbSaveChanges();

                    CommonMsg.RMOk(this, "记录:" + model.UserName + "新建成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同工号的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.Message);
                    }
                }
                catch(Exception e)
                {
                    if (e.InnerException.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同工号的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.Message);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(string id = null, string returnUrl = "Index") 
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(id);
            if(result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditSave(AntennaUser model, string returnUrl = "Index")
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(model.Id);
            if(result == null)
            {
                return Redirect(Url.Content(returnUrl));
            }
            if(ModelState.IsValid)
            {
                try 
                {
                    result.Edit(model);
                    unitOfWork.DbSaveChanges();
                    CommonMsg.RMOk(this, "记录" + result.JobNumber + "保存成功！");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.Message);
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError(string.Empty, "编辑记录失败！" + e.Message);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(string id = null, string returnUrl = "Index") 
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(id);
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Delete.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteSave(string id, string returnUrl = "Index") 
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(id);
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            try 
            {
                unitOfWork.AntennaUserRepository.Delete(result);
                unitOfWork.AntennaUserSave();
                CommonMsg.RMOk(this, "记录：" + result.JobNumber + "删除成功！");
                return Redirect(Url.Content(returnUrl));
            }
            catch(Exception e)
            {
                CommonMsg.RMError(this, "记录：" + result.JobNumber + "删除失败！" + e.Message);
            }
            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Restore(string id = null, string returnUrl = "Index") 
        {
            var result = unitOfWork.AntennaUserRepository.Get(a => a.Id == id && a.IsDeleted == true).SingleOrDefault();
            if(result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Restore.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult RestoreSave(AntennaUser model, string returnUrl) 
        {
            var result = unitOfWork.AntennaUserRepository.Get(a => a.Id == model.Id && a.IsDeleted == true).SingleOrDefault();
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            try 
            {
                result.IsDeleted = false;
                unitOfWork.AntennaSaveChanges();
                CommonMsg.RMOk(this, "记录:" + result.JobNumber + "恢复成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch(Exception e)
            {
                CommonMsg.RMOk(this, "记录" + result.JobNumber + "恢复失败!" + e.Message);
            }
            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string id, string returnUrl = "Index")
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(id);
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ResetPasswordModel model = new ResetPasswordModel { 
                Id = result.Id,
                JobNumber = result.JobNumber,
                UserName = result.UserName
            };

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPasswordSave(ResetPasswordModel model, string returnUrl = "Index") 
        {
            var UserManager = new UserManager<AntennaUser>(new UserStore<AntennaUser>(unitOfWork.context));

            if (ModelState.IsValid)
            {
                try
                {
                    AntennaUser user = UserManager.FindById(model.Id);
                    string HashNewPassword = UserManager.PasswordHasher.HashPassword(model.NewPassword);
                    UserStore<AntennaUser> store = new UserStore<AntennaUser>(new AntennaDbContext());
                    store.SetPasswordHashAsync(user, HashNewPassword);
                    store.UpdateAsync(user);

                    unitOfWork.AntennaUserSave();

                    CommonMsg.RMOk(this, "记录:" + user.JobNumber + "重置密码成功！");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                }
            }
            return View(ViewPath1 + ViewPath + ViewPath2 + "ResetPassword.cshtml", model);
        }

        [ChildActionOnly]
        public virtual PartialViewResult Abstract(string id)
        {
            var result = unitOfWork.AntennaUserRepository.GetByID(id);
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Abstract.cshtml", result);
        }
	}
}