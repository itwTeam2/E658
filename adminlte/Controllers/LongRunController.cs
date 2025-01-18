using E658.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E658.Controllers
{
    public class LongRunController : Controller
    {
        private dbContext _db = new dbContext();
        private dbContextCommonData _dbCommonData = new dbContextCommonData();
        // GET: LongRun
        public ActionResult Index()
        {
            ///Created BY   : Sqn ldr Wicky
            /// Create Date : 2024/02/22
            /// Description : load E658 Creater View
            return View();
        }

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


            return View();
        }
    }
}