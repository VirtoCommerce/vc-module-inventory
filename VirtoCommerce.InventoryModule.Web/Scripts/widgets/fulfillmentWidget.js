angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.fulfillmentWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.inventoryModule.fulfillments', function ($scope, bladeNavigationService, fulfillments) {
    var blade = $scope.widget.blade;

    $scope.widget.refresh = function () {
        $scope.currentNumberInfo = '...';
        fulfillments.search({
            searchPhrase: undefined,
            sort: null,
            skip: 0,
            take: 0
        }, function (response) {
            $scope.currentNumberInfo = response.totalCount;
        });
    }

    $scope.openBlade = function () {
        var newBlade = {
            id: 'fulfillmentCenterList',
            controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
            template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    $scope.widget.refresh();
}]);
