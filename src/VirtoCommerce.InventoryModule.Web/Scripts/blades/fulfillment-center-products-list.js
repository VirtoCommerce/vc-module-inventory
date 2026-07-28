angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.fulfillmentCenterProductsListController',
        [
            '$scope',
            'platformWebApp.bladeUtils',
            'platformWebApp.uiGridHelper',
            'platformWebApp.bladeNavigationService',
            'uiGridConstants',
            'virtoCommerce.inventoryModule.inventories',
            'virtoCommerce.catalogModule.items',
            function ($scope, bladeUtils, uiGridHelper, bladeNavigationService, uiGridConstants, inventories, items) {
                $scope.uiGridConstants = uiGridConstants;
                var blade = $scope.blade;
                blade.headIcon = 'fas fa-boxes';

                blade.refresh = function () {
                    blade.isLoading = true;

                    inventories.search({
                        fulfillmentCenterIds: [blade.currentEntityId],
                        withNonZeroQuantityOnly: true,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    }, function (data) {
                        $scope.pageSettings.totalItems = data.totalCount;
                        fillProductInfo(data.results);
                    }, function (error) {
                        blade.isLoading = false;
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                };

                // Inventory records store the product ID only, so name and code are resolved from the catalog.
                // Records whose product no longer exists in the catalog are skipped, therefore a page may
                // contain fewer rows than the page size and the total count may include such orphans.
                function fillProductInfo(inventoryList) {
                    var productIds = _.uniq(_.pluck(inventoryList, 'productId'));

                    if (!_.any(productIds)) {
                        blade.currentEntities = [];
                        blade.isLoading = false;
                        return;
                    }

                    items.plenty({ respGroup: 'ItemInfo' }, productIds, function (products) {
                        var productsById = _.indexBy(products, 'id');

                        blade.currentEntities = _.filter(inventoryList, function (inventory) {
                            var product = productsById[inventory.productId];
                            if (product) {
                                inventory.productName = product.name;
                                inventory.productCode = product.code;
                            }
                            return product;
                        });

                        blade.isLoading = false;
                    }, function (error) {
                        blade.isLoading = false;
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.refresh", icon: 'fa fa-refresh',
                        executeMethod: blade.refresh,
                        canExecuteMethod: function () {
                            return true;
                        }
                    }
                ];

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });
                    bladeUtils.initializePagination($scope);
                };
            }]);
