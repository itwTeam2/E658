using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E658.Models
{
    public class VMLongRunCreate
    {
        public string UnitSerialNo { get; set; }
        public string FromLocID { get; set; }
        public string ToLocId { get; set; }
        public string OtherLocName { get; set; }
        public string Purpose { get; set; }
        public string Route { get; set; }
        public string E658RunType { get; set; }
        public int ERTID { get; set; }
        public int ECDID { get; set; }
        public int EFTID { get; set; }
        public int IsOMTAvail { get; set; }
        public int? IsCombineRun { get; set; }
        public int RoleID { get; set; }
        public string OMTStatus { get; set; }
        public int IsVehicleAvail { get; set; }
        public string VehicleStatus { get; set; }
        public string TypeName { get; set; }
        public DateTime E658Date { get; set; }
        public DateTime JournryStartTime { get; set; }
        public string RequiredDuration { get; set; }
        public string OMTServiceNo { get; set; }
        public string PreviousOMTServiceNo { get; set; }
        public string OMTSNo { get; set; }
        public string Rank { get; set; }
        public string Name { get; set; }
        public string SLAFRegNo { get; set; }
        public string PreviousSLAFRegNo { get; set; }
        public string VehicleType { get; set; }
        public string DivisionFullName { get; set; }
        public string reportedLoc { get; set; }
        public DateTime ReturnDate { get; set; }
        public string UserGERMSLocation { get; set; }
        public string Message { get; set; }
        public string Comment { get; set; }
        public int RecordStatus { get; set; }
        public int RaisedTypeID { get; set; }
        public string TypeNameLong { get; set; }
        public string StafOffName { get; set; }
        public string AdditionalDuties { get; set; }
        public string CreatorServiceNo { get; set; }
        public string CreatorLocation { get; set; }

    }
}