namespace ShopRestAPI.Models
{
    public class Product
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public uint Count { get; set; }
        public ulong SellerId { get; set; }
        public ulong CategoryId { get; set; }
    }

    public class ProductDTO
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public uint Count { get; set; }
        public ulong CategoryId { get; set; }
    }

    public class ProductImage
    {
        public ulong Id { get; set; }
        public string ImageURL { get; set; }
        public ulong ProductId { get; set; }
    }

    public class ProductImageDTO
    {
        public string ImageURL { get; set; }
    }
}
