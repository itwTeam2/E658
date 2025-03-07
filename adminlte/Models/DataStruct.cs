using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E658.Enum
{
    public enum EnumE658UserType
    {
        FormationUser = 1,
        StaffCarUser = 2,
        MToOCT = 3,
        FinalizedAuthorization = 4,
        MTController = 5,
        E658CreateUser = 7,
        RecordCertified = 6,
        NoUser  = 0,
        RunComplete = 8,
        RunHold = 9,
        HQUser = 10,
        HQAppAuth = 11,
        SOGO = 12,
        LngRunRequest = 13


    }

    public enum EnumRecordStatus
    {
        Forward = 2000,
        Reject = 3000,
        HoldRun = 4000

    }
 
    public enum EnumAdminUserType
    {
        Admin = 1,
        SOIT = 8,
        DIT = 15,
    }   

    public enum E658RaisedType
    {
        StaffVehiE = 10,
        FormationE = 11,
        MTSecE = 12,
        DGSE = 13,
        TA = 14,
        RR = 15

    }
}