angular.module('virtoCommerce.inventoryModule')
.controller('virtoCommerce.inventoryModule.inventoryFulfillmentcentersListController', ['$scope', '$timeout', 'platformWebApp.bladeNavigationService', function ($scope, $timeout, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.selectedItem = null;
    var openFirstEntityDetailsOnce = _.once(function () {
        if (_.any(blade.currentEntities))
            $timeout(function () {
                $scope.openBlade(blade.currentEntities[0]);
            }, 0, false);
    });

    blade.clearKeyword = function() {
        blade.searchKeyword = undefined;
        blade.refresh();
    };

    blade.refresh = function() {
        blade.isLoading = true;
        return blade.parentWidgetRefresh().$promise.then(function(results) {
            blade.isLoading = false;
            blade.currentEntities = results;

            var grouped = _.groupBy(blade.currentEntities,
                function(entity) {
                    return entity.inStockQuantity > 0 ? 'inStock' : 'empty';
                });
            blade.currentEntities = grouped.inStock.sort($scope.sortByName)
                .concat(grouped.empty.sort($scope.sortByName));

            if (blade.searchKeyword) {
                blade.currentEntities = _.filter(blade.currentEntities,
                    function(entity) {
                        return entity.fulfillmentCenter.name.toLowerCase().contains(blade.searchKeyword.toLowerCase());
                    });
            }

            blade.count = blade.currentEntities.length || 0;
            openFirstEntityDetailsOnce();
            return results;
        });
    };

    $scope.openBlade = function (data) {
        $scope.selectedItem = data;

        var newBlade = {
            id: "inventoryDetailBlade",
            itemId: blade.itemId,
            data: data,
            title: data.fulfillmentCenter.name,
            subtitle: 'inventory.blades.inventory-detail.subtitle',
            controller: 'virtoCommerce.inventoryModule.inventoryDetailController',
            template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/inventory-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    blade.headIcon = 'fa-cubes';
    blade.toolbarCommands = [
        {
            name: "platform.commands.refresh", icon: 'fa fa-refresh',
            executeMethod: blade.refresh,
            canExecuteMethod: function () {
                return true;
            }
        },
		{
		    name: "core.blades.fulfillment-center-list.subtitle", icon: 'fa fa-wrench',
		    executeMethod: function () {
		        var newBlade = {
		            id: 'fulfillmentCenterList',
		            controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
		            template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
		        };
		        bladeNavigationService.showBlade(newBlade, blade.parentBlade);
		    },
		    canExecuteMethod: function () { return true; },
		    permission: 'inventory:fulfillment:edit'
		}
    ];

    blade.refresh();

    $scope.sortByName = function (a, b) {
        if (a.fulfillmentCenter.name !== b.fulfillmentCenter.name) {
            return a.fulfillmentCenter.name > b.fulfillmentCenter.name ? 1 : -1;
        }
        return 0;
    };
}]);
