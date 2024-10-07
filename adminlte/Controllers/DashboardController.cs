using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WRMS.Models;
using PagedList;
using System.Data;

namespace adminlte.Controllers
{
    public class DashboardController : Controller
    {
    
        // GET: Dashboard
        public ActionResult Index()
        {
            ///Create By: Flt Lt WAKY Wickramasinghe
            ///Create Date:2019/11/14
            ///Description : Get job Status wise Count
           
                return View();           
          
        }
        public ActionResult ViewJobNoStatusWise(string sortOrder, string currentFilter, string searchString, int? page, int id)
        {
            ///Create By: Flt Lt WAKY Wickramasinghe
            ///Create Date:2019/11/15
            ///Description : Show Job No When Click SNCO dashboard more Detail
            return View();
        }
        public ActionResult Dashboardv1ToMTO()
        
        {
            ///Create By: Flt Lt WAKY Wickramasinghe
            ///Create Date:2024/07/08
            ///Description : Load e658 summary details to MTO/OCT

            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();
            DataTable dt3 = new DataTable();

            try
            {
                if (Session["MToOctLocation"] != null && Session["UserLoginType"] != null)
                {
                    string MToOctLocation = Session["MToOctLocation"].ToString();
                    int userLoginType = Convert.ToInt32(Session["UserLoginType"]);

                    ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();                


                    switch (userLoginType)
                    {
                        case (int)E658.Enum.EnumE658UserType.MToOCT:
                        case (int)E658.Enum.EnumE658UserType.MTController:

                           

                            if (userLoginType == (int)E658.Enum.EnumE658UserType.MTController)
                            {
                                TempData["DashboardHeadingName"] = "MT Controller";
                            }
                            else
                            {
                                TempData["DashboardHeadingName"] = "MTO/OCT Dashboard";
                            }
                            //// Get Data Count related to MTO/OCT User Type
                            {
                                dt = objDALCommanQuery.CallE65MoreDetailsSP(userLoginType);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var pendingRecordCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                         == (int)E658.Enum.EnumRecordStatus.Forward).Count();

                                    TempData["MTOPendingApprovedCount"] = (pendingRecordCount != 0) ? pendingRecordCount : 0;
                                }
                                else
                                {
                                    TempData["MTOPendingApprovedCount"] = 0;
                                }                               
                            }

                            //// Get Data Count related to Final approval User Type
                            {

                                dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.FinalizedAuthorization);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var pendingFinalApprovedCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                         == (int)E658.Enum.EnumRecordStatus.Forward).Count();

                                    TempData["FinalPendingApprovedCount"] = (pendingFinalApprovedCount != 0) ? pendingFinalApprovedCount : 0;
                                }
                                else
                                {
                                    TempData["FinalPendingApprovedCount"] = 0;
                                }                            

                            }

                            //// Get Data Count related to Certified record Count
                            {
                                dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.RecordCertified);

                                if (dt != null && dt.Rows.Count > 0)
                                {

                                    var recordCertifiedCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                            == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.RecordCertified).Count();

                                    TempData["RecordCertifiedCount"] = (recordCertifiedCount != 0) ? recordCertifiedCount : 0;

                                }
                                else
                                {
                                    TempData["RecordCertifiedCount"] = 0;
                                }

                            }

                            break;
                        case (int)E658.Enum.EnumE658UserType.FinalizedAuthorization:
                            
                            TempData["DashboardHeadingName"] = "Final Authority Dashboard";
                            //// Get Data Count related to Final Authority User Type
                            
                            {

                                dt = objDALCommanQuery.CallE65MoreDetailsSP(userLoginType);

                                if (dt != null && dt.Rows.Count > 0)
                                {

                                    var pendingApprovedCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                            == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization).Count();

                                    TempData["FinalPendingApprovedCount"] = (pendingApprovedCount != 0) ? pendingApprovedCount : 0;

                                }
                                else
                                {
                                    TempData["FinalPendingApprovedCount"] = 0;
                                }
                            }

                            //// Get Data Count related to Final Authority User Type
                            {
                                dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.MToOCT);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var pendingRecordCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                         == (int)E658.Enum.EnumRecordStatus.Forward).Count();

                                    TempData["MTOPendingApprovedCount"] = (pendingRecordCount != 0) ? pendingRecordCount : 0;
                                }
                                else
                                {
                                    TempData["MTOPendingApprovedCount"] = 0;
                                } 
                            }

                            //// Get Data Count related to Certified record Count
                            {
                                dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.RecordCertified);

                                if (dt != null && dt.Rows.Count > 0)
                                {

                                    var recordCertifiedCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                            == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.RecordCertified).Count();

                                    TempData["RecordCertifiedCount"] = (recordCertifiedCount != 0) ? recordCertifiedCount : 0;

                                }
                                else
                                {
                                    TempData["RecordCertifiedCount"] = 0;
                                }

                            }

                            break;

                        default:

                            dt = objDALCommanQuery.CallE65MoreDetailsSP((int)E658.Enum.EnumE658UserType.RecordCertified);

                            if (dt != null && dt.Rows.Count > 0)
                            {

                                var recordCertifiedCount = dt.AsEnumerable().Where(x => x.Field<int>("Active") == 1 && x.Field<string>("UserGERMSLocation") == MToOctLocation && x.Field<int>("RecordStatusID")
                                        == (int)E658.Enum.EnumRecordStatus.Forward && x.Field<int>("RoleID") == (int)E658.Enum.EnumE658UserType.RecordCertified).Count();

                                TempData["RecordCertifiedCount"] = (recordCertifiedCount != 0) ? recordCertifiedCount : 0;

                            }
                            
                            else
                            {
                                TempData["RecordCertifiedCount"] = 0;
                            }

                            break;
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
            return View();
        }
        public ActionResult Dashboardv2ToFinalApproved()
        {
            ///Create By: Flt Lt WAKY Wickramasinghe
            ///Create Date:2024/07/08
            ///Description : Load e658 summary details to Final Authorized person
            return View();
        }
       

              
       
    }
}