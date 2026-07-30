angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.fulfillmentCenterProductsListController',
        [
            '$scope',
            '$translate',
            'platformWebApp.bladeUtils',
            'platformWebApp.uiGridHelper',
            'platformWebApp.bladeNavigationService',
            'platformWebApp.authService',
            'uiGridConstants',
            'virtoCommerce.inventoryModule.inventories',
            function ($scope, $translate, bladeUtils, uiGridHelper, bladeNavigationService, authService, uiGridConstants, inventories) {
                $scope.uiGridConstants = uiGridConstants;
                $scope.noProductName = $translate.instant('inventory.blades.fulfillment-center-products-list.labels.no-product');

                var blade = $scope.blade;
                blade.headIcon = 'fas fa-boxes';
                blade.updatePermission = 'inventory:update';

                var filter = $scope.filter = { inStockOnly: true };

                filter.criteriaChanged = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    } else {
                        blade.refresh();
                    }
                };

                blade.refresh = function () {
                    blade.isLoading = true;

                    inventories.searchProducts(getSearchCriteria(), function (data) {
                        $scope.pageSettings.totalItems = data.totalCount;
                        blade.currentEntities = _.each(data.results, fillMissingProduct);
                        blade.isLoading = false;

                        if (blade.parentWidgetRefresh) {
                            blade.parentWidgetRefresh();
                        }
                    }, function (error) {
                        blade.isLoading = false;
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                };

                function getSearchCriteria() {
                    return {
                        fulfillmentCenterIds: [blade.currentEntityId],
                        withPositiveQuantityOnly: filter.inStockOnly,
                        // the grid sends no sort expression until the user clicks a column header
                        sort: uiGridHelper.getSortExpression($scope) || 'inStockQuantity:desc',
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    };
                }

                // Inventory records store the product ID only, the product itself is resolved by the API.
                // It is missing when the product has been deleted from the catalog.
                function fillMissingProduct(productInventory) {
                    productInventory.product = productInventory.product || { name: $scope.noProductName };
                }

                $scope.selectNode = function (productInventory) {
                    $scope.selectedNodeId = productInventory.productId;

                    var newBlade = {
                        id: 'fulfillmentCenterInventoryDetail',
                        itemId: productInventory.productId,
                        data: productInventory.inventory,
                        getEntity: function () {
                            return getInventory(productInventory);
                        },
                        title: productInventory.product.name,
                        subtitle: 'inventory.blades.inventory-detail.subtitle',
                        controller: 'virtoCommerce.inventoryModule.inventoryDetailController',
                        template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/inventory-detail.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                function getInventory(productInventory) {
                    blade.refresh();

                    return inventories.searchProducts({
                        fulfillmentCenterIds: [blade.currentEntityId],
                        productIds: [productInventory.productId],
                        take: 1
                    }).$promise.then(function (data) {
                        var actual = _.first(data.results);
                        return actual ? actual.inventory : productInventory.inventory;
                    });
                }

                function openAddProductsBlade() {
                    $scope.selectedNodeId = null;
                    var selectedProducts = [];

                    var newBlade = {
                        id: 'CatalogItemsSelect',
                        title: 'inventory.blades.fulfillment-center-products-list.select-products-title',
                        controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                        breadcrumbs: [],
                        toolbarCommands: [
                            {
                                name: "platform.commands.add", icon: 'fas fa-plus',
                                executeMethod: function (selectBlade) {
                                    addProducts(selectedProducts, selectBlade);
                                },
                                canExecuteMethod: function () {
                                    return selectedProducts.length > 0;
                                }
                            }
                        ]
                    };

                    newBlade.options = {
                        checkItemFn: function (listItem, isSelected) {
                            if (listItem.type === 'category') {
                                newBlade.error = 'Categories are not supported';
                                listItem.selected = undefined;
                            } else {
                                if (isSelected) {
                                    if (_.all(selectedProducts, function (x) { return x.id !== listItem.id; })) {
                                        selectedProducts.push(listItem);
                                    }
                                } else {
                                    selectedProducts = _.reject(selectedProducts, function (x) { return x.id === listItem.id; });
                                }
                                newBlade.error = undefined;
                            }
                        }
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }

                function addProducts(products, selectBlade) {
                    selectBlade.isLoading = true;
                    var productIds = _.pluck(products, 'id');

                    inventories.searchProducts({
                        fulfillmentCenterIds: [blade.currentEntityId],
                        productIds: productIds,
                        take: productIds.length
                    }, function (data) {
                        var newInventories = _.chain(productIds)
                            .difference(_.pluck(data.results, 'productId'))
                            .map(function (productId) {
                                return {
                                    productId: productId,
                                    fulfillmentCenterId: blade.currentEntityId,
                                    inStockQuantity: 0
                                };
                            })
                            .value();

                        if (!newInventories.length) {
                            bladeNavigationService.closeBlade(selectBlade);
                            showAddedProducts();
                            return;
                        }

                        inventories.upsert(newInventories, function () {
                            bladeNavigationService.closeBlade(selectBlade);
                            showAddedProducts();
                        }, function (error) {
                            selectBlade.isLoading = false;
                            bladeNavigationService.setError('Error ' + error.status, selectBlade);
                        });
                    }, function (error) {
                        selectBlade.isLoading = false;
                        bladeNavigationService.setError('Error ' + error.status, selectBlade);
                    });
                }

                // The added products have no stock yet and the products that already existed may have none either,
                // so the default filter would hide them all and the add action would look like it did nothing.
                function showAddedProducts() {
                    filter.inStockOnly = false;
                    filter.criteriaChanged();
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.refresh", icon: 'fa fa-refresh',
                        executeMethod: blade.refresh,
                        canExecuteMethod: function () {
                            return true;
                        }
                    },
                    {
                        name: "platform.commands.add", icon: 'fas fa-plus',
                        executeMethod: openAddProductsBlade,
                        canExecuteMethod: function () {
                            // products are selected from the catalog
                            return authService.checkPermission('catalog:read');
                        },
                        permission: blade.updatePermission
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
