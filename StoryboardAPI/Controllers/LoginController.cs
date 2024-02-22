using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ems.utilities.Functions;
using ems.utilities.Models;
using StoryboardAPI.Models;
using StoryboardAPI.Authorization;
using System.Data;
using System.Data.Odbc;
using Newtonsoft.Json;
using RestSharp;
using System.Web.UI;
using System.Web.UI.WebControls;
using Spire.Pdf;
using System.IO;
using MimeKit;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Owin;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System.Net.Mail;
using System.Web.Mail;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Buffers.Text;
using System.Reflection.Emit;
using System.Web.Http.Results;
using Spire.Pdf.Exporting.XPS.Schema;
using System.Collections;

namespace StoryboardAPI.Controllers
{
    [RoutePrefix("api/Login")]
    [AllowAnonymous]
    public class LoginController : ApiController
    {
        dbconn objdbconn = new dbconn();
        // MySqlDataReader objMySqlDataReader;

        OdbcDataReader objMySqlDataReader;
        cmnfunctions objcmnfunctions = new cmnfunctions();
        string dashboard_flag = string.Empty;
        string msSQL = string.Empty;
        int mnResult;
        string user_status;
        string vendoruser_status;
        string tokenvalue = string.Empty;
        string user_gid = string.Empty;
        string employee_gid = string.Empty;
        string department_gid = string.Empty;
        string password = string.Empty;
        string username = string.Empty;
        string departmentname = string.Empty;
        string lscompany_code;
        string lscompany_dbname;
        string domain = string.Empty;
        string lsexpiry_time;
        DataTable dt_datatable;
        string lsuser_password, lsuser_code, lsemployee_mobileno, lsuser_gid, lscompanyid, lscontact_id, lsusercode, msGetGid, msGetGid1;

        [HttpPost]
        [ActionName("UserLogin")]
        public HttpResponseMessage PostUserLogin(PostUserLogin values)
        {
            loginresponse objloginresponse = new loginresponse();
            try
            {
                if (!String.IsNullOrEmpty(values.company_code))
                {
                    var ObjToken = Token(values.user_code, objcmnfunctions.ConvertToAscii(values.user_password), values.company_code.ToLower());
                    dynamic newobj = JsonConvert.DeserializeObject(ObjToken);
                    if (newobj.access_token != null)
                    {
                        tokenvalue = "Bearer " + newobj.access_token;
                        msSQL = "call adm_mst_spstoretoken('" + tokenvalue + "','" + values.user_code + "','" + objcmnfunctions.ConvertToAscii(values.user_password) + "','" + values.company_code + "')";
                        objMySqlDataReader = objdbconn.GetDataReader(msSQL);

                        if (objMySqlDataReader.HasRows)
                        {
                            objloginresponse.token = tokenvalue;
                            objloginresponse.user_gid = objMySqlDataReader["user_gid"].ToString();
                            objloginresponse.dashboard_flag = objMySqlDataReader["dashboard_flag"].ToString();
                            objloginresponse.c_code = values.company_code;
                            objloginresponse.message = "Login Successful!";
                            objloginresponse.status = true;
                        }
                        else
                        {
                            objloginresponse.message = "Invalid Credentials!";
                        }
                    }
                    else
                    {
                        objloginresponse.message = "Invalid Credentials!";
                    }
                }
                else
                {
                    objloginresponse.message = "Company Code cannot be empty!";
                }
            }
            catch (Exception ex)
            {
                objloginresponse.message = "Exception occured while loggin in!";
            }
            finally
            {
                if (objMySqlDataReader != null)
                    objMySqlDataReader.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, objloginresponse);
        }
        [HttpPost]
        [ActionName("UserForgot")]
        public HttpResponseMessage PostUserForgot(PostUserForgot values)
        {
            PostUserForgot GetForgotResponse = new PostUserForgot();
            domain = Request.RequestUri.Host.ToLower();
            string jsonFilePath = @" " + ConfigurationManager.AppSettings["CmnConfigfile_path"].ToString();
            string jsonString = File.ReadAllText(jsonFilePath);
            var jsonDataArray = JsonConvert.DeserializeObject<MdlCmnConn[]>(jsonString);
            string lscompany_dbname = (from a in jsonDataArray
                                       where a.company_code == values.companyid
                                       select a.company_dbname).FirstOrDefault();
            string lscompany_code = (from a in jsonDataArray
                                     where a.company_code == values.companyid
                                     select a.company_code).FirstOrDefault();
            if (lscompany_code != null && lscompany_code != " ")
            {
                msSQL = " SELECT  user_code,user_password,user_gid from adm_mst_tuser    where user_code = '" + values.usercode + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsuser_code = objMySqlDataReader["user_code"].ToString();
                    lsuser_password = objMySqlDataReader["user_password"].ToString();
                    lsuser_gid = objMySqlDataReader["user_gid"].ToString();
                }

                if (lsuser_code != null && lsuser_code != "")
                {
                    lsuser_code = lsuser_code.ToUpper();
                }
                else
                {
                    lsuser_code = null;

                }
                msSQL = " select   employee_mobileno FROM hrm_mst_temployee     where user_gid = '" + lsuser_gid + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsemployee_mobileno = objMySqlDataReader["employee_mobileno"].ToString();

                }
                msSQL = " SELECT  company_code FROM adm_mst_tcompany ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lscompany_code = objMySqlDataReader["company_code"].ToString();

                }
                if (lscompany_code != null && lscompany_code != "")
                {
                    lscompany_code = lscompany_code.ToUpper();
                }
                else
                {
                    lscompany_code = null;

                }
                if (values.companyid != null && values.companyid != "")
                {
                    lscompanyid = values.companyid.ToUpper();
                }
                else
                {
                    lscompanyid = null;

                }
                if (values.usercode != null && values.usercode != "")
                {
                    lsusercode = values.usercode.ToUpper();
                }
                else
                {
                    lsusercode = null;

                }

