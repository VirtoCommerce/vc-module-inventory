angular.module('virtoCommerce.inventoryModule')
    .factory('virtoCommerce.inventoryModule.fulfillments', ['$resource', function ($resource) {
        return $resource('api/inventory/fulfillmentcenters', {}, {
            get: { url: 'api/inventory/fulfillmentcenters/:id' },
            update: { method: 'PUT', isArray: true},
            search: { url: 'api/inventory/fulfillmentcenters/search', method: 'POST' },
            getByIds: { method: 'POST', isArray: true}
        });
    }]);
