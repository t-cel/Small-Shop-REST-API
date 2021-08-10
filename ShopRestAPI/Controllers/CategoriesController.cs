using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopRestAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopRestAPI.Controllers
{
    [Route("shop_api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        public CategoriesController(ShopContext context)
            : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return Ok(await context.Categories.ToListAsync());
        }
    }
}
