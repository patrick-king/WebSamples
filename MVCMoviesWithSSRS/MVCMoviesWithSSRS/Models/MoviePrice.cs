using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMoviesWithSSRS.Models
{
    public class MoviePrice
    {
        [Key]
        public int Id { get; set; }
        public int MovieId { get; set; }
        public DateTime DateEntered { get; set; }
        public int SellerId { get; set; }
        public double Price { get; set; }
       
        public Seller Seller { get; set; }
    }
}