                if (lscompany_code == lscompanyid)
                {
                    if (lsuser_code == lsusercode)
                    {

                        if (lsemployee_mobileno == values.mobile)
                        {
                            //msSQL = " update  adm_mst_tuser set " +
                            //    " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                            //    " updated_by = '" + lsuser_gid + "'," +
                            //    " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            //mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                            msSQL = " update  adm_mst_tuser set " +
                   " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                   " updated_by = '" + lsuser_gid + "'," +
                   " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            mnResult = objdbconn.ExecuteNonQuerySQLForgot(msSQL, lscompany_dbname);
                            if (mnResult == 1)
                            {
                                GetForgotResponse.status = true;
                                GetForgotResponse.message = "Password Update Successfully !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                            }
                            else
                            {
                                GetForgotResponse.status = false;
                                GetForgotResponse.message = "Error Occur While Updating Password !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                            }

                        }
                        else
                        {

                            GetForgotResponse.status = false;
                            GetForgotResponse.message = "Mobile Number is Invaild  !";
                            return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                        }
                    }
                    else
                    {
                        GetForgotResponse.status = false;
                        GetForgotResponse.message = "User code is Invaild !";
                        return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);

                    }
                }
                else
                {

                    GetForgotResponse.status = false;
                    GetForgotResponse.message = "Company code is Invaild !";
                    return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                }

            }
            else
            {

                GetForgotResponse.status = false;
                GetForgotResponse.message = "Company code is Invaild !";
                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
            }


        }

        [HttpPost]
        [ActionName("UserReset")]
        public HttpResponseMessage PostUserReset(PostUserReset values)
        {
            PostUserReset GetRestResponse = new PostUserReset();
            domain = Request.RequestUri.Host.ToLower();
            string jsonFilePath = @" " + ConfigurationManager.AppSettings["CmnConfigfile_path"].ToString();
            string jsonString = File.ReadAllText(jsonFilePath);
            var jsonDataArray = JsonConvert.DeserializeObject<MdlCmnConn[]>(jsonString);
            string lscompany_dbname = (from a in jsonDataArray
                                       where a.company_code == values.companyid_reset
                                       select a.company_dbname).FirstOrDefault();
            string lscompany_code = (from a in jsonDataArray
                                     where a.company_code == values.companyid_reset
                                     select a.company_code).FirstOrDefault();
            if (lscompany_code != null && lscompany_code != " ")
            {
                msSQL = " SELECT  user_code,user_password,user_gid from adm_mst_tuser    where user_code = '" + values.usercode_reset + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsuser_code = objMySqlDataReader["user_code"].ToString();
                    lsuser_password = objMySqlDataReader["user_password"].ToString();
                    lsuser_gid = objMySqlDataReader["user_gid"].ToString();
                }

                if (lsuser_code != null && lsuser_code != "")
                {
                    lsuser_code = lsuser_code.ToUpper();
                }
                else
                {
                    lsuser_code = null;

                }

                msSQL = " SELECT  company_code FROM adm_mst_tcompany ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lscompany_code = objMySqlDataReader["company_code"].ToString();

                }
                if (lscompany_code != null && lscompany_code != "")
                {
                    lscompany_code = lscompany_code.ToUpper();
                }
                else
                {
                    lscompany_code = null;

                }
                if (values.companyid_reset != null && values.companyid_reset != "")
                {
                    lscompanyid = values.companyid_reset.ToUpper();
                }
                else
                {
                    lscompanyid = null;

                }
                if (values.usercode_reset != null && values.usercode_reset != "")
                {
                    lsusercode = values.usercode_reset.ToUpper();
                }
                else
                {
                    lsusercode = null;

                }

                if (lscompany_code == lscompanyid)
                {
                    if (lsuser_code == lsusercode)
                    {

                        if (lsuser_password == objcmnfunctions.ConvertToAscii(values.old_password))
                        {
                            msSQL = " update  adm_mst_tuser set " +
                                " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                                " updated_by = '" + lsuser_gid + "'," +
                                " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            mnResult = objdbconn.ExecuteNonQuerySQLForgot(msSQL, lscompany_dbname);
                            if (mnResult == 1)
                            {
                                GetRestResponse.status = true;
                                GetRestResponse.message = "Password Reset Successfully !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                            }
                            else
                            {
                                GetRestResponse.status = false;
                                GetRestResponse.message = "Error Occur While Password Reset !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                            }

                        }
                        else
                        {

                            GetRestResponse.status = false;
                            GetRestResponse.message = "Old Paaword is Invaild !";
                            return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                        }
                    }
                    else
                    {
                        GetRestResponse.status = false;
                        GetRestResponse.message = "User code is Invaild !";
                        return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);

                    }
                }
                else
                {

                    GetRestResponse.status = false;
                    GetRestResponse.message = "Company code is Invaild !";
                    return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                }

            }
            else
            {

                GetRestResponse.status = false;
                GetRestResponse.message = "Company code is Invaild !";
                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
            }


        }
        public class MdlCmnConn
        {
            public string connection_string { get; set; }
            public string company_code { get; set; }
            public string company_dbname { get; set; }
        }

        // ------------- For SSO Login & OTP Validation ------------------
        [AllowAnonymous]
        [ActionName("LoginReturn")]
        [HttpPost]
        public HttpResponseMessage GetLoginReturn(logininput values)
        {
            var url = ConfigurationManager.AppSettings["host"];
            if (url == ConfigurationManager.AppSettings["livedomain_url"].ToString())
            {
                var getSpireDocLicense = ConfigurationManager.AppSettings["SpireDocLicenseKey"];
                Spire.License.LicenseProvider.SetLicenseKey(getSpireDocLicense);
            }

            loginresponse GetLoginResponse = new loginresponse();
            string code = values.code;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestSharp.RestClient("https://login.microsoftonline.com/655a0e0e-4a74-4a0c-86d8-370a992e90a6/oauth2/v2.0/token");
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("client_id", ConfigurationManager.AppSettings["client_id"]);
            request.AddParameter("code", code);
            request.AddParameter("scope", "https://graph.microsoft.com/User.Read");
            request.AddParameter("client_secret", ConfigurationManager.AppSettings["client_secret"]);
            request.AddParameter("redirect_uri", ConfigurationManager.AppSettings["redirect_url"]);
            request.AddParameter("grant_type", "authorization_code");
            IRestResponse response = client.Execute(request);
            token json = JsonConvert.DeserializeObject<token>(response.Content);

            var client1 = new RestSharp.RestClient("https://graph.microsoft.com/v1.0/me");
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request1 = new RestRequest(Method.GET);
            request1.AddHeader("Authorization", "Bearer " + json.access_token);
            IRestResponse response1 = client1.Execute(request1);
            Rootobject json1 = JsonConvert.DeserializeObject<Rootobject>(response1.Content);
            object lsDBmobilePhone;

            if (json1.userPrincipalName != null && json1.userPrincipalName != "")
            {
                msSQL = " SELECT b.user_gid,a.department_gid, a.employee_gid, user_password, user_code, a.employee_mobileno, concat(user_firstname, ' ', user_lastname) as username FROM hrm_mst_temployee a " +
                        " INNER JOIN adm_mst_tuser b on b.user_gid = a.user_gid " +
                        " WHERE employee_emailid = '" + json1.userPrincipalName + "' and b.user_status = 'Y'";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL);

                if (objMySqlDataReader.HasRows == true)
                {

                    objMySqlDataReader.Read();
                    var tokenresponse = Token(objMySqlDataReader["user_code"].ToString(), objMySqlDataReader["user_password"].ToString());
                    dynamic newobj = Newtonsoft.Json.JsonConvert.DeserializeObject(tokenresponse);
                    tokenvalue = newobj.access_token;
                    employee_gid = objMySqlDataReader["employee_gid"].ToString();
                    user_gid = objMySqlDataReader["user_gid"].ToString();
                    department_gid = objMySqlDataReader["department_gid"].ToString();
                    GetLoginResponse.username = objMySqlDataReader["username"].ToString();
                    lsDBmobilePhone = objMySqlDataReader["employee_mobileno"].ToString();
                    objMySqlDataReader.Close();
                }
                else
                    objMySqlDataReader.Close();

                msSQL = " INSERT INTO adm_mst_ttoken ( " +
                         " token, " +
                         " employee_gid, " +
                         " user_gid, " +
                         " department_gid, " +
                         " company_code " +
                         " )VALUES( " +
                         " 'Bearer " + tokenvalue + "'," +
                         " '" + employee_gid + "'," +
                         " '" + user_gid + "'," +
                         " '" + department_gid + "'," +
                         " '" + ConfigurationManager.AppSettings["company_code"] + "')";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                GetLoginResponse.status = true;
                GetLoginResponse.message = "";
                GetLoginResponse.token = "Bearer " + tokenvalue;
                GetLoginResponse.user_gid = user_gid;

            }
            else
            {
                GetLoginResponse.user_gid = null;
            }
            return Request.CreateResponse(HttpStatusCode.OK, GetLoginResponse);
        }


        //OTP LOGIN
        [AllowAnonymous]
        [ActionName("OTPlogin")]
        [HttpPost]
        public HttpResponseMessage GetUserotpReturn(otplogin values)

        {

            try
            {

                msSQL = " SELECT * FROM hrm_mst_temployee ";

                dt_datatable = objdbconn.GetDataTable(msSQL);

                List<string> employeeemailid_List = new List<string>();

                employeeemailid_List = dt_datatable.AsEnumerable().Select(p => p.Field<string>("employee_emailid")).ToList();

                if (employeeemailid_List.Contains(values.employee_emailid))
                {
                    var username = string.Empty;
                    //string *randomNumber*/;
                    Random rnd = new Random();
                    values.otpvalue = (rnd.Next(100000, 999999)).ToString();

                    //msSQL = " SELECT * FROM hrm_mst_temployee a " +
                    //        " INNER JOIN adm_mst_tuser b on b.user_gid = a.user_gid " +
                    //        " WHERE employee_emailid = '" + values.emailid + "'";
                    //msSQL= "SELECT employee_gid,user_gid,employee_emailid,employee_mobileno, FROM hrm_mst_temployee where employee_emailid ='" + values.employee_emailid + "'";
                    msSQL = "SELECT * FROM hrm_mst_temployee where employee_emailid ='" + values.employee_emailid + "'";
                    objMySqlDataReader = objdbconn.GetDataReader(msSQL);
                    if (objMySqlDataReader.HasRows == true)
                    {
                        objMySqlDataReader.Read();
                        employee_gid = objMySqlDataReader["employee_gid"].ToString();
                        user_gid = objMySqlDataReader["user_gid"].ToString();
                        //values.employee_mobileno = objMySqlDataReader["employee_mobileno"].ToString();
                        //values.created_time= objMySqlDataReader["created_time"].ToString();
                        //values.expiry_time = objMySqlDataReader["expiry_time"].ToString();
                        values.employee_mobileno = objMySqlDataReader["employee_mobileno"].ToString();
                        values.employee_emailid = objMySqlDataReader["employee_emailid"].ToString();
                        string requestUri = ConfigurationManager.AppSettings["smspushapi_url"].ToString();
                        var client = new RestClient(requestUri);
                        var request = new RestRequest(Method.GET);
                        request.AddParameter("appid", ConfigurationManager.AppSettings["smspushapi_appid"].ToString());
                        request.AddParameter("userId", ConfigurationManager.AppSettings["smspushapi_userid"].ToString());
                        request.AddParameter("pass", ConfigurationManager.AppSettings["smspushapi_password"].ToString());
                        request.AddParameter("contenttype", "3");
                        request.AddParameter("from", ConfigurationManager.AppSettings["smspushapi_from"].ToString());
                        request.AddParameter("selfid", "true");
                        request.AddParameter("alert", "1");
                        request.AddParameter("dlrreq", "true");
                        request.AddParameter("intflag", "false");

                        request.AddParameter("to", values.employee_mobileno);
                        request.AddParameter("text", "Use Verification code " + values.otpvalue + " for One.Samunnati portal authentication.\nTEAM SAMUNNATI");

                        IRestResponse response = client.Execute(request);


                        objMySqlDataReader.Close();
                    }
                    msSQL = " INSERT INTO adm_mst_totplogin ( " +
                             " otpvalue, " +
                             " employee_gid, " +
                             " user_gid," +
                             " employee_mobileno," +
                             " created_time," +
                             " expiry_time" +
                             " )VALUES( " +
                             " '" + values.otpvalue + "'," +
                             " '" + employee_gid + "'," +
                             " '" + user_gid + "'," +
                             " '" + values.employee_mobileno + "'," +
                           " '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                          " '" + DateTime.Now.AddSeconds(60).ToString("yyyy-MM-dd HH:mm:ss") + "'" + ")";
                    mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);

                    if (mnResult == 1)
                    {
                        values.status = true;
                        values.message = "OTP sent successfully to your registered mobile number ending with" + " " + values.employee_mobileno.Substring(values.employee_mobileno.Length - 4) + "... ";

                    }
                    else
                    {
                        values.status = false;
                        values.message = "Error occurred while sending the OTP to your registered mobile number";

                    }

                }

                else
                {
                    values.status = false;
                    values.message = "Invalid email id";

                }




            }
            catch (Exception ex)
            {
                values.status = false;

                values.message = ex.ToString();
            }
            finally
            {

            }

            return Request.CreateResponse(HttpStatusCode.OK, values);
        }

        // OTPLogin verification
        [AllowAnonymous]
        [ActionName("otpverify")]
        [HttpPost]
        public HttpResponseMessage GetUserReturn(otpverify values)
        {
            otpverifyresponse GetLoginResponse = new otpverifyresponse();
            try
            {
                var username = string.Empty;

                msSQL = "SELECT expiry_time FROM adm_mst_totplogin where otpvalue ='" + values.otpvalue + "'";
                lsexpiry_time = objdbconn.GetExecuteScalar(msSQL);



                DateTime expiry_time = DateTime.Parse(lsexpiry_time);

                DateTime now = DateTime.Now;




                if (expiry_time > now)
                {
                    msSQL = "SELECT user_gid FROM adm_mst_totplogin where otpvalue ='" + values.otpvalue + "'";
                    //msSQL = " SELECT b.user_gid, a.employee_gid, a.otpvalue, b.employee_mobileno FROM adm_mst_totplogin a " +
                    //        " INNER JOIN hrm_mst_temployee b on b.employee_mobileno = a.employee_mobileno " +
                    //        " WHERE otpvalues = '" + values.otpvalue + "'";
                    //msSQL = " SELECT b.user_gid,a.department_gid, a.employee_gid, user_password, user_code, concat(user_firstname, ' ', user_lastname) as username FROM hrm_mst_temployee a " +
                    //        " INNER JOIN adm_mst_tuser b on b.user_gid = a.user_gid " +
                    //        " WHERE otpvalue = '" + values.otpvalue + "'";
                    objMySqlDataReader = objdbconn.GetDataReader(msSQL);
                    if (objMySqlDataReader.HasRows == true)
                    {
                        objMySqlDataReader.Read();
                        var user_gid = objMySqlDataReader["user_gid"].ToString();
                        msSQL = "select user_code, user_password from adm_mst_tuser where user_gid = '" + user_gid + "'";
                        objMySqlDataReader = objdbconn.GetDataReader(msSQL);
                        if (objMySqlDataReader.HasRows == true)
                        {
                            values.user_code = objMySqlDataReader["user_code"].ToString();
                            values.user_password = objMySqlDataReader["user_password"].ToString();
                            var ObjToken = Token(values.user_code, values.user_password);
                            dynamic newobj = JsonConvert.DeserializeObject(ObjToken);
                            tokenvalue = "Bearer " + newobj.access_token;

                            if (tokenvalue != null)
                            {
                                msSQL = "CALL storyboard.adm_mst_spstoretoken('" + tokenvalue + "', '" + values.user_code + "',  '" + values.user_password + "', '" + ConfigurationManager.AppSettings[domain].ToString() + "')";
                                user_gid = objdbconn.GetExecuteScalar("CALL storyboard.adm_mst_spstoretoken('" + tokenvalue + "','" + values.user_code + "','" + values.user_password + "','" + ConfigurationManager.AppSettings[domain].ToString() + "','Web','')");
                                GetLoginResponse.status = true;
                                GetLoginResponse.message = "";
                                GetLoginResponse.token = tokenvalue;
                                GetLoginResponse.user_gid = user_gid;
                            }
                        }



                    }
                    objMySqlDataReader.Close();
                }

                else
                {
                    GetLoginResponse.status = false;
                    GetLoginResponse.message = "Login time has been expired. kindly click the blade resend OTP ";

                }

            }
            catch (Exception ex)
            {
                GetLoginResponse.status = false;
                GetLoginResponse.message = "Invalid mail ID. Kindly contact your administrator";
            }
            finally
            {

            }
            return Request.CreateResponse(HttpStatusCode.OK, GetLoginResponse);
        }

        [AllowAnonymous]
        [ActionName("GetOTPFlag")]
        [HttpGet]
        public HttpResponseMessage GetOTPFlag()
        {
            otpresponse GetOtpResponse = new otpresponse();
            try
            {
                GetOtpResponse.otp_flag = ConfigurationManager.AppSettings["otpFlag"].ToString();

            }
            catch (Exception ex)
            {
                GetOtpResponse.otp_flag = "N";

            }

            return Request.CreateResponse(HttpStatusCode.OK, GetOtpResponse);
        }

        public string Token(string userName, string password, string company_code = null)
        {

            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "username", userName ),
                            new KeyValuePair<string, string> ( "Password", password ),
                            new KeyValuePair<string, string>("Scope",company_code)
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                domain = Request.RequestUri.Authority.ToLower();
                var host = HttpContext.Current.Request.Url.Host;
                if (host == "localhost")
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var response = client.PostAsync(ConfigurationManager.AppSettings["protocol"].ToString() + domain +
                               "/StoryboardAPI/token", new FormUrlEncodedContent(pairs)).Result;
                    return response.Content.ReadAsStringAsync().Result;


                }
                else
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var response = client.PostAsync(ConfigurationManager.AppSettings["protocol"].ToString() + domain +
                               "/token", new FormUrlEncodedContent(pairs)).Result;
                    return response.Content.ReadAsStringAsync().Result;

                }

            }
        }

        public void LoginErrorLog(string strVal)
        {
            try
            {
                string lspath = ConfigurationManager.AppSettings["file_path"].ToString() + "/erpdocument/LOGIN_ERRLOG/" + DateTime.Now.Year + @"\" + DateTime.Now.Month;
                if ((!System.IO.Directory.Exists(lspath)))
                    System.IO.Directory.CreateDirectory(lspath);

                lspath = lspath + @"\" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(lspath, true);
                sw.WriteLine(strVal);
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }


        [ActionName("incomingMessage")]
        [HttpPost]
        public HttpResponseMessage incomingMessage(mdlIncomingMessage values)
        {

            IEnumerable<string> headerAPIkeyValues = null;

            string APIKeyConfigured = ConfigurationManager.AppSettings["API_Key"].ToString();

            string type = "";

            if (Request.Headers.TryGetValues("API_Key", out headerAPIkeyValues))

            {

                var secretKey = headerAPIkeyValues.First();

                if (!string.IsNullOrEmpty(secretKey) && APIKeyConfigured.Equals(secretKey))

                {

                    if (ModelState.IsValid)

                    {

                        string mediaURL = "";

                        string c_code = Request.Headers.GetValues("c_code").FirstOrDefault();

                        result objresult = new result();

                        msSQL = "insert into " + c_code + ".crm_trn_twhatsappmessages(" +

                         "message_id," +

                         "contact_id," +

                         "direction," +

                         "type," +

                         "message_text," +

                         "content_type," +

                         "status," +

                         "created_date)" +

                         "values(" +

                         "'" + values.message.messageId + "'," +

                         "'" + values.message.sender.contact.contactId + "'," +

                         "'incoming'," +

                         "'" + values.message.body.type + "',";

                        if (values.message.body.type == "text")

                        {

                            type = "text";

                            msSQL += "'" + values.message.body.text.text + "'," +

                                     "null,";

                        }

                        else if (values.message.body.type == "image")

                        {

                            type = "image";

                            mediaURL = values.message.body.image.images[0].mediaUrl;

                            msSQL += "'Image'," +

                                     "null,";

                        }

                        else if (values.message.body.type == "list")

                        {

                            type = "list";

                            msSQL += "'List'," +

                                     "null,";

                        }

                        else

                        {

                            type = "file";

                            msSQL += "'File'," +

                                     "'" + values.message.body.file.files[0].contentType + "',";

                        }

                        msSQL += "'" + values.message.status + "'," +

                                 "'" + values.message.createdAt.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                        mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);

                        if (mnResult == 1)

                        {

                            if (type == "image" || type == "file")

                                fnFile(values, type, c_code);

                            objresult.status = true;

                            objresult.message = "success";


                        }

                        else

                        {

                            objresult.message = "fail";

                        }

                        msSQL = "select wvalue from " + c_code + ". crm_smm_whatsapp where id ='" + values.message.sender.contact.contactId + "'";

                        objMySqlDataReader = objdbconn.GetDataReader(msSQL);

                        if (objMySqlDataReader.HasRows)

                        {
                            objMySqlDataReader.Read();

                            lscontact_id = objMySqlDataReader["wvalue"].ToString();
                            objMySqlDataReader.Close();
                        }


                        if (lscontact_id != values.message.sender.contact.identifierValue)

                        {

                            msSQL = "insert into " + c_code + ".crm_smm_whatsapp(" +

                                    "id," +

                                    "displayName," +

                                    "wkey," +

                                    "wvalue," +

                                    "created_date )" +

                                    "values(" +

                                    "'" + values.message.sender.contact.contactId + "'," +

                                    "'Unknown Number '," +

                                    "'" + values.message.sender.contact.identifierKey + "'," +

                                    "'" + values.message.sender.contact.identifierValue + "'," +

                                    "'" + values.message.createdAt.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                            mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);

                            if (mnResult == 1)

                            {


                            }



                        }

                        else

                        {

                        }



                        return Request.CreateResponse(HttpStatusCode.OK, objresult);

                    }

                    else

                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

                    }

                }

            }

            return Request.CreateResponse(System.Net.HttpStatusCode.Forbidden, "API key is invalid.");

        }

        [ActionName("incomingMail")]
        [HttpPost]
        public HttpResponseMessage incomingMail()
        {
            string c_code = Request.Headers.GetValues("c_code").FirstOrDefault();
            msSQL = " SELECT " + c_code + ".company_code FROM adm_mst_tcompany";
            string lscompany_code = objdbconn.GetExecuteScalar(msSQL);
            result objresult = new result();
            try
            {
                
                string jsonString = Request.Content.ReadAsStringAsync().Result;
                List<incomingMail> relayMessage1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<incomingMail>>(jsonString);
                incomingMail relayMessage = relayMessage1[0];
                msSQL = "select " + c_code + ".fn_getgid('MILC', '');";
                string mailmanagement_gid = objdbconn.GetExecuteScalar(msSQL);

                msSQL = "INSERT INTO  " + c_code + ".crm_smm_mailmanagement(" +
                        "mailmanagement_gid," +
                        "to_mail," +
                        "reply_to, " +
                        "sub," +
                        "body," +
                        "direction," +
                        "created_date)" +
                        "VALUES(" +
                        " '" + mailmanagement_gid + "'," +
                        " '" + relayMessage.msys.relay_message.friendly_from + "'," +
                        " '" + relayMessage.msys.relay_message.rcpt_to + "'," +
                        " '" + relayMessage.msys.relay_message.content.subject.Replace("'", "\\'") + "'," +
                        " '" + relayMessage.msys.relay_message.content.html.Replace("'", "\\'") + "'," +
                        "'incoming'," +
                        "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                if (mnResult == 0)
                {
                    objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + objresult.message.ToString() + "*****Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Mail/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
                    objresult.message = "Error occured while inserting";
                }
                else
                {
                    bool status = mailAttachments(relayMessage.msys.relay_message.content.email_rfc822, mailmanagement_gid, c_code);
                    objresult.message = "success";
                    objresult.status = true;
                }
                msSQL = "select leadbank_gid from " + c_code + ".crm_smm_mailmanagement where to_mail='" + relayMessage.msys.relay_message.friendly_from + "';";
                string leadbank_gid = objdbconn.GetExecuteScalar(msSQL);
                if (leadbank_gid != null)
                {
                    msSQL = "update " + c_code + ".crm_smm_mailmanagement set leadbank_gid ='" + leadbank_gid + "' where mailmanagement_gid='" + mailmanagement_gid + " '; ";
                    mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                }
            }
            catch (Exception ex)
            {
                objresult.message = "Exception occured:" + ex.ToString();
                objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + objresult.message.ToString() + "*****Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Mail/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
               
            }
            return Request.CreateResponse(HttpStatusCode.OK, objresult);
        }


        public void fnFile(mdlIncomingMessage values, string type, string c_code)
        {
            try
            {
                if (type == "image")
                {
                    foreach (var item in values.message.body.image.images)
                    {
                        string ext, filename, lspath, filepath = "", lspath1 = "/erpdocument/CRM/Whatsapp/" + values.message.sender.contact.contactId + "/";
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        var client = new RestClient(ConfigurationManager.AppSettings["messageBirdMediaURL"].ToString());
                        var request = new RestRequest(item.mediaUrl.Replace("https://media.nest.messagebird.com", ""), Method.GET);
                        request.AddHeader("authorization", ConfigurationManager.AppSettings["messagebirdaccesskey"].ToString());
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            filename = response.Headers.FirstOrDefault(h => h.Name.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase))?.Value.ToString().Replace("inline; filename=\"", "").Replace("\"", "");
                            ext = System.IO.Path.GetExtension(filename).ToLower();
                            msSQL = "select " + c_code + ".fn_getgid('UPLF','')";
                            string file_gid = objdbconn.GetExecuteScalar(msSQL);
                            filepath = lspath1 + file_gid + ext;
                            lspath = ConfigurationManager.AppSettings["file_path"].ToString() + lspath1 + file_gid + ext;
                            // Save the content of the response to the specified local file
                            if ((!System.IO.Directory.Exists(ConfigurationManager.AppSettings["file_path"].ToString() + lspath1)))
                                System.IO.Directory.CreateDirectory(ConfigurationManager.AppSettings["file_path"].ToString() + lspath1);
                            File.WriteAllBytes(lspath, response.RawBytes);

                            msSQL = "insert into " + c_code + ".crm_trn_tfiles(" +
                                    "file_gid," +
                                    "message_gid," +
                                    "contact_gid," +
                                    "document_name," +
                                    "document_path)values(" +
                                    "'" + file_gid + "'," +
                                    "'" + values.message.messageId + "'," +
                                    "'" + values.message.sender.contact.contactId + "'," +
                                    "'" + filename.Replace("'", "\\'") + "'," +
                                    "'" + filepath.Replace("'", "\\'") + "')";
                            mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                            if (mnResult == 0)
                            {
                                objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***************Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Whatsapp/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
                            }
                        }
                    }
                }
                else if (type == "file")
                {
                    foreach (var item in values.message.body.file.files)
                    {
                        string ext, filename, lspath, filepath = "", lspath1 = "/erpdocument/CRM/Whatsapp/" + values.message.sender.contact.contactId + "/";
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        var client = new RestClient(ConfigurationManager.AppSettings["messageBirdMediaURL"].ToString());
                        var request = new RestRequest(item.mediaUrl.Replace("https://media.nest.messagebird.com", ""), Method.GET);
                        request.AddHeader("authorization", ConfigurationManager.AppSettings["messagebirdaccesskey"].ToString());
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            filename = response.Headers.FirstOrDefault(h => h.Name.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase))?.Value.ToString().Replace("inline; filename=\"", "").Replace("\"", "");
                            ext = System.IO.Path.GetExtension(filename).ToLower();
                            msSQL = "select " + c_code + ".fn_getgid('UPLF','')";
                            string file_gid = objdbconn.GetExecuteScalar(msSQL);
                            filepath = lspath1 + file_gid + ext;
                            lspath = ConfigurationManager.AppSettings["file_path"].ToString() + lspath1 + file_gid + ext;
                            // Save the content of the response to the specified local file
                            if ((!System.IO.Directory.Exists(ConfigurationManager.AppSettings["file_path"].ToString() + lspath1)))
                                System.IO.Directory.CreateDirectory(ConfigurationManager.AppSettings["file_path"].ToString() + lspath1);
                            File.WriteAllBytes(lspath, response.RawBytes);

                            msSQL = "insert into " + c_code + ".crm_trn_tfiles(" +
                                    "file_gid," +
                                    "message_gid," +
                                    "contact_gid," +
                                    "document_name," +
                                    "document_path)values(" +
                                    "'" + file_gid + "'," +
                                    "'" + values.message.messageId + "'," +
                                    "'" + values.message.sender.contact.contactId + "'," +
                                    "'" + filename.Replace("'", "\\'") + "'," +
                                    "'" + filepath.Replace("'", "\\'") + "')";
                            mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                            if (mnResult == 0)
                            {
                                objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "****************Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Whatsapp/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + values.message.messageId.ToString() + "*****Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Whatsapp/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
                objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + ex.Message.ToString() + "*****Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Whatsapp/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
            }
        }

        //public void LogForAuditWhatsApp(string strVal)
        //{
        //    try
        //    {
        //        string lspath = ConfigurationManager.AppSettings["file_path"] + "/erpdocument/CRM/Whatsapp/ErrorLog/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";
        //        if ((!System.IO.Directory.Exists(lspath)))
        //            System.IO.Directory.CreateDirectory(lspath);

        //        lspath = lspath + @"\" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";

        //        System.IO.StreamWriter sw = new System.IO.StreamWriter(lspath, true);
        //        sw.WriteLine(strVal);
        //        sw.Close();

        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        private bool mailAttachments(string rfc822_content, string mail_gid, string c_code)
        {

            msSQL = " SELECT a.company_code FROM adm_mst_tcompany a ";
            string lscompany_code = objdbconn.GetExecuteScalar(msSQL);
            MemoryStream ms = new MemoryStream();
            var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(rfc822_content)));

            foreach (var part in message.Attachments.Where(a => a is MimeKit.MimePart).Cast<MimeKit.MimePart>())
            {
                msSQL = "select " + c_code + ".fn_getgid('UPLF', '');";
                string file_gid = objdbconn.GetExecuteScalar(msSQL); 
                string fileName = part.FileName ?? "NoFileName";
                string fileExtension = System.IO.Path.GetExtension(fileName);
                bool status1;

                status1 = objcmnfunctions.UploadStream(ConfigurationManager.AppSettings["blob_containername"], lscompany_code + "/" + "CRM/Mail/IncomingMail/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + file_gid + fileExtension, fileExtension, ms);

                string final_path = ConfigurationManager.AppSettings["blob_containername"] + "/" + lscompany_code + "/" + "CRM/Mail/IncomingMail/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";

                string httpsUrl = ConfigurationManager.AppSettings["blob_imagepath1"] + final_path + file_gid + fileExtension + ConfigurationManager.AppSettings["blob_imagepath2"] +
                                        '&' + ConfigurationManager.AppSettings["blob_imagepath3"] + '&' + ConfigurationManager.AppSettings["blob_imagepath4"] + '&' + ConfigurationManager.AppSettings["blob_imagepath5"] +
                                        '&' + ConfigurationManager.AppSettings["blob_imagepath6"] + '&' + ConfigurationManager.AppSettings["blob_imagepath7"] + '&' + ConfigurationManager.AppSettings["blob_imagepath8"];

                using (var stream = File.Create(httpsUrl))
                {
                    part.Content.DecodeTo(stream);
                }
                ms.Close();
                msSQL = "INSERT INTO " + c_code + ".crm_trn_tfiles (" +
                           "file_gid, " +
                          "mailmanagement_gid, " +
                          "document_name, " +
                        "document_path, " +
                          "created_date) " +
                          "VALUES (" +
                           "'" + file_gid + "', " +
                          "'" + mail_gid + "', " +
                          "'" + fileName + "', " +
                           "'" + httpsUrl + "', " +
                          "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                if (mnResult == 0)
                {
                    objcmnfunctions.LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***************Query****" + msSQL + "*******Apiref********", "SocialMedia/ErrorLog/Mail/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt");
                }
            }
            return true;
        }


    }
}
