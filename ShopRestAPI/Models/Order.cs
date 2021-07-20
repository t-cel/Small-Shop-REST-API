namespace ShopRestAPI.Models
{
    public enum OrderStatus : int
    {
        Ordered,
        Sent,
        Delivered
    }

    public class Order
    {
        public ulong Id { get; set; }
        public ulong BuyerId { get; set; }
        public uint Count { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public ulong ProductId { get; set; }
        public Product Product { get; set; }
    }
}
