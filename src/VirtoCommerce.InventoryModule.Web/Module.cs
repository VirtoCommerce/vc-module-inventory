using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.InventoryModule.Core;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.ExportImport;
using VirtoCommerce.InventoryModule.Data.Handlers;
using VirtoCommerce.InventoryModule.Data.MySql;
using VirtoCommerce.InventoryModule.Data.PostgreSql;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.InventoryModule.Data.SqlServer;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        private IApplicationBuilder _appBuilder;

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<InventoryDbContext>((provider, options) =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<IInventoryRepository, InventoryRepositoryImpl>();
            serviceCollection.AddTransient<Func<IInventoryRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IInventoryRepository>());
            serviceCollection.AddTransient<IInventoryService, InventoryServiceImpl>();
            serviceCollection.AddTransient<IInventorySearchService, InventorySearchService>();
            serviceCollection.AddTransient<IFulfillmentCenterSearchService, FulfillmentCenterSearchService>();
            serviceCollection.AddTransient(x => (ISearchService<FulfillmentCenterSearchCriteria, FulfillmentCenterSearchResult, FulfillmentCenter>)x.GetRequiredService<IFulfillmentCenterSearchService>());
            serviceCollection.AddTransient<IFulfillmentCenterService, FulfillmentCenterService>();
            serviceCollection.AddTransient(x => (ICrudService<FulfillmentCenter>)x.GetRequiredService<IFulfillmentCenterService>());
            serviceCollection.AddTransient<IProductInventorySearchService, ProductInventorySearchService>();
            serviceCollection.AddTransient<IFulfillmentCenterGeoService, FulfillmentCenterGeoService>();
            serviceCollection.AddTransient<InventoryExportImport>();
            serviceCollection.AddTransient<ProductAvailabilityChangesProvider>();
            serviceCollection.AddTransient<ProductAvailabilityDocumentBuilder>();
            serviceCollection.AddTransient<LogChangesChangedEventHandler>();
            serviceCollection.AddTransient<IndexInventoryChangedEventHandler>();
            serviceCollection.AddTransient<FulfillmentCenterChangedEventHandler>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x => new Permission() { GroupName = "Inventory", Name = x }).ToArray());

            //Force migrations
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var inventoryDbContext = serviceScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    inventoryDbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                inventoryDbContext.Database.Migrate();
            }

            //Register product availability indexation 
            #region Search

            var productIndexingConfigurations = appBuilder.ApplicationServices.GetServices<IndexDocumentConfiguration>();
            if (productIndexingConfigurations != null)
            {
                var productAvailabilitySource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetService<ProductAvailabilityChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetService<ProductAvailabilityDocumentBuilder>(),
                };

                foreach (var configuration in productIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }
                    configuration.RelatedSources.Add(productAvailabilitySource);
                }
            }

            #endregion

            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<InventoryChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<LogChangesChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<InventoryChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<IndexInventoryChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<FulfillmentCenterChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<FulfillmentCenterChangedEventHandler>().Handle(message));
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<InventoryExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<InventoryExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
