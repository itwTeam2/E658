using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;


/// <summary>
/// Summary description for DALConnectionManager
/// </summary>
public class DALConnectionManager
{
    public static SqlConnection open() 
    {
        SqlConnection ConnectionManager = new SqlConnection();
        try
        {
            //ConnectionManager.ConnectionString = "Data Source=135.22.67.113\\; Initial Catalog = HELPDESK; User ID = helpdeskuser;password=password";
            //ConnectionManager.ConnectionString = "Data Source=135.22.67.71\\SQL2019; Initial Catalog = GERMS_NEW; User ID = germsuser;password=password";
            ConnectionManager.ConnectionString = "Data Source=135.22.67.105\\; Initial Catalog = GERMS; User ID = germsuser;password=password";
            ConnectionManager.Open();              
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return ConnectionManager;
    }
    public static void Close(SqlConnection Connection) 
    {
        try
        {
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
        catch (Exception)
        {            
            throw;
        }
    }
}
