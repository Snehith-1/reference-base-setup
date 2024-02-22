using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO; 
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using System.Collections;
using System.Web.Configuration;
using System.Reflection;
using System.Drawing;
using System.Data.Odbc;

namespace ems.utilities.Functions
{
    public class cmnfunctions
    {
        ems.utilities.Functions.dbconn objdbconn = new ems.utilities.Functions.dbconn();
        OdbcCommand cmdQuery = new OdbcCommand();
        OdbcDataReader objreader;
        DataSet objdataset = new DataSet();
        string lsTempGid = string.Empty;
        int mnResult, ls_port;
        string msSQL, ls_username, ls_password, ls_server;
        string scalar = string.Empty;
        DataTable objTblRQ = new DataTable("objTblRQ");
        DataTable table = new DataTable("table");
        DataTable dt_table;
        DataColumn myCol0;
        string lblemployeereporting_to, lsemployeeGID;
        int lscount;
        String[] lsCCReceipients;
        string return_path, upload_gid, path, company_code, file_path, file_name,lsfile_name;
        HttpRequest httpRequest;
        HttpPostedFile httpPostedFile;
        // Split By Expression
        MemoryStream ms = new MemoryStream();
        MemoryStream ms_stream = new MemoryStream();
        Stream ls_readStream;
        string lsconverted_date;
        string storageConnectionString = "DefaultEndpointsProtocol=https;"
                   + "AccountName=samunnatidevelopment"
                   + ";AccountKey=e/XI6jwDON4PpBZdY2WAymGe57h/kJko87mDlfP1FA8lTRr5zBzizhYR+PGElz5gezSKuEG5jYG1e2402cJCTw=="
                   + ";EndpointSuffix=core.windows.net";

        public string[] Split(string input, string pattern)
        {
            string[] elements = Regex.Split(input, pattern);
            return elements;
        }

        public string ConvertToAscii(string str)
        {
            int iIndex;
            int lenOfUserString;
            string newUserPass = string.Empty;
            string tmp;
            lenOfUserString = str.Length;
            for (iIndex = 0; iIndex < lenOfUserString; iIndex++)
            {
                tmp = str.Substring(iIndex, 1);
                tmp = (((int)Convert.ToChar(tmp)) - lenOfUserString).ToString();
                newUserPass = newUserPass + (tmp.Length < 3 ? "0" : "") + tmp;
            }
            return newUserPass;
        }

