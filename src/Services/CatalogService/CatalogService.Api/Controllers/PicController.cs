using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly CatalogContext catalogContext;

        public PicController(IWebHostEnvironment env, CatalogContext catalogContext)
        {
            this.env = env;
            this.catalogContext = catalogContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("App Running");
        }

        [HttpGet]
        [Route("api/v1/catalog/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetImageAsync(int catalogItemId)
        {
            if (catalogItemId == 0)
            {
                return BadRequest();
            }

            var item = await catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == catalogItemId);

            if (item != null)
            {
                var webRoot = env.WebRootPath;
                var path = Path.Combine(webRoot, item.PictureFileName);

                string imageFileExtension = Path.GetExtension(item.PictureFileName);
                string mimeType = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = await System.IO.File.ReadAllBytesAsync(path);

                return File(buffer, mimeType);
            }

            return NotFound();
        }

        private string GetImageMimeTypeFromImageFileExtension(string imageFileExtension)
        {

            if (imageFileExtension == ".png")
            {
                return "image/png";
            }
            else if (imageFileExtension == "jpeg")
            {
                return "image/jpeg";
            }
            else
            {
                throw new System.Exception("Not Defined File Extension!");
            }
        }
    }
}
