using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMoviesWithSSRS.Models
{
    [Table("Seller")]
    public class Seller
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }

    }
}