        public bool Mailer(string from, string to, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient("smtp-mail.outlook.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential("no-reply@samunnati.com", "Vision18");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = subject;
            mailMessage.Body = body;

            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Document Upload //


        // Get Master GID
        public bool mail(string to, string sub, string body)
        {
            try
            {
                msSQL = "SELECT company_mail,pop_server,pop_port,pop_username,pop_password FROM adm_mst_tcompany ";
                objreader = objdbconn.GetDataReader(msSQL);
                if (objreader.HasRows)
                {
                    ls_server = objreader["pop_server"].ToString();
                    ls_port = Convert.ToInt32(objreader["pop_port"]);
                    ls_username = objreader["pop_username"].ToString();
                    ls_password = objreader["pop_password"].ToString();
                }
                objreader.Close();
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(ls_username);
                message.To.Add(new MailAddress(to));
                message.Subject = sub;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = body;
                smtp.Port = ls_port;
                smtp.Host = ls_server; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(ls_username, ls_password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //public string GetMasterGID(string pModule_Code)
        //{
        //    lsTempGid = null;

        //    msSQL = " CALL adm_mst_spSequenceGid('" + pModule_Code + "')";
        //    lsTempGid = objdbconn.GetExecuteScalar(msSQL);
        //    if (lsTempGid == null || lsTempGid == "")
        //        return "E";
        //    else
        //        return lsTempGid; 
        //}
        ///Snehith coding/ <summary>
        public string GetBiometricGID()
        {
            string lsSeqNo = null;
            int lsSeqNo1;
            objdbconn.OpenConn();
            msSQL = "Select max(biometric_id) as maxgid from hrm_mst_temployee";
            objreader = objdbconn.GetDataReader(msSQL);
            if (objreader.HasRows == true)
            {
                lsSeqNo = objreader["maxgid"].ToString();
                int currentValue = Convert.ToInt32(objreader["maxgid"]);
                if (string.IsNullOrEmpty(lsSeqNo))
                {
                    lsSeqNo = "1";
                }
                else
                {

                    int ls_sequence_curval = currentValue + 1;
                    lsSeqNo = ls_sequence_curval.ToString(); ;
                }
            }
            objreader.Close();
            objdbconn.CloseConn();
            return lsSeqNo;
        }

        public string GetMasterGID(string pModule_Code)
        {
            lsTempGid = null;

            msSQL = " select year(fyear_start) as finyear from adm_mst_tyearendactivities order by finyear_gid desc limit 0,1";
            string lsfinyear = objdbconn.GetExecuteScalar(msSQL);

            msSQL = " select sequence_flag from adm_mst_tsequence where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "' ";
            string lssequence_flag = objdbconn.GetExecuteScalar(msSQL);
            if (lssequence_flag == "N")
            {
                msSQL = " select  sequence_curval + 1 AS sequence_curval from adm_mst_tsequence where sequence_code = '" + pModule_Code + "' and finyear='" + lsfinyear + "'";
                string sequencecurval = objdbconn.GetExecuteScalar(msSQL);
               
                    DateTime currentDate = DateTime.Now;
                    msSQL = " update  adm_mst_tsequence set " +
                " sequence_curval = '" + sequencecurval + "'" +
                "  where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "'  ";
                    mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                    lsTempGid = pModule_Code + currentDate.ToString("yyMMdd") + sequencecurval;

                
            }
            else 
            {
              msSQL = " select  sequence_curval + 1 AS sequence_curval from adm_mst_tsequence where sequence_code = '" + pModule_Code + "' and finyear='" + lsfinyear + "'";
                string sequencecurval = objdbconn.GetExecuteScalar(msSQL);
               
                    DateTime currentDate = DateTime.Now;
                    msSQL = " update  adm_mst_tsequence set " +
                " sequence_curval = '" + sequencecurval + "'" +
                "  where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "'  ";
                    mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                    lsTempGid = pModule_Code + currentDate.ToString("yyMMdd") + sequencecurval;

                
            }
            //else
            //{
            //    msSQL = " select sequence_curval, sequence_format,branch_flag,year_flag," +
            //            " department_flag,location_flag,delimeter,company_code, " +
            //            " sequence_code, month_flag, runningno_prefix from adm_mst_tsequence " +
            //            " where sequence_code = '" + pModule_Code + "' and finyear='" + lsfinyear + "'";
            //    objreader = objdbconn.GetDataReader(msSQL);
            //    if (objreader.HasRows)
            //    {
            //        string ls_delimeter = objreader["delimeter"].ToString();
            //        string ls_branch_flag = objreader["branch_flag"].ToString();
            //        msSQL = " select company_code from adm_mst_tcompany ";
            //        string lscompany_code = objdbconn.GetExecuteScalar(msSQL);
            //        if (lscompany_code != null && lscompany_code != "")
            //        {
            //            string lscompanycode = lscompany_code;
            //            if (ls_branch_flag =="Y")
            //            {
            //                if (ls_branch_flag == default)
            //                {
            //                    msSQL = " Select a.branch_prefix from hrm_mst_tbranch a" + " left join hrm_mst_temployee b on b.branch_gid=a.branch_gid" + " where b.user_gid='" + HttpContext.Current.Session("user_gid").ToString + "'";

            //                    objreader = objdbconn.GetDataReader(msSQL);
            //                    if (objreader.HasRows == true)
            //                    {
            //                       string lsbranch_code = objreader["branch_prefix"].ToString() + ls_delimeter;
            //                    }
            //                    objreader.Close();
            //                }
            //                else
            //                {
            //                    msSQL = " Select a.branch_prefix from hrm_mst_tbranch a" + " where a.branch_gid='" + branch + "'";
            //                    objreader = objdbconn.GetDataReader(msSQL);
            //                    if (objreader.HasRows == true)
            //                    {
            //                        string lsbranch_code = objreader["branch_prefix"].ToString() + ls_delimeter;
            //                    }
            //                    objreader.Close();
            //                }

            //            }

            //        }
            //    }
            //}

            if (lsTempGid == null || lsTempGid == "")
                return "E";
            else
                return lsTempGid;
        }
        public void PopSummaryLGL(string lblEmployeeGID)
        {

            myCol0 = new DataColumn();
            myCol0.DataType = System.Type.GetType("System.String");
            myCol0.MaxLength = -1;
            myCol0.AllowDBNull = true;
            myCol0.ColumnName = "employee_gid";
            objTblRQ.Columns.Add(myCol0);
            DataColumn myCol1 = new DataColumn("hierarchy_level");
            myCol1.DataType = System.Type.GetType("System.Int32");
            myCol1.AllowDBNull = false;
            objTblRQ.Columns.Add(myCol1);
            objTblRQ.AcceptChanges();

            // Recursive Looping
            msSQL = " select a.employee_gid, a.hierarchy_level, concat(b.user_firstname, '-', b.user_code) as user" +
                    " from adm_mst_tsubmodule a  " + " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " + " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " + " where a.module_gid = 'LGL' and a.submodule_id='LGLLCMSRP' and a.employee_gid = '" + lblEmployeeGID + "' ";
            objreader = objdbconn.GetDataReader(msSQL);
            if (objreader.HasRows == true)
            {
                objreader.Read();
                DataRow myNewRow = objTblRQ.NewRow();
                myNewRow["employee_gid"] = objreader["employee_gid"].ToString();
                myNewRow["hierarchy_level"] = objreader["hierarchy_level"].ToString();
                objTblRQ.Rows.Add(myNewRow);
                objTblRQ.AcceptChanges();
            }
            objreader.Close();
            objdbconn.CloseConn();
            // childloop(HttpContext.Current.Session("employee_gid"))
            childloopTopLGL(lblEmployeeGID);
            foundRow(table);
            return;
        }
        public void PopSummary(string lblEmployeeGID, string lbluser, Double lscount)
        {

            myCol0 = new DataColumn();
            myCol0.DataType = System.Type.GetType("System.String");
            myCol0.MaxLength = -1;
            myCol0.AllowDBNull = true;
            myCol0.ColumnName = "employee_gid";
            objTblRQ.Columns.Add(myCol0);
            DataColumn myCol1 = new DataColumn("hierarchy_level");
            myCol1.DataType = System.Type.GetType("System.Int32");
            myCol1.AllowDBNull = false;
            objTblRQ.Columns.Add(myCol1);
            objTblRQ.AcceptChanges();

            // Recursive Looping
            msSQL = " select a.employee_gid, a.hierarchy_level, concat(b.user_firstname, '-', b.user_code) as user from adm_mst_tsubmodule a  " + " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " + " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " + " where a.module_gid = 'HRM' and a.submodule_id='HRMLEVARL' and a.employee_gid = '" + lblEmployeeGID + "' ";
            objreader = objdbconn.GetDataReader(msSQL);
            if (objreader.HasRows == true)
            {
                objreader.Read();
                DataRow myNewRow = objTblRQ.NewRow();
                myNewRow["employee_gid"] = objreader["employee_gid"].ToString();
                myNewRow["hierarchy_level"] = objreader["hierarchy_level"].ToString();
                objTblRQ.Rows.Add(myNewRow);
                objTblRQ.AcceptChanges();
            }
            objreader.Close();
            objdbconn.CloseConn();
            // childloop(HttpContext.Current.Session("employee_gid"))
            childloopTop(lblEmployeeGID);
            foundRow(table);
            return;
        }
        public DataTable foundRow(DataTable table)
        {
            lscount = objTblRQ.Rows.Count;
            table = objTblRQ;
            foundcount(lscount);
            return table;
        }
        public Double foundcount(Double lscount)
        {

            lscount = objTblRQ.Rows.Count;

            return lscount;
        }
        public void childloopTopLGL(string employee)
        {
            msSQL = " select a.employeereporting_to, concat(b.user_firstname, '-', b.user_code) as user " +
                    " from adm_mst_tmodule2employee a " + " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid " +
                    " inner join adm_mst_tuser b on c.user_gid = b.user_gid " + " where a.module_gid = 'LGL' " +
                    " and a.employee_gid = '" + employee + "'" + " and a.hierarchy_level <> '-1' ";
            objreader = objdbconn.GetDataReader(msSQL);
            objreader.Read();
            if (objreader.HasRows == true)
            {
                lblemployeereporting_to = objreader["employeereporting_to"].ToString();
                objreader.Close();
                msSQL = " select a.employee_gid, a.hierarchy_level, concat(b.user_firstname, '-', b.user_code) as user from adm_mst_tsubmodule a  " +
                    " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " + " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " +
                    " where a.module_gid = 'LGL' and a.submodule_id='LGLLCMSRP' and a.employee_gid = '" + lblemployeereporting_to + "' ";
                objreader = objdbconn.GetDataReader(msSQL);
                if (objreader.HasRows == true)
                {
                    objreader.Read();
                    DataRow myNewRow = objTblRQ.NewRow();
                    myNewRow["employee_gid"] = objreader["employee_gid"].ToString();
                    myNewRow["hierarchy_level"] = objreader["hierarchy_level"].ToString();
                    objTblRQ.Rows.Add(myNewRow);
                    objTblRQ.AcceptChanges();
                }
                objreader.Close();
                childloopTopLGL(lblemployeereporting_to);
            }
            objreader.Close();
            objdbconn.CloseConn();
            return;
        }

        public void childloopTop(string employee)
        {
            msSQL = " select a.employeereporting_to, concat(b.user_firstname, '-', b.user_code) as user " +
                    " from adm_mst_tmodule2employee a " + " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid " +
                    " inner join adm_mst_tuser b on c.user_gid = b.user_gid " + " where a.module_gid = 'HRM' " +
                    " and a.employee_gid = '" + employee + "'" + " and a.hierarchy_level <> '-1' ";
            objreader = objdbconn.GetDataReader(msSQL);
            objreader.Read();
            if (objreader.HasRows == true)
            {
                lblemployeereporting_to = objreader["employeereporting_to"].ToString();
                objreader.Close();
                msSQL = " select a.employee_gid, a.hierarchy_level, concat(b.user_firstname, '-', b.user_code) as user from adm_mst_tsubmodule a  " +
                    " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " + " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " +
                    " where a.module_gid = 'HRM' and a.submodule_id='HRMLEVARL' and a.employee_gid = '" + lblemployeereporting_to + "' ";
                objreader = objdbconn.GetDataReader(msSQL);
                if (objreader.HasRows == true)
                {
                    objreader.Read();
                    DataRow myNewRow = objTblRQ.NewRow();
                    myNewRow["employee_gid"] = objreader["employee_gid"].ToString();
                    myNewRow["hierarchy_level"] = objreader["hierarchy_level"].ToString();
                    objTblRQ.Rows.Add(myNewRow);
                    objTblRQ.AcceptChanges();
                }
                objreader.Close();
                childloopTop(lblemployeereporting_to);
            }
            objreader.Close();
            objdbconn.CloseConn();
            return;
        }


        public string childloop(string employee)
        {
            msSQL = " select a.*, concat(b.user_firstname, '-', b.user_code) as user  from adm_mst_tmodule2employee a  " +
                " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " +
                " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " +
                " where a.module_gid = 'HRM'  " +
                " and a.employeereporting_to = '" + employee + "'";
            dt_table = objdbconn.GetDataTable(msSQL);
            foreach (DataRow dr_datarow in dt_table.Rows)
            {
                msSQL = " select a.*, b.user_gid  from adm_mst_tmodule2employee a  " +
                    " inner join hrm_mst_temployee c on a.employee_gid = c.employee_gid  " +
                    " inner join adm_mst_tuser b on c.user_gid = b.user_gid  " +
                    " where a.module_gid = 'HRM' ";
                msSQL += " and a.employee_gid = '" + dr_datarow["employee_gid"].ToString() + "'";
                objreader = objdbconn.GetDataReader(msSQL);
                if (objreader.HasRows == true)
                {
                    objreader.Read();
                    lsemployeeGID = lsemployeeGID + "'" + objreader["employee_gid"].ToString() + "',";
                }
                objreader.Close();
                childloop(dr_datarow["employee_gid"].ToString());
            }

            dt_table.Dispose();
            return lsemployeeGID;
        }

        // Send Mail

        public bool Mail(string to, string cc, string sub, string body)
        {
            try
            {
                msSQL = "SELECT company_mail,pop_server,pop_port,pop_username,pop_password FROM adm_mst_tcompany ";
                objreader = objdbconn.GetDataReader(msSQL);
                if (objreader.HasRows)
                {
                    ls_server = objreader["pop_server"].ToString();
                    ls_port = Convert.ToInt32(objreader["pop_port"]);
                    ls_username = objreader["pop_username"].ToString();
                    ls_password = objreader["pop_password"].ToString();
                }
                objreader.Close();
                objdbconn.CloseConn();
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(ls_username);
                message.To.Add(new MailAddress(to));
                if (cc != null & cc != string.Empty & cc != "")
                {
                    lsCCReceipients = cc.Split(',');
                    if (cc.Length == 0)
                    {
                        message.CC.Add(new MailAddress(cc));
                    }
                    else
                    {
                        foreach (string CCEmail in lsCCReceipients)
                        {
                            message.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                        }
                    }
                }
                message.Subject = sub;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = body;
                smtp.Port = ls_port;
                smtp.Host = ls_server; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(ls_username, ls_password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public int SendSMTP2(string strFrom, string strpwd, string strTo, string strSubject, string strBody, string strCC, string strBCC, string strAttachments)
        {

            msSQL = "SELECT company_mail,pop_server,pop_port,pop_username,pop_password FROM adm_mst_tcompany ";
            objreader = objdbconn.GetDataReader(msSQL);
            if (objreader.HasRows)
            {
                ls_server = objreader["pop_server"].ToString();
                ls_port = Convert.ToInt32(objreader["pop_port"]);
                ls_username = objreader["pop_username"].ToString();
                ls_password = objreader["pop_password"].ToString();
            }
            objreader.Close();
            MailMessage objMailMessage = new MailMessage();
            objMailMessage.From = new MailAddress(strFrom);
            // Set the recepient address of the mail message
            objMailMessage.To.Add(new MailAddress(strTo));


            if (strCC != null & strCC != string.Empty)
            {
                lsCCReceipients = strCC.Split(',');
                if (strCC.Length == 0)
                {
                    objMailMessage.CC.Add(new MailAddress(strCC));
                }
                else
                {
                    foreach (string CCEmail in lsCCReceipients)
                    {
                        objMailMessage.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                    }
                }
            }

            if (strBCC != null & strBCC != string.Empty)
            {
                objMailMessage.Bcc.Add(new MailAddress(strBCC));
            }

            objMailMessage.Subject = strSubject;
            // Set the body of the mail message
            objMailMessage.Body = strBody;

            // Set the format of the mail message body as HTML
            objMailMessage.IsBodyHtml = true;
            //  Set the priority of the mail message to normal
            objMailMessage.Priority = MailPriority.Normal;
            SmtpClient objSmtpClient = new SmtpClient();
            objSmtpClient.Host = ls_server;
            objSmtpClient.Port = ls_port;
            objSmtpClient.EnableSsl = true;
            objSmtpClient.UseDefaultCredentials = true;
            objSmtpClient.Credentials = new NetworkCredential(strFrom, strpwd);
            try
            {
                objSmtpClient.Send(objMailMessage);
            }
            catch
            {
                return 0;
            }

            return 1;
        }
        public DataTable ExcelToDataTable(string FileName,string range)
        {
            DataTable datatable =new DataTable();
            int totalSheet = 1;
            string lsConnectionString = string.Empty;
            string fileExtension = Path.GetExtension(FileName);
            if (fileExtension == ".xls")
            {
                lsConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";" + "Extended Properties='Excel 8.0;HDR=YES;'";
            }
            else if(fileExtension == ".xlsx")
            {
                lsConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;MAXSCANROWS=0';";
            }
           
            using (OleDbConnection objConn = new OleDbConnection(lsConnectionString))
            {
                objConn.Open();
                OleDbCommand cmd = new OleDbCommand();
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                DataSet ds = new DataSet();
                DataTable dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = string.Empty;
                if (dt != null)
                {
                    var tempDataTable = (from dataRow in dt.AsEnumerable()
                                         where !dataRow["TABLE_NAME"].ToString().Contains("FilterDatabase")
                                         select dataRow).CopyToDataTable();
                    dt = tempDataTable;
                    totalSheet = dt.Rows.Count;
                    sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                }
                sheetName = sheetName.Replace("'", "").Trim () + range;
                cmd.Connection = objConn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM ["+ sheetName + "]";
                oleda = new OleDbDataAdapter(cmd);
                oleda.Fill(ds, "excelData");
               
                datatable = ds.Tables["excelData"];  
                objConn.Close();
            }
            return datatable;
        }
        public string uploadFile(string path, string file_name)
        {
            int iUploadedCnt = 0;
            string sPath = "";
            //    sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/locker/");
            sPath = path;
            System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
            for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
            {
                System.Web.HttpPostedFile hpf = hfc[iCnt];

                if (hpf.ContentLength > 0)
                {
                    if (!File.Exists(sPath + file_name))
                    {
                        hpf.SaveAs(sPath + file_name);
                        iUploadedCnt = iUploadedCnt + 1;
                    }
                }
            }
            if (iUploadedCnt > 0)
            {
                return iUploadedCnt + " Files Uploaded Successfully";
            }
            else
            {
                return "Upload Failed";
            }
        }

        public string ExtractLast4Digits(string inputstring )
        {
            int length = inputstring.Length;
            string last4Digits = length >= 4 ? inputstring.Substring(length - 4) : inputstring;
            return last4Digits;
        }
        //public static string ExtractLast4DigitsFromConcatenatedString(string inputString)
        //{
        //    // Use Substring to get the last 4 digits
        //    int length = inputString.Length;
        //    string last4Digits = length >= 4 ? inputString.Substring(length - 4) : inputString;
        //    return last4Digits;
        //}
        public void sendMessage(string number, string message)
        {

            WebClient webClient = new WebClient();
            dbconn objdbconn = new dbconn();
            OdbcDataReader objOdbcDataReader;
            string INSTANCE_ID;
            string CLIENT_ID;
            string CLIENT_SECRET, API_URL;
            msSQL = " select whatsapp_client_id,whatsapp_instance_id,whatsapp_client_secret from adm_mst_tcompany ";
            objOdbcDataReader = objdbconn.GetDataReader(msSQL);
            if (objOdbcDataReader.HasRows == true)
            {
                INSTANCE_ID = objOdbcDataReader["whatsapp_instance_id"].ToString();
                CLIENT_ID = objOdbcDataReader["whatsapp_client_id"].ToString();
                CLIENT_SECRET = objOdbcDataReader["whatsapp_client_secret"].ToString();
                API_URL = "http://enterprise.whatsmate.net/v3/whatsapp/single/text/message/" + INSTANCE_ID;
                objOdbcDataReader.Close();
                objdbconn.CloseConn();
                try
                {
                    Payload payloadObj = new Payload(number, message);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string postData = serializer.Serialize(payloadObj);

                    webClient.Headers["content-type"] = "application/json";
                    webClient.Headers["X-WM-CLIENT-ID"] = CLIENT_ID;
                    webClient.Headers["X-WM-CLIENT-SECRET"] = CLIENT_SECRET;

                    webClient.Encoding = Encoding.UTF8;
                    string response = webClient.UploadString(API_URL, postData);
                    Console.WriteLine(response);
                }
                catch
                {

                }
            }
            else
            {
                objOdbcDataReader.Close();
                objdbconn.CloseConn();
            }

        }

        private class Payload
        {
            public string number;
            public string message;

            public Payload(string num, string msg)
            {
                number = num;
                message = msg;
            }
        }

        public void LogForAudit(string strVal)
        {

            try
            {
                string lspath = HttpContext.Current.Server.MapPath("../../documents/") + ConfigurationManager.AppSettings["company_code"] + GetMasterGID("LOGF") + "_" + System.IO.Path.GetFileName(HttpContext.Current.Request.Url.ToString()).Replace(".aspx", string.Empty).Replace("?ls=", string.Empty) + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                if ((!System.IO.File.Exists(lspath)))
                    System.IO.File.Create(lspath).Dispose();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(lspath);
                sw.WriteLine(strVal);
                sw.Close();
            }
            catch
            {
            }
        }

        public string  PopTransactionUpload(string Document_path,string employee_gid,string module_name,string folder_name)
        {

            try
            {
                string lsfile_gid = "";
                string msdocument_gid = GetMasterGID("UPLF");
                msSQL = " select company_code from adm_mst_tcompany where 1=1";
                company_code = objdbconn.GetExecuteScalar(msSQL);
                file_path = ConfigurationManager.AppSettings["file_path"].ToString();
                path = file_path + "/documents/" + company_code + "/" + module_name + "/" + folder_name;
                if (httpRequest.Files.Count > 0)
                {
                    file_name = httpPostedFile.FileName;
                    ls_readStream = httpPostedFile.InputStream;
                    ls_readStream.CopyTo(ms);

                    file_name = msdocument_gid + file_name;

                    path = path + lsfile_gid;
                    FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);

                    ms.WriteTo(file);
                    file.Close();
                    ms.Close();
                    
                }
                return path;
            }
            catch
            {
                return "error";
            }
        }

        public string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }


        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            dt.Dispose();
            return data;
        }

        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    //in case you have a enum/GUID datatype in your model
                    //We will check field's dataType, and convert the value in it.
                    if (pro.Name == column.ColumnName.TrimEnd())
                    {
                        try
                        {
                            var convertedValue = GetValueByDataType(pro.PropertyType, dr[column.ColumnName.TrimEnd()]);
                            pro.SetValue(obj, convertedValue, null);
                        }
                        catch (Exception e)
                        {
                            //ex handle code                   
                            throw;
                        }
                        //pro.SetValue(obj, dr[column.ColumnName], null);
                    }
                    else
                        continue;
                }
            }
            return obj;
        }
        public static object GetValueByDataType(Type propertyType, object o)
        {
            if (o.ToString() == "null")
            {
                return null;
            }
            if (propertyType == (typeof(Guid)) || propertyType == typeof(Guid?))
            {
                return Guid.Parse(o.ToString());
            }
            else if (propertyType == typeof(int) || propertyType.IsEnum)
            {
                return Convert.ToInt32(o);
            }
            else if (propertyType == typeof(decimal))
            {
                return Convert.ToDecimal(o);
            }
            else if (propertyType == typeof(long))
            {
                return Convert.ToInt64(o);
            }
            else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                return Convert.ToBoolean(o);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                return Convert.ToDateTime(o);
            }
            return o.ToString();
        }

        //public MemoryStream DownloadStream(string container_name, string blob_filename)
        //{
        //    try
        //    {
        //        MemoryStream memoryStream = new MemoryStream();
        //        // Retrieve storage account from connection string.
        //        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        //        // Create the blob client.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        CloudBlobContainer container = blobClient.GetContainerReference(container_name);
        //        // Retrieve reference to a blob named "photo1.jpg".
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

        //        var ls_downfile = new MemoryStream();

        //        blockBlob.DownloadToStream(ls_downfile);
        //        return ls_downfile;
        //    }
        //    catch (Exception ex)
        //    {
        //        var ls_downfile1 = new MemoryStream();
        //        return ls_downfile1;
        //    }
        //}


        //public bool UploadStream(string container_name, string blob_filename, Stream upload_stream)
        //{
        //    try
        //    {
        //        // Retrieve storage account from connection string.
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        //        // Create the blob client.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        CloudBlobContainer container = blobClient.GetContainerReference(container_name);
        //        // Retrieve reference to a blob named "myblob".
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //        // Create or overwrite the "myblob" blob with contents from a local file.
        //        if (upload_stream.Length > 0)
        //            upload_stream.Position = 0;
        //        blockBlob.UploadFromStream(upload_stream);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}


        //public bool DeleteBlob(string container_name, string blob_filename)
        //{
        //    CloudStorageAccount _CloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        //    CloudBlobClient _CloudBlobClient = _CloudStorageAccount.CreateCloudBlobClient();

        //    CloudBlobContainer _CloudBlobContainer = _CloudBlobClient.GetContainerReference(container_name);

        //    CloudBlockBlob _CloudBlockBlob = _CloudBlobContainer.GetBlockBlobReference(blob_filename);

        //    _CloudBlockBlob.Delete();

        //    return true;
        //}

        //public string UploadBlob(string container_name, string blob_filename, string filepath)
        //{
        //    try
        //    {
        //        // Retrieve storage account from connection string.
        //        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        //        // Create the blob client.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        CloudBlobContainer container = blobClient.GetContainerReference(container_name);
        //        // Retrieve reference to a blob named "myblob".
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //        // Create or overwrite the "myblob" blob with contents from a local file.
        //        using (FileStream filestream = System.IO.File.OpenRead(filepath))
        //        {
        //            blockBlob.UploadFromStream(filestream);
        //        }
        //        return filepath;
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }
        //}

