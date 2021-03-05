angular.module('virtoCommerce.inventoryModule')
    .directive('uiInventorySelector', ['$q', 'virtoCommerce.inventoryModule.fulfillments', 
        function ($q, inventory) {
            const defaultPageSize = 20;
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    disabled: '=?',
                    multiple: '=?',
                    pageSize: '=?',
                    placeholder: '=?',
                    required: '=?',
                    selectedId: '=?',
                    selectedIds: '=?',
                },
                templateUrl: 'Modules/$(VirtoCommerce.Inventory)/Scripts/directives/uiInventorySelector.tpl.html',
                link: function ($scope, element, attrs, ngModelController) {
                    $scope.context = {
                        required: angular.isDefined(attrs.required) && (attrs.required === '' || attrs.required.toLowerCase() === 'true'),
                        multiple: angular.isDefined(attrs.multiple) && (attrs.multiple === '' || attrs.multiple.toLowerCase() === 'true')
                    };

                    var pageSize = $scope.pageSize || defaultPageSize;

                    $scope.choices = [];
                    $scope.isNoChoices = true;
                    var lastSearchPhrase = '';
                    var totalCount = 0;

                    $scope.setValue = (item, model) => {
                        $scope.selectedId = item.id;
                    }

                    $scope.setValueMultiple = (item, model) => {
                        $scope.selectedIds.push(item.id);
                    }

                    $scope.removeValue = (item, model) => {
                        let index = $scope.selectedIds.indexOf(item.id);
                        if (index >= 0) {
                            $scope.selectedIds.splice(index, 1);
                        }
                    }

                    $scope.fetch = function ($select) {
                        load();

                        if (!$scope.disabled) {
                            $scope.fetchNext($select);
                        }
                    };

                    function load() {
                        var fulfillmentIds = $scope.context.multiple ? $scope.selectedIds : [$scope.selectedId];

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

                        if ($select.page === 0 || totalCount > $scope.choices.length) {
                            return inventory.search(
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
                                }).$promise;
                        }

                        return $q.resolve();
                    };

                    function join(newItems) {
                        newItems = _.reject(newItems, x => _.any($scope.choices, y => y.id === x.id));

                        if (_.any(newItems)) {
                            $scope.choices = $scope.choices.concat(newItems);
                            $scope.isNoChoices = $scope.choices.length === 0;
                        }
                    }
                }
            }
        }]);
