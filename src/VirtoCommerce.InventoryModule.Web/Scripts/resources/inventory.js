angular.module('virtoCommerce.inventoryModule')
.factory('virtoCommerce.inventoryModule.inventories', ['$resource', function ($resource) {
    return $resource('api/inventory/products/:id', { id: '@Id' }, {
        // query: { },
        update: { method: 'PUT' },
        upsert: { method: 'PUT', url: 'api/inventory/plenty' },
        searchProducts: { method: 'POST', url: 'api/inventory/products/search' }
    });
}]);
