angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.inventoryFulfillmentcentersListController', ['$scope', '$q', '$timeout', 'platformWebApp.bladeUtils', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'virtoCommerce.inventoryModule.fulfillments',
        function($scope, $q, $timeout, bladeUtils, uiGridConstants, uiGridHelper, bladeNavigationService, fulfillments) {
            $scope.uiGridConstants = uiGridConstants;
            var blade = $scope.blade;

            var openFirstEntityDetailsOnce = _.once(function() {
                if (_.any(blade.currentEntities))
                    $timeout(function() {
                        openBlade(blade.currentEntities[0]);
                    }, 0, false);
            });

            blade.refresh = function() {
                blade.isLoading = true;
                var deferred = $q.defer();
                blade.parentWidgetRefresh().$promise.then(function(productInventories) {
                    fulfillments.search({ take: 10000, searchPhrase: filter.keyword }, function(data) {
                        _.forEach(data.results, function(item) {
                            var productInventory = _.find(productInventories, function(x) { return item.id === x.fulfillmentCenterId });
                            if (!!productInventory) {
                                item = Object.assign(item, productInventory);
                                console.log(item);
                            } else {
                                item = Object.assign(item, {
                                    fulfillmentCenterId: item.id,
                                    productId: blade.itemId,
                                    inStockQuantity: 0,
                                    reservedQuantity: 0,
                                    reorderMinQuantity: 0,
                                    preorderQuantity: 0,
                                    backorderQuantity: 0,
                                    allowBackorder: false,
                                    allowPreorder: false,
                                    inTransit: 0,
                                    status: "Disabled"
                                });
                            }
                        });
                        blade.currentEntities = data.results;
                        console.log(blade.currentEntities);
                        $scope.pageSettings.totalItems = blade.currentEntities.length;
                        blade.isLoading = false;
                        openFirstEntityDetailsOnce();
                        deferred.resolve(blade.currentEntities);
                    }, function(error) {
                        deferred.reject(error);
                        bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                    });
                });
                return deferred.promise;
            };

            openBlade = function openBlade(data) {
                $scope.selectedNodeId = data.id;

                var newBlade = {
                    id: "inventoryDetailBlade",
                    itemId: blade.itemId,
                    data: data,
                    title: data.name,
                    subtitle: 'inventory.blades.inventory-detail.subtitle',
                    controller: 'virtoCommerce.inventoryModule.inventoryDetailController',
                    template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/inventory-detail.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.selectNode = function(node) {
                openBlade(node);
            };

            blade.headIcon = 'fa-cubes';
            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh", icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function() {
                        return true;
                    }
                },
                {
                    name: "core.blades.fulfillment-center-list.subtitle", icon: 'fa fa-wrench',
                    executeMethod: function() {
                        var newBlade = {
                            id: 'fulfillmentCenterList',
                            controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
                            template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade.parentBlade);
                    },
                    canExecuteMethod: function() { return true; },
                    permission: 'inventory:fulfillment:edit'
                }
            ];

            // simple and advanced filtering
            var filter = $scope.filter = {};

            filter.criteriaChanged = function() {
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

            // ui-grid
            $scope.setGridOptions = function(gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function(gridApi) {
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });
                bladeUtils.initializePagination($scope);
            };
        }]);