        //public string DownloadBlobText(string container_name, string blob_filename)
        //{
        //    // Retrieve storage account from connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        //    // Create the blob client.
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //    // Retrieve reference to a previously created container.
        //    CloudBlobContainer container = blobClient.GetContainerReference(container_name);

        //    // Retrieve reference to a blob named "photo1.jpg".
        //    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //    try
        //    {
        //        string text;
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            blockBlob.DownloadToStream(memoryStream);
        //            text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        //        }
        //        return text;
        //    }
        //    // Save blob contents to a file.

        //    catch (Exception ex)
        //    {
        //        return "";
        //    }
        //    finally
        //    {
        //        System.GC.Collect();
        //        System.GC.WaitForPendingFinalizers();
        //        System.GC.WaitForFullGCComplete();

        //    }
        //}

        //public List<string> DownloadBlobList(string container_name)
        //{
        //    // Retrieve storage account from connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        //    // Create the blob client.
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //    // Retrieve reference to a previously created container.
        //    CloudBlobContainer container = blobClient.GetContainerReference(container_name);


        //    List<string> BlobList = new List<string>();
        //    try
        //    {
        //        BlobList = container.ListBlobs(null, false).AsEnumerable().Select(row =>
        //                (string)(row.Uri.Segments.Last())).ToList();



