//Call this to register our module to main application
var moduleName = "virtoCommerce.inventoryModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.InventoryState', {
                    url: '/Inventory',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'fulfillmentCenterList',
                                controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
                                template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(
        ['$state', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', 'platformWebApp.authService', 'platformWebApp.metaFormsService',
            function ($state, mainMenuService, widgetService, authService, metaFormsService) {
                //Register module in main menu
                var menuItem = {
                    path: 'browse/Inventory',
                    icon: 'fas fa-cubes',
                    title: 'Inventory',
                    priority: 100,
                    action: function () { $state.go('workspace.InventoryState'); },
                    permission: 'inventory:access',
                };
                mainMenuService.addMenuItem(menuItem);

                //Register widgets in catalog item details
                widgetService.registerWidget({
                    isVisible: function (blade) { return blade.productType !== 'Digital' && authService.checkPermission('inventory:update'); },
                    controller: 'virtoCommerce.inventoryModule.inventoryWidgetController',
                    template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/widgets/inventoryWidget.tpl.html'
                }, 'itemDetail');

                widgetService.registerWidget({
                    size: [2, 1],
                    controller: 'virtoCommerce.inventoryModule.fulfillmentAddressesWidgetController',
                    template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/widgets/fulfillmentAddressesWidget.tpl.html'
                }, 'fulfillmentCenterDetail');

                widgetService.registerWidget({
                    controller: 'platformWebApp.dynamicPropertyWidgetController',
                    template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html',
                    isVisible: function (blade) { return !blade.isNew && authService.checkPermission('platform:dynamic_properties:read'); }
                }, 'fulfillmentCenterDetail');

                widgetService.registerWidget({
                    controller: 'virtoCommerce.inventoryModule.storeFulfillmentWidgetController',
                    template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/widgets/storeFulfillmentWidget.tpl.html'
                }, 'storeDetail');

                metaFormsService.registerMetaFields('inventoryDetails', []);
                metaFormsService.registerMetaFields('fulfillmentCenterDetails', []);
            }]);
