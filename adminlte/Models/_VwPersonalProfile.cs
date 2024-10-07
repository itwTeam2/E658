using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WRMS.Models
{
    public class _VwPersonalProfile
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Civil_Status { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string F1250 { get; set; }
        public string Gender { get; set; }
        public string NIC { get; set; }
        public string Surname { get; set; }
        public string Rank { get; set; }
        public Nullable<int> RNKID { get; set; }
        public string ServiceNo { get; set; }
        public string SNo { get; set; }
        public string Service_Type { get; set; }
        public byte[] ProfilePicture { get; set; }
        public string PostedLocation { get; set; }
        public string Formation { get; set; }
        public string Initials { get; set; }
    }
}