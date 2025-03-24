using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E658.Models
{
    public class VMTrnsAuthIndex
    {
        public string CmpTrnsRefNo { get; set; }
        public string DgeAuthRefNo { get; set; }
        public string ReqSection { get; set; }
        public int ReqCategory { get; set; }
        public int IsAddiDutyReq { get; set; }
        public DateTime TrnsAuthCreatedDate { get; set; }
        public string DeptFrom { get; set; }
        public string DeptFromLocationFull { get; set; }
        public string DeptTo { get; set; }
        public string DeptToLocationFull { get; set; }
        public DateTime OutDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Purpose { get; set; }
        public string OMTStatus { get; set; }
        public string Rout { get; set; }
        public string OMTNo { get; set; }
        public string SLAFRegNo { get; set; }
        public string VehicleChassieNo { get; set; }
        public int IsNightPrkReq { get; set; }
        public string NightPrkLoc { get; set; }
        public DateTime DetailCreatedDate { get; set; }
        public int ETrnsCreatorDltId { get; set; }
        public string OMTSNo { get; set; }
        public string Rank { get; set; }
        public string Name { get; set; }
      
        public string PreviousSLAFRegNo { get; set; }
        public string VehicleType { get; set; }
        public string DivisionFullName { get; set; }
        public string reportedLoc { get; set; }
        
        public string UserGERMSLocation { get; set; }
        public string Message { get; set; }
        public string Comment { get; set; }
        public int RecordStatus { get; set; }
        public int RaisedTypeID { get; set; }
        public string TypeNameLong { get; set; }
        public string StafOffName { get; set; }
        public int IsNightPark { get; set; }
        public string NightParkStatus { get; set; }
        public string NightParkLoc { get; set; }
        public int Behavior { get; set; }
        public int RoleID { get; set; }
        public int EFTID { get; set; }
        
        public string HQAuthRemarks { get; set; }
        public string ReqCatName { get; set; }
        public string SOGORemark { get; set; }
        
    }
}