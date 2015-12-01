using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.DAL
{
    public class AntennaDbContext : IdentityDbContext<AntennaUser>
    {
        public AntennaDbContext() : 
            base("AntennaDbContext")
        {
            UserManager = new UserManager<AntennaUser>(new UserStore<AntennaUser>(this));
        }

        public UserManager<AntennaUser> UserManager { get; set; }

        public DbSet<AntennaRole> AntennaRole { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<SerialNumber> SerialNumber { get; set; }
        public DbSet<TestItem> TestItem { get; set; }
        public DbSet<TestStandard> TestStandard { get; set; }
        public DbSet<TestResult> TestResult { get; set; }
        public DbSet<TestResultItem> TestResultItem { get; set; }
        public DbSet<TestResultItemDegree> TestResultItemDegree { get; set; }
        public DbSet<TestResultItemDegreeVal> TestResultItemDegreeVal { get; set; }

        public DbSet<TestResultPim> TestResultPim { get; set; }
        public DbSet<TestEquipment> TestEquipment { get; set; }
        public DbSet<ImOrder> ImOrder { get; set; }
        public DbSet<Carrier> Carrier { get; set; }
        public DbSet<TestResultPimPoint> TestResultPimPoint { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //rename AspNetUsers table to AntennaUser
            modelBuilder.Entity<IdentityUser>().ToTable("AntennaUser");
            modelBuilder.Entity<AntennaUser>().ToTable("AntennaUser");

            modelBuilder.Entity<AntennaRole>().HasMany(a => a.Permissions).WithMany(b => b.AntennaRoles).Map(c => c.MapLeftKey("AntennaRoleId").MapRightKey("PermissionId").ToTable("AntennaRolePermission"));
            modelBuilder.Entity<TestStandard>().Property(a => a.StandardValue).HasPrecision(8,2);
            modelBuilder.Entity<TestResultItemDegree>().Property(a => a.Degree).HasPrecision(6,2);
            modelBuilder.Entity<TestResultItemDegreeVal>().Property(a => a.TestData).HasPrecision(8,2);
            modelBuilder.Entity<TestResult>().Property(a => a.TestTime).HasColumnType("datetime2").HasPrecision(0);
            modelBuilder.Entity<TestResultItem>().Property(a => a.TestTimeItem).HasColumnType("datetime2").HasPrecision(0);

            modelBuilder.Entity<TestResultPim>().Property(a => a.TestTime).HasColumnType("datetime2").HasPrecision(0);
            modelBuilder.Entity<TestResultPim>().Property(a => a.LimitLine).HasPrecision(8, 2);
            modelBuilder.Entity<Carrier>().Property(a => a.SetFreq).HasPrecision(8, 2);
            modelBuilder.Entity<Carrier>().Property(a => a.StartFreq).HasPrecision(8, 2);
            modelBuilder.Entity<Carrier>().Property(a => a.StopFreq).HasPrecision(8, 2);
            modelBuilder.Entity<Carrier>().Property(a => a.Power).HasPrecision(8, 2);
            modelBuilder.Entity<TestResultPimPoint>().Property(a => a.CarrierOneFreq).HasPrecision(8, 2);
            modelBuilder.Entity<TestResultPimPoint>().Property(a => a.CarrierTwoFreq).HasPrecision(8, 2);
            modelBuilder.Entity<TestResultPimPoint>().Property(a => a.ImFreq).HasPrecision(8, 2);
            modelBuilder.Entity<TestResultPimPoint>().Property(a => a.ImPower).HasPrecision(8, 2);
        }
    }
}