        //        //// Retrieve reference to a blob named "photo1.jpg".
        //        //CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //        //try
        //        //{
        //        //    string text;
        //        //    using (var memoryStream = new MemoryStream())
        //        //    {
        //        //        blockBlob.DownloadToStream(memoryStream);
        //        //        text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        //        //    }
        //        //    return text;

        //        // Save blob contents to a file.
        //        return BlobList;
        //    }

        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    finally
        //    {
        //        System.GC.Collect();
        //        System.GC.WaitForPendingFinalizers();
        //        System.GC.WaitForFullGCComplete();

        //    }
        //}


        //public string Localstoragepath(string localstoragename, string localfilename)
        //{
        //    try
        //    {
        //        // Retrieve an object that points to the local storage resource.


        //        //     Define the file name and path.

        //        String filePath = HttpContext.Current.Server.MapPath("../../Temp");

        //        using (FileStream writeStream = File.Create(filePath))
        //        {
        //            Byte[] textToWrite = new UTF8Encoding(true).GetBytes("Testing Web role storage");
        //            writeStream.Write(textToWrite, 0, textToWrite.Length);
        //        }

        //        filePath = DownloadBlobToPath("eml", localfilename, filePath);

        //        return filePath;
        //    }
        //    catch (Exception ex)
        //    {
        //        return "error";
        //    }
        //}


