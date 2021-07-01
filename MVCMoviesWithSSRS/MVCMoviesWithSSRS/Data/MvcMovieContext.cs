using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MVCMoviesWithSSRS.Data;

namespace MVCMoviesWithSSRS.Models
{
    public partial class MvcMovieContext : DbContext
    {
        
        public MvcMovieContext(DbContextOptions<MvcMovieContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Movie> Movie { get; set; }
        public virtual DbSet<Seller> Sellers {get;set;}
        public virtual DbSet<MoviePrice> MoviePrices { get; set; }

        
        //Reporting Entity Classes
        [System.ComponentModel.DataAnnotations.Schema.NotMapped()]
        public virtual DbSet<SellerMovieReportRow> SellerMoviesReport { get; set; }

       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            });

            //Indicate that this is a reporting structure, which has no key
            modelBuilder.Entity<SellerMovieReportRow>().HasNoKey();

        }
    }
}
