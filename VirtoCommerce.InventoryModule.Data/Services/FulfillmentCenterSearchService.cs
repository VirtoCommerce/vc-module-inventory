﻿using System;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterSearchService : IFulfillmentCenterSearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        public FulfillmentCenterSearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public GenericSearchResult<FulfillmentCenter> SearchCenters(FulfillmentCenterSearchCriteria criteria)
        {
            var result = new GenericSearchResult<FulfillmentCenter>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = repository.FulfillmentCenters;
                if (!string.IsNullOrEmpty(criteria.SearchPhrase))
                {
                    query = query.Where(x => x.Name.Contains(criteria.SearchPhrase));
                }
               
                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "Name" } };
                }

                query = query.OrderBySortInfos(sortInfos);

                result.TotalCount = query.Count();
                result.Results = query.Skip(criteria.Skip)
                                 .Take(criteria.Take)
                                 .ToArray()
                                 .Select(x => x.ToModel(AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance()))
                                 .ToList();
            }
            return result;
        }
    }
}
