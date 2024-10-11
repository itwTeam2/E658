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

namespace WRMS.Controllers
{
    public class UserController : Controller
    {
        private dbContext _db = new dbContext();
        dbContextCommonData _dbCommonData = new dbContextCommonData();
        db_ContextEpass _dbEpass = new db_ContextEpass();
        MacAddress mac = new MacAddress();
        
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 27/02/24
            /// Des : Login Function for E658

            try
            {
                ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x=>x.Active == 1), "EUTID", "E658Users");
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(_UserLogin obj_UserLogin)                                                                                                                                
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 27/02/24
            /// Des : 

            ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x=>x.Active == 1), "EUTID", "E658Users");

            try
            {
                if (ModelState.IsValid)
                {
                    byte[] EncriptPwd = EncriptPassword(obj_UserLogin.Password);
                    var dbPword = _dbEpass.EpasUsers.Where(r => r.userid == obj_UserLogin.UserName).Select(q => q.password).FirstOrDefault();
                    var v = _dbEpass.EpasUsers.Where(a => a.userid.Equals(obj_UserLogin.UserName) && dbPword == EncriptPwd).FirstOrDefault();
                    string ModifyServiceNo = CheckingVolAirWoman(obj_UserLogin.UserName);

                    var loginUserSNo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == ModifyServiceNo).Select(x => new { x.SNo, x.service_type }).FirstOrDefault();

                    if (v != null)
                    {
                        Session["UserLoginType"] = obj_UserLogin.EUTID;
                        switch (obj_UserLogin.EUTID)
                        {
                            case (int)E658.Enum.EnumE658UserType.FormationUser:

                                if (loginUserSNo.service_type == 1001 || loginUserSNo.service_type == 1002 || loginUserSNo.service_type == 1003 || loginUserSNo.service_type == 1004)
                                {
                                    var userLoginDeatils = _dbEpass.EpasUsers.Where(x => x.userid == obj_UserLogin.UserName).Select(x => new { x.KitIssParentUnit, x.DivisionName }).FirstOrDefault();

                                    Session["UserEpassLoc"] = userLoginDeatils.KitIssParentUnit;
                                    Session["UserEpassDivision"] = userLoginDeatils.DivisionName;
                                    Session["LoginUser"] = ModifyServiceNo; // obj_UserLogin.UserName;


                                    return RedirectToAction("E658List", "E658");
                                }
                                else
                                {
                                    TempData["Error"] = "You ara not allowed to login using this user type.";
                                }
                                break;
                            case (int)E658.Enum.EnumE658UserType.StaffCarUser:

                                Session["LoginUser"] = ModifyServiceNo; // obj_UserLogin.UserName;
                                return RedirectToAction("E658List", "E658");
                               
                            case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:
                            case (int)E658.Enum.EnumE658UserType.MToOCT:

                                var loginUserInfo = _db.E658UserMgt.Where(x => x.SNo == loginUserSNo.SNo && x.Active == 1 && x.IsTemHandOverStatus == 1)
                                                   .Select(x => new { x.IsTemHandOverStatus, x.HandOverPsnSNo, x.HandOverFromDate, x.HandOverToDate,x.EUMID ,x.UserLocation}).FirstOrDefault();

                                if (loginUserInfo != null)
                                {
                                    DateTime currentDate = DateTime.Today;
                                    DateTime? handOverFrom = loginUserInfo.HandOverFromDate;
                                    DateTime? handOverTo = loginUserInfo.HandOverToDate;

                                    if (loginUserInfo.IsTemHandOverStatus == 1)
                                    {
                                        if (currentDate >= handOverFrom && currentDate <= handOverTo)
                                        {
                                            var serviceNo = _db.Vw_PersonalDetail.Where(x => x.SNo == loginUserInfo.HandOverPsnSNo).Select(x => x.ServiceNo).FirstOrDefault();
                                            TempData["Error"] = "Your Account Temporally Handover to '" + serviceNo + "' Service Number and handover duration '"+ handOverFrom + "' to '"+ handOverTo + "'. ";
                                        }
                                        else
                                        {
                                            Session["LoginUser"] = ModifyServiceNo;
                                            Session["MToOctLocation"] = loginUserInfo.UserLocation;

                                            E658UserMgt objUserMgt = _db.E658UserMgt.Find(loginUserInfo.EUMID);
                                            objUserMgt.IsTemHandOverStatus = 0;
                                            objUserMgt.HandOverFromDate = null;
                                            objUserMgt.HandOverToDate = null;
                                            objUserMgt.HandOverPsnSNo = null;
                                            objUserMgt.ModifiedBy = Session["LoginUser"].ToString();
                                            objUserMgt.ModifiedDate = DateTime.Now;
                                            objUserMgt.ModifiedMac = mac.GetMacAddress();

                                            _db.Entry(objUserMgt).State = EntityState.Modified;

                                            _db.SaveChanges();

                                            
                                            return RedirectToAction("E658List", "E658");
                                        }
                                    }
                                }                          
                                else
                                {
                                    var loginUserInfo2 = _db.E658UserMgt.Where(x => x.SNo == loginUserSNo.SNo && x.Active == 1 && x.RoleID == obj_UserLogin.EUTID)
                                                  .Select(x => new { x.UserLocation }).FirstOrDefault();

                                    if (loginUserInfo2 != null)
                                    {
                                        Session["LoginUser"] = ModifyServiceNo;
                                        Session["MToOctLocation"] = loginUserInfo2.UserLocation;
                                        return RedirectToAction("Dashboardv1ToMTO", "Dashboard");
                                    }
                                    else
                                    {
                                        TempData["Error"] = "No Permission to login";
                                    }

                                    
                                }

                                break;
                            case (int)E658.Enum.EnumE658UserType.MTController:

                                DateTime Today = DateTime.Now.Date;
                                var userSno  = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == obj_UserLogin.UserName).Select(x=>x.SNo).FirstOrDefault();
                                var userAvailability = _db.E658UserMgt.Where(x => x.SNo == userSno.ToString() && x.MTCotrollerDutyDate == Today).FirstOrDefault();

                                if (userAvailability != null)
                                {
                                    Session["LoginUser"] = ModifyServiceNo;
                                    Session["MToOctLocation"] = userAvailability.UserLocation;

                                    return RedirectToAction("Dashboardv1ToMTO", "Dashboard");
                                }
                                else
                                {
                                    TempData["Error"] = "You are not a MT Controller today.";
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        TempData["Error"] = "The User Name/Password you've entered is incorrect";
                    }                 

                }

                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public byte[] EncriptPassword(string Passwd)
        {
            MD5CryptoServiceProvider MD5Pass = new MD5CryptoServiceProvider();
            byte[] HashPass;
            UTF8Encoding Encoder = new UTF8Encoding();
            HashPass = MD5Pass.ComputeHash(Encoder.GetBytes(Passwd));
            return HashPass;
        }
        public string CheckingVolAirWoman(string UID)
        {
            ///Created BY: Flt Lt WAKY Wickramasinghe
            ///Created Date : 2024/08/20
            /// Description : check the vol,and OC numbers

            string subSvcNo = UID.Substring(0, 3);

            if (subSvcNo == "VAW" || subSvcNo == "vaw")
            {
                subSvcNo = UID.Substring(1);
            }
            else if (subSvcNo == "OC/" || subSvcNo == "oc/")
            {
                subSvcNo = UID.Substring(3);
            }
            else
            {
                subSvcNo = UID;
            }
            return subSvcNo;
        }
        [HttpGet]
        public ActionResult UserMgtCreate()
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 05/04/24
            /// Des : User Creation for E658

            try
            {
                ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x => x.Active == 1), "EUTID", "E658Users");
                ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }
        [HttpPost]
        public ActionResult UserMgtCreate(_UserManagementE658 obj_UserManagementE658)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 16/04/24
            /// Des : Save Function of Users 

            ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x => x.Active == 1), "EUTID", "E658Users");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            var accountCount = _db.E658UserMgt.Where(x => x.SNo == obj_UserManagementE658.SNo && x.Active == 1).Count();

            if (accountCount == 0)
            {
                E658UserMgt obj = new E658UserMgt()
                {
                    SNo = obj_UserManagementE658.SNo.ToString(),
                    RoleID = obj_UserManagementE658.RLEID,
                    UserLocation = obj_UserManagementE658.Location,
                    Active = 1,
                    CreatedBy = Session["LoginUser"].ToString(),
                    CreatedDate = DateTime.Now,
                    CreatedMAC = mac.GetMacAddress(),
                    CreatedIP = this.Request.UserHostAddress,
                };

                _db.E658UserMgt.Add(obj);

                if (_db.SaveChanges() > 0)
                {
                    TempData["ScfMsg"] = "You have created the user Account Successfully.";
                }
                else
                {
                    TempData["ErrMsg"] = "Something wrong, Please contact IT division";
                }
            }
            else
            {
                TempData["ErrMsg"] = "You've already registered with this service number.Please make changes to your existing account or delete it before proceeding.";
            }
           

            return View();


            //if (Session["Location"] != null)
            //{


            //}
            //else
            //{
            //    return RedirectToAction("Login", "User");
            //}            
        }
        public ActionResult CreatedUserList(string sortOrder, string currentFilter, string SearchString, int? page, int? RSID)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 05/04/24
            /// Des : Load Created User List

            try
            {
                //string Location = Session["Location"].ToString();
                //int UserRole = Convert.ToInt32(Session["UserRole"]);
                //TempData["Location"] = Location;
                //TempData["UserRole"] = UserRole;

                List<_UserManagementE658> UserList = new List<_UserManagementE658>();

                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "JobNo" : "";
                string personSNo = "";

                if (!String.IsNullOrEmpty(SearchString))
                {
                    page = 1;
                    personSNo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == SearchString).Select(x => x.SNo).FirstOrDefault().ToString();
                }
                else
                {
                    SearchString = currentFilter;
                }

                ViewBag.CurrentFilter = SearchString;

                var UsersList = from s in _db.E658UserMgt
                                join pd in _db.Vw_PersonalDetail
                                on s.SNo equals pd.SNo
                                join ut in _db.E658UserType
                                on s.RoleID equals ut.EUTID
                                where s.Active == 1
                                select new
                                {
                                    SeviceNo = pd.ServiceNo,
                                    Rank = pd.Rank,
                                    FullName = pd.Name,
                                    UserRole = ut.E658Users,
                                    Location = s.UserLocation,
                                    CreatedDate = s.CreatedDate,
                                    EUMID = s.EUMID,
                                    Sno = s.SNo
                                };

                if (String.IsNullOrEmpty(SearchString))
                {
                    

                    foreach (var item in UsersList)
                    {
                        _UserManagementE658 objList = new _UserManagementE658();
                        objList.ServiceNo = item.SeviceNo;
                        objList.Rank = item.Rank;
                        objList.FullName = item.FullName;
                        objList.Location = item.Location;
                        objList.UserRole = item.UserRole;
                        objList.EUMID = item.EUMID;
                        UserList.Add(objList);
                    }
                }
                


                if (!String.IsNullOrEmpty(SearchString))
                {
                    UsersList = UsersList.Where(s => s.Sno == personSNo);

                    foreach (var item in UsersList)
                    {
                        _UserManagementE658 objList = new _UserManagementE658();
                        objList.ServiceNo = item.SeviceNo;
                        objList.Rank = item.Rank;
                        objList.FullName = item.FullName;
                        objList.Location = item.Location;
                        objList.UserRole = item.UserRole;
                        objList.EUMID = item.EUMID;
                        UserList.Add(objList);
                    }
                    SearchString = null;
                }
                //switch (sortOrder)
                //{
                //    case "Service No":
                //        UsersList = UsersList.OrderByDescending(s => s.SeviceNo);
                //        break;
                //    case "Rank":
                //        UsersList = UsersList.OrderBy(s =>s.Rank);
                //        break;
                //    case "Name":
                //        UsersList = UsersList.OrderBy(s => s.FullName);
                //        break;
                //    case "UserRole":
                //        UsersList = UsersList.OrderBy(s => s.UserRole);
                //        break;
                //    case "Location":
                //        UsersList = UsersList.OrderBy(s => s.Location);
                //        break;
                //    default:  // Name ascending 
                //        UsersList = UsersList.OrderByDescending(s => s.CreatedDate);
                //        break;
                //}

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(UserList.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        public ActionResult Delete(int EUMID)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 16/04/24
            /// Des : Update User status into 0

            E658UserMgt obj = _db.E658UserMgt.Find(EUMID);
            obj.Active = 0;
            obj.ModifiedDate = DateTime.Now;
            obj.ModifiedBy = Session["LoginUser"].ToString();
            obj.ModifiedMac = mac.GetMacAddress();
            _db.Entry(obj).State = EntityState.Modified;

            if (_db.SaveChanges() > 0)
            {
                TempData["ScfMsg"] = "Record Successfully deleted.";
            }
            else
            {
                TempData["ErrMsg"] = "Something wrong, Please contact IT division";
            }
            return RedirectToAction("CreatedUserList");
        }
        [HttpGet]
        public ActionResult Edit(int EUMID)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 05/04/24
            /// Des : Edit Created User List

            ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x => x.Active == 1), "EUTID", "E658Users");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            _UserManagementE658 mode = new _UserManagementE658();

            try
            {
                var userInfo = _db.E658UserMgt.Where(x => x.EUMID == EUMID && x.Active == 1).Select(x => new { x.SNo,x.UserLocation,x.RoleID } ).FirstOrDefault();
                var servicePsnInfo = _db.Vw_PersonalDetail.Where(x => x.SNo == userInfo.SNo).Select(x => new { x.ServiceNo, x.Name, x.Rank }).FirstOrDefault();
                var userRoleName = _db.E658UserType.Where(x => x.EUTID == userInfo.RoleID).Select(x => x.E658Users).FirstOrDefault();


                mode.ServiceNo = servicePsnInfo.ServiceNo;
                mode.Rank = servicePsnInfo.Rank;
                mode.FullName = servicePsnInfo.Name;
                mode.Location = userInfo.UserLocation;
                mode.UserRole = userRoleName;
                mode.EUMID = EUMID;

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View(mode);
        }
        [HttpPost]
        public ActionResult Edit(_UserManagementE658 obj)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 05/04/24
            /// Des : Edit Created User List

            ViewBag.DDL_E658UserType = new SelectList(_db.E658UserType.Where(x => x.Active == 1), "EUTID", "E658Users");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            _UserManagementE658 mode = new _UserManagementE658();

            E658UserMgt objE658UserMgt = new E658UserMgt();

            try
            {
                if (obj.RLEID == null && obj.EditLocation == null)
                {
                    TempData["ErrMsg"] = "Please select required fields";
                    return View();
                }
                else
                {
                    if (obj.RLEID != null)
                    {
                        objE658UserMgt = _db.E658UserMgt.Find(obj.EUMID);
                        objE658UserMgt.RoleID = obj.RLEID;
                        objE658UserMgt.ModifiedDate = DateTime.Now;
                        objE658UserMgt.ModifiedBy = Session["LoginUser"].ToString();
                        objE658UserMgt.ModifiedMac = mac.GetMacAddress();

                    }
                    if (obj.EditLocation != null)
                    {
                        objE658UserMgt = _db.E658UserMgt.Find(obj.EUMID);
                        objE658UserMgt.UserLocation = obj.EditLocation;
                        objE658UserMgt.ModifiedDate = DateTime.Now;
                        objE658UserMgt.ModifiedBy = Session["LoginUser"].ToString();
                        objE658UserMgt.ModifiedMac = mac.GetMacAddress();

                    }

                    _db.Entry(objE658UserMgt).State = EntityState.Modified;
                    if (_db.SaveChanges() > 0)
                    {
                        TempData["ScfMsg"] = "Record Successfully Edited.";
                    }
                    else
                    {
                        TempData["ErrMsg"] = "Something wrong, Please contact IT division";
                    }
                    return RedirectToAction("CreatedUserList");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            
        }
        public ActionResult HandOverAppoinment()
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 19/04/24
            /// Des : Handover function

            try
            {

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return View();
        }
        [HttpGet]
        public ActionResult UserProfile()
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 25/04/24
            /// Des : Show User Profile

            _UserManagementE658 mode = new _UserManagementE658();

            try
            {

                if (Session["LoginUser"] != null)
                {
                    string lgnUserServiceNo = Session["LoginUser"].ToString();

                    var servicePsnInfo = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == lgnUserServiceNo.Trim()).Select(x => new { x.SNo, x.Name, x.Rank }).FirstOrDefault();
                    var userInfo = _db.E658UserMgt.Where(x => x.SNo == servicePsnInfo.SNo && x.Active == 1).Select(x => new {x.UserLocation, x.RoleID,x.Division }).FirstOrDefault();
                    var userRoleName = _db.E658UserType.Where(x => x.EUTID == userInfo.RoleID).Select(x => x.E658Users).FirstOrDefault();

                    mode.ServiceNo = lgnUserServiceNo.Trim();
                    mode.SNo = servicePsnInfo.SNo;
                    mode.Rank = servicePsnInfo.Rank;
                    mode.FullName = servicePsnInfo.Name;
                    mode.Location = userInfo.UserLocation;
                    mode.UserRole = userRoleName;
                    mode.RoleId = userInfo.RoleID;
                    mode.Division = userInfo.Division;
                    //mode.EUMID = EUMID;

                    return View(mode);
                }
                else
                {
                    return RedirectToAction("Login");
                }
                

            }
            catch (Exception ex)
            {

                throw ex;
            }

           

        }
        [HttpPost]
        public ActionResult UserProfile(_UserManagementE658 objUser)
        {
            /// Created By : SL Wickramasinghe
            /// Created Date : 25/04/24
            /// Des : Show User Profile

            _UserManagementE658 mode = new _UserManagementE658();
            E658UserMgt objUseMgt = new E658UserMgt();
            try
            {
                var EUMID = _db.E658UserMgt.Where(x => x.SNo == objUser.SNo && x.RoleID == objUser.RoleId && x.UserLocation == objUser.Location 
                            && x.Active == 1).Select(x => x.EUMID).FirstOrDefault();               

                if (!objUser.isTempHandover )
                {                    
                    if (EUMID != 0)
                    {
                        E658UserMgt obj = new E658UserMgt();
                        obj = _db.E658UserMgt.Find(EUMID);
                        obj.Active = 0;
                        obj.ModifiedBy = Session["LoginUser"].ToString();
                        obj.ModifiedDate = DateTime.Now;
                        obj.ModifiedMac = mac.GetMacAddress();

                        _db.Entry(obj).State = EntityState.Modified;                      
                    }
                    else
                    {
                        TempData["ErrMsg"] = "Something wrong, Please contact IT division";
                    }                  
                }
                else
                {
                    if (EUMID != 0)
                    {
                        E658UserMgt obj = new E658UserMgt();
                        obj = _db.E658UserMgt.Find(EUMID);
                        obj.IsTemHandOverStatus = 1;
                        obj.HandOverFromDate = objUser.HandOverDateFrom;
                        obj.HandOverToDate = objUser.HandOverDateTo;
                        obj.HandOverPsnSNo = objUser.HandOverSNo;
                        obj.ModifiedBy = Session["LoginUser"].ToString();
                        obj.ModifiedDate = DateTime.Now;
                        obj.ModifiedMac = mac.GetMacAddress();

                        _db.Entry(obj).State = EntityState.Modified;

                        objUseMgt.HandOverToDate = objUser.HandOverDateTo;
                    }
                    else
                    {
                        TempData["ErrMsg"] = "Something wrong, Please contact IT division";
                    }
                }

                if (objUser.HandOverDateFrom != null)
                {
                    objUseMgt.SNo = objUser.HandOverSNo;
                    objUseMgt.HandOverFromDate = objUser.HandOverDateFrom;
                    objUseMgt.UserLocation = objUser.Location;
                    objUseMgt.Division = objUser.Division;
                    objUseMgt.RoleID = objUser.RoleId;
                    objUseMgt.Active = 1;
                    objUseMgt.CreatedBy = Session["LoginUser"].ToString();
                    objUseMgt.CreatedDate = DateTime.Now;
                    objUseMgt.CreatedMAC = mac.GetMacAddress();
                    objUseMgt.CreatedIP = this.Request.UserHostAddress;

                    _db.E658UserMgt.Add(objUseMgt);

                    if (_db.SaveChanges() > 0)
                    {
                        TempData["ScfMsg"] = "Handover Process sucessfully completed.";
                    }
                    else
                    {
                        TempData["ErrMsg"] = "Something Wrong, Please Contact IT division.";
                    }
                }
                else
                {
                    TempData["ErrMsg"] = "Please fill the required fields.";
                }        

                return View();

            }
            catch (Exception ex)
            {

                throw ex;
            }



        }                    
        [HttpGet]
        public ActionResult E658InitiateUser()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/15
            /// Description : load E658 Creater View

            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RaisedType, "RTID", "TypeName");
            ViewBag.DDL_EpasLocation = new SelectList(_dbCommonData.EpasLocations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            return View();
        }
        [HttpPost]
        public ActionResult E658InitiateUser(VME658InitiateUser obj)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/15
            /// Description : load E658 Creater View

            ViewBag.DDL_E658RaisedType = new SelectList(_db.E658RaisedType, "RTID", "TypeName");
            ViewBag.DDL_EpasLocation = new SelectList(_dbCommonData.EpasLocations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            E658CreaterDetails objCreater = new E658CreaterDetails();

            try
            {
                if (ModelState.IsValid)
                {
                    
                    objCreater.SNo = obj.SNo;
                    objCreater.RaisedTypeID = obj.RTID;
                    objCreater.CreatedDate = DateTime.Now;
                    objCreater.CreatedBy = obj.SNo;
                    objCreater.UserGERMSLocation = obj.UserLoginLocation.Trim();
                    objCreater.Active = 1;
                    objCreater.CreatedIP = this.Request.UserHostAddress;
                    objCreater.CreatedMAC = mac.GetMacAddress();
                    Session["E658RunType"] = obj.RTID;
                    Session["E658CreateUser"] = obj.SNo;
                    Session["UserLoginLoc"] = obj.UserLoginLocation.Trim();


                    if (obj.RTID == (int)E658.Enum.E658RaisedType.StaffVehiE)
                    {
                        if (obj.StaffServiceNo != null)
                        {
                            objCreater.StaffServiceNo = obj.StaffServiceNo;

                            _db.E658CreaterDetails.Add(objCreater);

                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                            {
                                if (_db.SaveChanges() > 0)
                                {
                                    scope.Complete();
                                    TempData["ScfMsg"] = "You completed the first step, Please fill in the E-658 Details (ඔබ පළමු පියවර සම්පූර්ණ කර ඇත, කරුණාකර E-658 විස්තර පුරවන්න)";

                                    return RedirectToAction("Create", "E658");
                                }
                            }

                        }
                        else
                        {
                            TempData["ErrMsg"] = "Please enter staff car Officer's Service Number";
                        }                      

                    }
                    else
                    {
                        objCreater.CreaterLoc = obj.LocationId.Trim();
                        objCreater.CreaterDivision = obj.DivisionId.Trim();
                        //objCreater.CreaterDivision = obj.LocationId.Trim();

                        _db.E658CreaterDetails.Add(objCreater);

                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            if (_db.SaveChanges() > 0)
                            {
                                scope.Complete();
                                TempData["ScfMsg"] = "you complete the first step, Please fill in the E 658 Details  (ඔබ පළමු පියවර සම්පූර්ණ කර ඇත, කරුණාකර E-658 විස්තර පුරවන්න)";

                                return RedirectToAction("Create", "E658");
                            }
                        }
                    }
                    
                }                            
               
            }
            catch (Exception ex)
            {
                TempData["ErrMsg"] = ex.ToString();
                throw ex;
            }

            return View();
        }
        [HttpGet]
        public ActionResult Inquiry()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/10/07
            /// Description : load E658 Inquiry to users

            return View();
        }
        [HttpPost]
        public ActionResult Inquiry(string SearchString)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/10/07
            /// Description : load E658 Inquiry to users

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            List<VME658Create> e658DetailsList = new List<VME658Create>();
            string FromLocID = "";
            string ToLocID = "";
            string trimSearchString = SearchString.Trim();
            try
            {
                //int E658CreatorID = 81;
                ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();
                dt = objDALCommanQuery.CallE65SP(0);

                var recordRow = dt.AsEnumerable().Where(x => x.Field<string>("UnitSerialNo").Contains(trimSearchString));

                if (recordRow.Any())
                {
                    dt2 = dt.AsEnumerable().Where(x => x.Field<string>("UnitSerialNo").Contains(trimSearchString)).CopyToDataTable();
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        VME658Create objVME658Create = new VME658Create();

                        FromLocID = dt2.Rows[i]["FLocation"].ToString();
                        ToLocID = dt2.Rows[i]["RealToLocation"].ToString();

                        objVME658Create.FromLocID = _db.Locations.Where(x => x.LocationID == FromLocID).Select(x => x.LocationName).FirstOrDefault();
                        objVME658Create.ToLocId = _db.Locations.Where(x => x.LocationID == ToLocID).Select(x => x.LocationName).FirstOrDefault();
                        objVME658Create.UnitSerialNo = dt2.Rows[i]["UnitSerialNo"].ToString();
                        objVME658Create.E658RunType = dt2.Rows[i]["TypeName"].ToString();
                        objVME658Create.E658Date = Convert.ToDateTime(dt2.Rows[i]["PDate"]);
                        objVME658Create.ReturnDate = Convert.ToDateTime(dt2.Rows[i]["ReturnDate"]);
                        objVME658Create.JournryStartTime = Convert.ToDateTime(dt2.Rows[i]["PTime"]);
                        objVME658Create.RequiredDuration = dt2.Rows[i]["PHrs"].ToString();
                        objVME658Create.Purpose = dt2.Rows[i]["Duty"].ToString();
                        objVME658Create.Route = dt2.Rows[i]["Route"].ToString();
                        objVME658Create.IsOMTAvail = Convert.ToInt32(dt2.Rows[i]["IsOMtReqFromMT"]);
                        objVME658Create.IsVehicleAvail = Convert.ToInt32(dt2.Rows[i]["IsVehicleReqFromMT"]);
                        objVME658Create.RoleID = 4; //RoleId;
                        objVME658Create.ECDID = Convert.ToInt32(dt2.Rows[i]["E658CreatorDltId"]);
                        //objVME658Create.EFTID = EFlowId;
                        objVME658Create.RecordStatus = Convert.ToInt32(dt2.Rows[i]["RecordStatus"]);
                        objVME658Create.TypeName = dt2.Rows[i]["TypeName1"].ToString();
                        //TempData["ECDID"] = E658CreatorID;
                        TempData["UserLoginType"] = Session["UserLoginType"];

                        if (objVME658Create.IsOMTAvail == 1)
                        {
                            objVME658Create.OMTStatus = dt2.Rows[i]["OMTAllocation"].ToString();
                        }
                        else
                        {
                            objVME658Create.OMTServiceNo = dt2.Rows[i]["OMTNo"].ToString();
                        }

                        if (objVME658Create.IsVehicleAvail == 10)
                        {
                            objVME658Create.VehicleStatus = dt2.Rows[i]["VehicleAllocation"].ToString();
                        }
                        else
                        {
                            objVME658Create.SLAFRegNo = dt2.Rows[i]["SLAFRegNo"].ToString();
                        }



                        e658DetailsList.Add(objVME658Create);
                    }
                    return View(e658DetailsList);
                }
                else
                {
                    return View();
                }             

                //List<VME658Create> locations = GetRptLocation(E658CreatorID);

                //TempData["RptLocation"] = locations;

                //PartialView("_E658ReportLoc", locations);


            }
            catch (Exception ex)
            {

                throw ex;
            }

           
            
        }       
        public ActionResult TransportAuthIndex()
        {
            return View();
        }        
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            return RedirectToAction("Index", "User");
        }
        [HttpGet]
        public ActionResult CreateMTController()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/15
            /// Description : load E658 Creater View

            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");


            return View();        
        
        }
        [HttpPost]
        public ActionResult CreateMTController(_UserManagementE658 objUser)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/15
            /// Description : load E658 Creater View

            ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");
            
            E658UserMgt objUserMgt = new E658UserMgt();

            try
            {
                if (!objUser.DutyDate.HasValue || objUser.DutyDate.Value == DateTime.MinValue)
                {
                    TempData["ErrMsg"] = "Please select the duty date";
                }
                else
                {
                    objUserMgt.SNo = objUser.SNo;
                    objUserMgt.RoleID = (int)E658.Enum.EnumE658UserType.MTController;
                    objUserMgt.UserLocation = objUser.Location;
                    objUserMgt.MTCotrollerDutyDate = objUser.DutyDate;
                    objUserMgt.Active = 1;
                    objUserMgt.CreatedDate = DateTime.Now;
                    objUserMgt.CreatedIP = this.Request.UserHostAddress;
                    objUserMgt.CreatedMAC = mac.GetMacAddress();

                    _db.E658UserMgt.Add(objUserMgt);

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        if (_db.SaveChanges() > 0)
                        {
                            scope.Complete();
                            TempData["ScfMsg"] = "Record sucessfully created";

                            //return RedirectToAction("Dashboardv1ToMTO", "Dashboard");
                        }
                    }                  

                }

            }
            catch (Exception ex)
            {

                throw ex;
                //TempData["ErrMsg"] = "Please select the duty date";
            }

            return View();

        }
        [HttpGet]
        public ActionResult MTControllerList(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/08/15
            /// Description : load MT Controller List

            //ViewBag.DDL_GERSLocation = new SelectList(_db.Locations.OrderBy(x => x.LocationID), "LocationID", "LocationName");

            if (Session["MToOctLocation"] != null)
            {
                string Location = Session["MToOctLocation"].ToString();
               
                ViewBag.CurrentSort = sortOrder;
               
                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;
                var MTList = from s in _db.E658UserMgt
                             join vp in _db.Vw_PersonalDetail
                             on s.SNo equals vp.SNo
                             where s.UserLocation == Location && s.Active == 1 && s.RoleID == (int)E658.Enum.EnumE658UserType.MTController
                             orderby s.MTCotrollerDutyDate descending
                             select new VME658InitiateUser
                             {
                                 SNo = s.SNo,
                                 UserLocation = s.UserLocation,
                                 MTCotrollerDutyDate = s.MTCotrollerDutyDate,
                                 ServiceNo = vp.ServiceNo,
                                 Rank = vp.Rank,
                                 FullName = vp.Name


                             };
                if (!String.IsNullOrEmpty(searchString))
                {
                    MTList = MTList.Where(s => s.ServiceNo.Contains(searchString)).OrderBy(y => y.MTCotrollerDutyDate);
                }
                switch (sortOrder)
                {
                    case "ServiceNo":
                        MTList = MTList.OrderByDescending(s =>s.ServiceNo);
                        break;
                    case "Rank":
                        MTList = MTList.OrderBy(s => s.Rank);
                        break;
                    case "Name":
                        MTList = MTList.OrderBy(s => s.FullName);
                        break;
                    case "Duty Date":
                        MTList = MTList.OrderBy(s => s.MTCotrollerDutyDate);
                        break;
                    default:  // Name ascending 
                        MTList = MTList.OrderByDescending(s => s.MTCotrollerDutyDate);
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(MTList.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "User");
            }


        }

        #region Json Method
        public JsonResult getUserInfo(string id)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date  : 2024/02/14
            /// Description : Load Full name details according to the service number

            Vw_PersonalDetail obj_VwPersonalProfile = new Vw_PersonalDetail();

            var count = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == id).Count();
            if (count == 1)
            {
                obj_VwPersonalProfile = _db.Vw_PersonalDetail.Where(x => x.ServiceNo == id).FirstOrDefault();

            }
            else
            {
                obj_VwPersonalProfile.Name = "Service Number is not valid.";
            }


            return Json(obj_VwPersonalProfile,JsonRequestBehavior.AllowGet);

        }
        public JsonResult FromLocation(string id)
        {
            ///Created BY   : sqn ldr pkp priyantha
            ///Created Date : 2021/02/16
            /// Description : Load division

            List<Location> FromEstablishment = new List<Location>();
            var FromLocationList = this.LinqFromLocation(id);

            var divisionListData = FromLocationList.Select(x => new SelectListItem()
            {
                Text = x.DivisionName.ToString(),
                Value = x.DivisionName.ToString(),
            });


            return Json(divisionListData, JsonRequestBehavior.AllowGet);
        }
        public IList<Vw_EpasDivision> LinqFromLocation(string id)
        {
            ///Created BY   : sqn ldr pkp priyantha
            ///Created Date : 2021/02/16
            /// Description : Load division
            List<Vw_EpasDivision> Result = new List<Vw_EpasDivision>();
            Result = _dbCommonData.Vw_EpasDivision.Where(x => x.LocationID == id).ToList();

            return Result;
        }
        public JsonResult CheckRunStatus(int id)
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/10/10
            /// Description : load E658 run Status
            /// 
            var maxTransaction = (from eft in _db.E658FlowTransaction
                                  join eut in _db.E658UserType
                                  on eft.RoleID equals eut.EUTID
                                  where eft.E658CreatorDltID == id
                                  && eft.EFTID == _db.E658FlowTransaction
                                  .Where(e => e.E658CreatorDltID == id)
                                  .Max(e => e.EFTID)
                                  select new { eft, eut }
                                 ).FirstOrDefault();


            return Json(maxTransaction, JsonRequestBehavior.AllowGet);


        }

        #endregion



    }
}