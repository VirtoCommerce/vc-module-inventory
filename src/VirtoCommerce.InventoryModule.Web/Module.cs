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
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.ExportImport;
using VirtoCommerce.InventoryModule.Data.Handlers;
using VirtoCommerce.InventoryModule.Data.MySql;
using VirtoCommerce.InventoryModule.Data.PostgreSql;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.InventoryModule.Data.SqlServer;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
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
            serviceCollection.AddDbContext<InventoryDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
                        break;
                }
            });

            serviceCollection.AddTransient<IInventoryRepository, InventoryRepositoryImpl>();
            serviceCollection.AddTransient<Func<IInventoryRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IInventoryRepository>());
            serviceCollection.AddTransient<IInventoryService, InventoryServiceImpl>();
            serviceCollection.AddTransient<IInventorySearchService, InventorySearchService>();
            serviceCollection.AddTransient<IFulfillmentCenterSearchService, FulfillmentCenterSearchService>();
            serviceCollection.AddTransient<IFulfillmentCenterService, FulfillmentCenterService>();
            serviceCollection.AddTransient<IProductInventorySearchService, ProductInventorySearchService>();
            serviceCollection.AddTransient<IFulfillmentCenterGeoService, FulfillmentCenterGeoService>();
            serviceCollection.AddTransient<IInventoryReservationService, InventoryReservationService>();
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

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Inventory", ModuleConstants.Security.Permissions.AllPermissions);

            // register dynamic properties
            var dynamicPropertyRegistrar = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertyRegistrar>();
            dynamicPropertyRegistrar.RegisterType<FulfillmentCenter>();

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
            var productIndexingConfigurations = appBuilder.ApplicationServices.GetServices<IndexDocumentConfiguration>()
                .Where(x => x.DocumentType == KnownDocumentTypes.Product)
                .ToList();

            if (productIndexingConfigurations.Any())
            {
                var productAvailabilitySource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetService<ProductAvailabilityChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetService<ProductAvailabilityDocumentBuilder>(),
                };

                foreach (var configuration in productIndexingConfigurations)
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();
                    configuration.RelatedSources.Add(productAvailabilitySource);
                }
            }

            appBuilder.RegisterEventHandler<InventoryChangedEvent, LogChangesChangedEventHandler>();
            appBuilder.RegisterEventHandler<InventoryChangedEvent, IndexInventoryChangedEventHandler>();
            appBuilder.RegisterEventHandler<FulfillmentCenterChangedEvent, FulfillmentCenterChangedEventHandler>();
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
