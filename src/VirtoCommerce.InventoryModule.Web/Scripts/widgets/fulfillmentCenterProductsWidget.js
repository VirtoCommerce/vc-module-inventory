angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.fulfillmentCenterProductsWidgetController',
        [
            '$scope',
            'platformWebApp.bladeNavigationService',
            'virtoCommerce.inventoryModule.inventories',
            function ($scope, bladeNavigationService, inventories) {
                var blade = $scope.widget.blade;

                function refresh() {
                    inventories.search({
                        fulfillmentCenterIds: [blade.currentEntityId],
                        withNonZeroQuantityOnly: true,
                        take: 0
                    }, function (data) {
                        $scope.productsCount = data.totalCount;
                    });
                }

                $scope.openBlade = function () {
                    var newBlade = {
                        id: 'fulfillmentCenterProductsList',
                        currentEntityId: blade.currentEntityId,
                        title: blade.title,
                        subtitle: 'inventory.widgets.fulfillmentCenterProductsWidget.blade-subtitle',
                        controller: 'virtoCommerce.inventoryModule.fulfillmentCenterProductsListController',
                        template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-products-list.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh();
            }]);
