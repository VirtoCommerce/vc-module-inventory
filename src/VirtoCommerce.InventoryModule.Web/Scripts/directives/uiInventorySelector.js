angular.module('virtoCommerce.inventoryModule')
    .directive('uiInventorySelector', ['virtoCommerce.inventoryModule.fulfillments',
        function (inventory) {
            const defaultPageSize = 20;
            return {
                restrict: 'E',
                require: 'ngModel',
                replace: true,
                scope: {
                    disabled: '=?',
                    multiple: '=?',
                    pageSize: '=?',
                    placeholder: '=?',
                    required: '=?'
                },
                templateUrl: 'Modules/$(VirtoCommerce.Inventory)/Scripts/directives/uiInventorySelector.tpl.html',
                link: function ($scope, element, attrs, ngModelController) {
                    $scope.context = {
                        modelValue: null,
                        required: angular.isDefined(attrs.required) && (attrs.required === '' || attrs.required.toLowerCase() === 'true'),
                        multiple: angular.isDefined(attrs.multiple) && (attrs.multiple === '' || attrs.multiple.toLowerCase() === 'true')
                    };

                    var pageSize = $scope.pageSize || defaultPageSize;

                    $scope.choices = [];
                    $scope.isNoChoices = true;
                    var lastSearchPhrase = '';
                    var totalCount = 0;

                    $scope.fetch = function ($select) {
                        load();

                        if (!$scope.disabled) {
                            $scope.fetchNext($select);
                        }
                    };

                    function load() {
                        var fulfillmentIds = $scope.context.multiple ? $scope.context.modelValue : [$scope.context.modelValue];

                        if ($scope.isNoChoices && _.any(fulfillmentIds)) {
                            inventory.search({
                                objectIds: fulfillmentIds,
                                take: fulfillmentIds.length
                            }, (data) => {
                                join(data.results);
                            });
                        }
                    }

                    $scope.fetchNext = ($select) => {
                        $select.page = $select.page || 0;

                        if (lastSearchPhrase !== $select.search && totalCount > $scope.choices.length) {
                            lastSearchPhrase = $select.search;
                            $select.page = 0;
                        }

                        inventory.search(
                            {
                                searchPhrase: $select.search,
                                take: pageSize,
                                skip: $select.page * pageSize
                            }, (data) => {
                                join(data.results);
                                $select.page++;

                                if ($select.page * pageSize < data.totalCount) {
                                    $scope.$broadcast('scrollCompleted');
                                }

                                totalCount = data.totalCount;
                            })
                    }

                    function join(newItems) {
                        newItems = _.reject(newItems, x => _.any($scope.choices, y => y.id === x.id));

                        if (_.any(newItems)) {
                            $scope.choices = $scope.choices.concat(newItems);
                            $scope.isNoChoices = $scope.choices.length === 0;
                        }
                    }

                    $scope.$watch('context.modelValue', function (newValue, oldValue) {
                        if (newValue !== oldValue) {
                            ngModelController.$setViewValue($scope.context.modelValue);
                        }
                    });

                    ngModelController.$render = function () {
                        $scope.context.modelValue = ngModelController.$modelValue;
                    };
                }
            }
        }]);
