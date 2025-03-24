using E658.HelperServices;
using E658.Models;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        
        int EFID = 0;
        int? RID = 0;
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

            ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
                                          join rt in _db.E658RaisedType
                                          on rrt.RaisedTypeID equals rt.RTID
                                          join runt in _db.E658RunsTypes
                                          on rrt.RunsTypeID equals runt.ERTID
                                          where rrt.RaisedTypeID == (int)E658.Enum.E658RaisedType.TA && rrt.Active == 1
                                          select new SelectListItem
                                          {
                                              Value = runt.ERTID.ToString(),
                                              Text = runt.TypeName
                                          }).ToList();

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
                    OICSNo  = objCreateDetails.OICSNo,
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("TransportAuthIndex", "User");
            }

            
        }
        [HttpPost]
        public async Task<ActionResult> CreateRequest(VMLongRunCreate objE658)
        {
            // Created BY   : Sqn Ldr Wicky
            // Create Date  : 2025/01/18
            // Description  : Transport authority requesting form

            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

            try
            {
                // Validate session
                var creatorDetailsJson = Session["CretorDetails"] as string;
                if (string.IsNullOrEmpty(creatorDetailsJson))
                    return RedirectToAction("Index", "User");

                var objCreateDetails = JsonConvert.DeserializeObject<VMLongRunCreate>(creatorDetailsJson);
                Session["E658CreateUser"] = objCreateDetails.Sno;

                string fromLocID = _db.Locations
                    .Where(x => x.LocationName == objE658.FromLocID)
                    .Select(x => x.LocationID)
                    .FirstOrDefault();

                string toLocID = _db.Locations
                    .Where(x => x.LocationName == objE658.ToLocId)
                    .Select(x => x.LocationID)
                    .FirstOrDefault();

                string createUnitSerialNo = CreateUnitSerialNo(fromLocID, objE658.IsRaisedMode, objE658.SectionName);
                string slafRegNumber = objE658.SLAFRegNo;

                // Fetch vehicle details
                var vehicle = _db.VehicleDetails
                    .Where(x => x.SlafRegNo.Contains(slafRegNumber) && (x.Status == 1 || x.Status == 2 || x.Status == 3))
                    .Select(x => new { x.ChassisNo, x.AttachedLocationID })
                    .FirstOrDefault();

                if (vehicle == null)
                    return Json(new { success = false, message = "Vehicle not found or inactive." });

                string chassisNo = vehicle.ChassisNo;

                using (var dbContextTransaction = _db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    // Creator Details
                    var creator = new E658CreaterDetails
                    {
                        SNo = objCreateDetails.Sno,
                        RaisedTypeID = (int)E658.Enum.E658RaisedType.TA,
                        CreatedDate = DateTime.Now,
                        CreatedBy = objCreateDetails.Sno,
                        UserGERMSLocation = objCreateDetails.CreatorLocation?.Trim(),
                        UserGERMSSection = objCreateDetails.SectionName,
                        IsBaseOfFormation = objE658.IsRaisedMode,
                        OICServiceNo = objE658.OICSNo,
                        Active = 1,
                        CreatedIP = Request.UserHostAddress,
                        IsE658Create = 1,
                        CreatedMAC = mac.GetMacAddress(),
                    };
                    _db.E658CreaterDetails.Add(creator);
                    await _db.SaveChangesAsync().ConfigureAwait(false);

                    // Header
                    var transHeader = new ETransReqHeader
                    {
                        CmpTrnsRefNo = createUnitSerialNo,
                        ReqSection = objE658.SectionName,
                        ReqCategory = objE658.ERTID,
                        IsAddiDutyReq = Convert.ToInt16(objE658.IsAddiDutyReq),
                        ETrnsCreatorDltId = creator.ECDID,
                        Active = 1,
                        CreatedDate = DateTime.Now,
                        CreatedBy = objCreateDetails.Sno.ToString(),
                        CreatedIP = Request.UserHostAddress,
                        CreatedMAC = mac.GetMacAddress()
                    };
                    _db.ETransReqHeaders.Add(transHeader); 
                    await _db.SaveChangesAsync().ConfigureAwait(false);
                    
                    // Detail
                    var transDetails = new ETransReqDetail
                    {
                        ETrnsHedID = transHeader.TAHID,
                        DeptFrom = fromLocID,
                        DeptTo = toLocID.ToString(),
                        DeptDate = objE658.E658Date,
                        ReturnDate = objE658.ReturnDate,
                        DeptTime = objE658.JournryStartTime.TimeOfDay.ToString(),
                        Purpose = objE658.Purpose,
                        Rout = objE658.Route,
                        OMTSvcNo = objE658.OMTServiceNo,
                        SLAFRegNo = slafRegNumber,
                        VehicleChassieNo = chassisNo,
                        IsNightPrkReq = Convert.ToInt16(objE658.IsNightPark),
                        NightPrkLoc = objE658.NightParkLoc,
                        CreatedDate = DateTime.Now,
                        CreatedBy = objCreateDetails.Sno.ToString(),
                        CreatedIP = Request.UserHostAddress,
                        CreatedMAC = mac.GetMacAddress(),
                        Active = 1
                    };
                    _db.ETransReqDetails.Add(transDetails);
                    await _db.SaveChangesAsync().ConfigureAwait(false);
                    
                    // Additional Duties if requested
                    if (objE658.IsAddiDutyReq == 1 && objE658.DutyList != null)
                    {
                        foreach (var duty in objE658.DutyList)
                        {
                            var addiDuty = new ETransAddiDuty
                            {
                                AddiDutyDate = Convert.ToDateTime(duty.Date),
                                AddiDutyRout = duty.Route,
                                AddiDutyPurpose = duty.Purpose,
                                ETrnsHedID = transHeader.TAHID,
                                Active = 1,
                                CreatedDate = DateTime.Now,
                                CreatedBy = objCreateDetails.Sno.ToString(),
                                CreatedIP = Request.UserHostAddress,
                                CreatedMAC = mac.GetMacAddress()
                            };
                            _db.ETransAddiDuties.Add(addiDuty);
                        }
                        await _db.SaveChangesAsync().ConfigureAwait(false);
                        
                    }

                    // Flow Management
                    var flowList = RecordFlowMgtId((int)E658.Enum.E658RaisedType.TA, (int)E658.Enum.EnumE658UserType.LngRunRequest, objE658.ERTID);
                    foreach (var item in flowList)
                    {
                        var flowTransaction = new E658FlowTransaction
                        {
                            EFlowMgtID = item.EFMID,
                            RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward,
                            RecordLocID = fromLocID,
                            E658CreatorDltID = creator.ECDID,
                            RoleID = item.RoleID,
                            Active = 1,
                            CreatedDate = DateTime.Now,
                            CreatedBy = objCreateDetails.Sno.ToString(),
                            CreatedIP = Request.UserHostAddress,
                            CreatedMAC = mac.GetMacAddress()
                        };
                        _db.E658FlowTransaction.Add(flowTransaction);
                    }

                    await _db.SaveChangesAsync().ConfigureAwait(false);
                    dbContextTransaction.Commit();

                    TempData["ScfMsg"] = "You have created the E658 Successfully.";

                    // Redirect with encoded ID
                    string encodedId = new HashingService().EncodeMultipleValues(creator.ECDID, 0, 0);
                    return Json(new { success = true, redirectUrl = Url.Action("ETransReqDetails", new { userID = encodedId }) });
                }
            }
            catch (Exception ex)
            {
                // Consider logging the error properly
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]
        public ActionResult ETransReqDetails(string userID)
        {
            /// Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/22
            /// Description : View E658 Details
            
            DataTable dt = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();           
            //string ToLocID = "";

            var hashingService = new HashingService();

            // Decode the hashId back to the original string
            string decodedString = hashingService.DecodeHashId(userID);

            // Split the decoded string to retrieve the original values
            var splitValues = decodedString.Split(':');
            int creatorId = int.Parse(splitValues[0]);
            int roleId = int.Parse(splitValues[1]);
            int eFlowId = int.Parse(splitValues[2]);

            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CalleTranReqDetailsSP(creatorId);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    VME658Create objVME658Create = new VME658Create();

                    //FromLocID = dt.Rows[i]["FLocation"].ToString();
                    //ToLocID = (dt.Rows[i]["TLocation"].ToString() == "OTR") ? dt.Rows[i]["RealToLocation"].ToString() : dt.Rows[i]["TLocation"].ToString();

                    objVME658Create.FromLocID = dt.Rows[i]["FromLocationFull"].ToString(); //_db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();
                    objVME658Create.ToLocId = dt.Rows[i]["ToLocationFull"].ToString(); //ToLocID;
                    objVME658Create.UnitSerialNo = dt.Rows[i]["CmpTrnsRefNo"].ToString();
                    objVME658Create.E658RunType = dt.Rows[i]["TypeName"].ToString();
                    objVME658Create.E658Date = Convert.ToDateTime(dt.Rows[i]["DeptDate"]);
                    objVME658Create.ReturnDate = Convert.ToDateTime(dt.Rows[i]["ReturnDate"]);
                    objVME658Create.JournryStartTime = Convert.ToDateTime(dt.Rows[i]["DeptTime"]);
                    //objVME658Create.RequiredDuration = dt.Rows[i]["PHrs"].ToString();
                    objVME658Create.Purpose = dt.Rows[i]["Purpose"].ToString();
                    objVME658Create.Route = dt.Rows[i]["Rout"].ToString();                 
                    objVME658Create.RoleID = roleId;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["ETrnsCreatorDltId"]);
                    objVME658Create.EFTID = eFlowId;
                    objVME658Create.RecordStatus = Convert.ToInt32(dt.Rows[i]["RecordStatus"]);
                    objVME658Create.TypeName = dt.Rows[i]["RunBehavior"].ToString();
                    objVME658Create.RaisedTypeID = Convert.ToInt32(dt.Rows[i]["RaisedTypeID"]);
                    objVME658Create.NightParkStatus = dt.Rows[i]["NightParkStatus"].ToString();
                    objVME658Create.OMTServiceNo = dt.Rows[i]["OMTSvcNo"].ToString();
                    objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    objVME658Create.IsNightPark = Convert.ToInt32(dt.Rows[i]["IsNightPrkReq"]);
                    objVME658Create.IsAddiDutyReq = Convert.ToInt32(dt.Rows[i]["IsAddiDutyReq"]);
                    objVME658Create.Behavior = Convert.ToInt32(dt.Rows[i]["ReqCategory"]);

                    int EtrnHdID = Convert.ToInt32(dt.Rows[i]["TAHID"]);

                    if (objVME658Create.IsNightPark == 1)
                    {
                        objVME658Create.NightParkLoc = dt.Rows[i]["NightPrkLoc"].ToString();
                    }

                    if (objVME658Create.IsAddiDutyReq == 1)
                    {
                        var addiDutyList = _db.ETransAddiDuties.Where(x => x.ETrnsHedID == EtrnHdID && x.Active == 1).ToList();

                        ViewBag.ETransAddiDuty = addiDutyList;
                    }



                    TempData["ECDID"] = creatorId;
                    TempData["UserLoginType"] = Session["UserLoginType"];                    

                    e658DetailsList.Add(objVME658Create);
                }               


                //PartialView("_E658ReportLoc", locations);


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(e658DetailsList);
        }
        public ActionResult GenerateTrans658()
        {
            return View();
        }
        private string CreateUnitSerialNo(string FromLocID, int IsRaisedMode, string SectionName)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2025/03/07
            /// Description : Create Unit Serial No for the run

            string unitSerialNo = "";
            try
            {
                DateTime currentDate = DateTime.Now;
                int currentYear = currentDate.Year;
                int currentMonth = currentDate.Month;
                int currentDay = currentDate.Day;

                var query = _db.E658CreaterDetails.Where(x => x.RaisedTypeID == (int)E658.Enum.E658RaisedType.TA
                                                              && x.Active == 1
                                                              && x.CreatedDate.HasValue
                                                              && x.CreatedDate.Value.Year == currentYear
                                                              && x.CreatedDate.Value.Month == currentMonth
                                                              && x.CreatedDate.Value.Day == currentDay
                                                              && x.UserGERMSLocation == FromLocID);

                if (IsRaisedMode != 1)
                {
                    query = query.Where(x => x.UserGERMSSection == SectionName && x.IsBaseOfFormation == 2);
                }
                else
                {
                    query = query.Where(x => x.IsBaseOfFormation == 1);
                }
                int recordCnt = query.Count() + 1;

                unitSerialNo = IsRaisedMode == 1
                    ? $"{FromLocID}/{currentDay}/{currentMonth}/{currentYear}/{recordCnt}"
                    : $"{SectionName}/{currentDay}/{currentMonth}/{currentYear}/{recordCnt}";
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // Example: Logger.LogError(ex);
                throw ex;
            }

            return unitSerialNo;
        }

        private string CreateDGEAuthorityNo(int E658CreatorDltId)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2025/03/24
            /// Description : Create the DGE Authority No when giving the final Authority number

            string dgeFinalAuthNo = "";
            try
            {
                DateTime currentDate = DateTime.Now;
                int currentYear = currentDate.Year;
                int currentMonth = currentDate.Month;
                int currentDay = currentDate.Day;

                var campAuthority = _db.ETransReqHeaders.Where(x => x.ETrnsCreatorDltId == E658CreatorDltId
                                                              && x.Active == 1).Select(x=>x.CmpTrnsRefNo).FirstOrDefault();


                 dgeFinalAuthNo = "DGE/" + campAuthority;
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // Example: Logger.LogError(ex);
                throw ex;
            }

            return dgeFinalAuthNo;
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
        public List<E658FlowMagt> RecordFlowMgtId(int RaisedTypeID, int roleId,int runType)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/25
            /// Description : load E658 Creater View
            /// Get the submit status table ID 

            List<E658FlowMagt> flowList = new List<E658FlowMagt>();
            try
            {
                var flowGroup = _db.E658RaisedRunType.Where(x => x.RunsTypeID == runType
                                && x.Active == 1 && x.RaisedTypeID == RaisedTypeID).Select(x => x.RecordGroup).FirstOrDefault();

                var SubmitStatus = _db.E658FlowMagt.Where(x => x.RaisedTyId == RaisedTypeID && x.RoleID == roleId 
                                   && x.Active == 1 && x.RecordGroup == flowGroup).Select(x => x.SubmitStatus).FirstOrDefault();

                var EFlowMgtId = _db.E658FlowMagt.Where(x => x.RoleID == SubmitStatus && x.RaisedTyId == RaisedTypeID  && x.Active == 1 && x.RecordGroup == flowGroup)
                               .Select(x => new { x.EFMID, x.RoleID }).FirstOrDefault();

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
        [HttpGet]
        public ActionResult TrnsPortReqList(string sortOrder, string currentFilter, string searchString, int? page, int? RSID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2025/03/04
            /// Description : Load Transport Request List to Different Users

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();
            List<VMTrnsAuthIndex> TrnsList = new List<VMTrnsAuthIndex>();

            int pageSize = 20;
            int pageNumber = page ?? 1;

            if (Session["UserLoginType"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
            int userLoginType2 = Convert.ToInt32(Session["UserLoginType"]);
            string MToOctLocation = Session["MToOctLocation"] != null ? Session["MToOctLocation"].ToString() : string.Empty;

            ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();

            if (userLoginType == (int)E658.Enum.EnumE658UserType.MTController)
            {
                userLoginType = (int)E658.Enum.EnumE658UserType.MToOCT;
            }

            dt = objDALCommanQuery.CallTrnsAuthIndexSP(userLoginType);

            var filteredRows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 &&
                x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).ToList();

            if (filteredRows.Any())  // Check if any rows match the condition
            {
                dt2 = filteredRows.CopyToDataTable();  // Copy the filtered rows to a new DataTable
            }
            else
            {
                // Handle the case when no rows match the condition
                dt2 = dt.Clone();  // Clone the structure of the original DataTable (empty but same schema)
                                   // Or, you can set dt2 to null or handle it according to your needs
                                   // dt2 = null;
            }

            if (dt2 != null && dt2.Rows.Count > 0)
            {
                dt3 = FilterDataTable(dt2, userLoginType, searchString, currentFilter, MToOctLocation);
                PopulateE658List(dt3, TrnsList);

                if (userLoginType == (int)E658.Enum.EnumE658UserType.MTController || userLoginType == (int)E658.Enum.EnumE658UserType.MToOCT)
                {
                    SetViewBagForMTController(dt3);
                    TempData["MTControllerStatus"] = userLoginType2 == (int)E658.Enum.EnumE658UserType.MTController ? "5" : "0";
                }
            }

            ViewBag.CurrentFilter = page >= 2 ? currentFilter : searchString;
            return View(TrnsList.ToPagedList(pageNumber, pageSize));

        }
        private DataTable FilterDataTable(DataTable dt, int userLoginType, string searchString, string currentFilter, string MToOctLocation)
        {
            string filterString = searchString ?? currentFilter;
            DataTable dt2 = new DataTable();
            if (string.IsNullOrEmpty(filterString))
            {

                switch (userLoginType)
                {
                    case (int)E658.Enum.EnumE658UserType.HQAppAuth:
                        return FilterByHQAppAuthUser(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.FormationUser:
                        return FilterByFormationUser(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.MToOCT:
                    case (int)E658.Enum.EnumE658UserType.MTController:
                        return FilterByMToOCT(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:
                        return FilterByFinalizedAuthorization(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.RunHold:
                        return FilterByRunHold(dt, filterString);
                    default:
                        return dt;
                }
            }
            else
            {
                switch (userLoginType)
                {
                    case (int)E658.Enum.EnumE658UserType.HQAppAuth:
                        return FilterByHQAppAuthUser(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.FormationUser:
                        return FilterByFormationUser(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.MToOCT:
                    case (int)E658.Enum.EnumE658UserType.MTController:
                        return FilterByMToOCT(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:
                        return FilterByFinalizedAuthorization(dt, filterString);
                    case (int)E658.Enum.EnumE658UserType.RunHold:
                        return FilterByRunHold(dt, filterString);
                    default:
                        return dt;
                }

            }
        }
        private DataTable FilterByHQAppAuthUser(DataTable dt, string filterString)
        {
            string staffServiceNo = Session["LoginUser"].ToString();

            IEnumerable<DataRow> rows; // Define rows outside the if-else block

            if (filterString == null)
            {
                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 &&
                                           x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);
            }
            else
            {
                DateTime filterDate;
                if (!DateTime.TryParse(filterString, out filterDate))
                {
                    return new DataTable(); // Return empty table if date conversion fails
                }

                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 &&
                                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward &&
                                                    x.Field<DateTime>("OutDate") == filterDate);
            }


            return rows.Any() ? rows.CopyToDataTable() : new DataTable();
        }
        private DataTable FilterByFormationUser(DataTable dt, string filterString)
        {
            
            string loginuser = Session["LoginUser"].ToString();
            var loginUserSno = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == loginuser).Select(x => x.SNo).FirstOrDefault();
            IEnumerable<DataRow> rows; // Define rows outside the if-else block

            if (filterString == null)
            {
                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 &&
                                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);

                //rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("OICSNo") == loginUserSno &&
                //                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);
            }
            else
            {
                DateTime filterDate;
                if (!DateTime.TryParse(filterString, out filterDate))
                {
                    return new DataTable(); // Return empty table if date conversion fails
                }

                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("OICSNo") == loginUserSno &&
                                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward &&
                                                    x.Field<DateTime>("OutDate") == filterDate);
            }

            return rows.Any() ? rows.CopyToDataTable() : new DataTable();
        }
        private DataTable FilterByMToOCT(DataTable dt, string filterString)
        {
            string mToOctLocation = Session["MToOctLocation"].ToString();

            IEnumerable<DataRow> rows;

            if (filterString == null)
            {
                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);
            }
            else
            {
                DateTime filterDate;
                if (!DateTime.TryParse(filterString, out filterDate))
                {
                    return new DataTable(); // Return empty table if date conversion fails
                }

                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward &&
                                                    x.Field<DateTime>("OutDate") == filterDate);
            }

            return rows.Any() ? rows.CopyToDataTable() : new DataTable();
        }
        private DataTable FilterByFinalizedAuthorization(DataTable dt, string filterString)
        {
            string mToOctLocation = Session["MToOctLocation"].ToString();

            IEnumerable<DataRow> rows;

            if (filterString == null)
            {
                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                  x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward &&
                                                  x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization);
            }
            else
            {
                DateTime filterDate;
                if (!DateTime.TryParse(filterString, out filterDate))
                {
                    return new DataTable(); // Return empty table if date conversion fails
                }

                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                   x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward &&
                                                   x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization &&
                                                   x.Field<DateTime>("OutDate") == filterDate);
            }


            return rows.Any() ? rows.CopyToDataTable() : new DataTable();
        }
        private DataTable FilterByRunHold(DataTable dt, string filterString)
        {
            string mToOctLocation = Session["MToOctLocation"].ToString();

            IEnumerable<DataRow> rows;

            if (filterString == null)
            {
                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                  x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.HoldRun &&
                                                  x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.RunHold);
            }
            else
            {
                DateTime filterDate;
                if (!DateTime.TryParse(filterString, out filterDate))
                {
                    return new DataTable(); // Return empty table if date conversion fails
                }

                rows = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == mToOctLocation &&
                                                   x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.HoldRun &&
                                                   x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.RunHold &&
                                                   x.Field<DateTime>("OutDate") == filterDate);
            }

            return rows.Any() ? rows.CopyToDataTable() : new DataTable();
        }
        private void PopulateE658List(DataTable dt2, List<VMTrnsAuthIndex> TrnsList)
        {
            //foreach (DataRow row in dt2.Rows)
            //{
            //    //VME658Create objList = new VME658Create
            //    //{
            //    //    UnitSerialNo = row["UnitSerialNo"].ToString(),
            //    //    FromLocID = row["FromLocationFull"].ToString(),
            //    //    ToLocId = row["ToLocationFull"].ToString(),
            //    //    E658Date = Convert.ToDateTime(row["PDate"]),
            //    //    JournryStartTime = Convert.ToDateTime(row["PTime"]),
            //    //    E658RunType = row["TypeName"].ToString(),
            //    //    ECDID = Convert.ToInt32(row["E658CreatorDltId"]),
            //    //    RoleID = Convert.ToInt32(row["RoleID"]),
            //    //    UserGERMSLocation = row["UserGERMSLocation"].ToString(),
            //    //    IsOMTAvail = Convert.ToInt32(row["IsOMtReqFromMT"]),
            //    //    IsVehicleAvail = Convert.ToInt32(row["IsVehicleReqFromMT"]),
            //    //    EFTID = Convert.ToInt32(row["EFTID"]),
            //    //    RaisedTypeID = Convert.ToInt32(row["RaisedTypeID"]),
            //    //    TypeNameLong = row["TypeNameLong"].ToString(),
            //    //    DivisionFullName = row.Table.Columns.Contains("CreaterDivision") ? row["CreaterDivision"].ToString() : null,
            //    //    IsCombineRun = row.Table.Columns.Contains("IsCombineRun") ? Convert.ToInt32(row["IsCombineRun"]) : (int?)null
            //    //};
            //    VME658Create objList = new VME658Create();
            //    objList.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
            //    objList.FromLocID = dt2.Rows[i]["FromLocationFull"].ToString();
            //    objList.ToLocId = dt2.Rows[i]["ToLocationFull"].ToString();
            //    objList.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
            //    objList.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
            //    objList.E658RunType = dt2.Rows[i]["TypeName"].ToString();
            //    objList.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);
            //    objList.RoleID = Convert.ToInt32(dt2.Rows[i]["RoleID"]);
            //    objList.UserGERMSLocation = dt2.Rows[i]["UserGERMSLocation"].ToString();
            //    objList.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
            //    objList.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
            //    objList.EFTID = Convert.ToInt32(dt.Rows[i]["EFTID"]);
            //    objList.RaisedTypeID = Convert.ToInt32(dt.Rows[i]["RaisedTypeID"]);
            //    objList.TypeNameLong = dt.Rows[i]["TypeNameLong"].ToString();
            //    if (objList.RaisedTypeID == (int)E658.Enum.E658RaisedType.FormationE || objList.RaisedTypeID == (int)E658.Enum.E658RaisedType.MTSecE)
            //    {

            //        objList.DivisionFullName = dt2.Rows[i]["CreaterDivision"].ToString();
            //    }

            //    if (!Convert.IsDBNull(dt2.Rows[i]["IsCombineRun"]))
            //    {
            //        objList.IsCombineRun = Convert.ToInt32(dt2.Rows[i]["IsCombineRun"]);
            //    }


            //    E658List.Add(objList);
            //    //E658List.Add(objList);
            //}

            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                VMTrnsAuthIndex objList = new VMTrnsAuthIndex();
                objList.CmpTrnsRefNo = dt2.Rows[i]["CmpTrnsRefNo"].ToString();
                objList.ReqSection = dt2.Rows[i]["ReqSection"].ToString();
                objList.ReqCategory = Convert.ToInt32(dt2.Rows[i]["ReqCategory"]);
                objList.IsAddiDutyReq = Convert.ToInt32(dt2.Rows[i]["IsAddiDutyReq"]);
                objList.TrnsAuthCreatedDate = Convert.ToDateTime(dt2.Rows[i]["TrnsAuthCreatedDate"]);
                objList.DeptFrom = dt2.Rows[i]["DeptFrom"].ToString();
                objList.DeptFromLocationFull = dt2.Rows[i]["DeptFromLocationFull"].ToString();
                objList.DeptTo = dt2.Rows[i]["DeptTo"].ToString();
                objList.DeptToLocationFull = dt2.Rows[i]["DeptToLocationFull"].ToString();
                objList.OutDate = Convert.ToDateTime(dt2.Rows[i]["OutDate"]);
                objList.ReturnDate = Convert.ToDateTime(dt2.Rows[i]["ReturnDate"]);
                objList.Purpose = dt2.Rows[i]["Purpose"].ToString();
                objList.Rout = dt2.Rows[i]["Rout"].ToString();
                objList.OMTNo = dt2.Rows[i]["OMTNo"].ToString(); 
                objList.SLAFRegNo = dt2.Rows[i]["SLAFRegNo"].ToString();
                objList.VehicleChassieNo = dt2.Rows[i]["VehicleChassieNo"].ToString();
                objList.IsNightPrkReq = Convert.ToInt32(dt2.Rows[i]["IsNightPrkReq"]);
                objList.NightPrkLoc = dt2.Rows[i]["NightPrkLoc"].ToString();
                objList.DetailCreatedDate = Convert.ToDateTime(dt2.Rows[i]["DetailCreatedDate"]);
                objList.ETrnsCreatorDltId = Convert.ToInt32(dt2.Rows[i]["ETrnsCreatorDltId"]); 
                objList.RoleID = Convert.ToInt32(dt2.Rows[i]["RoleID"]);
                objList.EFTID = Convert.ToInt32(dt2.Rows[i]["EFTID"]); 
                objList.ReqCatName = dt2.Rows[i]["ReqCatName"].ToString();
                //objList.ToLocId = (dt2.Rows[i]["TLocation"].ToString() == "OTR") ? dt2.Rows[i]["RealToLocation"].ToString() : dt2.Rows[i]["ToLocationFull"].ToString(); //dt2.Rows[i]["ToLocationFull"].ToString();

                TrnsList.Add(objList);

            }
        }
        private void SetViewBagForMTController(DataTable dt2)
        {
            var selectList = new SelectList(dt2.AsEnumerable().Where(x => x.Field<int>("RaisedTypeID") != (int)E658.Enum.E658RaisedType.StaffVehiE).Select(row => new SelectListItem
            {
                Text = row["UnitSerialNo"].ToString(),
                Value = row["UnitSerialNo"].ToString()
            }), "Value", "Text");

            ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));
        }
        [HttpGet]
        public ActionResult Forward(int roleId, int E658CreatorDltId, int Behavior)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/04/01
            /// Description : Data forward to user by user
            /// 
            try
            {
                var e658Type = _db.E658CreaterDetails.Where(x => x.ECDID == E658CreatorDltId && x.Active == 1).Select(x => new { x.RaisedTypeID, x.UserGERMSLocation }).FirstOrDefault();

                //var mt658Details = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == E658CreatorDltId && x.Active == 1).Select(x => new { x.OMTNo, x.SLAFRegNo }).FirstOrDefault();



                flowList = RecordFlowMgtId((int)E658.Enum.E658RaisedType.TA, roleId, Behavior);

                foreach (var item in flowList)
                {
                    EFID = item.EFMID;
                    RID = item.RoleID;
                }

                E658FlowTransaction FlowTranc = new E658FlowTransaction();  //{};

                if (roleId == (int)E658.Enum.EnumE658UserType.MToOCT)
                {
                    //if (mt658Details.OMTNo != "MT Sect" && mt658Details.SLAFRegNo != null )
                    //{

                    //}
                    //else
                    //{
                    //    TempData["ErrMsg"] = "Please nominate OMT or Vehicle number to continue the process.";
                    //    return RedirectToAction("E658List");
                    //}

                    FlowTranc.EFlowMgtID = EFID;
                    FlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward;
                    FlowTranc.RecordLocID = e658Type.UserGERMSLocation;
                    FlowTranc.E658CreatorDltID = E658CreatorDltId;
                    FlowTranc.RoleID = RID;
                    FlowTranc.Active = 1;
                    FlowTranc.CreatedDate = DateTime.Now;
                    FlowTranc.CreatedBy = (Session["LoginUser"]).ToString();
                    FlowTranc.CreatedIP = this.Request.UserHostAddress;
                    FlowTranc.CreatedMAC = mac.GetMacAddress();

                    _db.E658FlowTransaction.Add(FlowTranc);

                    if (_db.SaveChanges() > 0)
                    {
                        TempData["ScfMsg"] = "You have Forwarded the Transport Authority Request Successfully.";

                        return RedirectToAction("TrnsPortReqList");
                    }
                    else
                    {
                        return View();
                    }

                }
                else
                {
                    FlowTranc.EFlowMgtID = EFID;
                    FlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward;
                    FlowTranc.RecordLocID = e658Type.UserGERMSLocation;
                    FlowTranc.E658CreatorDltID = E658CreatorDltId;
                    FlowTranc.RoleID = RID;
                    FlowTranc.Active = 1;
                    FlowTranc.CreatedDate = DateTime.Now;
                    FlowTranc.CreatedBy = (Session["LoginUser"]).ToString();
                    FlowTranc.CreatedIP = this.Request.UserHostAddress;
                    FlowTranc.CreatedMAC = mac.GetMacAddress();

                    _db.E658FlowTransaction.Add(FlowTranc);

                    if (_db.SaveChanges() > 0)
                    {
                        TempData["ScfMsg"] = "You have Forwarded the Transport Authority Request Successfully.";

                        return RedirectToAction("TrnsPortReqList");
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                // return View();
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ForwardHQAuth(int roleId, int E658CreatorDltId, int Behavior, string hqAuthRemark)
        {
            try
            {
                // Get E658 creator details
                var e658Type = _db.E658CreaterDetails
                                  .Where(x => x.ECDID == E658CreatorDltId && x.Active == 1)
                                  .Select(x => new { x.RaisedTypeID, x.UserGERMSLocation })
                                  .FirstOrDefault();

                if (e658Type == null)
                    return Json(new { success = false, message = "Invalid E658 Creator Detail." });

                // Get related transaction headers and details together
                var eTransData = (from eth in _db.ETransReqHeaders
                                  join etd in _db.ETransReqDetails on eth.TAHID equals etd.ETrnsHedID
                                  where eth.ETrnsCreatorDltId == E658CreatorDltId
                                  select new { eth, etd }).ToList();

                if (!eTransData.Any())
                    return Json(new { success = false, message = "No Transaction Request Details found to update." });

                string macAddress = mac.GetMacAddress();
                string loginUser = Session["LoginUser"]?.ToString();

                // If DGE, generate once
                string DGEAuthRefNo = string.Empty;
                if (roleId != (int)E658.Enum.EnumE658UserType.SOGO)
                {
                    DGEAuthRefNo = CreateDGEAuthorityNo(E658CreatorDltId);
                }

                // Update all related details and headers
                foreach (var item in eTransData)
                {
                    // Update remarks based on role
                    if (roleId == (int)E658.Enum.EnumE658UserType.SOGO)
                        item.etd.SOGORemarks = hqAuthRemark;
                    else
                        item.etd.HQDGERemarks = hqAuthRemark;

                    item.etd.ModifiedDate = DateTime.Now;
                    item.etd.ModifiedMac = macAddress;
                    item.etd.ModifiedBy = loginUser;

                    // Update DGE Auth Ref No if needed
                    if (roleId != (int)E658.Enum.EnumE658UserType.SOGO)
                    {
                        item.eth.DgeAuthRefNo = DGEAuthRefNo;
                        item.eth.ModifiedDate = DateTime.Now;
                        item.eth.ModifiedMac = macAddress;
                        item.eth.ModifiedBy = loginUser;
                    }
                }

                // Process flow management
                var flowList = RecordFlowMgtId((int)E658.Enum.E658RaisedType.TA, roleId, Behavior);
                if (!flowList.Any())
                    return Json(new { success = false, message = "Flow Management configuration missing." });

                var flowItem = flowList.First();

                // Add flow transaction
                var flowTransaction = new E658FlowTransaction
                {
                    EFlowMgtID = flowItem.EFMID,
                    RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward,
                    RecordLocID = e658Type.UserGERMSLocation,
                    E658CreatorDltID = E658CreatorDltId,
                    RoleID = flowItem.RoleID,
                    Active = 1,
                    CreatedDate = DateTime.Now,
                    CreatedBy = loginUser,
                    CreatedIP = Request.UserHostAddress,
                    CreatedMAC = macAddress
                };
                _db.E658FlowTransaction.Add(flowTransaction);

                // Save all changes
                if (_db.SaveChanges() > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "You have forwarded the Transport Authority Request successfully.",
                        redirectUrl = Url.Action("TrnsPortReqList")
                    });
                }

                return Json(new { success = false, message = "No changes were saved." });
            }
            catch (Exception ex)
            {
                // Ideally log the exception
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public ActionResult EditTransRequest(string userID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/22
            /// Description : View E658 Details
            /// 
            VME658Create objVME658Create = new VME658Create();
            DataTable dt = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();
            string FromLocID = "";
            string ToLocID = "";
            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");           

            var hashingService = new HashingService();

            // Decode the hashId back to the original string
            string decodedString = hashingService.DecodeHashId(userID);

            // Split the decoded string to retrieve the original values
            var splitValues = decodedString.Split(':');
            int creatorId = int.Parse(splitValues[0]);
            int roleId = int.Parse(splitValues[1]);
            int eFlowId = int.Parse(splitValues[2]);


            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(creatorId);
                ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
                                              join rt in _db.E658RaisedType
                                              on rrt.RaisedTypeID equals rt.RTID
                                              join runt in _db.E658RunsTypes
                                              on rrt.RunsTypeID equals runt.ERTID
                                              where rrt.RaisedTypeID == (int)E658.Enum.E658RaisedType.TA && rrt.Active == 1
                                              select new SelectListItem
                                              {
                                                  Value = runt.ERTID.ToString(),
                                                  Text = runt.TypeName
                                              }).ToList();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Session["UnitSerialNo"] = dt.Rows[i]["UnitSerialNo"].ToString();
                    Session["ChassisNo"] = dt.Rows[i]["ChassisNo"].ToString();
                    Session["Seq"] = dt.Rows[i]["Seq"].ToString();

                    FromLocID = dt.Rows[i]["FLocation"].ToString();
                    ToLocID = dt.Rows[i]["TLocation"].ToString();

                    objVME658Create.FromLocID = _db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();
                    objVME658Create.ToLocId = _db.Locations.Where(x => x.LocationID == ToLocID).Select(x => x.LocationName).FirstOrDefault();
                    objVME658Create.UnitSerialNo = dt.Rows[i]["UnitSerialNo"].ToString();
                    objVME658Create.E658RunType = dt.Rows[i]["TypeName"].ToString();
                    objVME658Create.E658Date = Convert.ToDateTime(dt.Rows[i]["PDate"]);
                    objVME658Create.ReturnDate = Convert.ToDateTime(dt.Rows[i]["ReturnDate"]);
                    objVME658Create.JournryStartTime = Convert.ToDateTime(dt.Rows[i]["PTime"]);
                    objVME658Create.RequiredDuration = dt.Rows[i]["PHrs"].ToString();
                    objVME658Create.Purpose = dt.Rows[i]["Duty"].ToString();
                    objVME658Create.Route = dt.Rows[i]["Route"].ToString();
                    objVME658Create.IsOMTAvail = Convert.ToInt32(dt.Rows[i]["IsOMtReqFromMT"]);
                    objVME658Create.IsVehicleAvail = Convert.ToInt32(dt.Rows[i]["IsVehicleReqFromMT"]);
                    objVME658Create.RoleID = roleId;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);

                    TempData["ECDID"] = creatorId;


                    if (objVME658Create.IsOMTAvail == 1)
                    {
                        objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.PreviousOMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
                    }

                    if (objVME658Create.IsVehicleAvail == 10)
                    {
                        objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.PreviousSLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    }

                    e658DetailsList.Add(objVME658Create);
                }             
             
                //PartialView("_E658ReportLoc", locations);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(objVME658Create);
        }
        [HttpPost]
        public ActionResult EditTransRequest(VME658Create objE658)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/22
            /// Description : load E658 Creater View
            /// if OMT Request From MT, Here I assign 1 to indicate it, if not happend I assign 0 and save the OMT Service No
            /// if Vehicle Request From MT, Here I assign 1 to indicate it, if not happend I assign 0 and save the SLAF Reg No


            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");
            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RunsTypes, "ERTID", "TypeName");

            string seqNo = "";
            string VehiChassisNo = "";
            int isOMTReqFromMT = 0;
            int IsVehiReqFromMT = 0;
            string SLAFRegNumber = null;
            string vehicleAttaLoc = "";


            List<Cls_ItemList> lst_ListPartItem = new List<Cls_ItemList>();

            try
            {
                //if (Session["E658RunType"] != null && Session["E658CreateUser"] != null && Session["UserLoginLoc"] != null)
                //{
                var FromLocID = _db.Locations.Where(x => x.LocationName == objE658.FromLocID).Select(x => x.LocationID).FirstOrDefault();
                var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();
                //RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
                //string createUnitSerialNo = CreateUnitSerialNo(FromLocID, RaisedTypeID);

                TimeSpan StartTime = objE658.JournryStartTime.TimeOfDay;

                if (objE658.PreviousOMTServiceNo != null)
                {
                    objE658.OMTServiceNo = objE658.PreviousOMTServiceNo;
                }
                else
                {
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
                }

                if (objE658.PreviousSLAFRegNo != null)
                {
                    SLAFRegNumber = objE658.PreviousSLAFRegNo;
                }
                else
                {
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

                        var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo.Contains(objE658.SLAFRegNo)).Select(x => new { x.ChassisNo, x.AttachedLocationID }).FirstOrDefault();

                        VehiChassisNo = sysChassisNo.ChassisNo;
                        vehicleAttaLoc = sysChassisNo.AttachedLocationID;
                        seqNo = sysChassisNo.ChassisNo + "/" + DateTime.Now;
                        IsVehiReqFromMT = 0;
                    }

                }

                F658RegistryHeader RegHeader = new F658RegistryHeader();
                // _db.F658RegistryHeader.Add(RegHeader);
                RegHeader = _db.F658RegistryHeader.Find(Session["UnitSerialNo"], Session["ChassisNo"], Session["Seq"]);
                RegHeader.ChassisNo = VehiChassisNo;
                RegHeader.Seq = seqNo;
                RegHeader.FLocation = FromLocID;
                RegHeader.ELocation = FromLocID;
                RegHeader.TLocation = ToLocID.Trim();

                // E658CreatorDltId = objE658.ECDID,
                RegHeader.PLocation = vehicleAttaLoc;
                RegHeader.IsOMtReqFromMT = isOMTReqFromMT;
                RegHeader.OMTNo = objE658.OMTServiceNo;//OMServiceNo,
                RegHeader.SLAFRegNo = SLAFRegNumber;
                RegHeader.IsVehicleReqFromMT = IsVehiReqFromMT;
                RegHeader.Duty = objE658.Purpose;
                RegHeader.PDate = objE658.E658Date;
                RegHeader.ReturnDate = objE658.ReturnDate;
                RegHeader.PTime = StartTime.ToString();
                RegHeader.Route = objE658.Route;
                RegHeader.PHrs = objE658.RequiredDuration;
                RegHeader.Active = 1;
                RegHeader.CreatedDate = DateTime.Now;
                RegHeader.E658CreatedUser = Convert.ToInt64(Session["E658CreateUser"]);

                _db.Entry(RegHeader).State = EntityState.Modified;
                if (_db.SaveChanges() > 0)
                {
                    if (objE658.RoleID != 0)
                    {
                        TempData["ScfMsg"] = "You have Update the E658 Successfully.";
                        //return Json(new { success = true, redirectUrl = Url.Action("E658List", new { E658CreatorID = objE658.ECDID, RoleId = 0 }) });
                        var hashingService = new HashingService();

                        string encodedId = hashingService.EncodeMultipleValues(objE658.ECDID, 0, 0);

                        return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { userID = encodedId }) });

                    }
                    else
                    {
                        TempData["ScfMsg"] = "You have Update the E658 Successfully.";
                        //return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { E658CreatorID = objE658.ECDID, RoleId = 0, EFlowId = 0 }) });

                        var hashingService = new HashingService();

                        string encodedId = hashingService.EncodeMultipleValues(objE658.ECDID, 0, 0);

                        return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { userID = encodedId }) });

                    }

                }
                else
                {

                }
            }
            catch (Exception ex)
            {

                throw ex;

            }
            return RedirectToAction("E658", "E658List");
        }

        //[HttpGet]
        //public ActionResult EditTranportAuth(string userID)
        //{
        //    ///Created BY   : Sqn ldr Wicky
        //    /// Create Date : 2024/03/22
        //    /// Description : View E658 Details
        //    /// 
        //    VME658Create objVME658Create = new VME658Create();
        //    DataTable dt = new DataTable();
        //    List<VME658Create> e658DetailsList = new List<VME658Create>();
        //    string FromLocID = "";
        //    string ToLocID = "";
        //    ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");
        //    ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RunsTypes, "ERTID", "TypeName");

        //    var hashingService = new HashingService();

        //    // Decode the hashId back to the original string
        //    string decodedString = hashingService.DecodeHashId(userID);

        //    // Split the decoded string to retrieve the original values
        //    var splitValues = decodedString.Split(':');
        //    int creatorId = int.Parse(splitValues[0]);
        //    int roleId = int.Parse(splitValues[1]);
        //    int eFlowId = int.Parse(splitValues[2]);


        //    try
        //    {
        //        //int E658CreatorID = 81;
        //        ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
        //        dt = objDALCommanQuery.CallE65SP(creatorId);
        //        ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
        //                                      join rt in _db.E658RaisedType
        //                                      on rrt.RaisedTypeID equals rt.RTID
        //                                      join runt in _db.E658RunsTypes
        //                                      on rrt.RunsTypeID equals runt.ERTID
        //                                      where rrt.RaisedTypeID == RaisedTypeID && rrt.Active == 1
        //                                      select new SelectListItem
        //                                      {
        //                                          Value = runt.ERTID.ToString(),
        //                                          Text = runt.TypeName
        //                                      }).ToList();

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {

        //            Session["UnitSerialNo"] = dt.Rows[i]["UnitSerialNo"].ToString();
        //            Session["ChassisNo"] = dt.Rows[i]["ChassisNo"].ToString();
        //            Session["Seq"] = dt.Rows[i]["Seq"].ToString();

        //            FromLocID = dt.Rows[i]["FLocation"].ToString();
        //            ToLocID = dt.Rows[i]["TLocation"].ToString();

        //            objVME658Create.FromLocID = _db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();
        //            objVME658Create.ToLocId = _db.Locations.Where(x => x.LocationID == ToLocID).Select(x => x.LocationName).FirstOrDefault();
        //            objVME658Create.UnitSerialNo = dt.Rows[i]["UnitSerialNo"].ToString();
        //            objVME658Create.E658RunType = dt.Rows[i]["TypeName"].ToString();
        //            objVME658Create.E658Date = Convert.ToDateTime(dt.Rows[i]["PDate"]);
        //            objVME658Create.ReturnDate = Convert.ToDateTime(dt.Rows[i]["ReturnDate"]);
        //            objVME658Create.JournryStartTime = Convert.ToDateTime(dt.Rows[i]["PTime"]);
        //            objVME658Create.RequiredDuration = dt.Rows[i]["PHrs"].ToString();
        //            objVME658Create.Purpose = dt.Rows[i]["Duty"].ToString();
        //            objVME658Create.Route = dt.Rows[i]["Route"].ToString();
        //            objVME658Create.IsOMTAvail = Convert.ToInt32(dt.Rows[i]["IsOMtReqFromMT"]);
        //            objVME658Create.IsVehicleAvail = Convert.ToInt32(dt.Rows[i]["IsVehicleReqFromMT"]);
        //            objVME658Create.RoleID = roleId;
        //            objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);

        //            TempData["ECDID"] = creatorId;


        //            if (objVME658Create.IsOMTAvail == 1)
        //            {
        //                objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
        //            }
        //            else
        //            {
        //                objVME658Create.PreviousOMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
        //            }

        //            if (objVME658Create.IsVehicleAvail == 10)
        //            {
        //                objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
        //            }
        //            else
        //            {
        //                objVME658Create.PreviousSLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
        //            }

        //            e658DetailsList.Add(objVME658Create);
        //        }

        //        List<VME658Create> locations = GetRptLocation(creatorId);

        //        TempData["RptLocation"] = locations;

        //        //PartialView("_E658ReportLoc", locations);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }

        //    return View(objVME658Create);
        //}


        //[HttpPost]
        //public ActionResult CreateRequest(VMLongRunCreate objE658)
        //{
        //    ///Created BY   : Sqn ldr Wicky
        //    /// Create Date : 2025/01/18
        //    /// Description : transport authority requesting form

        //    ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

        //    string seqNo = "";
        //    string VehiChassisNo = "";
        //    //int isOMTReqFromMT = 0;
        //    //int IsVehiReqFromMT = 0;
        //    string SLAFRegNumber = null;
        //    string vehicleAttaLoc = "";

        //    //string recordCreateBy =

        //    List<Cls_ItemList> lst_ListPartItem = new List<Cls_ItemList>();

        //    try
        //    {

        //        var FromLocID = _db.Locations.Where(x => x.LocationName == objE658.FromLocID).Select(x => x.LocationID).FirstOrDefault();
        //        var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();
        //        //RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
        //        string createUnitSerialNo = CreateUnitSerialNo(FromLocID, objE658.IsRaisedMode, objE658.SectionName);

        //        TimeSpan StartTime = objE658.JournryStartTime.TimeOfDay;

        //        var creatorDetailsJson = Session["CretorDetails"] as string;

        //        if (string.IsNullOrEmpty(creatorDetailsJson))
        //        {
        //            return RedirectToAction("Index", "User"); // Adjust "Account" and "Login" to your actual controller and action names
        //        }
        //        var objCreateDetails = JsonConvert.DeserializeObject<VMLongRunCreate>(creatorDetailsJson);


        //        Session["E658CreateUser"] = objCreateDetails.Sno;

        //        //isOMTReqFromMT = 0;
        //        SLAFRegNumber = objE658.SLAFRegNo;

        //        //var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo == objE658.SLAFRegNo && x.Status == 1).Select(x => x.ChassisNo).FirstOrDefault();

        //        var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo.Contains(objE658.SLAFRegNo) && (x.Status == 1 || x.Status == 2 || x.Status == 3)).Select(x => new { x.ChassisNo, x.AttachedLocationID }).FirstOrDefault();

        //        VehiChassisNo = sysChassisNo.ChassisNo;
        //        vehicleAttaLoc = sysChassisNo.AttachedLocationID;
        //        seqNo = sysChassisNo.ChassisNo + "/" + DateTime.Now;
        //        //IsVehiReqFromMT = 0;

        //        E658CreaterDetails objCreater = new E658CreaterDetails
        //        {
        //            SNo = objCreateDetails.Sno,
        //            RaisedTypeID = (int)E658.Enum.E658RaisedType.TA,
        //            CreatedDate = DateTime.Now,
        //            CreatedBy = objCreateDetails.Sno,
        //            UserGERMSLocation = objCreateDetails.CreatorLocation.Trim(),
        //            UserGERMSSection = objCreateDetails.SectionName,
        //            IsBaseOfFormation = objE658.IsRaisedMode,
        //            OICServiceNo = objE658.OICSNo,
        //            Active = 1,
        //            CreatedIP = this.Request.UserHostAddress,
        //            IsE658Create = 1,
        //            CreatedMAC = mac.GetMacAddress(),
        //        };



        //        _db.E658CreaterDetails.Add(objCreater);
        //        if (_db.SaveChanges() > 0)
        //        {
        //            objE658.ECDID = objCreater.ECDID;

        //            ETransReqHeader transHeader = new ETransReqHeader()
        //            {
        //                CmpTrnsRefNo = createUnitSerialNo,
        //                ReqSection = objE658.SectionName,
        //                ReqCategory = objE658.ERTID,
        //                IsAddiDutyReq = Convert.ToInt16(objE658.IsAddiDutyReq),
        //                ETrnsCreatorDltId = objE658.ECDID,
        //                CreatedDate = DateTime.Now,
        //                CreatedBy = (Session["E658CreateUser"]).ToString(),
        //                CreatedIP = this.Request.UserHostAddress,
        //                CreatedMAC = mac.GetMacAddress()
        //            };

        //            _db.ETransReqHeaders.Add(transHeader);

        //            if (_db.SaveChanges() > 0)
        //            {
        //                int id = transHeader.TAHID;

        //                ETransReqDetail transDetails = new ETransReqDetail()
        //                {
        //                    ETrnsHedID = id,
        //                    DeptFrom = FromLocID,
        //                    DeptTo = ToLocID.Trim(),
        //                    DeptDate = objE658.E658Date,
        //                    ReturnDate = objE658.ReturnDate,
        //                    DeptTime = StartTime.ToString(),
        //                    Purpose = objE658.Purpose,
        //                    Rout = objE658.Route,
        //                    OMTSvcNo = objE658.OMTServiceNo,
        //                    SLAFRegNo = SLAFRegNumber,
        //                    VehicleChassieNo = VehiChassisNo,
        //                    IsNightPrkReq = Convert.ToInt16(objE658.IsNightPark),
        //                    NightPrkLoc = objE658.NightParkLoc,
        //                    CreatedDate = DateTime.Now,
        //                    CreatedBy = (Session["E658CreateUser"]).ToString(),
        //                    CreatedIP = this.Request.UserHostAddress,
        //                    CreatedMAC = mac.GetMacAddress()
        //                };
        //                _db.ETransReqDetails.Add(transDetails);
        //                _db.SaveChanges();

        //                if ((objE658.IsAddiDutyReq == 1) && (objE658.DutyList != null && objE658.DutyList.Count > 0))
        //                {
        //                    foreach (var duty in objE658.DutyList)
        //                    {
        //                        // Example: Process each duty record
        //                        string date = duty.Date;
        //                        string route = duty.Route;
        //                        string purpose = duty.Purpose;

        //                        ETransAddiDuty Obj = new ETransAddiDuty()
        //                        {
        //                            AddiDutyDate = Convert.ToDateTime(duty.Date),
        //                            AddiDutyRout = duty.Route,
        //                            AddiDutyPurpose = duty.Purpose,
        //                            ETrnsHedID = id,
        //                            CreatedDate = DateTime.Now,
        //                            CreatedBy = (Session["E658CreateUser"]).ToString(),
        //                            CreatedIP = this.Request.UserHostAddress,
        //                            CreatedMAC = mac.GetMacAddress(),


        //                        };
        //                        _db.ETransAddiDuties.Add(Obj);
        //                        _db.SaveChanges();
        //                        // You can save these to the database or process as needed
        //                    }
        //                }

        //            }

        //            //F658RegistryHeader RegHeader = new F658RegistryHeader()
        //            //{
        //            //    ChassisNo = VehiChassisNo,
        //            //    Seq = seqNo,
        //            //    UnitSerialNo = createUnitSerialNo,
        //            //    FLocation = FromLocID,
        //            //    ELocation = FromLocID,
        //            //    //TLocation = ToLocID.Trim(),
        //            //    TLocation = ToLocID.Trim(),
        //            //    RealToLocation = (objE658.ToLocId == "Other Location") ? objE658.OtherLocName : ToLocID.Trim(),
        //            //    E658CreatorDltId = objE658.ECDID,
        //            //    PLocation = vehicleAttaLoc,
        //            //    IsRR658 = 2,
        //            //    Behavior = objE658.ERTID,
        //            //    IsOMtReqFromMT = isOMTReqFromMT,
        //            //    OMTNo = objE658.OMTServiceNo,//OMServiceNo,
        //            //    SLAFRegNo = SLAFRegNumber,
        //            //    IsVehicleReqFromMT = IsVehiReqFromMT,
        //            //    Duty = objE658.Purpose,
        //            //    AuthorityType = 1,
        //            //    PDate = objE658.E658Date,
        //            //    ReturnDate = objE658.ReturnDate,
        //            //    PTime = StartTime.ToString(),
        //            //    Route = objE658.Route,
        //            //    PHrs = objE658.RequiredDuration,
        //            //    Status = 50,
        //            //    Active = 1,
        //            //    CreatedDate = DateTime.Now,
        //            //    E658CreatedUser = Convert.ToInt64(objCreateDetails.Sno),
        //            //    IsNightPark = objE658.IsNightPark,
        //            //    Description = objE658.AdditionalDuties,
        //            //    NightParkLoc = (objE658.IsNightPark == 1)?  objE658.NightParkLoc: null,


        //            //};

        //            //_db.F658RegistryHeader.Add(RegHeader);

        //            flowList = RecordFlowMgtId((int)E658.Enum.E658RaisedType.TA, (int)E658.Enum.EnumE658UserType.LngRunRequest, objE658.ERTID);

        //            foreach (var item in flowList)
        //            {
        //                EFID = item.EFMID;
        //                RID = item.RoleID;
        //            }

        //            E658FlowTransaction FlowTranc = new E658FlowTransaction()
        //            {
        //                EFlowMgtID = EFID,
        //                RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward,
        //                RecordLocID = FromLocID,
        //                E658CreatorDltID = objE658.ECDID,
        //                RoleID = RID,
        //                Active = 1,
        //                CreatedDate = DateTime.Now,
        //                CreatedBy = (Session["E658CreateUser"]).ToString(),
        //                CreatedIP = this.Request.UserHostAddress,
        //                CreatedMAC = mac.GetMacAddress()

        //            };
        //            _db.E658FlowTransaction.Add(FlowTranc);

        //            _db.SaveChanges();

        //            TempData["ScfMsg"] = "You have created the E658 Successfully.";

        //        }

        //        var hashingService = new HashingService();

        //        string encodedId = hashingService.EncodeMultipleValues(objE658.ECDID, 0, 0);

        //        return Json(new { success = true, redirectUrl = Url.Action("ETransReqDetails", new { userID = encodedId }) });

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;

        //    }
        //}
    }
}