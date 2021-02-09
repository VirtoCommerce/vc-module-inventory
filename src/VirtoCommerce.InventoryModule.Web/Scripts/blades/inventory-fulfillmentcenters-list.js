angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.inventoryFulfillmentcentersListController', ['$scope', '$q', '$timeout', 'platformWebApp.bladeUtils', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'virtoCommerce.inventoryModule.fulfillments',
        function ($scope, $q, $timeout, bladeUtils, uiGridConstants, uiGridHelper, bladeNavigationService, fulfillments) {
            $scope.uiGridConstants = uiGridConstants;
            var blade = $scope.blade;

            var openFirstEntityDetailsOnce = _.once(function() {
                if (_.any($scope.items))
                    $timeout(function () {
                        $scope.selectedNodeId = $scope.items[0];
                        openBlade($scope.items[0]);
                    }, 0, false);
            });

            blade.refresh = function() {
                blade.isLoading = true;
                var deferred = $q.defer();

                if ($scope.pageSettings.currentPage !== 1)
                    $scope.pageSettings.currentPage = 1;

                var searchCriteria = getSearchCriteria();

                fulfillments.searchFulfillmentCenterInventories(searchCriteria,
                    function (data) {
                        _.each(data.results, fillProductIdIfEmpty);
                        $scope.items = data.results;
                        $scope.pageSettings.totalItems = data.totalCount;
                        $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                    }).$promise.finally(function () {
                        blade.isLoading = false;
                        openFirstEntityDetailsOnce();
                        deferred.resolve($scope.items);
                });
                //reset state grid
                resetStateGrid();

                return deferred.promise;
            };

            function showMore() {
                if ($scope.hasMore) {
                    ++$scope.pageSettings.currentPage;
                    $scope.gridApi.infiniteScroll.saveScrollPercentage();
                    blade.isLoading = true;
                    var searchCriteria = getSearchCriteria();

                    fulfillments.searchFulfillmentCenterInventories(searchCriteria,
                        function (data) {
                            _.each(data.results, fillProductIdIfEmpty);
                            $scope.items = $scope.items.concat(data.results);
                            $scope.pageSettings.totalItems = data.totalCount;
                            $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                            $scope.gridApi.infiniteScroll.dataLoaded();

                        }).$promise.finally(function () {
                        blade.isLoading = false;
                    });
                }
            }

            function fillProductIdIfEmpty(inventory) {
                if (!inventory.productId) {
                    inventory.productId = blade.itemId;
                }
            }

            var openBlade = function openBlade(data) {
                $scope.selectedNodeId = data.fulfillmentCenterId;

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

            blade.headIcon = 'fa fa-cubes';
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
            $scope.setGridOptions = function (gridOptions) {

                //disable watched
                bladeUtils.initializePagination($scope, true);
                //сhoose the optimal amount that ensures the appearance of the scroll
                $scope.pageSettings.itemsPerPageCount = 50;

                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    //update gridApi for current grid
                    $scope.gridApi = gridApi;

                    uiGridHelper.bindRefreshOnSortChanged($scope);
                    $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);
                });

                blade.refresh();
            };

            //reset state grid (header checkbox, scroll)
            function resetStateGrid() {
                if ($scope.gridApi) {
                    $scope.items = [];
                    if ($scope.gridApi.selection) {
                        $scope.gridApi.selection.clearSelectedRows();
                    }
                    $scope.gridApi.infiniteScroll.resetScroll(true, true);
                    $scope.gridApi.infiniteScroll.dataLoaded();
                }
            }

            // Search Criteria
            function getSearchCriteria() {
                var searchCriteria = {
                    searchPhrase: filter.keyword ? filter.keyword : undefined,
                    productId: blade.itemId ,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
                return searchCriteria;
            }


        }]);