        //public string DownloadBlobToPath(string container_name, string blob_filename, string filepath)
        //{
        //    // Retrieve storage account from connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

        //    // Create the blob client.
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //    // Retrieve reference to a previously created container.
        //    CloudBlobContainer container = blobClient.GetContainerReference(container_name);

        //    // Retrieve reference to a blob named "photo1.jpg".
        //    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //    try
        //    {
        //        string text;
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            blockBlob.DownloadToStream(memoryStream);
        //            text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        //        }
        //        return text;
        //    }
        //    // Save blob contents to a file.

        //    catch (Exception ex)
        //    {
        //        return "";
        //    }
        //    finally
        //    {
        //        System.GC.Collect();
        //        System.GC.WaitForPendingFinalizers();
        //        System.GC.WaitForFullGCComplete();

        //    }
        //}

        //public bool CheckBlobExist(string container_name, string blob_filename)
        //{
        //    try
        //    {
        //        MemoryStream memoryStream = new MemoryStream();
        //        // Retrieve storage account from connection string.
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
        //        // Create the blob client.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        CloudBlobContainer container = blobClient.GetContainerReference(container_name);
        //        // Retrieve reference to a blob named "photo1.jpg".
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

        //        if (blockBlob.Exists())
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        } 
        //    }
        //    catch
        //    {
        //        var ls_downfile1 = new MemoryStream();
        //        return false;
        //    }
        //}
        public string GetApiMasterGID(string pModule_Code)
        {

            string company_code = "";
            lsTempGid = null;
            msSQL = " CALL ocs_mst_spApiSequenceGid('" + pModule_Code + "','" + company_code + "')";
            lsTempGid = objdbconn.GetExecuteScalar(msSQL);
            if (lsTempGid == null || lsTempGid == "")
                return "E";
            else
                return lsTempGid;
        }

