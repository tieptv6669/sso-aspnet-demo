
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestEfCore.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() : base()
        {
            //Database.SetInitializer<ApplicationContext>(null);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings in safe place
                optionsBuilder.UseSqlServer(@"Server=192.168.68.223\sqlexpress2012;Database=TestEfCore;User ID=sa;Password=123456");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(200).IsUnicode(true);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne<User>(o => o.Owner).WithMany(m => m.Products).HasForeignKey(o => o.OwnerId);
            });
        }

    }
}
