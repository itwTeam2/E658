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

            try
            {
                if (Session["MToOctLocation"] != null && Session["UserLoginType"] != null)
                {
                    string MToOctLocation = Session["MToOctLocation"].ToString();
                    int userLoginType = Convert.ToInt32(Session["UserLoginType"]);

                    ReportData.DAL.DALCommanQuery objDALCommanQuery = new ReportData.DAL.DALCommanQuery();

                    SetDashboardHeading(userLoginType);

                    TempData["MTOPendingApprovedCount"] = GetRecordCount(objDALCommanQuery, userLoginType, MToOctLocation, E658.Enum.EnumRecordStatus.Forward);
                    TempData["FinalPendingApprovedCount"] = GetRecordCount(objDALCommanQuery, (int)E658.Enum.EnumE658UserType.FinalizedAuthorization, MToOctLocation, E658.Enum.EnumRecordStatus.Forward);
                    TempData["RecordCertifiedCount"] = GetRecordCount(objDALCommanQuery, (int)E658.Enum.EnumE658UserType.RecordCertified, MToOctLocation, E658.Enum.EnumRecordStatus.Forward);
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
            return View();
        }

        private void SetDashboardHeading(int userLoginType)
        {
            if (userLoginType == (int)E658.Enum.EnumE658UserType.MTController)
            {
                TempData["DashboardHeadingName"] = "MT Controller";
            }
            else if (userLoginType == (int)E658.Enum.EnumE658UserType.FinalizedAuthorization)
            {
                TempData["DashboardHeadingName"] = "Final Authority Dashboard";
            }
            else
            {
                TempData["DashboardHeadingName"] = "MTO/OCT Dashboard";
            }
        }

        private int GetRecordCount(ReportData.DAL.DALCommanQuery objDALCommanQuery, int userType, string location, E658.Enum.EnumRecordStatus status)
        {
            DataTable dt = objDALCommanQuery.CallE65MoreDetailsSP(userType);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.AsEnumerable().Count(x => x.Field<int>("Active") == 1 &&
                x.Field<string>("UserGERMSLocation") == location && x.Field<int>("RecordStatusID") == (int)status);
            }
            return 0;
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