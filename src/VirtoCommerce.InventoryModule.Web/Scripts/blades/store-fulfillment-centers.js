angular.module('virtoCommerce.inventoryModule')
    .controller('virtoCommerce.inventoryModule.storeFulfillmentController', ['$scope', 'platformWebApp.bladeNavigationService', '$timeout',
        function ($scope, bladeNavigationService, $timeout) {
            $scope.shown = true;

            $scope.saveChanges = function () {
                angular.copy($scope.blade.currentEntity, $scope.blade.origEntity);
                $scope.bladeClose();
            };

            $scope.setForm = function (form) {
                $scope.formScope = form;
            }

            $scope.isValid = function () {
                return $scope.formScope && $scope.formScope.$valid;
            }

            $scope.cancelChanges = function () {
                $scope.bladeClose();
            }

            $scope.blade.refresh = function () {
                $scope.shown = false;

                $timeout(callAtTimeout, 0);
            }

            function callAtTimeout() {
                $scope.shown = true;
            }

            $scope.openFulfillmentCentersList = function () {
                var newBlade = {
                    id: 'fulfillmentCenterList',
                    controller: 'virtoCommerce.inventoryModule.fulfillmentListController',
                    template: 'Modules/$(VirtoCommerce.Inventory)/Scripts/blades/fulfillment-center-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade);
            }

            $scope.blade.headIcon = 'fa fa-archive';

            $scope.blade.isLoading = false;
            $scope.blade.currentEntity = angular.copy($scope.blade.entity);
            $scope.blade.origEntity = $scope.blade.entity;
        }]);
