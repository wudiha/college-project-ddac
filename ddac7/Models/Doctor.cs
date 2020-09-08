using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Models
{
    
    public class BlobModel
    {
        public String id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4)]
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; }
        [Required]
        [Display(Name = "Doctor Contact Number")]
        public string DoctorContactNumber { get; set; }
        public string imgurl { get; set; }
        public IEnumerable<Uri> profileImage { get; set; }
        public List<BlobModel> doctor { get; set; }
    }

    public class Doctor
    {

        public String id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4)]
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; }
        [Required]
        [Display(Name = "Doctor Contact Number")]
        public string DoctorContactNumber { get; set; }
        public String profileImage { get; set; }
    }
}
