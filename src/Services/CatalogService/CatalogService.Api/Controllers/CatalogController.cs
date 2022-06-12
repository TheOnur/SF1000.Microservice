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
        private readonly IOptions<CatalogSettings> settings;

        public CatalogController(CatalogContext catalogContext, IOptions<CatalogSettings> settings)
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

        [HttpGet]
        [Route("items/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);
            var baseUri = settings.Value.PicBaseUrl;

            if (item != null)
            {
                item.PictureUri = baseUri + item.PictureFileName;

                return Ok(item);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginetedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginetedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var totalItems = await catalogContext.CatalogItems.Where(x => x.Name.StartsWith(name)).LongCountAsync();

            var itemsOnPage = await catalogContext.CatalogItems.Where(x => x.Name.StartsWith(name)).Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            return Ok(itemsOnPage);
        }

        [HttpGet]
        [Route("items/type/{catalogTypeId:int}/brand/{brandTypeId:int?}")]
        [ProducesResponseType(typeof(PaginetedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<PaginetedItemsViewModel<CatalogItem>>> ItemsByTypeAndBrandAsync(int catalogTypeId, int? brandTypeId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            Func<CatalogItem, bool> exp = null;

            if (brandTypeId.HasValue)
            {
                exp = new Func<CatalogItem, bool>(x => x.CatalogTypeId == catalogTypeId && x.CatalogBrandId == brandTypeId);
            }
            else
            {
                exp = new Func<CatalogItem, bool>(x => x.CatalogTypeId == catalogTypeId);
            }

            var totalItems = await catalogContext.CatalogItems.Where(exp).AsQueryable().ToListAsync();

            if (totalItems.Count == 0)
            {
                return NotFound();
            }

            var itemsOnPage = await catalogContext.CatalogItems.Where(exp).AsQueryable().Skip(pageSize * pageIndex).Take(pageSize).ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            return Ok(itemsOnPage);
        }

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> itemsOnPage)
        {
            var baseUri = settings.Value.PicBaseUrl;

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
