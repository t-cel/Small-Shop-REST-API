﻿using Gridify;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] GridifyQuery gQuery)
        {
            var user = await GetUser();
            var orders = context.Orders.Where(order => order.Product.SellerId == user.Id);

            try
            {
                return await orders.GridifyQueryable(gQuery).Query.ToListAsync();
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
                return StatusCode((int)HttpStatusCode.Forbidden);

            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
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

            if (order.Product.SellerId != user.Id)
                return StatusCode((int)HttpStatusCode.Forbidden);

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
            if (order.Product.SellerId != user.Id)
                return StatusCode((int)HttpStatusCode.Forbidden);

            context.Entry(order).State = EntityState.Modified; 

            try
            {
                await context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!context.Orders.Any(order => order.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }
    }
}
