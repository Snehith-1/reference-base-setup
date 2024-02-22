using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ems.storage.Models;
using ems.utilities.Functions;
using System.IO;

namespace ems.utilities.Controllers
{
    [RoutePrefix("api/azurestorage")]
    [Authorize]
    public class azurestorageController : ApiController
    {
        Fnazurestorage objFnazurestorage = new Fnazurestorage();
       
        [ActionName("DownloadDocument")]
        [HttpPost]
        public HttpResponseMessage download_Collateraldoc(MdlTelecallingDownload values)
        {
            var ls_response = new Dictionary<string, object>();
            //values.file_path = objFnazurestorage.DecryptData(values.file_path);
            ls_response = objFnazurestorage.FnDownloadDocument(values.file_path, values.file_name);
            return Request.CreateResponse(HttpStatusCode.OK, ls_response);
        }
        
        [ActionName("FileUploadDocument")]
        [HttpPost]
        public HttpResponseMessage FileUpload_Document(MdlTelecallingDownload values)
        {
            var ls_response = new Dictionary<string, object>();
            ls_response = objFnazurestorage.DaFileUploadDocument(values.file_path);
            return Request.CreateResponse(HttpStatusCode.OK, ls_response);
        }


        [ActionName("Framework_DownloadDocument")]
        [HttpGet]
        public HttpResponseMessage Framework_DownloadDocument(string file_path, string file_name)
        {
            var ls_response = new Dictionary<string, object>();
            ls_response = objFnazurestorage.FnFramework_DownloadDocument(file_path, file_name);
            return Request.CreateResponse(HttpStatusCode.OK, ls_response);
        }
        [AllowAnonymous]
        [ActionName("FrameworkuploadDocument")]
        [HttpGet]
        public HttpResponseMessage Framework_uploadDocument(string file_path, string file_name, MemoryStream file_stream)
        {
            var ls_response = new Dictionary<string, object>();
            ls_response = objFnazurestorage.FnFramework_uploadDocument(file_path, file_name, file_stream);
            return Request.CreateResponse(HttpStatusCode.OK, ls_response);
        }


        [ActionName("OtherDownloadDocument")]
        [HttpPost]
        public HttpResponseMessage OtherDownload_Document(MdlTelecallingDownload values)
        {
            var ls_response = new Dictionary<string, object>();
            values.file_path = objFnazurestorage.DecryptData(values.file_path);
            ls_response = objFnazurestorage.FnOtherDownloadDocument(values.file_path, values.file_name,values.other_download);
            return Request.CreateResponse(HttpStatusCode.OK, ls_response);
        }
    }
}