        public string GetDateFormat(string lsdate)
        {
            DateTime Date;
            string[] formats = { "dd/MM/yyyy","dd-MM-yyyy",
                                 "dd/M/yyyy","dd-M-yyyy",
                                 "d/M/yyyy", "d-M-yyyy",
                                 "d/MM/yyyy","d-MM-yyyy",
                                 "dd/MM/yy", "dd-MM-yy",
                                 "dd/M/yy","dd-M-yy",
                                 "d/M/yy", "d-M-yy",
                                 "d/MM/yy", "d-MM-yy",
                                 "MMM/dd/yyyy","MMM-dd-yyyy",
                                 "MMM/d/yy","MMM-d-yy",
                                  "MMM/dd/yy","MMM-dd-yy",
                                  "M/d/yyyy h:mm:ss tt","M-d-yyyy h:mm:ss tt",
                                 "d/M/yyyy h:mm:ss tt","d-M-yyyy h:mm:ss tt",
                                  "MM/d/yyyy h:mm:ss tt","MM-d-yyyy h:mm:ss tt",
                                  "M/dd/yyyy h:mm:ss tt","M-dd-yyyy h:mm:ss tt",
                                 "dd/MM/yyyy h:mm:ss tt","dd-MM-yyyy h:mm:ss tt",
                                 "dd-M-yyyy h:mm:ss tt" ,"d-MM-yyyy h:mm:ss tt",
                                 "dd/M/yyyy h:mm:ss tt","dd-MM-yy h:mm:ss tt",
                                 "dd/MM/yy h:mm:ss tt","d/M/yyyy h:mm:ss",
                                 "d-M-yyyy h:mm:ss","dd/MM/yyyy h:mm:ss",
                                 "dd-MM-yyyy h:mm:ss","dd-M-yyyy h:mm:ss" ,
                                 "d-MM-yyyy h:mm:ss","dd/M/yyyy h:mm:ss",
                                 "dd-MM-yy h:mm:ss","dd/MM/yy h:mm:ss"};
            DateTime.TryParseExact(lsdate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out Date);
            lsconverted_date = Convert.ToDateTime(Date).ToString("yyyy-MM-dd HH:mm:ss");

            return lsconverted_date;
        }

