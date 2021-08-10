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
    public class ProductControllerTests : BaseControllerTests
    {
        public ProductControllerTests(ITestOutputHelper output)
            : base(output)
        { }

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

        private void AddTestProductImages(ShopContext context)
        {
            context.ProductsImages.AddRange(new ProductImage[]
            {
                new ProductImage { ProductId = 1, ImageURL = "637624233261872009.png" },
                new ProductImage { ProductId = 1, ImageURL = "637624232403142398.png" },
                new ProductImage { ProductId = 4, ImageURL = "637624232403142398.png" }
            });
            context.SaveChanges();
        }

        [Fact]
        public async void GetProducts_ReturnsAllUserProducts()
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
                Assert.Equal(5, (okResult.Value as List<Product>).Count); //all products of logged user
            }
        }

        [Theory]
        [InlineData("Name==Board Game", 1)]
        [InlineData("Price>>10,Price<<60", 2)]
        [InlineData("Count>>8", 2)]
        [InlineData("Price<<3|Count>>9", 2)]
        public async void GetProducts_ReturnsFilteredUserProducts(string filter, int expectedItemsCount)
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var gQuery = new GridifyQuery() { Filter = filter };

                //act
                var result = await controller.GetProducts(gQuery);

                //assert
                Assert.IsType<OkObjectResult>(result.Result);

                var okResult = result.Result as OkObjectResult;

                Assert.IsType<List<Product>>(okResult.Value);
                Assert.Equal(expectedItemsCount, (okResult.Value as List<Product>).Count);
            }
        }

        [Fact]
        public async void GetProducts_ReturnsPaginatedUserProducts()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var gQuery = new GridifyQuery() { Page = 2, PageSize = 2 };

                //act
                var result = await controller.GetProducts(gQuery);

                //assert
                Assert.IsType<OkObjectResult>(result.Result);

                var okResult = result.Result as OkObjectResult;
                Assert.IsType<List<Product>>(okResult.Value);

                var expectedList = new List<Product>() { context.Products.ToList()[2], context.Products.ToList()[4] };
                Assert.Equal(expectedList, okResult.Value);
            }
        }

        [Fact]
        public async void GetProducts_ReturnsSortedUserProducts()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var gQuery = new GridifyQuery() { SortBy = "Name", IsSortAsc = true };

                //act
                var result = await controller.GetProducts(gQuery);

                //assert
                Assert.IsType<OkObjectResult>(result.Result);

                var okResult = result.Result as OkObjectResult;
                Assert.IsType<List<Product>>(okResult.Value);

                var expectedList = new List<Product>(context.Products.ToList()).Where(p => p.SellerId == 1).OrderBy(p => p.Name).ToList();
                Assert.Equal(expectedList, (okResult.Value as List<Product>));
            }
        }

        [Fact]
        public async void GetProduct_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProduct(4);

                //assert
                Assert.IsType<ForbidResult>(result.Result);
            }
        }

        [Fact]
        public async void GetProduct_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProduct(1249);

                //assert
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async void GetProduct_ReturnsProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProduct(2);

                //assert
                Assert.IsType<OkObjectResult>(result.Result);
                Assert.Equal(context.Products.ToList()[1], (result.Result as OkObjectResult).Value);
            }
        }

        [Fact]
        public async void CreateProduct_ReturnsNewProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var newProduct = new ProductDTO
                {
                    Name = "Test Product 123456789",
                    Price = 25.99f,
                    Count = 18,
                    CategoryId = 1
                };

                //act
                var result = await controller.CreateProduct(newProduct);

                //assert
                Assert.IsType<CreatedAtActionResult>(result.Result);
                var resultProduct = (result.Result as CreatedAtActionResult).Value as ProductDTO;
                Assert.Equal(newProduct.Name, resultProduct.Name);
            }
        }

        [Fact]
        public async void CreateProduct_CreatesOneProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var newProduct = new ProductDTO
                {
                    Name = "Test Product 123456789",
                    Price = 25.99f,
                    Count = 18,
                    CategoryId = 1
                };

                //act
                await controller.CreateProduct(newProduct);

                //assert
                Assert.Equal(8, context.Products.Count());
            }
        }

        [Fact]
        public async void DeleteProduct_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProduct(1249521);

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteProduct_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProduct(6);

                //assert
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void DeleteProduct_ReturnsNoContent()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProduct(1);

                //assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        [Fact]
        public async void DeleteProduct_DeletesOneProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProduct(1);

                //assert
                Assert.Equal(6, context.Products.Count());
            }
        }

        [Fact]
        public async void UpdateProduct_ReturnsBadRequest()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var updatedProduct = new Product
                {
                    Id = 2,
                    Name = "Sport Shoes",
                    Price = 16.99f,
                    Count = 4,
                    CategoryId = 2,
                    SellerId = 1
                };

                //act
                var result = await controller.UpdateProduct(6, updatedProduct);

                //assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void UpdateProduct_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var updatedProduct = new Product
                {
                    Id = 214,
                    Name = "Sport Shoes",
                    Price = 16.99f,
                    Count = 4,
                    CategoryId = 2,
                    SellerId = 1
                };

                //act
                var result = await controller.UpdateProduct(214, updatedProduct);

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void UpdateProduct_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var updatedProduct = new Product
                {
                    Id = 6,
                    Name = "Sport Shoes",
                    Price = 16.99f,
                    Count = 4,
                    CategoryId = 2,
                    SellerId = 1
                };

                //act
                var result = await controller.UpdateProduct(6, updatedProduct);

                //assert
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void UpdateProduct_ReturnsNoContent()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                var updatedProduct = new Product
                {
                    Id = 1,
                    Name = "Football Ball",
                    Price = 12.99f,
                    Count = 8,
                    CategoryId = 1,
                    SellerId = 1
                };

                //act
                var result = await controller.UpdateProduct(1, updatedProduct);

                //assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        [Fact]
        public async void UpdateProduct_UpdatesProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);

                var updatedProduct = new Product
                {
                    Id = 1,
                    Name = "Football Ball",
                    Price = 12.99f,
                    Count = 8,
                    CategoryId = 1,
                    SellerId = 1
                };

                //act
                var result = await controller.UpdateProduct(1, updatedProduct);

                //assert
                Assert.Equal(updatedProduct.Price, context.Products.ToList()[0].Price);
            }
        }

        [Fact]
        public async void AddImageToProduct_ReturnsNotFoundProduct()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var productImage = new ProductImageDTO { ImageURL = "637624231306098227.png" };

                //act
                var result = await controller.AddImageToProduct(256_000, productImage);

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void AddImageToProduct_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var productImage = new ProductImageDTO { ImageURL = "637624231306098227.png" };

                //act
                var result = await controller.AddImageToProduct(4, productImage);

                //assert
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void AddImageToProduct_ReturnsNotFoundImage()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                var controller = GetMockedController<ProductsController>(context);
                var productImage = new ProductImageDTO { ImageURL = "thereisnosuchimage.png" };

                //act
                var result = await controller.AddImageToProduct(1, productImage);

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetProductImages_ReturnsNotFound()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProductImages(1912995);

                //assert
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async void GetProductImages_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProductImages(4);

                //assert
                Assert.IsType<ForbidResult>(result.Result);
            }
        }

        [Fact]
        public async void GetProductImages_ReturnsOkResult()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProductImages(1);

                //assert
                Assert.IsType<OkObjectResult>(result.Result);
            }
        }

        [Fact]
        public async void GetProductImages_ReturnsProductImages()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.GetProductImages(1);

                //assert
                var expected = new List<ProductImage>()
                {
                    new ProductImage { Id = 1, ProductId = 1, ImageURL = "637624233261872009.png" },
                    new ProductImage { Id = 2, ProductId = 1, ImageURL = "637624232403142398.png" },
                };
                var resultList = (result.Result as OkObjectResult).Value as List<ProductImage>;
                expected.Should().BeEquivalentTo(resultList);
            }
        }

        [Theory]
        [InlineData(1241512, 1)]
        [InlineData(1, 12352352)]
        public async void DeleteProductImage_ReturnsNotFound(ulong productId, ulong imageId)
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProductImage(productId, imageId);

                //assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteProductImage_ReturnsForbidden()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProductImage(4, 3);

                //assert
                Assert.IsType<ForbidResult>(result);
            }
        }

        [Fact]
        public async void DeleteProductImage_ReturnsNoContent()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProductImage(1, 1);

                //assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        [Fact]
        public async void DeleteProductImage_DeletesOneImage()
        {
            using (var context = new ShopContext(CreateNewDbContextOptions()))
            {
                //arrange
                AddTestUsers(context);
                AddTestProducts(context);
                AddTestProductImages(context);
                var controller = GetMockedController<ProductsController>(context);

                //act
                var result = await controller.DeleteProductImage(1, 1);

                //assert
                Assert.Equal(2, context.ProductsImages.Count());
            }
        }
    }
}
