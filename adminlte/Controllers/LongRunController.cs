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
                    //Section = Userdetails.Section,


                };
                return View(model);
            }
            else
            {
                return RedirectToAction("TransportAuthIndex", "User");
            }

            
        }

        [HttpPost]
        public ActionResult CreateRequest(VME658Create objE658)
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


            List<Cls_ItemList> lst_ListPartItem = new List<Cls_ItemList>();

            try
            {
                if (Session["E658RunType"] != null && Session["E658CreateUser"] != null && Session["UserLoginLoc"] != null)
                {
                    var FromLocID = _db.Locations.Where(x => x.LocationName == objE658.FromLocID).Select(x => x.LocationID).FirstOrDefault();
                    var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();
                    //RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
                    string createUnitSerialNo = CreateUnitSerialNo(FromLocID, 2);

                    TimeSpan StartTime = objE658.JournryStartTime.TimeOfDay;

                    if (objE658.OMTServiceNo == null)
                    {
                        /// assing 1 // When user Request OMT From MT section isOMTReqFromMT coloum assign 1
                        isOMTReqFromMT = 1;
                        objE658.OMTServiceNo = "MT Sect";
                    }
                    else
                    {
                        isOMTReqFromMT = 0;
                    }

                    if (objE658.SLAFRegNo == null)
                    {
                        // When user Request Vehicle From MT section IsVehiReqFromMT coloum assign 10
                        IsVehiReqFromMT = 10;
                        string randomStringKey = GenerateRandomStringKey();
                        VehiChassisNo = randomStringKey + "/" + FromLocID.Trim();
                        vehicleAttaLoc = "Not Assign";
                        seqNo = randomStringKey + "/" + FromLocID.Trim()
                                + "/" + ToLocID.Trim() + "/" + DateTime.Now;
                    }
                    else
                    {
                        SLAFRegNumber = objE658.SLAFRegNo;

                        //var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo == objE658.SLAFRegNo && x.Status == 1).Select(x => x.ChassisNo).FirstOrDefault();



                        var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo.Contains(objE658.SLAFRegNo) && (x.Status == 1 || x.Status == 2 || x.Status == 3)).Select(x => new { x.ChassisNo, x.AttachedLocationID }).FirstOrDefault();

                        VehiChassisNo = sysChassisNo.ChassisNo;
                        vehicleAttaLoc = sysChassisNo.AttachedLocationID;
                        seqNo = sysChassisNo.ChassisNo + "/" + DateTime.Now;
                        IsVehiReqFromMT = 0;
                    }

                    if (objE658.ToLocId == "Other Location")
                    {

                    }

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
                        E658CreatedUser = Convert.ToInt64(Session["E658CreateUser"]),


                    };

                    _db.F658RegistryHeader.Add(RegHeader);

                    //E658RptLocHeader RptLocheader = new E658RptLocHeader()
                    //{

                    //    SeqID = seqNo,
                    //    ECDID = objE658.ECDID,
                    //    Active = 1,
                    //    CreatedDate = DateTime.Now,
                    //    CreatedBy = (Session["E658CreateUser"]).ToString(),
                    //    CreatedMAC = mac.GetMacAddress(),
                    //    CreatedIP = this.Request.UserHostAddress,
                    //};

                    //_db.E658RptLocHeader.Add(RptLocheader);

                    if (_db.SaveChanges() > 0)
                    {
                        //var RptLocHedID = _db.E658RptLocHeader.Where(x => x.SeqID == seqNo && x.Active == 1).Select(x => x.ERLID).FirstOrDefault();

                        //lst_ListPartItem = (List<Cls_ItemList>)Session["ListPartItem"];

                        //E658RptLocDetails locDetails = new E658RptLocDetails();

                        //foreach (var item in lst_ListPartItem)
                        //{
                        //    locDetails.LocationID = item.LocName;
                        //    locDetails.E658RptLocHedID = RptLocHedID;
                        //    locDetails.Active = 1;
                        //    locDetails.CreatedDate = DateTime.Now;
                        //    locDetails.CreatedBy = (Session["E658CreateUser"]).ToString();
                        //    locDetails.CreatedIP = this.Request.UserHostAddress;
                        //    locDetails.CreatedMAC = mac.GetMacAddress();

                        //    _db.E658RptLocDetails.Add(locDetails);

                        //    if (_db.SaveChanges() > 0)
                        //    {


                        //    }
                        //};


                        //// Have to use this in later

                        //flowList = RecordFlowMgtId(RaisedTypeID, (int)E658.Enum.EnumE658UserType.E658CreateUser);

                        //foreach (var item in flowList)
                        //{
                        //    EFID = item.EFMID;
                        //    RID = item.RoleID;
                        //}


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

                        E658CreaterDetails obj = _db.E658CreaterDetails.Find(objE658.ECDID);
                        obj.IsE658Create = 1;
                        _db.Entry(obj).State = EntityState.Modified;

                        if (_db.SaveChanges() > 0)
                        {

                        }

                        //message = "You have created the E658 Successfully.";
                        Session["ListPartItem"] = null;
                        TempData["ScfMsg"] = "You have created the E658 Successfully.";
                    }
                    //return RedirectToAction("E658Details", "E658", new { E658CreatorID = objE658.ECDID } );

                    //// Value Hasing Area

                    var hashingService = new HashingService();

                    string encodedId = hashingService.EncodeMultipleValues(objE658.ECDID, 0, 0);

                    return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { userID = encodedId }) });

                    //return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { E658CreatorID = objE658.ECDID, RoleId = 0, EFlowId = 0 }) });
                }
                else
                {
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        private string CreateUnitSerialNo(string FromLocID, int E658RunType)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/07
            /// Description : Create Unit Serial No for the run

            string unitSerialNo = "";
            try
            {
                int currentyear = Convert.ToInt32(DateTime.Now.Year);
                int currentDate = Convert.ToInt32(DateTime.Now.Day);
                var typeShort = _db.E658RaisedType.Where(x => x.RTID == E658RunType && x.Active == 1).Select(x => x.TypeShortName).FirstOrDefault();

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