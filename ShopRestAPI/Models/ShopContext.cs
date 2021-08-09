using Microsoft.EntityFrameworkCore;
using System;

namespace ShopRestAPI.Models
{
    public class ShopContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ProductImage> ProductsImages { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options) : base(options) 
            => Database.EnsureCreated();
    }
}
