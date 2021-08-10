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
    public class CategoriesControllerTests : BaseControllerTests
    {
        public CategoriesControllerTests(ITestOutputHelper output)
            : base(output) { }

        private void AddTestCategories(ShopContext context)
        {
            context.Categories.AddRange(new Category[]
            {
                new Category{ Name = "Sports" },
                new Category{ Name = "Household" },
                new Category{ Name = "Games" },
                new Category{ Name = "Car Parts" },
                new Category{ Name = "Art" },
            });
            context.SaveChanges();
        }

        [Fact]
        public async void GetCategories_ReturnsOkResult()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestCategories(context);
                var controller = GetMockedController<CategoriesController>(context);

                var result = await controller.GetCategories();

                Assert.IsType<OkObjectResult>(result.Result);
            }
        }

        [Fact]
        public async void GetCategories_ReturnsAllCategories()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);
                AddTestCategories(context);
                var controller = GetMockedController<CategoriesController>(context);

                var result = await controller.GetCategories();

                var okResult = result.Result as OkObjectResult;
                Assert.IsType<List<Category>>(okResult.Value);
                Assert.Equal(5, (okResult.Value as List<Category>).Count); 
            }
        }
    }
}
