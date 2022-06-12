using CatalogService.Api.Core.Application;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext catalogContext;
        private readonly IOptionsSnapshot<CatalogSettings> settings;

        public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> settings)
        {
            this.catalogContext = catalogContext;
            this.settings = settings;

            catalogContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginetedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);

                if (!items.Any())
                {
                    return BadRequest("ids value invalid. Must be comma-seperated list of numbers");
                }

                return Ok(items);
            }

            var totalItems = await catalogContext.CatalogItems.LongCountAsync();

            var itemsOnPage = await catalogContext
                .CatalogItems
                .OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginetedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> itemsOnPage)
        {
            var baseUri = settings.Get("PicBaseUrl");

            itemsOnPage.ForEach(item =>
            {
                if (item != null)
                {
                    item.PictureUri = string.Format($"{baseUri}{item.PictureFileName}");
                }
            });

            return itemsOnPage;

        }
        private async Task<IEnumerable<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

            if (!numIds.All(pid => pid.Ok))
            {
                return new List<CatalogItem>();
            }

            var idsToSelect = numIds.Select(id => id.Value);
            var items = await catalogContext.CatalogItems.Where(x => idsToSelect.Contains(x.Id)).ToListAsync();

            items = ChangeUriPlaceholder(items);

            return items;
        }
    }
}
