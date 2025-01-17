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
    }
}