        ////public bool UploadStream(string containername, string blob_filename, string FileExtension, MemoryStream upload_stream)
        ////{
        ////    try
        ////    {
        ////        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ////        string lsfile_name = blob_filename.ToLower();
        ////        // Retrieve storage account from connection string.
        ////        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureBlobStorageConnectionString"].ToString());
        ////        // Create the blob client.
        ////        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        ////        // Retrieve reference to a previously created container.
        ////        CloudBlobContainer container = blobClient.GetContainerReference(containername);

        ////        // Retrieve reference to a blob named "myblob".
        ////        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        ////        // Upload file content type changes based on file type code by snehith".
        ////        blockBlob.UploadFromStream(upload_stream);
        ////        if (FileExtension == ".png")
        ////        {
        ////            blockBlob.Properties.ContentType = "image/png";
        ////        }
        ////        else if (FileExtension == ".jpg" || FileExtension == ".jpeg")
        ////        {
        ////            blockBlob.Properties.ContentType = "image/jpeg";
        ////        }
        ////        else if (FileExtension == ".gif")
        ////        {
        ////            blockBlob.Properties.ContentType = "image/gif";
        ////        }
        ////        else if (FileExtension == ".tif")
        ////        {
        ////            blockBlob.Properties.ContentType = "image/tiff";
        ////        }
        ////        else if (FileExtension == ".mp4")
        ////        {
        ////            blockBlob.Properties.ContentType = "video/mp4";
        ////        }
        ////        else if (FileExtension == ".mp3")
        ////        {
        ////            blockBlob.Properties.ContentType = "audio/mpeg";
        ////        }
        ////        else if (FileExtension == ".doc")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/msword";
        ////        }
        ////        else if (FileExtension == ".docx")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        ////        }
        ////        else if (FileExtension == ".pdf")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/pdf";
        ////        }
        ////        else if (FileExtension == ".xlsx")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        ////        }
        ////        else if (FileExtension == ".xls")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/vnd.ms-excel";
        ////        }
        ////        else if (FileExtension == ".ppt")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/vnd.ms-powerpoint";
        ////        }
        ////        else if (FileExtension == ".pptx")
        ////        {
        ////            blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        ////        }
        ////        blockBlob.SetProperties();

