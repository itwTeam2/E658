using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace E658.Models
{
    public class _UserManagementE658
    {
        public int UMID { get; set; }
        public string SNo { get; set; }
        public Nullable<int> EUMID { get; set; }
        
        public string FullName { get; set; }
        [Required(ErrorMessage = "Enter Service No")]
        public string ServiceNo { get; set; }
        public string HandOverServiceNo { get; set; }
        public string HandOverRank { get; set; }
        public string HandOverName { get; set; }
        public string HandOverSNo { get; set; }
        public int TempHandOverStatus{ get; set; }
        public string Rank { get; set; }
        public Nullable<int> RLEID { get; set; }
        [Required(ErrorMessage = "Select Section")]
        public string Section { get; set; }
        [Required(ErrorMessage = "Select Location")]
        public string Location { get; set; }
        public string EditLocation { get; set; }
        public string UserRole { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> Active { get; set; }
        public string IPAddress { get; set; }
        public DateTime HandOverDateFrom { get; set; }
        public DateTime HandOverDateTo { get; set; }
        public int? RoleId { get; set; }
        public string Division  { get; set; }
        public DateTime? DutyDate { get; set; }
        public bool isTempHandover { get; set; }
        

    }
}