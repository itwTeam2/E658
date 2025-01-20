using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E658.Models
{
    public class VME658InitiateUser
    {
        [Required(ErrorMessage = "Please Enter Service")]
        public string ServiceNo { get; set; }
        public string StaffServiceNo { get; set; }
        public string SNo { get; set; }
        public string StaffSNo { get; set; }
        public string Rank { get; set; }
        public string StaffRank { get; set; }
        public string StaffFullName { get; set; }
        public string FullName { get; set; }
        public string Trade { get; set; }
        [Required(ErrorMessage = "Please Select the E658 Run Type")]
        public int RTID { get; set; }
        [Required(ErrorMessage = "Please Select the User Location")]
        public string UserLoginLocation { get; set; }
        public string FromLocID { get; set; }
        public string ToLocId { get; set; }
        public string LocationId { get; set; }
        public string JournryStartTime { get; set; }
        public string RequiredDuration { get; set; }
        public string DivisionId { get; set; }
        [Required(ErrorMessage = "Please Select the User Location")]
        public SelectList UserLocations { get; set; }
        public string SelectedUserLocation { get; set; }
        public string Route { get; set; }

        public DateTime? MTCotrollerDutyDate { get; set; }
        public string UserLocation { get; set; }

    }
}