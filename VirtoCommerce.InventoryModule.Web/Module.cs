using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.InventoryModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.InventoryModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly string _connectionString = ConfigurationHelper.GetConnectionStringValue("VirtoCommerce.Inventory") 
                                                        ?? ConfigurationHelper.GetConnectionStringValue("VirtoCommerce");
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var context = new InventoryRepositoryImpl(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<InventoryRepositoryImpl, VirtoCommerce.InventoryModule.Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IInventoryRepository>(new InjectionFactory(c => new InventoryRepositoryImpl(_connectionString, new EntityPrimaryKeyGeneratorInterceptor(), 
                                                          _container.Resolve<AuditableInterceptor>(), new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { nameof(InventoryEntity) }))));

            _container.RegisterType<IInventoryService, InventoryServiceImpl>();
            _container.RegisterType<IInventorySearchService, InventorySearchService>();
            _container.RegisterType<IFulfillmentCenterSearchService, FulfillmentCenterSearchService>();
            _container.RegisterType<IFulfillmentCenterService, FulfillmentCenterService>();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            //Register product availability indexation 
            #region Search

            var productIndexingConfigurations = _container.Resolve<IndexDocumentConfiguration[]>();
            if (productIndexingConfigurations != null)
            {
                var productAvaibilitySource = new IndexDocumentSource
                {
                    ChangesProvider = _container.Resolve<ProductAvailabilityChangesProvider>(),
                    DocumentBuilder = _container.Resolve<ProductAvailabilityDocumentBuilder>(),
                };

                foreach (var configuration in productIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }
                    configuration.RelatedSources.Add(productAvaibilitySource);
                }
            }

            #endregion
        }

        #endregion

        #region ISupportExportImportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<InventoryExportImport>();
            job.DoExport(outStream, progressCallback);
        }

        public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<InventoryExportImport>();
            job.DoImport(inputStream, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Inventory.ExportImport.Description", String.Empty);
            }
        }
        #endregion


    }
}
