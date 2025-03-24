using E658.Controllers;
using E658.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WRMS.Models;

namespace E658.Controllers
{
    public class RoutineRunController : Controller
    {
        // GET: RoutineRun
        db_ContextEpass _dbEpass = new db_ContextEpass();
        dbContext _db = new dbContext();
        private dbContextCommonData _dbCommonData = new dbContextCommonData();
        MacAddress mac = new MacAddress();
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult RRList(string LocationId, string DivisionId, VME658InitiateUser objVME658InitiateUser)
        {
            ///Created BY   : Sqn ldrPriya
            /// Create Date : 2024/02/15
            /// Description : load rr Creater View

            if (LocationId == "RMA")
            {
                LocationId = "RMT";
            }
            if (LocationId == "MRG")
            {
                LocationId = "MGR";
            }
            if (LocationId == null && DivisionId == null)
            {
                string LoginServiceNo = Session["LoginUser"].ToString();
                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
                string SNo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == LoginServiceNo && x.Service_Status == 1024).Select(x => x.SNo).FirstOrDefault();
                string Location = _db.E658UserMgt.Where(x => x.SNo == SNo && x.Active == 1).Select(x => x.UserLocation).FirstOrDefault();
                ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RaisedType, "RTID", "TypeName");
                ViewBag.DDL_EpasLocation = new SelectList(_dbCommonData.EpasLocations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
                ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

                if (userLoginType == (int)E658.Enum.EnumE658UserType.FormationUser)
                {
                    ViewBag.RRList = _db.Vw_RRFlowStatusManagement.Where(x => (x.SubmitServiceNo == LoginServiceNo && x.status == 1 && x.DivisionId == 2)).OrderBy(x => x.SeqId).ToList();
                }
                if (userLoginType == (int)E658.Enum.EnumE658UserType.MToOCT)
                {
                    ViewBag.RRList = _db.Vw_RRFlowStatusManagement.Where(x => (x.UserRoleId == (int)E658.Enum.EnumE658UserType.MToOCT && x.status == 1 && x.DivisionId == 2 && x.Location == Location)).OrderBy(x => x.SeqId).ToList();
                }
                if (userLoginType == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization)
                {
                    ViewBag.RRList = _db.Vw_RRFlowStatusManagement.Where(x => (x.UserRoleId == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization && x.status == 1 && x.DivisionId == 2 && x.Location == Location)).OrderBy(x => x.SeqId).ToList();
                }
            }
            else
            {

                {
                    ViewBag.RRList = _db.RRHeaders.Where(x => (x.Location == LocationId && x.Division == DivisionId && x.status == 1 && x.DivisionId == null)).OrderBy(x => x.SeqId).ToList();
                }
            }
            return View();
        }

        //rr forward
        [HttpPost]
        public ActionResult RRList(int[] id, string ServiceNo)

        ///Created By   : Flt Lt Priyantha
        ///Created Date :2024.01.06
        /// Description : Index Page for Forward Load Person Contact Details 

