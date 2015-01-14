using HxAntenna.Models.Constant;
using HxAntenna.Models.DAL;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.Initializer
{
    public class AntennaInitializer : DropCreateDatabaseIfModelChanges<AntennaDbContext>
    {
        protected override void Seed(AntennaDbContext db)
        {
            UniqueIndexInitial.Create(db);

            var testItems = new List<TestItem>()
            { 
                new TestItem { Name = "驻波"},
                new TestItem { Name = "隔离"},
                new TestItem { Name = "交调"},
            };
            testItems.ForEach(a => db.TestItem.Add(a));
            db.SaveChanges();

            var roles = new List<AntennaRole>
            {
                new AntennaRole { Name = "系统管理员" },
                new AntennaRole { Name = "测试员"}
            };
            roles.ForEach(a => db.AntennaRole.Add(a));
            db.SaveChanges();

            var testStandards = new List<TestStandard> { 
                new TestStandard { Symbol = Symbol.LessOrEqual, StandardValue = 1.4M},
                new TestStandard { Symbol = Symbol.GreatOrEqual, StandardValue = 30},
                new TestStandard { Symbol = Symbol.LessOrEqual, StandardValue = 107}
            };
            testStandards.ForEach(a => db.TestStandard.Add(a));
            db.SaveChanges();

            var UserManager = db.UserManager;
            //允许用户名包含非字母或数字
            UserManager.UserValidator = new UserValidator<AntennaUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            var username = "系统管理员";
            var jobNumer = "001";
            var password = "123456";
            var userAdmin = new AntennaUser { JobNumber = jobNumer, UserName = username, AntennaRoleId = db.AntennaRole.Where(a => a.Name == "系统管理员").Single().Id };
            UserManager.Create(userAdmin, password);
            username = "NO001";
            jobNumer = "NO001";
            var userTester = new AntennaUser { JobNumber = jobNumer, UserName = username, AntennaRoleId = db.AntennaRole.Where(a => a.Name == "测试员").Single().Id };
            UserManager.Create(userTester, password);
            db.SaveChanges();

            base.Seed(db);
        }
    }

    public class UniqueIndexInitial 
    {
        public static void Create(AntennaDbContext context) 
        {
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_RoleName ON AntennaRole(Name)");
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_TestItemName ON TestItem(Name)");
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_SerialNumberName ON SerialNumber(Name)");
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_SymbolStandardValue ON TestStandard(Symbol, StandardValue)");
            context.Database.ExecuteSqlCommand("Create UNIQUE INDEX index_Jobnumber ON AntennaUser(JobNumber)");
        }
    }
}