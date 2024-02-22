using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using System.Collections;
using System.Text;
using System.Web.Configuration;
using ems.utilities.Functions;
using System.Net;
using System.Net.Http;
using ems.storage.Models;
using System.Security.Cryptography;
using System.Web.Http;
using System.Data.Odbc;

namespace ems.utilities.Functions
{
    public class Fnazurestorage
    {
        string lsblopname;
        string msSQL, lscompany_document_flag;
        dbconn objdbconn = new dbconn();
        cmnfunctions objcmnfunctions = new cmnfunctions();
        OdbcDataReader objOdbcDataReader;
        public MemoryStream DownloadStream(string container_name, string blob_filename, string downlod_type)
        {
            if (downlod_type == "L")
            {
                MemoryStream ms = new MemoryStream();
                lsblopname = ConfigurationManager.AppSettings["file_path"] + "/" + blob_filename;
                using (FileStream file = new FileStream(lsblopname, FileMode.Open, System.IO.FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }
                return ms;
            }

            else
            {
                try
                {
                    blob_filename = blob_filename.Replace("erpdocuments/", "");
                    MemoryStream memoryStream = new MemoryStream();
                    // Retrieve storage account from connection string.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureBlobStorageConnectionString"].ToString());
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["blob_containername"].ToString());
                    // Retrieve reference to a blob named "photo1.jpg".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

                    var ls_downfile = new MemoryStream();

                    blockBlob.DownloadToStream(ls_downfile);
                    return ls_downfile;
                }
                catch (Exception ex)
                {
                    var ls_downfile1 = new MemoryStream();
                    return ls_downfile1;
                }
            }
        }



        public bool UploadStream(string container_name, string blob_filename, MemoryStream upload_stream)
        {
            msSQL = "select company_document_flag from adm_mst_tcompany";
            lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);

            if (lscompany_document_flag == "L")
            {
                lsblopname = ConfigurationManager.AppSettings["file_path"] + "/erpdocument" + "/" + blob_filename;
                FileStream file = new FileStream(lsblopname, FileMode.Create, FileAccess.Write);
                upload_stream.WriteTo(file);
                file.Close();
                return true;
            }
            else if (lscompany_document_flag == "B")
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    // Retrieve storage account from connection string.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                    // Retrieve reference to a blob named "myblob".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
                    // Create or overwrite the "myblob" blob with contents from a local file.
                    if (upload_stream.Length > 0)
                        upload_stream.Position = 0;
                    blockBlob.UploadFromStream(upload_stream);

