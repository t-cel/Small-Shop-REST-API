using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShopRestAPI.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ShopRestAPI.Controllers
{
    /// <summary>
    /// Base controller that provides database access and query for current user
    /// </summary>
    public class ControllerBase : Controller
    {
        protected readonly ShopContext context;
        private string AuthorizationDataHeaderKey => "Authorization";

        public ControllerBase(ShopContext context)
        {
            this.context = context;
        }

        protected string DecodeAuthorizationHeader()
        {
            var emailPasswordEncoded = Request.Headers[AuthorizationDataHeaderKey].ToString().Split(' ')[1];
            var emailPassowordDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(emailPasswordEncoded));

            return emailPassowordDecoded;
        }

        /// <summary>
        /// We assume that user was authorized before which is default for any action on API.
        /// </summary>
        /// <returns></returns>
        protected async Task<User> GetUser()
        {
            var email = DecodeAuthorizationHeader().Split(':')[0];
            return 
                await context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        private async Task<bool> CheckAuthorizationHeader(HttpContext context)
        {
            if (!context.Request.Headers.Keys.Contains(AuthorizationDataHeaderKey))
                return false;

            var authorizationData = context.Request.Headers[AuthorizationDataHeaderKey].ToString();

            if (!authorizationData.StartsWith("Basic ") && authorizationData != "Basic ")
                return false;

            var decoded = DecodeAuthorizationHeader();

            if (!decoded.Contains(':'))
                return false;

            var emailAndPassword = decoded.Split(':');
            var user = await this.context.Users.FirstOrDefaultAsync(user => user.Email == emailAndPassword[0]);

            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(emailAndPassword[1], user.PasswordHash))
                return false;

            return true;
        }

        /// <summary>
        /// All of our actions require authorization, this method will be called before any action call.
        /// </summary>
        public override async void OnActionExecuting(ActionExecutingContext context)
        {        
            var authorized = await CheckAuthorizationHeader(context.HttpContext);
            context.Result = authorized ? null : new UnauthorizedResult();
            base.OnActionExecuting(context);
        }
    }
}
