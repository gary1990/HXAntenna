using HxAntenna.Models.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.DAL
{
    public class UnitOfWork : IDisposable
    {
        public AntennaDbContext context = new AntennaDbContext();
        private GenericRepository<AntennaUser> antennaUserRepository;
        private GenericRepository<AntennaRole> antennaRoleRepository;
        private GenericRepository<Permission> permissionRepository;
        private GenericRepository<TestItem> testItemRepository;
        private GenericRepository<TestStandard> testStandardRepository;
        private GenericRepository<SerialNumber> serialNumberRepository;
        private GenericRepository<TestResult> testResultRepository;
        private GenericRepository<TestResultItem> testResultItemRepository;
        private GenericRepository<TestResultItemDegree> testResultItemDegreeRepository;
        private GenericRepository<TestResultItemDegreeVal> testResultItemDegreeValRepository;

        private GenericRepository<TestResultPim> testResultPimRepository;
        private GenericRepository<TestEquipment> testEquipmentRepository;
        private GenericRepository<ImOrder> imOrderRepository;
        private GenericRepository<Carrier> carrierRepository;
        private GenericRepository<TestResultPimPoint> testResultPimPointRepository;
        private GenericRepository<TestImage> testImageRepository;

        public GenericRepository<AntennaUser> AntennaUserRepository 
        {
            get 
            {
                if(this.antennaUserRepository == null)
                {
                    this.antennaUserRepository = new GenericRepository<AntennaUser>(context);
                }
                return antennaUserRepository;
            }
        }

        public GenericRepository<AntennaRole> AntennaRoleRepository
        {
            get
            {
                if (this.antennaRoleRepository == null)
                {
                    this.antennaRoleRepository = new GenericRepository<AntennaRole>(context);
                }
                return antennaRoleRepository;
            }
        }

        public GenericRepository<Permission> PermissionRepository
        {
            get
            {
                if (this.permissionRepository == null)
                {
                    this.permissionRepository = new GenericRepository<Permission>(context);
                }
                return permissionRepository;
            }
        }

        public GenericRepository<TestItem> TestItemRepository
        {
            get
            {
                if (this.testItemRepository == null)
                {
                    this.testItemRepository = new GenericRepository<TestItem>(context);
                }
                return testItemRepository;
            }
        }

        public GenericRepository<TestStandard> TestStandardRepository
        {
            get
            {
                if (this.testStandardRepository == null)
                {
                    this.testStandardRepository = new GenericRepository<TestStandard>(context);
                }
                return testStandardRepository;
            }
        }

        public GenericRepository<SerialNumber> SerialNumberRepository
        {
            get
            {
                if (this.serialNumberRepository == null)
                {
                    this.serialNumberRepository = new GenericRepository<SerialNumber>(context);
                }
                return serialNumberRepository;
            }
        }

        public GenericRepository<TestResult> TestResultRepository
        {
            get
            {
                if (this.testResultRepository == null)
                {
                    this.testResultRepository = new GenericRepository<TestResult>(context);
                }
                return testResultRepository;
            }
        }

        public GenericRepository<TestResultItem> TestResultItemRepository
        {
            get
            {
                if (this.testResultItemRepository == null)
                {
                    this.testResultItemRepository = new GenericRepository<TestResultItem>(context);
                }
                return testResultItemRepository;
            }
        }

        public GenericRepository<TestResultItemDegree> TestResultItemDegreeRepository
        {
            get
            {
                if (this.testResultItemDegreeRepository == null)
                {
                    this.testResultItemDegreeRepository = new GenericRepository<TestResultItemDegree>(context);
                }
                return testResultItemDegreeRepository;
            }
        }

        public GenericRepository<TestResultItemDegreeVal> TestResultItemDegreeValRepository
        {
            get
            {
                if (this.testResultItemDegreeValRepository == null)
                {
                    this.testResultItemDegreeValRepository = new GenericRepository<TestResultItemDegreeVal>(context);
                }
                return testResultItemDegreeValRepository;
            }
        }

        public GenericRepository<TestResultPim> TestResultPimRepository
        {
            get
            {
                if (this.testResultPimRepository == null)
                {
                    this.testResultPimRepository = new GenericRepository<TestResultPim>(context);
                }
                return testResultPimRepository;
            }
        }

        public GenericRepository<TestEquipment> TestEquipmentRepository
        {
            get
            {
                if (this.testEquipmentRepository == null)
                {
                    this.testEquipmentRepository = new GenericRepository<TestEquipment>(context);
                }
                return testEquipmentRepository;
            }
        }

        public GenericRepository<ImOrder> ImOrderRepository
        {
            get
            {
                if (this.imOrderRepository == null)
                {
                    this.imOrderRepository = new GenericRepository<ImOrder>(context);
                }
                return imOrderRepository;
            }
        }

        public GenericRepository<Carrier> CarrierRepository
        {
            get
            {
                if (this.carrierRepository == null)
                {
                    this.carrierRepository = new GenericRepository<Carrier>(context);
                }
                return carrierRepository;
            }
        }

        public GenericRepository<TestResultPimPoint> TestResultPimPointRepository
        {
            get
            {
                if (this.testResultPimPointRepository == null)
                {
                    this.testResultPimPointRepository = new GenericRepository<TestResultPimPoint>(context);
                }
                return testResultPimPointRepository;
            }
        }

        public GenericRepository<TestImage> TestImageRepository
        {
            get
            {
                if (this.testImageRepository == null)
                {
                    this.testImageRepository = new GenericRepository<TestImage>(context);
                }
                return testImageRepository;
            }
        }

        public void AntennaSaveChanges() 
        {
            foreach(var deleteEntity in context.ChangeTracker.Entries<BaseModel>())
            {
                if(deleteEntity.State == EntityState.Deleted)
                {
                    deleteEntity.State = EntityState.Unchanged;
                    deleteEntity.Entity.IsDelete = true;
                }
            }
            context.SaveChanges();
        }

        public void DbSaveChanges() 
        {
            context.SaveChanges();
        }

        public void AntennaUserSave()
        {
            foreach (var deletedEntity in context.ChangeTracker.Entries<AntennaUser>())
            {
                if (deletedEntity.State == EntityState.Deleted)
                {
                    deletedEntity.State = EntityState.Unchanged;
                    deletedEntity.Entity.IsDeleted = true;
                }
            }
            context.SaveChanges();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing){
            if(!this.disposed){
                if(disposing){
                    context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}