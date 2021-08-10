using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // use DbContext for EF Core
using Microsoft.Extensions.DependencyInjection;
using ShopRestAPI.Models;
using Xunit.Abstractions;
using System;

namespace ShopRestAPI.Tests
{
    public class ControllerTestsBase
    {
        protected readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes in memory database options and base auth data
        /// </summary>
        /// <param name="output"></param>
        protected ControllerTestsBase(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected DbContextOptions<ShopContext> CreateNewDbContextOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ShopContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString())
                   .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        protected void AddTestUsers(ShopContext context)
        {
            context.Users.Add(new User
            {
                Email = "esupplier@mail.com",
                PasswordHash = "$2a$11$4g3epsIxxnO4.Zjn0FixAuKhUNivqK8P/fND5zq8hlQ2mBEDw2Iem",
                Name = "ESupplier Fake"
            });
            context.SaveChanges();
        }

        /// <summary>
        /// Returns controller with mocked auth data
        /// </summary>
        /// <returns></returns>
        protected T GetMockedController<T>(ShopContext context) where T : Controller
        {
            var controller = Activator.CreateInstance(typeof(T), context) as T;
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Base ZXN1cHBsaWVyQG1haWwuY29tOmVzdXBwbGllcg==";
            return controller;
        }
    }
}
