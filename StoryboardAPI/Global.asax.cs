using System;
using System.IO;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;



namespace StoryboardAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {    
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);           
        }
      
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {

            if (HttpContext.Current.Request.HttpMethod == "POST")
            {
                using (var stream = new MemoryStream())
                {
                    HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    HttpContext.Current.Request.InputStream.CopyTo(stream);
                    string requestBody = Encoding.UTF8.GetString(stream.ToArray());
                    if(HttpContext.Current.Request.Path == "/StoryboardAPI/api/Login/incomingMail")
                    {
                        requestBody = requestBody.Substring(1, requestBody.Length - 2);
                    }
                    List<string> validateFile = new List<string>() { "filename", "_path", "Content-Type" };

                    if (requestBody != string.Empty)
                    {
                        if (validateFile.Any(requestBody.ToLower().Contains) == false)
                        {
                            JObject inputReqObj = JObject.Parse(requestBody);

                            foreach (var dictionary in inputReqObj)
                            {
                                if (Convert.ToString(dictionary) != null)
                                {
                                    Regex tagRegex = new Regex(@"<script[^<]*</script>");

                                    bool hasScriptTags = tagRegex.IsMatch(Convert.ToString(dictionary));
                                    if (hasScriptTags == true)
                                    {                                       
                                         throw new HttpException(500, "Bad Request");                                       
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                    }
                }
            }

        }

    }
}



