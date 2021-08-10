using FluentAssertions;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using ShopRestAPI.Controllers;
using ShopRestAPI.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ShopRestAPI.Tests
{
    public class OrderControllerTests : BaseControllerTests
    {
        public OrderControllerTests(ITestOutputHelper output)
            : base(output) { }

        private void AddTestProducts(ShopContext context)
        {
            context.Products.AddRange(new Product[]
            {
                new Product{ Name="Football Ball", Price=9.99f, Count=10, SellerId=1, CategoryId=1 },
                new Product{ Name="TV 60\"", Price=699.99f, Count=8, SellerId=1, CategoryId=3 },
                new Product{ Name="TV 50\"", Price=499.99f, Count=25, SellerId=1, CategoryId=3 },
                new Product{ Name="TV 50\"", Price=499.99f, Count=25, SellerId=2, CategoryId=3 },
                new Product{ Name="Board Game", Price=40.0f, Count=5, SellerId=1, CategoryId=4 },
                new Product{ Name="Sport Shoes", Price=20.0f, Count=2, SellerId=3, CategoryId=2 },
                new Product{ Name="Sport TShirt", Price=50.0f, Count=5, SellerId=1, CategoryId=2 },
            });
            context.SaveChanges();
        }

        private void AddTestOrders(ShopContext context)
        {
            context.Orders.AddRange(new Order[]
            {
                new Order{ ProductId = 1, OrderStatus = OrderStatus.Ordered, BuyerId = 12, Count = 2 },
                new Order{ ProductId = 2, OrderStatus = OrderStatus.Sent, BuyerId = 6, Count = 5 },
                new Order{ ProductId = 3, OrderStatus = OrderStatus.Delivered, BuyerId = 6, Count = 8 },
                new Order{ ProductId = 2, OrderStatus = OrderStatus.Sent, BuyerId = 4, Count = 4 },
                new Order{ ProductId = 1, OrderStatus = OrderStatus.Ordered, BuyerId = 16, Count = 3 },
                new Order{ ProductId = 4, OrderStatus = OrderStatus.Delivered, BuyerId = 2, Count = 1 },
                new Order{ ProductId = 4, OrderStatus = OrderStatus.Delivered, BuyerId = 1, Count = 2 },
            });
            context.SaveChanges();
        }

        [Fact]
        public async void GetOrders_ReturnsAllUserOrders()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.GetOrders(null);

                Assert.IsType<OkObjectResult>(result.Result);
                var okResult = result.Result as OkObjectResult;
                Assert.IsType<List<Order>>(okResult.Value);
                Assert.Equal(5, (okResult.Value as List<Order>).Count);
            }
        }

        [Fact]
        public async void GetOrder_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.GetOrder(124124);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async void GetOrder_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.GetOrder(6);

                Assert.IsType<ForbidResult>(result.Result);
            }
        }

        [Fact]
        public async void GetOrder_ReturnsOrder()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.GetOrder(2);

                Assert.IsType<OkObjectResult>(result.Result);
                Assert.IsType<Order>((result.Result as OkObjectResult).Value);

                var expectedOrder = new Order { 
                    Id = 2, 
                    ProductId = 2, 
                    OrderStatus = OrderStatus.Sent, 
                    BuyerId = 6,
                    Count = 5 
                };

                var resultOrder = ((result.Result as OkObjectResult).Value as Order);
                resultOrder.Should().BeEquivalentTo(expectedOrder, options => options.Excluding(order => order.Product));
            }
        }

        [Fact]
        public async void CreateOrder_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    ProductId = 252,
                    OrderStatus = OrderStatus.Ordered,
                    BuyerId = 6,
                    Count = 2
                };

                var result = await controller.CreateOrder(order);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async void CreateOrder_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    ProductId = 4,
                    OrderStatus = OrderStatus.Ordered,
                    BuyerId = 6,
                    Count = 2
                };

                var result = await controller.CreateOrder(order);

                Assert.IsType<ForbidResult>(result.Result);
            }
        }

        [Fact]
        public async void CreateOrder_ReturnsBadRequest()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    ProductId = 1,
                    OrderStatus = OrderStatus.Ordered,
                    BuyerId = 6,
                    Count = 255
                };

                var result = await controller.CreateOrder(order);

                Assert.IsType<BadRequestResult>(result.Result);
            }
        }

        [Fact]
        public async void CreateOrder_CreatesOneOrder()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    ProductId = 1,
                    OrderStatus = OrderStatus.Ordered,
                    BuyerId = 6,
                    Count = 2
                };
                int countBefore = context.Orders.Count();

                await controller.CreateOrder(order);

                Assert.Equal(countBefore + 1, context.Orders.Count());
            }
        }

        [Fact]
        public async void DeleteOrder_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.DeleteOrder(255);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async void DeleteOrder_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);

                var result = await controller.DeleteOrder(7);

                Assert.IsType<ForbidResult>(result.Result);
            }
        }

        /// <summary>
        /// When deleting orders, if order that is going to be deleted has status other than
        /// Delivered, we assume that products may be return to store, so we need to update 
        /// products count that was subtracted when creating order.
        /// </summary>
        [Theory]
        [InlineData(1, 12)] //not delivered so 10 -> 12
        [InlineData(3, 25)] //delivered, 25 -> 25
        public async void DeleteOrder_DeletesOneOrder(ulong orderId, uint expectedProductsCount)
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                int countBefore = context.Orders.Count();
                var productId = context.Orders.Find(orderId).ProductId;

                var result = await controller.DeleteOrder(orderId);

                Assert.IsType<NoContentResult>(result.Result);
                Assert.Equal(expectedProductsCount, context.Products.Find(productId).Count);
                Assert.Equal(countBefore - 1, context.Orders.Count());
            }
        }

        [Fact]
        public async void UpdateOrder_ReturnsBadRequest()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    Id = 1,
                    ProductId = 1,
                    OrderStatus = OrderStatus.Delivered,
                    BuyerId = 12,
                    Count = 2
                };

                var result = await controller.UpdateOrder(5, order);

                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void UpdateOrder_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    Id = 6,
                    ProductId = 4,
                    OrderStatus = OrderStatus.Delivered,
                    BuyerId = 12,
                    Count = 2
                };

                var result = await controller.UpdateOrder(6, order);

                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void UpdateOrder_UpdatesOrder()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestOrders(context);
                var controller = GetMockedController<OrdersController>(context);
                var order = new Order
                {
                    Id = 1,
                    ProductId = 1,
                    OrderStatus = OrderStatus.Delivered,
                    BuyerId = 12,
                    Count = 2
                };

                var result = await controller.UpdateOrder(1, order);

                Assert.IsType<NoContentResult>(result);
                Assert.Equal(OrderStatus.Delivered, context.Orders.Find(1ul).OrderStatus);
            }
        }
    }
}
