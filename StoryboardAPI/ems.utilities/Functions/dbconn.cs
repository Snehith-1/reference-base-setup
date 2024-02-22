using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text.Json;
using System.Web;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Data.Odbc;
namespace ems.utilities.Functions
{
    public class dbconn
    {
        private string lsConnectionString = string.Empty;

        // Get Connection String 

        public string GetConnectionString(string companyCode = "")
        {
            try
            {
                if (HttpContext.Current.Request.Headers["Authorization"] == null || HttpContext.Current.Request.Headers["Authorization"] == "null")
                {
                    lsConnectionString = ConfigurationManager.ConnectionStrings["AuthConn"].ConnectionString;
                }
                else
                {
                    using (OdbcConnection conn = new OdbcConnection(ConfigurationManager.ConnectionStrings["AuthConn"].ToString()))
                    {
                        using (OdbcCommand cmd = new OdbcCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "CALL adm_mst_spgetconnectionstring('" + HttpContext.Current.Request.Headers["Authorization"].ToString() + "')";
                            cmd.Connection = conn;
                            conn.Open();
                            lsConnectionString = cmd.ExecuteScalar().ToString();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "***********GetConnectionString");
                lsConnectionString = "error";
            }
            return lsConnectionString;
        }

        public class MdlCmnConn
        {
            public string connection_string { get; set; }
            public string company_code { get; set; }
            public string company_dbname { get; set; }
        }

        // Open Connection 

        public OdbcConnection OpenConn(string companyCode = "")
        {
            try
            {
                OdbcConnection gs_ConnDB;
                gs_ConnDB = new OdbcConnection(GetConnectionString(companyCode));
                if (gs_ConnDB.State != ConnectionState.Open)
                {
                    gs_ConnDB.Open();
                }
                return gs_ConnDB;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "***********OpenConn");
                var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "UnAuthorized" };
                throw new HttpResponseException(msg);
            }

        }

        // Close Connection



        public void CloseConn()
        {
            if (OpenConn().State != ConnectionState.Closed)
            {
                OpenConn().Dispose();
                OpenConn().Close();
            }
        }

        // Execute a Query

        public int ExecuteNonQuerySQL(string query, string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            int mnResult = 0;
            OdbcConnection ObjOdbcConnection = OpenConn();
            try
            {
                OdbcCommand cmd = new OdbcCommand(query, ObjOdbcConnection);
                mnResult = cmd.ExecuteNonQuery();
                mnResult = 1;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
            }
            ObjOdbcConnection.Close();
            return mnResult;
        }
        public int ExecuteNonQuerySQLForgot(string query, string companyCode = "", string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            int mnResult = 0;
            string val;
            OdbcConnection ObjOdbcConnection = OpenConn(companyCode);
            try
            {
                OdbcCommand cmd = new OdbcCommand(query, ObjOdbcConnection);
                mnResult = cmd.ExecuteNonQuery();
                mnResult = 1;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
            }
            ObjOdbcConnection.Close();
            return mnResult;
        }

        // Get Scalar Value
        public string GetExecuteScalar(string query, string companyCode = "", string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            string val;
            OdbcConnection ObjOdbcConnection = OpenConn(companyCode);
            try
            {
                OdbcCommand cmd = new OdbcCommand(query, ObjOdbcConnection);
                val = cmd.ExecuteScalar().ToString();
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
                val = "";
            }
            ObjOdbcConnection.Close();
            return val;

        }

        // Get Data Reader
        public OdbcDataReader GetDataReader(string query, string companyCode = "", string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand(query, OpenConn(companyCode));
                OdbcDataReader rdr;
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //rdr.Read();
                return rdr;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
                return null;
            }

        }

        // Get Data Table

        public DataTable GetDataTable(string query, string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            try
            {
                OdbcConnection ObjOdbcConnection = OpenConn();
                DataTable dt = new DataTable();
                OdbcDataAdapter da = new OdbcDataAdapter(query, ObjOdbcConnection);
                da.Fill(dt);
                ObjOdbcConnection.Close();
                return dt;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
                return null;
            }

        }

        // Get Data Set

        public DataSet GetDataSet(string query, string table, string user_gid = null, string module_reference = null, string module_name = "Log")
        {
            try
            {
                OdbcConnection ObjOdbcConnection = OpenConn();
                DataSet ds = new DataSet();
                OdbcDataAdapter da = new OdbcDataAdapter(query, ObjOdbcConnection);
                da.Fill(ds, table);
                ObjOdbcConnection.Close();
                return ds;
            }
            catch (Exception e)
            {
                LogForAudit("*******Date*****" + DateTime.Now.ToString("yyyy - MM - dd HH: mm:ss") + "***********" + e.Message.ToString() + "*****Query****" + query + "*******Apiref********" + module_reference);
                return null;
            }

        }

        public void LogForAudit(string content)
        {
            try
            {
                string file_path = "ErrorLog/" + "Log" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
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


                CloudBlockBlob blockBlob1 = container.GetBlockBlobReference(file_path);
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