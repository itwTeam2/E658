//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace E658.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class E658FlowMagt
    {
        public int EFMID { get; set; }
        public Nullable<int> RoleID { get; set; }
        public Nullable<int> RejectStatus { get; set; }
        public Nullable<int> CurrentStatus { get; set; }
        public Nullable<int> SubmitStatus { get; set; }
        public Nullable<int> RaisedTyId { get; set; }
        public string RecordGroup { get; set; }
        public Nullable<int> Active { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedIP { get; set; }
        public string CreatedMAC { get; set; }
    }
}
