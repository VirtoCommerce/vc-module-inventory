angular.module('virtoCommerce.inventoryModule')
    .factory('virtoCommerce.inventoryModule.fulfillments', ['$resource', function ($resource) {
        return $resource('api/inventory/fulfillmentcenters', {}, {
            get: { url: 'api/inventory/fulfillmentcenters/:id' },
            update: { method: 'PUT' },
            search: { url: 'api/inventory/fulfillmentcenters/search', method: 'POST' },
            searchFulfillmentCenterInventories: { url: 'api/inventory/fulfillmentcenterinventories/search', method: 'POST' },
            getByIds: { url: 'api/inventory/fulfillmentcenters/plenty', method: 'POST', isArray: true },
            updateBatch: { url: 'api/inventory/fulfillmentcenters/batch', method: 'POST', isArray: true }
        });
    }]);
