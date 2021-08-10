using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopRestAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace ShopRestAPI.Controllers
{
    [Route("shop_api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public ProductsController(ShopContext context)
            : base(context) { }

        [HttpGet, Produces(typeof(Paging<Product>))]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] GridifyQuery gQuery)
        {          
            var user = await GetUser();
            var products = context.Products.Where(product => product.SellerId == user.Id);

            try
            {
                return Ok(await products.GridifyQueryable(gQuery).Query.ToListAsync());
            }
            catch(GridifyFilteringException)
            {
                return BadRequest();
            }         
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(ulong id)
        {
            var product = await context.Products.FindAsync(id);
            var user = await GetUser();

            if(product == null)
                return NotFound();

            if(product.SellerId != user.Id)
                return Forbid();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct(ProductDTO productDTO)
        {
            var user = await GetUser();
            var product = new Product
            {
                Name = productDTO.Name,
                Price = productDTO.Price,
                Count = productDTO.Count,
                SellerId = user.Id,
                CategoryId = productDTO.CategoryId
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ProductToDTO(product));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(ulong id)
        {
            var product = await context.Products.FindAsync(id);
            var user = await GetUser();

            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            //remove also all product images
            context.ProductsImages.RemoveRange(context.ProductsImages.Where(image => image.ProductId == product.Id));
            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(ulong id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            var _product = await context.Products.FindAsync(id);
            if (_product == null)
                return NotFound();
            
            var user = await GetUser();
            if (_product.SellerId != user.Id)
                return Forbid();

            context.Entry(_product).State = EntityState.Detached;
            context.Entry(product).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/images")]
        public async Task<ActionResult> AddImageToProduct(ulong id, ProductImageDTO productImage)
        {
            var user = await GetUser();
            var product = await context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            if (!System.IO.File.Exists($"{ImagesController.ImagesLocalPath}\\{productImage.ImageURL}"))
                return NotFound();

            context.ProductsImages.Add(new ProductImage { ImageURL = productImage.ImageURL, ProductId = product.Id });
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages(ulong id)
        {
            var user = await GetUser();
            var product = await context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            return Ok(await context.ProductsImages.Where(image => image.ProductId == id).ToListAsync());
        }

        [HttpDelete("{id}/images/{imageID}")]
        public async Task<ActionResult> DeleteProductImage(ulong id, ulong imageID)
        {
            var user = await GetUser();
            var image = await context.ProductsImages.FindAsync(imageID);
            var product = await context.Products.FindAsync(id);

            if (image == null)
                return NotFound();

            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            context.Remove(image);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private static ProductDTO ProductToDTO(Product product) =>
            new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Count = product.Count,
                CategoryId = product.CategoryId
            };
    }
}
