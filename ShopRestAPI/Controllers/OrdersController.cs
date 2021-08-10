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
    [Route("shop_api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public OrdersController(ShopContext context)
            : base(context) { }

        [HttpGet, Produces(typeof(Paging<Order>))]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] GridifyQuery gQuery)
        {
            var user = await GetUser();
            var orders = context.Orders.Where(order => order.Product.SellerId == user.Id);

            try
            {
                return Ok(await orders.GridifyQueryable(gQuery).Query.ToListAsync());
            }
            catch (GridifyFilteringException)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(ulong id)
        {
            var order = await context.Orders.FindAsync(id);
            var user = await GetUser();

            if (order == null)
                return NotFound();

            if (order.Product.SellerId != user.Id)
                return Forbid();

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var user = await GetUser();
            var product = await context.Products.FindAsync(order.ProductId);

            if (product == null)
                return NotFound();

            if(product.SellerId != user.Id)
                return Forbid();

            if(order.Count > product.Count)
                return BadRequest();

            product.Count -= order.Count;
            context.Entry(product).State = EntityState.Modified;

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(ulong id)
        {
            var order = await context.Orders.FindAsync(id);
            var user = await GetUser();

            if (order == null)
                return NotFound();

            var product = await context.Products.FindAsync(order.ProductId);
            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            // if order was not finalized, products are still in shop
            if (order.OrderStatus != OrderStatus.Delivered)
            {
                product.Count += order.Count;
                context.Entry(product).State = EntityState.Modified;
            }

            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(ulong id, Order order)
        {
            if(id != order.Id)
                return BadRequest();

            var user = await GetUser();
            var product = await context.Products.FindAsync(order.ProductId);

            if (product == null)
                return NotFound();

            if (product.SellerId != user.Id)
                return Forbid();

            var _order = await context.Orders.FindAsync(id);
            if (_order == null)
                return NotFound();

            context.Entry(_order).State = EntityState.Detached; 
            context.Entry(order).State = EntityState.Modified;

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
