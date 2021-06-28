using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMoviesWithSSRS.Data
{
    
    public class SellerMovieReportRow
    {
        public string SellerName { get; set; }
        
        //[Key()]
        public int SellerId { get; set; }
    
        public string MovieTitle { get; set; }
        public int MovieId { get; set; }
        public double MoviePrice { get; set; }

    }
}
