using Microsoft.AspNetCore.Mvc;
using ShopRestAPI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShopRestAPI.Controllers
{
    [Route("shop_api/images")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        public ImagesController(ShopContext context)
            : base(context) { }

        public static string GetImagesPath() =>
            $"{Directory.GetCurrentDirectory()}\\{ImagesLocalPath}";

        public static string ImagesLocalPath => "Images";

        [HttpGet("{id}")]
        public ActionResult GetImage(string id)
        {
            return PhysicalFile($"{GetImagesPath()}\\{id}", "image/jpeg");
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateImage(ImageUploadData image)
        {
            var ticks = DateTime.Now.Ticks.ToString();
            var imageName = $"{ticks}.{image.Extension}";

            var bytes = Convert.FromBase64String(image.File);
            using (var imageFile = new FileStream($"{GetImagesPath()}\\{imageName}", FileMode.Create))
            {
                await imageFile.WriteAsync(bytes, 0, bytes.Length);
                await imageFile.FlushAsync();
            }

            return new ProductImageDTO{ ImageURL = imageName };
        }
    }
}
