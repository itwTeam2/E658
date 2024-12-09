using E658.Models;
using E658.HelperServices;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Text;

namespace E658.Controllers
{
    public class E658Controller : Controller
    {

        private dbContext _db = new dbContext();
        private dbContextCommonData _dbCommonData = new dbContextCommonData();
        private static readonly Random _random = new Random();

        MacAddress mac = new MacAddress();
        string FromLocID = "";
        string Sno = "";
        string UserGERMSLocation = "";
        int RaisedTypeID = 0;
        int EFID = 0;
        int? RID = 0;
        List<E658FlowMagt> flowList = new List<E658FlowMagt>();
       

        // GET: E658
        //[HttpGet]
        public ActionResult Index()
        {


            return View();

            //return PartialView("_E658Details", e658DetailsList);

        }
        public ActionResult Index2(string id)
        {

            DataTable dt = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();
            string FromLocID = "";
            string ToLocID = "";

            var creatorID = _db.F658RegistryHeader.Where(x => x.UnitSerialNo == id).Select(x => x.E658CreatorDltId).FirstOrDefault();
            int cID = Convert.ToInt32(creatorID);
            VME658Create objVME658Create = new VME658Create();
            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(cID);


                for (int i = 0; i < dt.Rows.Count; i++)
                {

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
                    //objVME658Create.RoleID = RoleId;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);
                    //objVME658Create.EFTID = EFlowID;
                    objVME658Create.RecordStatus = Convert.ToInt32(dt.Rows[i]["RecordStatus"]);
                    //TempData["ECDID"] = ECDID;


                    if (objVME658Create.IsOMTAvail == 1)
                    {
                        objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.OMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
                    }

                    if (objVME658Create.IsVehicleAvail == 10)
                    {
                        objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    }
                    e658DetailsList.Add(objVME658Create);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

            //return View(e658DetailsList);

            return PartialView("_E658Details", e658DetailsList);

        }
        [HttpGet]
        public ActionResult Create()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/22
            /// Description : load E658 Creater View

            VME658Create model = new VME658Create();

            if (Session["E658RunType"] != null && Session["E658CreateUser"] != null && Session["UserLoginLoc"] != null)
            {

                FromLocID = Session["UserLoginLoc"].ToString();
                Sno = Session["E658CreateUser"].ToString();
                UserGERMSLocation = Session["UserLoginLoc"].ToString();
                RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);

                ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

                ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
                                              join rt in _db.E658RaisedType
                                              on rrt.RaisedTypeID equals rt.RTID
                                              join runt in _db.E658RunsTypes
                                              on rrt.RunsTypeID equals runt.ERTID
                                              where rrt.RaisedTypeID == RaisedTypeID && rrt.Active == 1
                                              select new SelectListItem
                                              {
                                                  Value = runt.ERTID.ToString(),
                                                  Text = runt.TypeName
                                              }).ToList();



                var FromLoction = _db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();

                var E658CreatorID = _db.E658CreaterDetails.Where(x => x.RaisedTypeID == RaisedTypeID && x.Active == 1 &&
                                   x.SNo == Sno && x.UserGERMSLocation == UserGERMSLocation).OrderByDescending(x => x.CreatedDate).
                                   Select(x => x.ECDID).FirstOrDefault();

                model.FromLocID = FromLoction;
                model.ECDID = E658CreatorID;
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(VME658Create objE658)
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
                if (Session["E658RunType"] != null && Session["E658CreateUser"] != null && Session["UserLoginLoc"] != null)
                {
                    var FromLocID = _db.Locations.Where(x => x.LocationName == objE658.FromLocID).Select(x => x.LocationID).FirstOrDefault();
                    var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();
                    RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
                    string createUnitSerialNo = CreateUnitSerialNo(FromLocID, RaisedTypeID);

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

                    F658RegistryHeader RegHeader = new F658RegistryHeader()
                    {
                        ChassisNo = VehiChassisNo,
                        Seq = seqNo,
                        UnitSerialNo = createUnitSerialNo,
                        FLocation = FromLocID,
                        ELocation = FromLocID,
                        TLocation = ToLocID.Trim(),
                        RealToLocation = ToLocID.Trim(),
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

                        flowList = RecordFlowMgtId(RaisedTypeID, (int)E658.Enum.EnumE658UserType.E658CreateUser);

                        foreach (var item in flowList)
                        {
                            EFID = item.EFMID;
                            RID = item.RoleID;
                        }


                        E658FlowTransaction FlowTranc = new E658FlowTransaction()
                        {
                            EFlowMgtID = EFID,
                            RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward,
                            RecordLocID = FromLocID,
                            E658CreatorDltID = objE658.ECDID,
                            RoleID = RID,
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
        public JsonResult DateCalc(VME658Create objE658)
        {
            /// Created BY   : Sgt  Madushanka
            /// Create Date : 2024/06/25
            /// Description : Check the return date exceed more than 7 days   

            DateTime FromDate = Convert.ToDateTime(objE658.E658Date);
            DateTime ToDate = Convert.ToDateTime(objE658.ReturnDate);
            TimeSpan Time = ToDate - FromDate;
            double g = Time.TotalDays;


            objE658.RequiredDuration = g.ToString();
            return Json(objE658, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult E658List(string sortOrder, string currentFilter, string searchString, int? page, int? RSID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/29
            /// Description : Load E658 List to Different Users

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();
            List<VME658Create> E658List = new List<VME658Create>();

            int pageSize = 0;
            int pageNumber = 1;
                                

            if (Session["UserLoginType"] !=  null)
            {
                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
                int userLoginType2 = Convert.ToInt32(Session["UserLoginType"]);

                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();

                if (userLoginType == (int)E658.Enum.EnumE658UserType.MTController)
                {
                    userLoginType = (int)E658.Enum.EnumE658UserType.MToOCT;
                }
                dt = objDALCommanQuery.CallE65MoreDetailsSP(userLoginType);

                if ((searchString != null && searchString != "") || currentFilter != null)
                {

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        switch (userLoginType)
                        {
                            case (int)E658.Enum.EnumE658UserType.StaffCarUser:

                                string staffServiceNo = Session["LoginUser"].ToString();

                                var row1 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward
                                    && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter)));

                                if (row1.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward
                                    && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter))).CopyToDataTable();
                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.FormationUser:

                                string UserEpaLoc = Session["UserEpassLoc"].ToString();
                                string UserEpassDivi = Session["UserEpassDivision"].ToString();

                                var row2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc &&
                                x.Field<string>("CreaterDivision") == UserEpassDivi
                                && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward
                                && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter)));

                                if (row2.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc &&
                                x.Field<string>("CreaterDivision") == UserEpassDivi
                                && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward
                                && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter))).CopyToDataTable();
                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.MToOCT:
                            case (int)E658.Enum.EnumE658UserType.MTController:

                                string MToOctLocation = Session["MToOctLocation"].ToString();

