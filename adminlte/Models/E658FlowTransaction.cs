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
    
    public partial class E658FlowTransaction
    {
        public int EFTID { get; set; }
        public Nullable<int> EFlowMgtID { get; set; }
        public Nullable<int> RecordStatusID { get; set; }
        public Nullable<int> E658CreatorDltID { get; set; }
        public Nullable<int> RoleID { get; set; }
        public string RecordLocID { get; set; }
        public string Comment { get; set; }
        public Nullable<int> Active { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedIP { get; set; }
        public string CreatedMAC { get; set; }
    }
}
