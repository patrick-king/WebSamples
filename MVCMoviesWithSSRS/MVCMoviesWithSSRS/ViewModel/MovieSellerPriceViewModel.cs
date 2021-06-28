using MVCMoviesWithSSRS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMoviesWithSSRS.ViewModel
{
    /// <summary>
    /// For entering movie, seller, and seller price in one screen
    /// </summary>
    public class MovieSellerPriceViewModel
    {

        [Required]
        [Display(Name = "Movie Title")]
        public string Title { get; set; }

        [Display(Name = "Release Date")]
        [DisplayFormat(DataFormatString ="MM/dd/yyyy")]
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        [Display(Name = "MFR Suggested Price")]
        public decimal MSRPPrice { get; set; }

        [Display(Name ="Seller Name")]
        public string SellerName { get; set; }
        [Display(Name = "Seller Website URL")]
        public string URL { get; set; }
        [Display(Name = "Address")]
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        [Display(Name = "Seller Price")]
        public double SellerPrice { get; set; }

        [Display(Name ="Save with Stored Proc using EF Core")]
        public bool SaveWithStoredProcEFCore { get; set; }

        [Display(Name = "Save with Stored Proc using ADO.NET core")]
        public bool SaveWithStoredProcADONET { get; set; }

    }
}