                                var row3 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward
                                          && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter)));

                                if (row3.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward
                                          && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter))).CopyToDataTable();

                                    List<VME658Create> items = new List<VME658Create>();


                                    var selectList = new SelectList(dt2.AsEnumerable().Where(x => x.Field<int>("RaisedTypeID") != (int)E658.Enum.E658RaisedType.StaffVehiE).Select(row => new SelectListItem
                                    {
                                        Text = row["UnitSerialNo"].ToString(), // Change "TextColumn" to your actual column name
                                        Value = row["UnitSerialNo"].ToString() // Change "ValueColumn" to your actual column name
                                    }), "Value", "Text");

                                    ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));

                                }

                                TempData["MTControllerStatus"] = userLoginType2 == (int)E658.Enum.EnumE658UserType.MTController ? "5" : "0";

                                break;
                            case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:

                                string MToOctLocation2 = Session["MToOctLocation"].ToString();

                                var row4 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization
                                          && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter)));

                                if (row4.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization
                                          && (x.Field<DateTime>("PDate") == Convert.ToDateTime(searchString) || x.Field<DateTime>("PDate") == Convert.ToDateTime(currentFilter))).CopyToDataTable();
                                }
                                break;

                            default:
                                break;
                        }

                    }
                }
                
                else
                {

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        switch (userLoginType)
                        {
                            case (int)E658.Enum.EnumE658UserType.StaffCarUser:

                                string staffServiceNo = Session["LoginUser"].ToString();

                                var row1 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row1.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();
                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.FormationUser:

                                string UserEpaLoc = Session["UserEpassLoc"].ToString();
                                string UserEpassDivi = Session["UserEpassDivision"].ToString();

                                var row2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc && x.Field<string>("CreaterDivision") == UserEpassDivi && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row2.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc &&
                                x.Field<string>("CreaterDivision") == UserEpassDivi && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();

                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.MToOCT:
                            case (int)E658.Enum.EnumE658UserType.MTController:

                                string MToOctLocation = Session["MToOctLocation"].ToString();

                                var row3 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row3.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();

                                    List<VME658Create> items = new List<VME658Create>();


                                    var selectList = new SelectList(dt2.AsEnumerable().Where(x => x.Field<int>("RaisedTypeID") != (int)E658.Enum.E658RaisedType.StaffVehiE).Select(row => new SelectListItem
                                    {
                                        Text = row["UnitSerialNo"].ToString(), // Change "TextColumn" to your actual column name
                                        Value = row["UnitSerialNo"].ToString() // Change "ValueColumn" to your actual column name
                                    }), "Value", "Text");

                                    ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));

                                }

                                TempData["MTControllerStatus"] = userLoginType2 == (int)E658.Enum.EnumE658UserType.MTController ? "5" : "0";

                                break;
                            case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:

                                string MToOctLocation2 = Session["MToOctLocation"].ToString();

                                var row4 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization);

                                if (row4.Any())
                                {
                                    //dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 &&
                                    //     x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization).CopyToDataTable();

                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization).CopyToDataTable();


                                }
                                break;

                            default:
                                break;
                        }

                    }
                }

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    VME658Create objList = new VME658Create();
                    objList.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
                    objList.FromLocID = dt2.Rows[i]["FromLocationFull"].ToString();
                    objList.ToLocId = dt2.Rows[i]["ToLocationFull"].ToString();
                    objList.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
                    objList.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
                    objList.E658RunType = dt2.Rows[i]["TypeName"].ToString();
                    objList.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);
                    objList.RoleID = Convert.ToInt32(dt2.Rows[i]["RoleID"]);
                    objList.UserGERMSLocation = dt2.Rows[i]["UserGERMSLocation"].ToString();
                    objList.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
                    objList.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
                    objList.EFTID = Convert.ToInt32(dt.Rows[i]["EFTID"]);
                    objList.RaisedTypeID = Convert.ToInt32(dt.Rows[i]["RaisedTypeID"]);
                    
                    if (objList.RaisedTypeID == (int)E658.Enum.E658RaisedType.FormationE || objList.RaisedTypeID == (int)E658.Enum.E658RaisedType.MTSecE)
                    {
                       
                        objList.DivisionFullName = dt2.Rows[i]["CreaterDivision"].ToString();
                    }

                    if (!Convert.IsDBNull(dt2.Rows[i]["IsCombineRun"]))
                    {
                        objList.IsCombineRun = Convert.ToInt32(dt2.Rows[i]["IsCombineRun"]);
                    }


                    E658List.Add(objList);

                }

                if (page >= 2)
                {
                    ViewBag.CurrentFilter = currentFilter;
                }
                else
                {
                    ViewBag.CurrentFilter = searchString;
                }
                pageSize = 20;
                pageNumber = (page ?? 1);
                return View(E658List.ToPagedList(pageNumber, pageSize));

            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
        
        //[HttpGet]
        //public ActionResult E658Details(int E658CreatorID, int RoleId, int EFlowId)
        //{
        //    ///Created BY   : Sqn ldr Wicky
        //    /// Create Date : 2024/03/22
        //    /// Description : View E658 Details
        //    /// 

        //    DataTable dt = new DataTable();
        //    List<VME658Create> e658DetailsList = new List<VME658Create>();
        //    string FromLocID = "";
        //    string ToLocID = "";
        //    try
        //    {
        //        //int E658CreatorID = 81;
        //        ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
        //        dt = objDALCommanQuery.CallE65SP(E658CreatorID);

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            VME658Create objVME658Create = new VME658Create();

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
        //            objVME658Create.RoleID = RoleId;
        //            objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);
        //            objVME658Create.EFTID = EFlowId;
        //            objVME658Create.RecordStatus = Convert.ToInt32(dt.Rows[i]["RecordStatus"]);
        //            objVME658Create.TypeName = dt.Rows[i]["TypeName1"].ToString();
        //            objVME658Create.RaisedTypeID = Convert.ToInt32(dt.Rows[i]["RaisedTypeID"]);

        //            TempData["ECDID"] = E658CreatorID;
        //            TempData["UserLoginType"] = Session["UserLoginType"];

        //            if (objVME658Create.IsOMTAvail == 1)
        //            {
        //                objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
        //            }
        //            else
        //            {
        //                objVME658Create.OMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
        //            }

        //            if (objVME658Create.IsVehicleAvail == 10)
        //            {
        //                objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
        //            }
        //            else
        //            {
        //                objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
        //            }

        //            if (objVME658Create.RaisedTypeID == (int)E658.Enum.E658RaisedType.StaffVehiE)
        //            {
        //                string staffOffSvcNo = dt.Rows[i]["StaffServiceNo"].ToString();

        //                var offceInfo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == staffOffSvcNo).Select(x => new { x.Rank, x.Name }).FirstOrDefault();
        //                objVME658Create.StafOffName = staffOffSvcNo + " " + offceInfo.Rank + " " + offceInfo.Name;
        //            }
        //            else
        //            {
        //                objVME658Create.DivisionFullName = dt.Rows[i]["CreaterDivision"].ToString();
        //            }


        //            e658DetailsList.Add(objVME658Create);
        //        }

        //        List<VME658Create> locations = GetRptLocation(E658CreatorID);

        //        TempData["RptLocation"] = locations;

        //        //PartialView("_E658ReportLoc", locations);


        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }

        //    return View(e658DetailsList);
        //}

        [HttpGet]
        public ActionResult E658Details(string userID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/22
            /// Description : View E658 Details
            /// 

            DataTable dt = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();
            string FromLocID = "";
            string ToLocID = "";

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

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    VME658Create objVME658Create = new VME658Create();

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
                    objVME658Create.EFTID = eFlowId;
                    objVME658Create.RecordStatus = Convert.ToInt32(dt.Rows[i]["RecordStatus"]);
                    objVME658Create.TypeName = dt.Rows[i]["TypeName1"].ToString();
                    objVME658Create.RaisedTypeID = Convert.ToInt32(dt.Rows[i]["RaisedTypeID"]);

                    TempData["ECDID"] = creatorId;
                    TempData["UserLoginType"] = Session["UserLoginType"];

                    if (objVME658Create.IsOMTAvail == 1)
                    {
                        objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.OMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
                    }

                    if (objVME658Create.IsVehicleAvail == 10)
                    {
                        objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    }

                    if (objVME658Create.RaisedTypeID == (int)E658.Enum.E658RaisedType.StaffVehiE)
                    {
                        string staffOffSvcNo = dt.Rows[i]["StaffServiceNo"].ToString();

                        var offceInfo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == staffOffSvcNo).Select(x => new { x.Rank, x.Name }).FirstOrDefault();
                        objVME658Create.StafOffName = staffOffSvcNo + " " + offceInfo.Rank + " " + offceInfo.Name;
                    }
                    else
                    {
                        objVME658Create.DivisionFullName = dt.Rows[i]["CreaterDivision"].ToString();
                    }


                    e658DetailsList.Add(objVME658Create);
                }

                List<VME658Create> locations = GetRptLocation(creatorId);

                TempData["RptLocation"] = locations;

                //PartialView("_E658ReportLoc", locations);


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(e658DetailsList);
        }

        //[HttpGet]
        public ActionResult E658DetailsFinalView(string id)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/03/22
            /// Description : View E658 Details partial View for Model pop up. this is for Final Authorized person
            /// 

            DataTable dt = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();

            // Decode the Base64 encoded id back to the original value
            byte[] data = Convert.FromBase64String(id);
            string decodedUnitSerialNo = Encoding.UTF8.GetString(data);

            string FromLocID = "";
            string ToLocID = "";

            var creatorID = _db.F658RegistryHeader.Where(x => x.UnitSerialNo == decodedUnitSerialNo).Select(x => x.E658CreatorDltId).FirstOrDefault();
            int cID = Convert.ToInt32(creatorID);
            VME658Create objVME658Create = new VME658Create();
            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(cID);


                for (int i = 0; i < dt.Rows.Count; i++)
                {

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
                    objVME658Create.RoleID = (int)E658.Enum.EnumE658UserType.FinalizedAuthorization;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);
                    //objVME658Create.EFTID = EFlowID;
                    objVME658Create.RecordStatus = Convert.ToInt32(dt.Rows[i]["RecordStatus"]);
                    TempData["ECDID"] = cID;


                    if (objVME658Create.IsOMTAvail == 1)
                    {
                        objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.OMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
                    }

                    if (objVME658Create.IsVehicleAvail == 10)
                    {
                        objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    }
                    e658DetailsList.Add(objVME658Create);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

            //return View(e658DetailsList);

            return PartialView("_E658Details", e658DetailsList);

        }
        [HttpGet]
        public ActionResult Reject(int ECDID, int EFlowID, int RoleId)
        {
            ///Created BY   : Sqn ldr WAKY Wickramasinghe 
            ///Created Date : 2024/07/03
            /// Description : this function is to reject the record

            VME658Create model = new VME658Create();
            List<VME658Create> objList = new List<VME658Create>();
            try
            {
                model.ECDID = ECDID;
                model.EFTID = EFlowID;
                model.RoleID = RoleId;

                objList.Add(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_E658RejectComment", objList);
        }
        public ActionResult RejectRecordFinalAuthority(int roleId, int ECDID, int EFTID, string rejectComment)
        {
            ///Created BY   : Sqnl ldr WAKY Wickramasinghe 
            ///Created Date : 2024/08/21
            /// Description : this function is to reject the record. record reject done by final arroval person

            VME658Create model = new VME658Create();

            //string PreviousFlowStatus_UserRole;

            var raisedTypeInfo = _db.E658CreaterDetails.Where(x => x.ECDID == ECDID && x.Active == 1).Select(x => new { x.UserGERMSLocation, x.RaisedTypeID })
                                 .FirstOrDefault();
            var rejectStatusRoleId = _db.E658FlowMagt.Where(x => x.RoleID == roleId && x.RaisedTyId == raisedTypeInfo.RaisedTypeID && x.Active == 1)
                                    .Select(x => x.RejectStatus).FirstOrDefault();

            var rejectRoleFlowMgtId = _db.E658FlowMagt.Where(x => x.RoleID == rejectStatusRoleId && x.RaisedTyId == raisedTypeInfo.RaisedTypeID && x.Active == 1)
                                    .Select(x => x.EFMID).FirstOrDefault();

            E658FlowTransaction objFlowTranc = new E658FlowTransaction();

            objFlowTranc.EFlowMgtID = rejectRoleFlowMgtId;
            objFlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Reject;
            objFlowTranc.E658CreatorDltID = ECDID;
            objFlowTranc.RoleID = rejectStatusRoleId;
            objFlowTranc.RecordLocID = raisedTypeInfo.UserGERMSLocation;
            objFlowTranc.Comment = "Record rejected by Final Authority.";
            objFlowTranc.Active = 1;
            objFlowTranc.CreatedDate = DateTime.Now;
            objFlowTranc.CreatedIP = this.Request.UserHostAddress;
            objFlowTranc.CreatedMAC = mac.GetMacAddress();
            objFlowTranc.CreatedBy = Session["LoginUser"].ToString();

            _db.E658FlowTransaction.Add(objFlowTranc);

            E658CreaterDetails objCreator = _db.E658CreaterDetails.Find(ECDID);
            objCreator.Active = 2;
            objCreator.ModifiedDate = DateTime.Now;
            objCreator.ModifiedMAC = mac.GetMacAddress();
            objCreator.ModifiedBy = Session["LoginUser"].ToString();


            _db.Entry(objCreator).State = EntityState.Modified;

            if (_db.SaveChanges() > 0)
            {

                TempData["ScfMsg"] = "Successfully Rejected the E658";

                //return RedirectToAction("RejectIndex");
            }
            else
            {
                TempData["ErrMsg"] = "Process Unsuccessful.Try again...";
            }

            return RedirectToAction("E658List"); ;
        }
        [HttpGet]
        public ActionResult FinalApprovalReject(int ECDID, int EFlowID, int RoleId)
        {
            ///Created BY   : Sqn ldr WAKY Wickramasinghe 
            ///Created Date : 2024/07/03
            /// Description : this function is to reject the record

            VME658Create model = new VME658Create();
            try
            {
                model.ECDID = ECDID;
                model.EFTID = EFlowID;
                model.RoleID = RoleId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_FinalApprovalReject", model);
        }
        [HttpGet]
        public ActionResult E658RejectedList(string sortOrder, string currentFilter, string searchString, int? page, int? RSID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/07/03
            /// Description : Load E658 Rejected List to Different Users


            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();
            List<VME658Create> E658List = new List<VME658Create>();

            int pageSize = 0;
            int pageNumber = 1;

            
            if (Session["UserLoginType"] != null)
            {
                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65MoreDetailsSP(0);

                if (dt != null && dt.Rows.Count > 0)
                {
                    switch (userLoginType)
                    {
                        case (int)E658.Enum.EnumE658UserType.StaffCarUser:

                            string staffServiceNo = Session["LoginUser"].ToString();

                            var row1 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject);

                            if (row1.Any())
                            {
                                dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                  x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject).CopyToDataTable();
                            }

                            break;

                        case (int)E658.Enum.EnumE658UserType.FormationUser:

                            string UserEpaLoc = Session["UserEpassLoc"].ToString();
                            string UserEpassDivi = Session["UserEpassDivision"].ToString();

                            var row2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc &&
                            x.Field<string>("CreaterDivision") == UserEpassDivi && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject);

                            if (row2.Any())
                            {
                                dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc && x.Field<string>("CreaterDivision") == UserEpassDivi
                               && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject).CopyToDataTable();
                            }

                            break;

                        case (int)E658.Enum.EnumE658UserType.MToOCT:
                        case (int)E658.Enum.EnumE658UserType.MTController:

                            string MToOctLocation = Session["MToOctLocation"].ToString();

                            var row3 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                      == (int)E658.Enum.EnumRecordStatus.Reject);

                            if (row3.Any())
                            {
                                dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation &&
                                     x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject).CopyToDataTable();

                                List<VME658Create> items = new List<VME658Create>();


                                var selectList = new SelectList(dt2.AsEnumerable().Where(x => x.Field<int>("RaisedTypeID") != (int)E658.Enum.E658RaisedType.StaffVehiE).Select(row => new SelectListItem
                                {
                                    Text = row["UnitSerialNo"].ToString(), // Change "TextColumn" to your actual column name
                                    Value = row["UnitSerialNo"].ToString() // Change "ValueColumn" to your actual column name
                                }), "Value", "Text");

                                ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));

                            }

                            break;
                        case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:

                            string MToOctLocation2 = Session["MToOctLocation"].ToString();

                            var row4 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                      == (int)E658.Enum.EnumRecordStatus.Reject);

                            if (row4.Any())
                            {
                                dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 &&
                                     x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Reject).CopyToDataTable();
                            }


                            break;

                        default:
                            break;
                    }

                }

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    VME658Create objList = new VME658Create();
                    objList.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
                    objList.FromLocID = dt2.Rows[i]["FromLocationFull"].ToString();
                    objList.ToLocId = dt2.Rows[i]["ToLocationFull"].ToString();
                    //objList.FromLocID = dt2.Rows[i]["FLocation"].ToString();
                    //objList.ToLocId = dt2.Rows[i]["TLocation"].ToString();
                    objList.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
                    objList.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
                    objList.E658RunType = dt2.Rows[i]["TypeName"].ToString();
                    objList.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);
                    objList.RoleID = Convert.ToInt32(dt2.Rows[i]["RoleID"]);
                    objList.UserGERMSLocation = dt2.Rows[i]["UserGERMSLocation"].ToString();
                    objList.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
                    objList.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
                    objList.EFTID = Convert.ToInt32(dt.Rows[i]["EFTID"]);
                    objList.Comment = dt2.Rows[i]["RejectComment"].ToString();

                    if (!Convert.IsDBNull(dt2.Rows[i]["IsCombineRun"]))
                    {
                        objList.IsCombineRun = Convert.ToInt32(dt2.Rows[i]["IsCombineRun"]);
                    }


                    E658List.Add(objList);

                }

                pageSize = 20;
                pageNumber = (page ?? 1);
                return View(E658List.ToPagedList(pageNumber, pageSize));

            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
        [HttpGet]
        public ActionResult E658FinalApprovedList(string sortOrder, string currentFilter, string searchString, int? page, int? RSID)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/07/04
            /// Description : Load final Approved  E658 List to MTO/OCT


            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();
            List<VME658Create> E658List = new List<VME658Create>();

            int pageSize = 0;
            int pageNumber = 1;
                       
            if (Session["UserLoginType"] != null)
            {
                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);

                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.RecordCertified);

                try
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        switch (userLoginType)
                        {
                            case (int)E658.Enum.EnumE658UserType.StaffCarUser:

                                string staffServiceNo = Session["LoginUser"].ToString();

                                var row1 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                    x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row1.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("StaffServiceNo") == staffServiceNo &&
                                      x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();
                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.FormationUser:

                                string UserEpaLoc = Session["UserEpassLoc"].ToString();
                                string UserEpassDivi = Session["UserEpassDivision"].ToString();

                                var row2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc &&
                                x.Field<string>("CreaterDivision") == UserEpassDivi && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row2.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("CreaterLoc") == UserEpaLoc && x.Field<string>("CreaterDivision") == UserEpassDivi
                                   && x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();
                                }

                                break;

                            case (int)E658.Enum.EnumE658UserType.MToOCT:
                            case (int)E658.Enum.EnumE658UserType.MTController:


                                string MToOctLocation = Session["MToOctLocation"].ToString();

                                var row3 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row3.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation &&
                                         x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();

                                    List<VME658Create> items = new List<VME658Create>();


                                    //var selectList = new SelectList(dt2.AsEnumerable().Where(x => x.Field<int>("RaisedTypeID") != (int)E658.Enum.E658RaisedType.StaffVehiE).Select(row => new SelectListItem
                                    //{
                                    //    Text = row["UnitSerialNo"].ToString(), // Change "TextColumn" to your actual column name
                                    //    Value = row["UnitSerialNo"].ToString() // Change "ValueColumn" to your actual column name
                                    //}), "Value", "Text");

                                    //ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));

                                }

                                break;
                            case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:

                                string MToOctLocation2 = Session["MToOctLocation"].ToString();

                                var row4 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 && x.Field<int>("RecordStatusID")
                                          == (int)E658.Enum.EnumRecordStatus.Forward);

                                if (row4.Any())
                                {
                                    dt2 = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation2 &&
                                         x.Field<int>("RecordStatusID") == (int)E658.Enum.EnumRecordStatus.Forward).CopyToDataTable();
                                }


                                break;

                            default:
                                break;
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }               

                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    VME658Create objList = new VME658Create();
                    objList.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
                    objList.FromLocID = dt2.Rows[i]["FromLocationFull"].ToString();
                    objList.ToLocId = dt2.Rows[i]["ToLocationFull"].ToString();
                    //objList.FromLocID = dt2.Rows[i]["FLocation"].ToString();
                    //objList.ToLocId = dt2.Rows[i]["TLocation"].ToString();
                    objList.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
                    objList.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
                    objList.E658RunType = dt2.Rows[i]["TypeName"].ToString();
                    objList.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);
                    objList.RoleID = Convert.ToInt32(dt2.Rows[i]["RoleID"]);
                    objList.UserGERMSLocation = dt2.Rows[i]["UserGERMSLocation"].ToString();
                    objList.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
                    objList.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
                    objList.EFTID = Convert.ToInt32(dt.Rows[i]["EFTID"]);

                    if (!Convert.IsDBNull(dt2.Rows[i]["IsCombineRun"]))
                    {
                        objList.IsCombineRun = Convert.ToInt32(dt2.Rows[i]["IsCombineRun"]);
                    }


                    E658List.Add(objList);

                }

                pageSize = 20;
                pageNumber = (page ?? 1);
                return View(E658List.ToPagedList(pageNumber, pageSize));

            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
        [HttpGet]
        public ActionResult CombineE658List(string sortOrder, string currentFilter, string searchString, int? page, int? RSID)
        {
            /// Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/05/29
            /// Description : Load E658 List to Different Users

            if (Session["MToOctLocation"] != null)
            {
                string mtoLoginLocation = Session["MToOctLocation"].ToString();

                var combineJobList = from s in _db.F658RegistryHeader
                                     join cr in _db.E658CreaterDetails
                                     on s.E658CreatorDltId equals cr.ECDID
                                     join rt in _db.E658RaisedType
                                     on cr.RaisedTypeID equals rt.RTID
                                     where s.IsCombineRun == 1 && cr.UserGERMSLocation == mtoLoginLocation
                                     orderby s.CreatedDate descending
                                     select s;

                List<VME658Create> combineList = new List<VME658Create>();

                foreach (var item in combineJobList)
                {
                    VME658Create obj = new VME658Create();
                    obj.UnitSerialNo = item.UnitSerialNo;
                    obj.FromLocID = item.ELocation;
                    obj.ToLocId = item.TLocation;
                    obj.E658Date = Convert.ToDateTime(item.PDate);
                    obj.JournryStartTime = Convert.ToDateTime(item.PTime);
                    obj.ECDID = Convert.ToInt32(item.E658CreatorDltId);
                    combineList.Add(obj);
                }


                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(combineList.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Index", "User");
            }



        }
        [HttpGet]
        public ActionResult CombineE658Details(string E658CreatorID, string RoleId)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/05/29
            /// Description : View Combine E658 Details
            /// 

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();

            List<VME658Create> e658DetailsList = new List<VME658Create>();
            List<VME658Create> childRune658DetailsList = new List<VME658Create>();
            string FromLocID = "";
            string ToLocID = "";

            ViewBag.DDL_GermsLocation = new SelectList(_db.Locations, "LocationName", "LocationName");

            //ViewBag.DDL_E658ListUnitNo = new SelectList(selectList.Select(x => x.Text));

            try
            {
                int CreID = Convert.ToInt32(E658CreatorID);
                int rID = Convert.ToInt32(RoleId);
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(CreID);

                int childE658CreatorId = Convert.ToInt32(_db.F658RegistryHeader.Where(x => x.ParentRunCretID == CreID && x.Active == 2).Select(x => x.E658CreatorDltId).FirstOrDefault());

                dt2 = objDALCommanQuery.CallE65SP(childE658CreatorId);

                /// Parent Run Details Dt
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    VME658Create objVME658Create = new VME658Create();

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
                    objVME658Create.RoleID = rID;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);

                    TempData["ECDID"] = E658CreatorID;


                    if (objVME658Create.IsOMTAvail == 1)
                    {
                        objVME658Create.OMTStatus = dt.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.OMTServiceNo = dt.Rows[i]["OMTNo"].ToString();
                    }

                    if (objVME658Create.IsVehicleAvail == 10)
                    {
                        objVME658Create.VehicleStatus = dt.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        objVME658Create.SLAFRegNo = dt.Rows[i]["SLAFRegNo"].ToString();
                    }

                    e658DetailsList.Add(objVME658Create);
                }

                /// Child Run Details Dt
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    VME658Create childobjVME658Create = new VME658Create();

                    FromLocID = dt2.Rows[i]["FLocation"].ToString();
                    ToLocID = dt2.Rows[i]["TLocation"].ToString();

                    childobjVME658Create.FromLocID = _db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();
                    childobjVME658Create.ToLocId = _db.Locations.Where(x => x.LocationID == ToLocID).Select(x => x.LocationName).FirstOrDefault();
                    childobjVME658Create.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
                    childobjVME658Create.E658RunType = dt2.Rows[i]["TypeName"].ToString();
                    childobjVME658Create.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
                    childobjVME658Create.ReturnDate = Convert.ToDateTime(dt2.Rows[i]["ReturnDate"]);
                    childobjVME658Create.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
                    childobjVME658Create.RequiredDuration = dt2.Rows[i]["PHrs"].ToString();
                    childobjVME658Create.Purpose = dt2.Rows[i]["Duty"].ToString();
                    childobjVME658Create.Route = dt2.Rows[i]["Route"].ToString();
                    childobjVME658Create.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
                    childobjVME658Create.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
                    childobjVME658Create.RoleID = rID;
                    childobjVME658Create.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);

                    TempData["ECDID"] = E658CreatorID;


                    if (childobjVME658Create.IsOMTAvail == 1)
                    {
                        childobjVME658Create.OMTStatus = dt2.Rows[i]["OMTAllocation"].ToString();
                    }
                    else
                    {
                        childobjVME658Create.OMTServiceNo = dt2.Rows[i]["OMTNo"].ToString();
                    }

                    if (childobjVME658Create.IsVehicleAvail == 10)
                    {
                        childobjVME658Create.VehicleStatus = dt2.Rows[i]["VehicleAllocation"].ToString();
                    }
                    else
                    {
                        childobjVME658Create.SLAFRegNo = dt2.Rows[i]["SLAFRegNo"].ToString();
                    }

                    childRune658DetailsList.Add(childobjVME658Create);

                    ViewBag.ChildE658DetailsList = childRune658DetailsList;
                }

                List<VME658Create> locations = GetRptLocation(CreID);

                TempData["RptLocation"] = locations;

                PartialView("_E658ReportLoc", locations);


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(e658DetailsList);
        }
        //[HttpGet]
        //public ActionResult Forward(string RecID)
        //{
        //    ///Created BY   : Sqn ldr Wicky
        //    /// Create Date : 2024/04/01
        //    /// Description : Data forward to user by user
        //    /// 
        //    try
        //    {
        //        var hashingService = new HashingService();

        //        // Decode the hashId back to the original string
        //        string decodedString = hashingService.DecodeHashId(RecID);

        //        // Split the decoded string to retrieve the original values
        //        var splitValues = decodedString.Split(':');
        //        int roleId = int.Parse(splitValues[0]);
        //        int creatorId = int.Parse(splitValues[1]);
                
               

        //        var e658Type = _db.E658CreaterDetails.Where(x => x.ECDID == creatorId && x.Active == 1).Select(x => new { x.RaisedTypeID, x.UserGERMSLocation }).FirstOrDefault();

        //        var mt658Details = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == creatorId && x.Active == 1).Select(x => new { x.OMTNo, x.SLAFRegNo }).FirstOrDefault();

        //        RaisedTypeID = Convert.ToInt32(e658Type.RaisedTypeID);

        //        flowList = RecordFlowMgtId(RaisedTypeID, roleId);

        //        foreach (var item in flowList)
        //        {
        //            EFID = item.EFMID;
        //            RID = item.RoleID;
        //        }

        //        E658FlowTransaction FlowTranc = new E658FlowTransaction();  //{};

        //        if (roleId == (int)E658.Enum.EnumE658UserType.MToOCT)
        //        {
        //            //if (mt658Details.OMTNo != "MT Sect" && mt658Details.SLAFRegNo != null )
        //            //{

        //            //}
        //            //else
        //            //{
        //            //    TempData["ErrMsg"] = "Please nominate OMT or Vehicle number to continue the process.";
        //            //    return RedirectToAction("E658List");
        //            //}

        //            FlowTranc.EFlowMgtID = EFID;
        //            FlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward;
        //            FlowTranc.RecordLocID = e658Type.UserGERMSLocation;
        //            FlowTranc.E658CreatorDltID = creatorId;
        //            FlowTranc.RoleID = RID;
        //            FlowTranc.Active = 1;
        //            FlowTranc.CreatedDate = DateTime.Now;
        //            FlowTranc.CreatedBy = (Session["LoginUser"]).ToString();
        //            FlowTranc.CreatedIP = this.Request.UserHostAddress;
        //            FlowTranc.CreatedMAC = mac.GetMacAddress();

        //            _db.E658FlowTransaction.Add(FlowTranc);

        //            if (_db.SaveChanges() > 0)
        //            {
        //                TempData["ScfMsg"] = "You have Forwarded the E658 Successfully.";

        //                return RedirectToAction("E658List");
        //            }
        //            else
        //            {
        //                return View();
        //            }

        //        }
        //        else
        //        {
        //            FlowTranc.EFlowMgtID = EFID;
        //            FlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Forward;
        //            FlowTranc.RecordLocID = e658Type.UserGERMSLocation;
        //            FlowTranc.E658CreatorDltID = creatorId;
        //            FlowTranc.RoleID = RID;
        //            FlowTranc.Active = 1;
        //            FlowTranc.CreatedDate = DateTime.Now;
        //            FlowTranc.CreatedBy = (Session["LoginUser"]).ToString();
        //            FlowTranc.CreatedIP = this.Request.UserHostAddress;
        //            FlowTranc.CreatedMAC = mac.GetMacAddress();

        //            _db.E658FlowTransaction.Add(FlowTranc);

        //            if (_db.SaveChanges() > 0)
        //            {
        //                TempData["ScfMsg"] = "You have Forwarded the E658 Successfully.";

        //                return RedirectToAction("E658List");
        //            }
        //            else
        //            {
        //                return View();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // return View();
        //        throw ex;
        //    }
        //}

        [HttpGet]
        public ActionResult Forward(int roleId, int E658CreatorDltId)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/04/01
            /// Description : Data forward to user by user
            /// 
            try
            {
                var e658Type = _db.E658CreaterDetails.Where(x => x.ECDID == E658CreatorDltId && x.Active == 1).Select(x => new { x.RaisedTypeID, x.UserGERMSLocation }).FirstOrDefault();

                var mt658Details = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == E658CreatorDltId && x.Active == 1).Select(x => new { x.OMTNo, x.SLAFRegNo }).FirstOrDefault();

                RaisedTypeID = Convert.ToInt32(e658Type.RaisedTypeID);

                flowList = RecordFlowMgtId(RaisedTypeID, roleId);

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
                        TempData["ScfMsg"] = "You have Forwarded the E658 Successfully.";

                        return RedirectToAction("E658List");
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
                        TempData["ScfMsg"] = "You have Forwarded the E658 Successfully.";

                        return RedirectToAction("E658List");
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
        private List<VME658Create> GetRptLocation(int E658CreatorID)
        {

            List<VME658Create> locList = new List<VME658Create>();

            var rptLoc = (from rh in _db.E658RptLocHeader
                          join rd in _db.E658RptLocDetails
                          on rh.ERLID equals rd.E658RptLocHedID
                          where rh.ECDID == E658CreatorID && rh.Active == 1
                          select new SelectListItem
                          {
                              Value = rd.LocationID.ToString(),
                              Text = rd.LocationID
                          }).ToList();


            foreach (var item in rptLoc)
            {
                VME658Create obj = new VME658Create();
                obj.reportedLoc = item.Value;

                locList.Add(obj);
            }
            return locList;
        }
        public string CreateUnitSerialNo(string FromLocID, int E658RunType)
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
        public static string GenerateRandomStringKey()
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
        public int EnterNew658RecordWithChassisNo(int ECDID, string ChassisNo, string VehicleNo, string VehicleAttachedLoc)
        {
            ///Create By   : Sqn ldr Wicky
            ///Create Date : 2021/05/21
            ///Description : get the all record related to ECDID from F6585Header and enter new record with new ChassisNo and Seq Number

            int status = 0;

            try
            {
                var oldE658Details = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == ECDID && x.Active == 1).ToList();

                //F658RegistryHeader obj = new F658RegistryHeader()

                string SeqNo = ChassisNo + "/" + DateTime.Now;

                foreach (var item in oldE658Details)
                {
                    F658RegistryHeader RegHeader = new F658RegistryHeader()
                    {
                        UnitSerialNo = item.UnitSerialNo,
                        ChassisNo = ChassisNo,
                        Seq = SeqNo,
                        E658CreatorDltId = ECDID,
                        PLocation = VehicleAttachedLoc,
                        ELocation = item.ELocation,
                        FLocation = item.FLocation,
                        TLocation = item.TLocation.Trim(),
                        IsRR658 = item.IsRR658,
                        AuthorityType = item.AuthorityType,
                        Behavior = item.Behavior,
                        Description = item.Description,
                        OMTNo = item.OMTNo,//OMServiceNo,
                        IsOMtReqFromMT = item.IsOMtReqFromMT,
                        IsVehicleReqFromMT = 0,
                        SLAFRegNo = VehicleNo,
                        Duty = item.Duty,
                        Section = item.Section,
                        SeniorPassanger = item.SeniorPassanger,
                        MyNav = item.MyNav,
                        MaxNav = item.MaxNav,
                        PDate = item.PDate,
                        PTime = item.PTime,
                        ReturnDate = item.ReturnDate,
                        PHrs = item.PHrs,
                        Status = item.Status,
                        Active = item.Active,
                        CreatedDate = item.CreatedDate,
                        CreatedUser = item.CreatedUser,
                        Route = item.Route,
                        ModifiedDate = DateTime.Now,
                        ModifiedUser = Convert.ToInt32(Session["LoginUser"]),

                    };

                    _db.F658RegistryHeader.Add(RegHeader);
                    if (_db.SaveChanges() > 0)
                    {
                        status = 1;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return status;

        }
        [HttpGet]
        public ActionResult Edit658(int E658CreatorID, int RoleId)
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
            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RunsTypes, "ERTID", "TypeName");

            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(E658CreatorID);
                ViewBag.DDL_E658RaisedType = (from rrt in _db.E658RaisedRunType
                                              join rt in _db.E658RaisedType
                                              on rrt.RaisedTypeID equals rt.RTID
                                              join runt in _db.E658RunsTypes
                                              on rrt.RunsTypeID equals runt.ERTID
                                              where rrt.RaisedTypeID == RaisedTypeID && rrt.Active == 1
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
                    objVME658Create.RoleID = RoleId;
                    objVME658Create.ECDID = Convert.ToInt32(dt.Rows[i]["E658CreatorDltId"]);

                    TempData["ECDID"] = E658CreatorID;


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

                List<VME658Create> locations = GetRptLocation(E658CreatorID);

                TempData["RptLocation"] = locations;

                //PartialView("_E658ReportLoc", locations);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(objVME658Create);
        }
        [HttpPost]
        public ActionResult Edit658(VME658Create objE658)
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
                RaisedTypeID = Convert.ToInt32(Session["E658RunType"]);
                string createUnitSerialNo = CreateUnitSerialNo(FromLocID, RaisedTypeID);

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
                        return Json(new { success = true, redirectUrl = Url.Action("E658List", new { E658CreatorID = objE658.ECDID, RoleId = 0 }) });
                    }
                    else
                    {
                        TempData["ScfMsg"] = "You have Update the E658 Successfully.";
                        return Json(new { success = true, redirectUrl = Url.Action("E658Details", new { E658CreatorID = objE658.ECDID, RoleId = 0, EFlowId = 0 }) });
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

        #region JsonFunction

        public JsonResult UpdateCombineRun(VME658Create objE658)
        {

            string message = "";

            try
            {
                var ToLocID = _db.Locations.Where(x => x.LocationName == objE658.ToLocId).Select(x => x.LocationID).FirstOrDefault();

                var f658PK = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == objE658.ECDID).Select(x => new { x.UnitSerialNo, x.ChassisNo, x.Seq }).FirstOrDefault();

                var existingModel = _db.F658RegistryHeader.Find(f658PK.UnitSerialNo, f658PK.ChassisNo, f658PK.Seq);

                TimeSpan StartTime = objE658.JournryStartTime.TimeOfDay;

                if (existingModel != null)
                {
                    existingModel.TLocation = ToLocID;
                    existingModel.ReturnDate = objE658.ReturnDate;
                    existingModel.PTime = StartTime.ToString();
                    existingModel.Duty = objE658.Purpose;
                    existingModel.Route = objE658.Route;
                    existingModel.ModifiedDate = DateTime.Now;
                    existingModel.ModifiedUser = Convert.ToInt32(Session["LoginUser"]);

                    if (_db.SaveChanges() > 0)
                    {
                        message = "Record updated Sucefully.";
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }
        public JsonResult CombineRun(string rowE658RefNo, string dpE658RefNo, string rawE658RunType)
        {
            ///Created BY   : Sqn ldr Wickramasinghe
            ///Created Date : 2024/05/28
            /// Description : run combine function

            string message = "";

            var data = new VME658Create();
            try
            {
                string rightrowE658RefNo = rowE658RefNo.Trim();

                if (rowE658RefNo.Trim() != dpE658RefNo)
                {
                    var e658CreatorID = _db.F658RegistryHeader.Where(x => x.UnitSerialNo == dpE658RefNo.Trim() && x.Active == 1).Select(x => x.E658CreatorDltId).FirstOrDefault();


                    var row658Details = _db.F658RegistryHeader.Where(x => x.UnitSerialNo == rightrowE658RefNo && x.Active == 1).Select(x => new { x.ChassisNo, x.Seq, x.E658CreatorDltId }).FirstOrDefault();

                    var select658Details = _db.F658RegistryHeader.Where(x => x.UnitSerialNo == dpE658RefNo.Trim() && x.Active == 1).Select(x => new { x.ChassisNo, x.Seq, x.E658CreatorDltId }).FirstOrDefault();

                    F658RegistryHeader obj = _db.F658RegistryHeader.Find(rightrowE658RefNo, row658Details.ChassisNo, row658Details.Seq);
                    F658RegistryHeader obj2 = _db.F658RegistryHeader.Find(dpE658RefNo.Trim(), select658Details.ChassisNo, select658Details.Seq);

                    if (obj != null)
                    {
                        obj.IsCombineRun = 1;
                        obj.ModifiedDate = DateTime.Now;
                        obj.ModifiedUser = Convert.ToInt32(Session["LoginUser"]);

                        _db.Entry(obj).State = EntityState.Modified;

                    }

                    if (obj2 != null)
                    {
                        obj2.ParentRunCretID = row658Details.E658CreatorDltId;
                        obj2.Active = 2;
                        obj.ModifiedDate = DateTime.Now;
                        obj.ModifiedUser = Convert.ToInt32(Session["LoginUser"]);

                        _db.Entry(obj).State = EntityState.Modified;

                    }

                    if (_db.SaveChanges() > 0)
                    {
                        message = "Run combine process has completed.";
                    }
                    else
                    {
                        message = "Process has not completed. Please Contact the IT Division.";
                    }

                    data.ECDID = Convert.ToInt32(row658Details.E658CreatorDltId);
                    data.RoleID = (int)Enum.EnumE658UserType.MToOCT;
                    data.Message = message;

                }
                else
                {
                    message = "Same Ref No. Please select the correct Ref No which need to combine";
                    data.Message = message;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }



            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateOMTRecord(string id, int ECDID)
        {
            string message = "";
            ///Create By   : Sqn ldr Wicky
            ///Create Date : 2020/10/20
            ///Description : Make a session with Job Assign Team Members
            var f658PK = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == ECDID).Select(x => new { x.UnitSerialNo, x.ChassisNo, x.Seq }).FirstOrDefault();

            var existingModel = _db.F658RegistryHeader.Find(f658PK.UnitSerialNo, f658PK.ChassisNo, f658PK.Seq);

            if (existingModel != null)
            {

                existingModel.OMTNo = id;
                existingModel.IsOMtReqFromMT = 0;
                existingModel.ModifiedDate = DateTime.Now;
                existingModel.ModifiedUser = Convert.ToInt32(Session["LoginUser"]);

                if (_db.SaveChanges() > 0)
                {
                    message = "OMT Service Number Successfully Updated";
                }

            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }
        public JsonResult UpdateVehicleRecord(string id, int ECDID, string ChassisNo, string VehicleAttachedLoc)
        {

            ///Create By   : Sqn ldr Wicky
            ///Create Date : 2020/10/20
            ///Description : Make a session with Job Assign Team Members

            string message = "";

            var f658PK = _db.F658RegistryHeader.Where(x => x.E658CreatorDltId == ECDID).Select(x => new { x.UnitSerialNo, x.ChassisNo, x.Seq }).FirstOrDefault();

            int status = EnterNew658RecordWithChassisNo(ECDID, ChassisNo, id, VehicleAttachedLoc);

            F658RegistryHeader obj = _db.F658RegistryHeader.Find(f658PK.UnitSerialNo, f658PK.ChassisNo, f658PK.Seq);

            if (obj != null && status == 1)
            {
                _db.F658RegistryHeader.Remove(obj);
                _db.SaveChanges();

                message = "Vehicle Number Successfully Updated";
            }
            else
            {
                message = "Something error, Please contact IT Division.";
            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });

        }
        public JsonResult LoadLocation(string id)
        {
            ///Create By   : Flt Lt Wicky
            ///Create Date : 2020/10/20
            ///Description : Make a session with Job Assign Team Members

            List<Cls_ItemList> lst_ListPartItem = new List<Cls_ItemList>();
            Cls_ItemList obj = new Cls_ItemList();
            obj.LocName = _db.Locations.Where(x => x.LocationName == id).Select(x => x.LocationID).FirstOrDefault();
            if (Session["ListPartItem"] != null)
            {
                lst_ListPartItem = (List<Cls_ItemList>)Session["ListPartItem"];

                lst_ListPartItem.Add(obj);
                Session["ListPartItem"] = lst_ListPartItem;
                return Json(1);
            }
            else
            {
                lst_ListPartItem.Add(obj);
                Session["ListPartItem"] = lst_ListPartItem;
                return Json(1);
            }
        }
        public JsonResult getVehicleChassisNo(string id)
        {
            ///Create By   : Flt Lt Wicky
            ///Create Date : 2024/05/20
            ///Description : load VehicleChassisNo in to text box

            //finalStatus ////
            //2 Servisable , 3 Differ , 20 Odometer Differ , 18 Accident Differ/////

            /// Status ////

            var sysChassisNo = _db.VehicleDetails.Where(x => x.SlafRegNo.Contains(id) && (x.Status == 1 || x.Status == 2 || x.Status == 3))
                              .Select(x => new { x.ChassisNo, x.AttachedLocationID }).FirstOrDefault();

            return Json(sysChassisNo, JsonRequestBehavior.AllowGet);

        }
        public JsonResult getWayPoints(string FromLoc, string ToLoc)
        {
            ///Create By   : Sqn Ldr Wickramasinghe
            ///Create Date : 2024/06/28
            ///Description : load the way points to the way points table related to the From Location

            var fromLocId = _db.Locations.Where(x => x.LocationName == FromLoc).Select(x => x.LocationID).FirstOrDefault();
            var toLocId = _db.Locations.Where(x => x.LocationName == ToLoc).Select(x => x.LocationID).FirstOrDefault();

            var wayPoints = _db.E658WayPoints.Where(x => x.FromLocation == fromLocId && x.ToLocation == toLocId && x.Active == 1).Select(x => x.WayPoints).FirstOrDefault();

            if (wayPoints != null)
            {
                return Json(wayPoints, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(wayPoints, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult RejectRecord(string rejectComment, int ECDID, int EFTID, int RoleId)
        {
            ///Created BY   : Sqnl ldr WAKY Wickramasinghe 
            ///Created Date : 2024/07/03
            /// Description : this function is to reject the record

            VME658Create model = new VME658Create();

            string message = "";

            //string PreviousFlowStatus_UserRole;

            var raisedTypeInfo = _db.E658CreaterDetails.Where(x => x.ECDID == ECDID && x.Active == 1).Select(x => new { x.UserGERMSLocation, x.RaisedTypeID })
                                 .FirstOrDefault();
            var rejectStatusRoleId = _db.E658FlowMagt.Where(x => x.RoleID == RoleId && x.RaisedTyId == raisedTypeInfo.RaisedTypeID && x.Active == 1)
                                    .Select(x => x.RejectStatus).FirstOrDefault();

            var rejectRoleFlowMgtId = _db.E658FlowMagt.Where(x => x.RoleID == rejectStatusRoleId && x.RaisedTyId == raisedTypeInfo.RaisedTypeID && x.Active == 1)
                                    .Select(x => x.EFMID).FirstOrDefault();

            E658FlowTransaction objFlowTranc = new E658FlowTransaction();

            objFlowTranc.EFlowMgtID = rejectRoleFlowMgtId;
            objFlowTranc.RecordStatusID = (int)E658.Enum.EnumRecordStatus.Reject;
            objFlowTranc.E658CreatorDltID = ECDID;
            objFlowTranc.RoleID = rejectStatusRoleId;
            objFlowTranc.RecordLocID = raisedTypeInfo.UserGERMSLocation;
            objFlowTranc.Comment = rejectComment;
            objFlowTranc.Active = 1;
            objFlowTranc.CreatedDate = DateTime.Now;
            objFlowTranc.CreatedIP = this.Request.UserHostAddress;
            objFlowTranc.CreatedMAC = mac.GetMacAddress();
            objFlowTranc.CreatedBy = Session["LoginUser"].ToString();

            _db.E658FlowTransaction.Add(objFlowTranc);

            E658CreaterDetails objCreator = _db.E658CreaterDetails.Find(ECDID);
            objCreator.Active = 2;
            objCreator.ModifiedDate = DateTime.Now;
            objCreator.ModifiedMAC = mac.GetMacAddress();
            objCreator.ModifiedBy = Session["LoginUser"].ToString();


            _db.Entry(objCreator).State = EntityState.Modified;

            if (_db.SaveChanges() > 0)
            {
                message = "Successfully Rejected the E658";

                //return RedirectToAction("RejectIndex");
            }
            else
            {
                message = "Process Unsuccessful.Try again...";
            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }
        public JsonResult getUserInfo(string id)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date  : 2024/02/14
            /// Description : Load Full name details according to the service number and check the OMT is assign to vechile or not

            string message = "";
            Vw_PersonalDetail obj_VwPersonalProfile = new Vw_PersonalDetail();

            //the service number and check the OMT is assign to vechile or not
            // Status 50 and Status 70 are the status of Pending Run
            //var oMTStatus = _db.F658RegistryHeader.Where(x => x.OMTNo == id && (x.Status == 50 || x.Status == 70)).Count();

            //if (oMTStatus == 0)
            //{

            //}
            //else
            //{

            //    message = "This OMT has been allocated to a journey.Please enter another OMT service Number.";

            //    return Json(new { Message = message, JsonRequestBehavior.AllowGet });
            //}

            var count = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == id).Count();
            if (count == 1)
            {
                obj_VwPersonalProfile = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == id).FirstOrDefault();

            }
            else
            {
                obj_VwPersonalProfile.Name = "Service Number is not valid.";
            }


            return Json(obj_VwPersonalProfile, JsonRequestBehavior.AllowGet);

        }

        #endregion


    }
}