        {

            RRHeader objAssetPsnInfo = new RRHeader();
            RRFlowManagementStatu objAssestFlowManagementStatu = new RRFlowManagementStatu();

            int SubmitStatus = (int)E658.Enum.EnumE658UserType.FormationUser;
            if (ServiceNo == null)
            {
                string LoginServiceNo = Session["LoginUser"].ToString();
                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
                SubmitStatus = Convert.ToInt32(_db.E658FlowMagt.Where(x => x.RaisedTyId == 11 && x.RoleID == userLoginType && x.Active == 1).Select(x => x.SubmitStatus).FirstOrDefault());
            }


            foreach (int IDs in id)
            {
                DateTime CurrentDate = DateTime.Now;
                int currentMonth = CurrentDate.Month;

                int userLoginType = Convert.ToInt32(Session["UserLoginType"]);
                int sno11 = _db.RRHeaders.Where(x => x.SeqId == IDs).Select(x => x.SeqId).FirstOrDefault();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {

                    if ((userLoginType == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization))
                    {
                        var APIID1 = _db.RRHeaders.Where(x => x.SeqId == sno11 && x.status == 1).FirstOrDefault();
                        if (currentMonth == APIID1.CurrentMonth)
                        {
                            objAssetPsnInfo = _db.RRHeaders.Find(APIID1.SeqId);
                            objAssetPsnInfo.DivisionId = null;
                            if (currentMonth == 12)
                            {
                                objAssetPsnInfo.NextMonth = 1;
                            }
                            else
                            {
                                objAssetPsnInfo.NextMonth = currentMonth + 1;
                            }


                        }


                        objAssetPsnInfo = _db.RRHeaders.Find(APIID1.SeqId);
                        objAssetPsnInfo.DivisionId = null;
                        objAssetPsnInfo.CurrentMonth = currentMonth;
                    }
                    else
                    {
                        var APIID1 = _db.RRHeaders.Where(x => x.SeqId == sno11 && x.status == 1).FirstOrDefault();

                        objAssetPsnInfo = _db.RRHeaders.Find(APIID1.SeqId);
                        objAssetPsnInfo.DivisionId = 2;

                    }


                    _db.Entry(objAssetPsnInfo).State = EntityState.Modified;

                    if (_db.SaveChanges() > 0)
                    {
                        scope.Complete();
                        TempData["ScfMsg"] = "Record Sucessfully Forwarded.";

                    }
                    else
                    {
                        TempData["ErrMsg"] = "Process Unsucessfull.";
                        scope.Dispose();
                    }

                }
                var APIID = _db.RRHeaders.Where(x => x.SeqId == sno11 && x.status == 1).FirstOrDefault();
                objAssestFlowManagementStatu.UserRoleId = SubmitStatus;
                objAssestFlowManagementStatu.EstablishmentId = APIID.Location;
                objAssestFlowManagementStatu.DivisionId = APIID.Division;
                objAssestFlowManagementStatu.Active = 1;
                objAssestFlowManagementStatu.SubmitServiceNo = ServiceNo;
                objAssestFlowManagementStatu.RRid = APIID.SeqId;
                objAssestFlowManagementStatu.Rsid = 1000;
                _db.RRFlowManagementStatus.Add(objAssestFlowManagementStatu);
                _db.SaveChanges();

            }
            // return RedirectToAction("IndexAssessClk", "Assessment");

            if (ServiceNo == null)
            {
                TempData["ScfMsg"] = "Record Sucessfully Forwarded.";
                return Json(new { success = true, redirectUrl = Url.Action("RRList") });

            }
            TempData["ScfMsg"] = "Record Sucessfully Forwarded.";
            return Json(new { success = true, redirectUrl = Url.Action("RRInitiateUser") });


        }


        //rr forwars






        [HttpGet]
        public ActionResult RRInitiateUser()
        {
            ///Created BY   : Sqn ldr Priya
            /// Create Date : 2024/02/15
            /// Description : load RR Creater View

            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RaisedType, "RTID", "TypeName");
            ViewBag.DDL_EpasLocation = new SelectList(_dbCommonData.EpasLocations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RRInitiateUser(VME658InitiateUser obj, string LocationId, string DivisionId)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/15
            /// Description : load E658 Creater View
            /// 

            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RaisedType, "RTID", "TypeName");
            ViewBag.DDL_EpasLocation = new SelectList(_dbCommonData.EpasLocations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            E658CreaterDetails objCreater = new E658CreaterDetails();



            return View();
        }


        public JsonResult FromLocation(string id)
        {
            ///Created BY   : sqn ldr pkp priyantha
            ///Created Date : 2021/02/16
            /// Description : Load division

            List<Location> FromEstablishment = new List<Location>();
            var FromLocationList = this.LinqFromLocation(id);


            var divisionListData = FromLocationList
                .GroupBy(x => x.Division)
                .Select(x => new SelectListItem()
                {
                    Text = x.Key.ToString(),
                    Value = x.Key.ToString(),
                });


            return Json(divisionListData, JsonRequestBehavior.AllowGet);
        }
        //public IList<Vw_EpasDivision> LinqFromLocation(string id)
        public IList<RRHeader> LinqFromLocation(string id)
        {
            ///Created BY   : sqn ldr pkp priyantha
            ///Created Date : 2021/02/16
            /// Description : Load division
            if (id == "RMA")
            {
                id = "RMT";
            }
            if (id == "MRG")
            {
                id = "MGR";
            }
            List<RRHeader> Result = new List<RRHeader>();
            //Result = _dbCommonData.Vw_EpasDivision.Where(x => x.LocationID == id).ToList();
            Result = _db.RRHeaders.Where(x => (x.Location == id && x.status == 1)).ToList();
            return Result;
        }
    }
}