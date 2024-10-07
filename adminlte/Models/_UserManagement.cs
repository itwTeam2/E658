using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WRMS.Models
{
    public class _UserManagement
    {
        [Key]
        public int UMID { get; set; }

        public Nullable<int> SNo { get; set; }
        [Required(ErrorMessage = "Enter Service No")]
        public string ServiceNo { get; set; }
        public Nullable<int> RLEID { get; set; }
        [Required(ErrorMessage = "Select Section")]
        public string Section { get; set; }
        [Required(ErrorMessage = "Select Location")]
        public string Location { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> Active { get; set; }
        public string IPAddress { get; set; }
    }
}