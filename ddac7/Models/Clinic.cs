using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Models
{
    public class Clinic
    {
        //暂时
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Clinic Name")]
        public string ClinicName { get; set; }


        [Required]
        [Display(Name = "Clinic Description")]
        public string ClinicDesc { get; set; }

        [Phone]
        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNum { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        public string Status { get; set; }

        public string UserID { get; set; }


    }
}
