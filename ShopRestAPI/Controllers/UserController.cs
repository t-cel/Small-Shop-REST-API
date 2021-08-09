using Microsoft.AspNetCore.Mvc;
using ShopRestAPI.Models;
using System.Threading.Tasks;

namespace ShopRestAPI.Controllers
{
    [Route("shop_api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public UserController(ShopContext context)
            : base(context)
        {}

        [HttpGet]
        public async Task<ActionResult<UserDTO>> GetUserData()
        {
            var user = await GetUser();
            var userDTO = new UserDTO { Id = user.Id, Name = user.Name, Email = user.Email };
            return Ok(userDTO);
        }
    }
}
