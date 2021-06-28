using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MVCMoviesWithSSRS.Models
{
    public class Soda
    {
        [Key()]
        public int Id { get; set; }
        public string Flavor { get; set; }
    }
}
