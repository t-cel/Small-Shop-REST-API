﻿using Microsoft.EntityFrameworkCore;
using System;

namespace ShopRestAPI.Models
{
    public class ShopContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductImage> ProductsImages { get; set; }

        public string DbPath { get; private set; } = "database.db";

        public ShopContext()
            => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

    }
}
