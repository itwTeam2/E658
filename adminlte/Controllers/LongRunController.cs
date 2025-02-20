using E658.HelperServices;
using E658.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E658.Controllers
{
    public class LongRunController : Controller
    {
        private dbContext _db = new dbContext();
        private dbContextCommonData _dbCommonData = new dbContextCommonData();
        private static readonly Random _random = new Random();
        List<E658FlowMagt> flowList = new List<E658FlowMagt>();
        MacAddress mac = new MacAddress();

        // GET: LongRun
        public ActionResult Index()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/22
            /// Description : load E658 Creater View
            return View();
        }

        [HttpGet]
        public ActionResult CreateRequest()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2025/01/18
            /// Description : transport authority requesting form

            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

            //ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
            //                              join rt in _db.E658RaisedType
            //                              on rrt.RaisedTypeID equals rt.RTID
            //                              join runt in _db.E658RunsTypes
            //                              on rrt.RunsTypeID equals runt.ERTID
            //                              where rrt.RaisedTypeID == RaisedTypeID && rrt.Active == 1
            //                              select new SelectListItem
            //                              {
            //                                  Value = runt.ERTID.ToString(),
            //                                  Text = runt.TypeName
            //                              }).ToList();

            var creatorDetailsJson = Session["CretorDetails"] as string;

            if (!string.IsNullOrEmpty(creatorDetailsJson))
            {
                var objCreateDetails = JsonConvert.DeserializeObject<VMLongRunCreate>(creatorDetailsJson);

                var creatorLocFullName = _db.Locations.Where(x => x.LocationID == objCreateDetails.CreatorLocation).Select(x => x.LocationName).FirstOrDefault();

                var model = new VMLongRunCreate
                {
                   
                    FromLocID = creatorLocFullName,
                    SectionName = objCreateDetails.SectionName,
                    IsRaisedMode = objCreateDetails.IsRaisedMode,
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("TransportAuthIndex", "User");
            }

            
        }

        [HttpPost]
        public ActionResult CreateRequest(VMLongRunCreate objE658)  
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2025/01/18
            /// Description : transport authority requesting form

            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

            string seqNo = "";
            string VehiChassisNo = "";
            int isOMTReqFromMT = 0;
            int IsVehiReqFromMT = 0;
            string SLAFRegNumber = null;
            string vehicleAttaLoc = "";
            //string recordCreateBy =

            List<Cls_ItemList> lst_ListPartItem = new List<Cls_ItemList>();

            try
            {
               

                var FromLocID = _db.Locations.Where(x => x.LocationName == objE658.FromLocID).Select(x => x.LocationID).FirstOrDefault();
                var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();
                //RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
                string createUnitSerialNo = CreateUnitSerialNo(FromLocID, objE658.IsVehicleAvail,objE658.SectionName);

                TimeSpan StartTime = objE658.JournryStartTime.TimeOfDay;

                var creatorDetailsJson = Session["CretorDetails"] as string;

                if (string.IsNullOrEmpty(creatorDetailsJson))
                {
                    return RedirectToAction("Index", "User"); // Adjust "Account" and "Login" to your actual controller and action names
                }
                var objCreateDetails = JsonConvert.DeserializeObject<VMLongRunCreate>(creatorDetailsJson);


                Session["E658CreateUser"] = objCreateDetails.Sno;
                //if (objE658.OMTServiceNo == null)
                //{
                //    /// assing 1 // When user Request OMT From MT section isOMTReqFromMT coloum assign 1
                //    isOMTReqFromMT = 1;
                //    objE658.OMTServiceNo = "MT Sect";
                //}
                //else
                //{

                //}


                //if (objE658.SLAFRegNo == null)
                //{
                //    // When user Request Vehicle From MT section IsVehiReqFromMT coloum assign 10
                //    IsVehiReqFromMT = 10;
                //    string randomStringKey = GenerateRandomStringKey();
                //    VehiChassisNo = randomStringKey + "/" + FromLocID.Trim();
                //    vehicleAttaLoc = "Not Assign";
                //    seqNo = randomStringKey + "/" + FromLocID.Trim()
                //            + "/" + ToLocID.Trim() + "/" + DateTime.Now;
                //}
                //else
                //{

                //}
                isOMTReqFromMT = 0;
                SLAFRegNumber = objE658.SLAFRegNo;

                //var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo == objE658.SLAFRegNo && x.Status == 1).Select(x => x.ChassisNo).FirstOrDefault();

                var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo.Contains(objE658.SLAFRegNo) && (x.Status == 1 || x.Status == 2 || x.Status == 3)).Select(x => new { x.ChassisNo, x.AttachedLocationID }).FirstOrDefault();

                VehiChassisNo = sysChassisNo.ChassisNo;
                vehicleAttaLoc = sysChassisNo.AttachedLocationID;
                seqNo = sysChassisNo.ChassisNo + "/" + DateTime.Now;
                IsVehiReqFromMT = 0;

                E658CreaterDetails objCreater = new E658CreaterDetails
                {
                    SNo = objCreateDetails.Sno,
                    RaisedTypeID = (int)E658.Enum.E658RaisedType.TA,
                    CreatedDate = DateTime.Now,
                    CreatedBy = objCreateDetails.Sno,
                    UserGERMSLocation = objCreateDetails.CreatorLocation.Trim(),
                    Active = 1,
                    CreatedIP = this.Request.UserHostAddress,
                    IsE658Create = 1,
                    CreatedMAC = mac.GetMacAddress(),
                };


                _db.E658CreaterDetails.Add(objCreater);
                if (_db.SaveChanges() > 0)
                {
                    objE658.ECDID = objCreater.ECDID;

                    F658RegistryHeader RegHeader = new F658RegistryHeader()
                    {
                        ChassisNo = VehiChassisNo,
                        Seq = seqNo,
                        UnitSerialNo = createUnitSerialNo,
                        FLocation = FromLocID,
                        ELocation = FromLocID,
                        //TLocation = ToLocID.Trim(),
                        TLocation = ToLocID.Trim(),
                        RealToLocation = (objE658.ToLocId == "Other Location") ? objE658.OtherLocName : ToLocID.Trim(),
                        E658CreatorDltId = objE658.ECDID,
                        PLocation = vehicleAttaLoc,
                        IsRR658 = 2,
                        Behavior = objE658.ERTID,
                        IsOMtReqFromMT = isOMTReqFromMT,
                        OMTNo = objE658.OMTServiceNo,//OMServiceNo,
                        SLAFRegNo = SLAFRegNumber,
                        IsVehicleReqFromMT = IsVehiReqFromMT,
                        Duty = objE658.Purpose,
                        AuthorityType = 1,
                        PDate = objE658.E658Date,
                        ReturnDate = objE658.ReturnDate,
                        PTime = StartTime.ToString(),
                        Route = objE658.Route,
                        PHrs = objE658.RequiredDuration,
                        Status = 50,
                        Active = 1,
                        CreatedDate = DateTime.Now,
                        E658CreatedUser = Convert.ToInt64(objCreateDetails.Sno),
                        IsNightPark = objE658.IsNightPark,
                        Description = objE658.AdditionalDuties,
                        NightParkLoc = (objE658.IsNightPark == 1)?  objE658.NightParkLoc: null,


                    };

                    _db.F658RegistryHeader.Add(RegHeader);

                    E658FlowTransaction FlowTranc = new E658FlowTransaction()
                    {
                        //EFlowMgtID = EFID,
                        RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward,
                        RecordLocID = FromLocID,
                        E658CreatorDltID = objE658.ECDID,
                        //RoleID = RID,
                        Active = 1,
                        CreatedDate = DateTime.Now,
                        CreatedBy = (Session["E658CreateUser"]).ToString(),
                        CreatedIP = this.Request.UserHostAddress,
                        CreatedMAC = mac.GetMacAddress()

                    };
                    _db.E658FlowTransaction.Add(FlowTranc);

                    _db.SaveChanges();
                    

                    TempData["ScfMsg"] = "You have created the E658 Successfully.";

                }        

                var hashingService = new HashingService();

                string encodedId = hashingService.EncodeMultipleValues(objE658.ECDID, 0, 0);

                return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { userID = encodedId }) });

             }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        public ActionResult GenerateTrans658()
        {
            return View();
        }
        private string CreateUnitSerialNo(string FromLocID, int IsVehicleAvail,string SectionName)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/07
            /// Description : Create Unit Serial No for the run

            string unitSerialNo = "";
            try
            {
                int currentyear = Convert.ToInt32(DateTime.Now.Year);
                int currentMonth = Convert.ToInt32(DateTime.Now.Month);
                int currentDate = Convert.ToInt32(DateTime.Now.Day);
                var typeShort = _db.E658CreaterDetails.Where(x => x.RaisedTypeID == (int)E658.Enum.E658RaisedType.TA
                                && x.Active == 1
                                && x.CreatedDate.HasValue
                                && x.CreatedDate.Value.Year == currentyear
                                && x.CreatedDate.Value.Month == currentMonth
                                && x.CreatedDate.Value.Day == currentDate).Count();

                int Count658 = _db.F658RegistryHeader.Where(x => x.ELocation == FromLocID && x.CreatedDate.Value.Year == currentyear && x.Active == 1).Count();
                int RocordId = Count658 + 1;

                unitSerialNo = FromLocID.Trim() + currentDate + RocordId;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return unitSerialNo;
        }

        private static string GenerateRandomStringKey()
        {
            string randomNumber = "";

            // Generate each digit individually
            for (int i = 0; i < 20; i++)
            {
                randomNumber += _random.Next(0, 10); // Generate a random digit between 0 and 9
            }

            return randomNumber;

        }

        public List<E658FlowMagt> RecordFlowMgtId(int RaisedTypeID, int roleId)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/25
            /// Description : load E658 Creater View
            /// Get the submit status table ID 

            List<E658FlowMagt> flowList = new List<E658FlowMagt>();
            try
            {
                var SubmitStatus = _db.E658FlowMagt.Where(x => x.RaisedTyId == RaisedTypeID && x.RoleID == roleId && x.Active == 1).Select(x => x.SubmitStatus).FirstOrDefault();

                var EFlowMgtId = _db.E658FlowMagt.Where(x => x.RoleID == SubmitStatus && x.Active == 1).Select(x => new { x.EFMID, x.RoleID }).FirstOrDefault();

                E658FlowMagt obj = new E658FlowMagt()
                {
                    RoleID = EFlowMgtId.RoleID,
                    EFMID = EFlowMgtId.EFMID,
                };

                flowList.Add(obj);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return flowList;
        }
    }
}