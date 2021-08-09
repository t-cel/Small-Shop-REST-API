using ShopRestAPI.Controllers;
using ShopRestAPI.Models;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Gridify;

namespace ShopRestAPI.Tests
{
    public class ProductControllerTests : ControllerTestsBase
    {
        public ProductControllerTests(ITestOutputHelper output)
            : base(output)
        {}

        private void AddTestProducts(ShopContext context)
        {
            context.Products.AddRange(new Product[]
            {
                new Product{ Name="Product1", Price=9.99f, Count=10, SellerId=1, CategoryId=1 },
                new Product{ Name="Product2", Price=2.99f, Count=8, SellerId=1, CategoryId=2 },
                new Product{ Name="Product3", Price=5.99f, Count=25, SellerId=2, CategoryId=3 },
                new Product{ Name="Product4", Price=50.0f, Count=5, SellerId=1, CategoryId=4 },
            });
            context.SaveChanges();
            //output.WriteLine($"products count: {context.Products.ToList().Count.ToString()}");
        }

        [Fact]
        public async void GetProducts_ReturnsAllProducts()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                var result = await controller.GetProducts(null);
                Assert.IsType<OkObjectResult>(result.Result);

                var okResult = result.Result as OkObjectResult;
                Assert.IsType<List<Product>>(okResult.Value);
                Assert.Equal(3, (okResult.Value as List<Product>).Count);
            }
        }

        [Theory]
        //[InlineData("Name==Product1", 1)]
        //[InlineData("Price>=2,Price<<5", 1)]
        [InlineData("Price!=0", 4)]
        //[InlineData("Count>>8", 1)]
        public async void GetProducts_ReturnsFilteredProducts(string filter, int expectedItemsCount)
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                controller.HttpContext.Request.QueryString = new QueryString($"?filter={filter}");
                var result = await controller.GetProducts(null);
                Assert.IsType<OkObjectResult>(result.Result);

                var okResult = result.Result as OkObjectResult;

                Assert.IsType<List<Product>>(okResult.Value);
                Assert.Equal(expectedItemsCount, (okResult.Value as List<Product>).Count);
            }
        }

        //[Fact]
        //public async void GetProducts_ReturnsPaginatedProducts()
        //{
        //    throw new System.NotImplementedException();
        //}

        //[Fact]
        //public async void GetProducts_ReturnsSortedProducts()
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
