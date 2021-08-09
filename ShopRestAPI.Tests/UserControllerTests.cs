using ShopRestAPI.Controllers;
using ShopRestAPI.Models;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ShopRestAPI.Tests
{
    public class UserControllerTests : ControllerTestsBase
    {
        public UserControllerTests(ITestOutputHelper output)
            : base(output)
        { }

        [Fact]
        public async void GetUserData_ReturnsCorrectUserData()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                AddTestUsers(context);

                // create controller with mocked http context and auth data
                var controller = GetMockedController<UserController>(context);

                var result = await controller.GetUserData();
                Assert.IsType<OkObjectResult>(result.Result);

                var user = (result.Result as OkObjectResult).Value;
                Assert.IsType<UserDTO>(user);
                Assert.Equal(1u, (user as UserDTO).Id);
            }
        }
    }
}