        ////        // Create or overwrite the "myblob" blob with contents from a local file.
        ////        if (upload_stream.Length > 0)
        ////            upload_stream.Position = 0;
        ////        blockBlob.UploadFromStream(upload_stream);

        ////        return true;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        return false;
        ////    }
        ////}

        public void ErrorLogAsync(string content, string file_path)
        {
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Retrievestorageaccountfromconnectionstring.

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureBlobStorageConnectionString"].ToString());

                //Createtheblobclient.

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //Retrievereferencetoapreviouslycreatedcontainer.

                CloudBlobContainer container = blobClient.GetContainerReference("erpdocuments");

                //Retrievereferencetoablobnamed"myblob".

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(file_path);

                //Createoroverwritethe"myblob"blobwithcontentsfromalocalfile.

                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);

                //Uploadthecontenttotheblob

                blockBlob.UploadTextAsync(content);
            }
            catch

            {

            }


        }
        public string PopTransactionUpload(string fldocument, string v1, string v2)

        {

            throw new NotImplementedException();

        }

        //code by snehith for azure blob storage account file upload

        public bool UploadStream(string containername, string blob_filename, string FileExtension, MemoryStream upload_stream)

        {


            try

            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                lsfile_name = blob_filename.ToLower();

                // Retrieve storage account from connection string.

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureBlobStorageConnectionString"].ToString());

                // Create the blob client.

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.

                CloudBlobContainer container = blobClient.GetContainerReference(containername);

                // Retrieve reference to a blob named "myblob".

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

                // Upload file content type changes based on file type code by snehith".

                blockBlob.UploadFromStream(upload_stream);

                if (FileExtension == ".png")

                {

                    blockBlob.Properties.ContentType = "image/png";

                }

                else if (FileExtension == ".jpg" || FileExtension == ".jpeg")

                {

                    blockBlob.Properties.ContentType = "image/jpeg";

                }

                else if (FileExtension == ".gif")

                {

                    blockBlob.Properties.ContentType = "image/gif";

                }

                else if (FileExtension == ".tif")

                {

                    blockBlob.Properties.ContentType = "image/tiff";

                }

                else if (FileExtension == ".mp4")

                {

                    blockBlob.Properties.ContentType = "video/mp4";

                }

                else if (FileExtension == ".mp3")

                {

                    blockBlob.Properties.ContentType = "audio/mpeg";

                }

                else if (FileExtension == ".doc")

                {

                    blockBlob.Properties.ContentType = "application/msword";

                }

                else if (FileExtension == ".docx")

                {

                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                }

                else if (FileExtension == ".pdf")

                {

                    blockBlob.Properties.ContentType = "application/pdf";

                }

                else if (FileExtension == ".xlsx")

                {

                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                }

                else if (FileExtension == ".xls")

                {

                    blockBlob.Properties.ContentType = "application/vnd.ms-excel";

                }

                else if (FileExtension == ".ppt")

                {

                    blockBlob.Properties.ContentType = "application/vnd.ms-powerpoint";

                }

                else if (FileExtension == ".pptx")

                {

                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                }

                blockBlob.SetProperties();

                // Create or overwrite the "myblob" blob with contents from a local file.

                if (upload_stream.Length > 0)

                    upload_stream.Position = 0;

                blockBlob.UploadFromStream(upload_stream);

                return true;

            }

            catch (Exception ex)

            {

                return false;

            }




        }

        public void LogForAudit(string content, string path)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //Retrievestorageaccountfromconnectionstring.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureBlobStorageConnectionString"].ToString());
                //Createtheblobclient.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                //Retrievereferencetoapreviouslycreatedcontainer.
                CloudBlobContainer container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["blob_containername"]);
                //Retrievereferencetoablobnamed"myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(path);
                //Createoroverwritethe"myblob"blobwithcontentsfromalocalfile.


                // Check if the blob exists
                if (!blockBlob.Exists())
                {
                    // If the blob doesn't exist, create the blob
                    blockBlob.UploadText(content);
                }
                else
                {
                    // If the blob exists, read existing content, append new error message, and upload
                    string existingContent = blockBlob.DownloadText();

                    // Append new error message
                    existingContent += Environment.NewLine + content;

                    // Upload updated content back to the blob
                    blockBlob.UploadText(existingContent);
                }
            }
            catch
            {
            }
        }

    }

}