                    lsblopname = ConfigurationManager.AppSettings["file_path"] + "/erpdocument" + "/" + blob_filename;
                    lsblopname = lsblopname.Replace("//", "/");
                    FileStream file = new FileStream(lsblopname, FileMode.Create, FileAccess.Write);
                    upload_stream.WriteTo(file);
                    file.Close();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }


            }
            else
            {
                try
                {
                    // Retrieve storage account from connection string.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                    // Retrieve reference to a blob named "myblob".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
                    // Create or overwrite the "myblob" blob with contents from a local file.
                    if (upload_stream.Length > 0)
                        upload_stream.Position = 0;
                    blockBlob.UploadFromStream(upload_stream);
                    
                    
                        return true;
                    
                }
                catch
                {
                    return false;
                }
            }


        }

        public bool DeleteBlob(string container_name, string blob_filename)
        {
            CloudStorageAccount _CloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            CloudBlobClient _CloudBlobClient = _CloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer _CloudBlobContainer = _CloudBlobClient.GetContainerReference(container_name);

            CloudBlockBlob _CloudBlockBlob = _CloudBlobContainer.GetBlockBlobReference(blob_filename);

            _CloudBlockBlob.Delete();

            return true;
        }

        public string UploadBlob(string container_name, string blob_filename, string filepath)
        {
            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
                // Create or overwrite the "myblob" blob with contents from a local file.
                using (FileStream filestream = System.IO.File.OpenRead(filepath))
                {
                    blockBlob.UploadFromStream(filestream);
                }
                return filepath;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string DownloadBlobText(string container_name, string blob_filename)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(container_name);

            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
            try
            {
                string text;
                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                return text;
            }
            // Save blob contents to a file.

            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.WaitForFullGCComplete();

            }
        }

        public List<string> DownloadBlobList(string container_name)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(container_name);


            List<string> BlobList = new List<string>();
            try
            {
                BlobList = container.ListBlobs(null, false).AsEnumerable().Select(row =>
                        (string)(row.Uri.Segments.Last())).ToList();



                //// Retrieve reference to a blob named "photo1.jpg".
                //CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
                //try
                //{
                //    string text;
                //    using (var memoryStream = new MemoryStream())
                //    {
                //        blockBlob.DownloadToStream(memoryStream);
                //        text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                //    }
                //    return text;

                // Save blob contents to a file.
                return BlobList;
            }

            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.WaitForFullGCComplete();

            }
        }


        public string Localstoragepath(string localstoragename, string localfilename)
        {
            try
            {
                // Retrieve an object that points to the local storage resource.


                //     Define the file name and path.

                String filePath = HttpContext.Current.Server.MapPath("../../Temp");

                using (FileStream writeStream = File.Create(filePath))
                {
                    Byte[] textToWrite = new UTF8Encoding(true).GetBytes("Testing Web role storage");
                    writeStream.Write(textToWrite, 0, textToWrite.Length);
                }

                filePath = DownloadBlobToPath("eml", localfilename, filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                return "error";
            }
        }


        public string DownloadBlobToPath(string container_name, string blob_filename, string filepath)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(container_name);

            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
            try
            {
                string text;
                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                return text;
            }
            // Save blob contents to a file.

            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.WaitForFullGCComplete();

            }
        }

        public bool CheckBlobExist(string container_name, string blob_filename)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());
                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                // Retrieve reference to a blob named "photo1.jpg".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

                if (blockBlob.Exists())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                var ls_downfile1 = new MemoryStream();
                return false;
            }
        }

        public dynamic FnDownloadDocument(string file_path, string file_name)
        {

            var ls_response = new Dictionary<string, object>();
            try
            {
                // string[] strlist = file_name.Split('.');

                int lastIndex = file_name.LastIndexOf('.');
                var filename = file_name.Substring(0, lastIndex);
                var format = file_name.Substring(lastIndex + 1);
                msSQL = "select company_document_flag from adm_mst_tcompany";
                lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);
                MemoryStream ms = new MemoryStream();
                ms = DownloadStream("erpdocument", file_path, lscompany_document_flag);
                //ls_response = ConvertDocumentToByteArray(ms, strlist[0], strlist[1]);
                ls_response = ConvertDocumentToByteArray(ms, filename, format);
            }
            catch (Exception ex)
            {



            }
            return ls_response;

            //var ls_response = new Dictionary<string, object>();
            //try
            //{
            //    string[] strlist = file_name.Split('.');
            //    msSQL = "select company_document_flag from adm_mst_tcompany";
            //    lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);
            //    MemoryStream ms = new MemoryStream();
            //    ms = DownloadStream("erpdocument", file_path, lscompany_document_flag);
            //    ls_response = ConvertDocumentToByteArray(ms, strlist[0], strlist[1]); 
            //}
            //catch (Exception ex)
            //{

            //} 
            //return ls_response;
        }

        //public bool Uploaddocumentfrompath(string container_name, string blob_filename, string upload_file)
        //{
        //    try
        //    {
        //        // Retrieve storage account from connection string.
        //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString").ToString());
        //        // Create the blob client.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        CloudBlobContainer container = blobClient.GetContainerReference(container_name);
        //        // Retrieve reference to a blob named "myblob".
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);
        //        // Create or overwrite the "myblob" blob with contents from a local file.
        //        blockBlob.UploadFromFile(upload_file);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}
        public Dictionary<string, object> ConvertDocumentToByteArray(MemoryStream msstream, string filename, string format)
        {
            var ls_response = new Dictionary<string, object>();
            string downloadfile = string.Empty;
            try
            {
                string message = "";
                string msg_type = "";
                if (msstream.Length > 0)
                {
                    msstream.Position = 0;
                    downloadfile = Convert.ToBase64String(msstream.ToArray());
                    //byte[] encodeddata= Encoding.UTF8.GetBytes(downloadfile);
                    //ls_response.Add("encodedfile", encodeddata);
                    ls_response.Add("file", downloadfile);
                    ls_response.Add("format", format);
                    ls_response.Add("name", filename + "." + format);
                    ls_response.Add("status", true);
                }
                else
                {

                    ls_response.Add("message", message);
                    ls_response.Add("status", false);
                }


            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return ls_response;
        }
        public Dictionary<string, object> returnpath(bool status, string filename)
        {
            var ls_response = new Dictionary<string, object>();
            string downloadfile = string.Empty;
            try
            {
                string message = "";
                string msg_type = "";
                if (status == true)
                {
                    ls_response.Add("filepath", filename);
                    ls_response.Add("status", true);
                }
                else
                {

                    ls_response.Add("message", message);
                    ls_response.Add("status", true);
                }


            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return ls_response;
        }
        public dynamic FnFramework_DownloadDocument(string file_path, string file_name)
        {

            var ls_response = new Dictionary<string, object>();
            try
            {
                // string[] strlist = file_name.Split('.');

                int lastIndex = file_name.LastIndexOf('.');
                var filename = file_name.Substring(0, lastIndex);
                var format = file_name.Substring(lastIndex + 1);
                msSQL = "select company_document_flag from adm_mst_tcompany";
                lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);
                MemoryStream ms = new MemoryStream();
                ms = DownloadStream("erpdocument", file_path, lscompany_document_flag);
                //ls_response = ConvertDocumentToByteArray(ms, strlist[0], strlist[1]);
                ls_response = ConvertDocumentToByteArray(ms, filename, format);
            }
            catch (Exception ex)
            {
            }
            return ls_response;
        }

        public dynamic FnFramework_uploadDocument(string file_path, string file_name, MemoryStream file_stream)
        {
            bool status;
            try
            {
                //var file = file_path;
                //var fileBytes = Encoding.UTF8.GetBytes(file);
                //using (var requestStream = new MemoryStream(fileBytes))

                status = UploadStream("erpdocument", file_path, file_stream);

            }
            catch
            {
                status = false;
            }
            return status;
        }

        public dynamic DaFileUploadDocument(string file_path)
        {

            bool status;

            var ls_response = new Dictionary<string, object>();
            try
            {
                MemoryStream ms = new MemoryStream();
                string local_file_path = ConfigurationManager.AppSettings["file_path"] + "/" + file_path;
                local_file_path = local_file_path.Replace("//", "/");
                FileStream file = new FileStream(local_file_path, FileMode.Open, FileAccess.Read);
                file.CopyTo(ms);
                status = UploadStream("erpdocument", file_path, ms);
                ls_response = returnpath(status, file_path);
                ms.Close();

            }
            catch (Exception ex)
            {
                status = false;
                ls_response = returnpath(status, file_path); ;
            }
            return ls_response;
        }



        public dynamic FnOtherDownloadDocument(string file_path, string file_name, string other_download)
        {

            var ls_response = new Dictionary<string, object>();
            try
            {
                string lsContainer = "", lsconnection_string = "";
                // string[] strlist = file_name.Split('.');
                msSQL = "select container_name,connection_string from adm_mst_tazurestorageinfo where migration_flag='" + other_download + "'";
                objOdbcDataReader = objdbconn.GetDataReader(msSQL);
                if (objOdbcDataReader.HasRows == true)
                {
                    lsContainer = objOdbcDataReader["container_name"].ToString();
                    lsconnection_string = objOdbcDataReader["connection_string"].ToString();
                }
                objOdbcDataReader.Close(); 

                int lastIndex = file_name.LastIndexOf('.');
                var filename = file_name.Substring(0, lastIndex);
                var format = file_name.Substring(lastIndex + 1);
                msSQL = "select company_document_flag from adm_mst_tcompany";
                lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);
                MemoryStream ms = new MemoryStream();
                ms = OtherDownloadStream(lsContainer, file_path, lscompany_document_flag, lsconnection_string);
                //ls_response = ConvertDocumentToByteArray(ms, strlist[0], strlist[1]);
                ls_response = ConvertDocumentToByteArray(ms, filename, format);
            }
            catch (Exception ex)
            {



            }
            return ls_response;

            //var ls_response = new Dictionary<string, object>();
            //try
            //{
            //    string[] strlist = file_name.Split('.');
            //    msSQL = "select company_document_flag from adm_mst_tcompany";
            //    lscompany_document_flag = objdbconn.GetExecuteScalar(msSQL);
            //    MemoryStream ms = new MemoryStream();
            //    ms = DownloadStream("erpdocument", file_path, lscompany_document_flag);
            //    ls_response = ConvertDocumentToByteArray(ms, strlist[0], strlist[1]); 
            //}
            //catch (Exception ex)
            //{

            //} 
            //return ls_response;
        }

        public MemoryStream OtherDownloadStream(string container_name, string blob_filename, string downlod_type, string lsConnectionstring)
        {
            if (downlod_type == "L")
            {
                MemoryStream ms = new MemoryStream();
                lsblopname = ConfigurationManager.AppSettings["file_path"] + "/" + blob_filename;
                using (FileStream file = new FileStream(lsblopname, FileMode.Open, System.IO.FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }
                return ms;
            }

            else
            {
                try
                {
                    //blob_filename = blob_filename.Replace("documents/", "");
                    MemoryStream memoryStream = new MemoryStream();
                    // Retrieve storage account from connection string. 
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(lsConnectionstring);
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                    // Retrieve reference to a blob named "photo1.jpg".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob_filename);

                    var ls_downfile = new MemoryStream();

                    blockBlob.DownloadToStream(ls_downfile);
                    return ls_downfile;
                }
                catch (Exception ex)
                {
                    var ls_downfile1 = new MemoryStream();
                    return ls_downfile1;
                }
            }
        }
        public bool CheckIsValidfilename(string FileExtension, string projectFlag)
        {
            List<string> validation;
            bool isValid = false;

            if (projectFlag == "Default")
            {
                validation = new List<string>() { ".pdf", ".jpg", ".png", ".jpeg", ".odt", ".csv", ".msg", ".xls", ".xlsx", ".txt", ".ppt", ".pptx", ".doc", ".docx", ".oft", ".html" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;
                }
            }
            else if (projectFlag == "photoformatonly")
            {
                validation = new List<string>() { ".pdf", ".jpg", ".png", ".jpeg" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;

                }
            }
            else if (projectFlag == "documentformatonly")
            {
                validation = new List<string>() { ".pdf", ".jpg", ".png", ".jpeg", ".odt", ".csv", ".msg", ".xls", ".xlsx", ".txt", ".ppt", ".pptx", ".doc", ".docx", ".oft", ".html" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;

                }
            }
            else if (projectFlag == "BD")
            {
                validation = new List<string>() { ".pdf", ".jpg", ".png", ".jpeg", ".odt", ".csv", ".msg", ".xls", ".xlsx", ".txt", ".ppt", ".pptx", ".doc", ".docx", ".oft", ".html", ".mp3" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;

                }
            }
            else if (projectFlag == "photo")
            {
                validation = new List<string>() { ".jpg", ".png", ".jpeg" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;

                }
            }
            else if (projectFlag == "RSK")
            {
                validation = new List<string>() { ".xlsx" };

                if (validation.Any(FileExtension.ToLower().Contains))
                {
                    isValid = true;

                }
            }
            return isValid;

        }
        public bool CheckIsExecutable(byte[] bytes)
        {
            try
            {
                var firstBytes = new byte[5];
                Stream stream = new MemoryStream(bytes);
                using (var fileStream = stream)
                {
                    fileStream.Read(firstBytes, 0, 2);
                }
                string val = Encoding.UTF8.GetString(firstBytes);
                bool val1 = new[] { "MZ", "@E", "@e", "<?", "<%", "%€", "$p", "%C", "Se", "<s", "RE", "fu", "#I", "Fu", "ex" }.Any(c => val.Contains(c));
                return val1;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        public string EncryptData(string textData)
        {
            try
            {
                string Encryptionkey = ConfigurationManager.AppSettings["aeskey"].ToString();
                RijndaelManaged objrij = new RijndaelManaged();
                objrij.Mode = CipherMode.CBC;
                objrij.Padding = PaddingMode.PKCS7;
                objrij.KeySize = 0x80;
                objrij.BlockSize = 0x80;
                byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
                byte[] EncryptionkeyBytes = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                int len = passBytes.Length; if (len > EncryptionkeyBytes.Length)
                    len = EncryptionkeyBytes.Length; Array.Copy(passBytes, EncryptionkeyBytes, len);
                objrij.Key = EncryptionkeyBytes;
                objrij.IV = EncryptionkeyBytes;
                ICryptoTransform objtransform = objrij.CreateEncryptor();
                byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
                return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
            }
            catch (Exception ex)
            {
                return "nothing";
            }
        }
        public string DecryptData(string EncryptedText)
        {
            try
            {
                string Encryptionkey = ConfigurationManager.AppSettings["aeskey"].ToString();
                RijndaelManaged objrij = new RijndaelManaged();
                objrij.Mode = CipherMode.CBC;
                objrij.Padding = PaddingMode.PKCS7;
                objrij.KeySize = 0x80;
                objrij.BlockSize = 0x80;
                byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
                byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
                byte[] EncryptionkeyBytes = new byte[16];
                int len = passBytes.Length; if (len > EncryptionkeyBytes.Length)
                    len = EncryptionkeyBytes.Length; Array.Copy(passBytes, EncryptionkeyBytes, len);
                objrij.Key = EncryptionkeyBytes;
                objrij.IV = EncryptionkeyBytes;
                byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
                return Encoding.UTF8.GetString(TextByte);
            }
            catch (Exception ex)
            {
                return "nothing";
            }
        }
        public string EncryptWithURLEncode(string textData)
        {
            try
            {
                string Encryptionkey = ConfigurationManager.AppSettings["aeskey"].ToString();
                RijndaelManaged objrij = new RijndaelManaged();
                objrij.Mode = CipherMode.CBC;
                objrij.Padding = PaddingMode.PKCS7;
                objrij.KeySize = 0x80;
                objrij.BlockSize = 0x80;
                byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
                byte[] EncryptionkeyBytes = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                int len = passBytes.Length; if (len > EncryptionkeyBytes.Length)
                    len = EncryptionkeyBytes.Length; Array.Copy(passBytes, EncryptionkeyBytes, len);
                objrij.Key = EncryptionkeyBytes;
                objrij.IV = EncryptionkeyBytes;
                ICryptoTransform objtransform = objrij.CreateEncryptor();
                byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
                string encrypted_value = Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
                return HttpUtility.UrlEncode(encrypted_value);
            }
            catch
            {
                return "nothing";
            }
        }
        public string DecryptWithURLDecode(string EncryptedText)
        {
            try
            {
                EncryptedText = HttpUtility.UrlDecode(EncryptedText);
                string Encryptionkey = ConfigurationManager.AppSettings["aeskey"].ToString();
                RijndaelManaged objrij = new RijndaelManaged();
                objrij.Mode = CipherMode.CBC;
                objrij.Padding = PaddingMode.PKCS7;
                objrij.KeySize = 0x80;
                objrij.BlockSize = 0x80;
                byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
                byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
                byte[] EncryptionkeyBytes = new byte[16];
                int len = passBytes.Length; if (len > EncryptionkeyBytes.Length)
                    len = EncryptionkeyBytes.Length; Array.Copy(passBytes, EncryptionkeyBytes, len);
                objrij.Key = EncryptionkeyBytes;
                objrij.IV = EncryptionkeyBytes;
                byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
                return Encoding.UTF8.GetString(TextByte);
            }
            catch
            {
                return "nothing";
            }
        }


        //REPORT DOWNLOADING FUNCTION
        public Dictionary<string, object> reportStreamDownload(string path)
        {
            var ls_response = new Dictionary<string, object>();
            string file_name = Path.GetFileName(path);
            string file_format = Path.GetExtension(file_name);
            MemoryStream ms = new MemoryStream();
            using (FileStream file = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                ms.Write(bytes, 0, (int)file.Length);
            }

            ls_response = ConvertDocumentToByteArray(ms, file_name, file_format);
            return ls_response;
        }

    }


}
