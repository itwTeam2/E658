using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WRMS.Models
{
    public class _UserLogin
    {
        [Key]
        public int UIID { get; set; }
        public int RLEID { get; set; }

        [Required(ErrorMessage = "User Name is Required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        //[StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ServiceNo { get; set; }
        [Required(ErrorMessage = "User Type is Required")]
        public int EUTID{ get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}