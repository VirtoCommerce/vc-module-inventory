angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.fulfillmentCenterProductsWidgetController',
        [
            '$scope',
            'platformWebApp.bladeNavigationService',
            'virtoCommerce.inventoryModule.inventories',
            function ($scope, bladeNavigationService, inventories) {
                var blade = $scope.widget.blade;

                function refresh() {
                    inventories.searchProducts({
                        fulfillmentCenterIds: [blade.currentEntityId],
                        withPositiveQuantityOnly: true,
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
                        parentWidgetRefresh: refresh,
                        controller: 'virtoCommerce.inventoryModule.fulfillmentCenterProductsListController',
                        template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-products-list.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh();
            }]);
