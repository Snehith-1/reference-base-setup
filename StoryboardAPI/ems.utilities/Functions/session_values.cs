using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using ems.utilities.Models;
namespace ems.utilities.Functions
{
    public class session_values
    {
        dbconn objdbconn = new dbconn();
        cmnfunctions objcmnfunctions = new cmnfunctions();
        OdbcDataReader objOdbcDataReader;
        string msSQL = string.Empty;

        public logintoken gettokenvalues(string token)
        {
            logintoken getlogintoken = new logintoken();
           
            msSQL = " select a.employee_gid,a.user_gid,a.department_gid, b.branch_gid from adm_mst_ttoken a " +
                " left join hrm_mst_temployee b on  a.employee_gid=b.employee_gid " +
                " WHERE token = '" + token + "'";
            objOdbcDataReader = objdbconn .GetDataReader(msSQL);
            if (objOdbcDataReader.HasRows == true)
            {
                objOdbcDataReader.Read();
                getlogintoken.employee_gid = objOdbcDataReader["employee_gid"].ToString();
                getlogintoken.user_gid = objOdbcDataReader["user_gid"].ToString();
                getlogintoken.department_gid = objOdbcDataReader["department_gid"].ToString();
                getlogintoken.branch_gid = objOdbcDataReader["branch_gid"].ToString();
                objOdbcDataReader.Close();
            }
            else
                objOdbcDataReader.Close();
            return getlogintoken;
        }
    }
}