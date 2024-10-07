using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WRMS.Models;
using System.Web.Mvc;

namespace WRMS.Helpers
{
    public class UpdateTable
    {
        //UserFaultRegistryRecord objUserFaultRegistryRecord = new UserFaultRegistryRecord();
       // db_Context _db = new db_Context();       
        //public void UpdateUserFaultRegistryRecord(int UFRID)
        //{
        //    UserFaultRegistryRecord objUserFaultRegistryRecord = _db.UserFaultRegistryRecords.Find(UFRID);
        //    objUserFaultRegistryRecord.FaultStatus = (int)WRMS.Enum.EnumFaultStatus.EnteringtoTechControl;
        //    objUserFaultRegistryRecord.ModifiedDate = DateTime.Now;
        //    objUserFaultRegistryRecord.ModifiedBy = Session["UID"].ToString();
        //    _db.Entry(objUserFaultRegistryRecord).State = EntityState.Modified;
        //}
        //public void UpdateTechConItemRegisterHeader(int TCRID)
        //{
        //    WsTechConItemRegisterHeader objWsTechConItemRegisterHeader = _db.WsTechConItemRegisterHeaders.Find(TCRID);
        //    objWsTechConItemRegisterHeader.FSID = (int)WRMS.Enum.EnumFaultStatus.EnteringtoTechControl;
        //    objWsTechConItemRegisterHeader.ModifyDate = DateTime.Now;
        //    objWsTechConItemRegisterHeader.ModifyBy = Session["UID"].ToString();
        //    _db.Entry(objWsTechConItemRegisterHeader).State = EntityState.Modified;
        //}
    }
}