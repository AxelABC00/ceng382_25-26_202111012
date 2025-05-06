// using System;
// using System.Collections.Generic;
// using Microsoft.EntityFrameworkCore;
// using MyRazorApp.Models;

// namespace MyRazorApp.Data;

// public partial class ProductsDbContext : DbContext
// {
//     public ProductsDbContext()
//     {
//     }

//     public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
//         : base(options)
//     {
//     }

//     public virtual DbSet<Class> Classes { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Server=DESKTOP-P0T026R\\SQLEXPRESS;Database=SchoolDb;Trusted_Connection=True;TrustServerCertificate=True;");

//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         OnModelCreatingPartial(modelBuilder);
//     }

//     partial void OnModelCreatingPartial(ModeBuilder modelBuilder);
// }
