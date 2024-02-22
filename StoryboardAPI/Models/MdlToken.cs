using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace StoryboardAPI.Models
{
    public class result
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class token
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
    }

    public class Rootobject
    {
        public string odatacontext { get; set; }
        public object businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public object jobTitle { get; set; }
        public string mail { get; set; }
        public object mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public object preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }
    }

    public class userlog : result
    {
        public List<userloglist> userloglist { get; set; }
    }

    public class userloglist
    {
        public string businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }
    }


    public class loginresponse
    {
        public string dashboard_flag { get; set; }
        public string token { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public string user_gid { get; set; }
        public string username { get; set; }
        public string usercode { get; set; }
        public string c_code { get; set; }
    }
    public class logininput
    {
        public string code { get; set; }
    }
    public class userlogininput
    {
        public string hostname { get; set; }
        public string company_code { get; set; }
        public string user_code { get; set; }
        public string user_password { get; set; }
        public string lawyer_email { get; set; }
    }

    public class loginERPinput
    {
        public string user_code { get; set; }
        public string company_code { get; set; }
    }

    public class loginVendorInput
    {
        public string user_code { get; set; }
        public string pass_word { get; set; }
    }

    public class appVendorInput
    {
        public string app_code { get; set; }
        public string password { get; set; }
    }
    //public class Mdladminlogin : result
    //{
    //    public string user_code { get; set; }
    //    public string user_password { get; set; }
    //    public string company_code { get; set; }
    //}
    public class PostUserLogin : result
    {
        public string user_code { get; set; }
        public string user_password { get; set; }
        public string company_code { get; set; }
    }
    public class PostUserForgot : result
    {
        public string confirmpassword { get; set; }
        public string companyid { get; set; }
        public string mobile { get; set; }
        public string password { get; set; }
        public string usercode { get; set; }
        public bool status { get; set; }
        public string message { get; set; }

    }
    public class PostUserReset : result
    {

        public string confirmpassword_reset { get; set; }
        public string companyid_reset { get; set; }
        public string old_password { get; set; }
        public string password { get; set; }
        public string usercode_reset { get; set; }
        public bool status { get; set; }
        public string message { get; set; }

    }

    public class otplogin
    {
        //internal string mobile_number;
        public string employee_emailid { get; set; }
        public string employee_mobileno { get; set; }
        public string message { get; set; }
        public string otpvalue { get; set; }
        public string created_time { get; set; }
        //public string expiry_time { get; set; }
        public bool status { get; set; }


    }
    public class otpverify : PostUserLogin
    {
        //internal string mobile_number;
        public string employee_emailid { get; set; }
        public string employee_mobileno { get; set; }
        public string message { get; set; }
        public string otpvalue { get; set; }
        public bool status { get; set; }

    }
    public class otpverifyresponse
    {
        //internal string mobile_number;
        public string token { get; set; }
        public string employee_emailid { get; set; }
        public string employee_mobileno { get; set; }
        public string message { get; set; }
        public string otpvalue { get; set; }
        public bool status { get; set; }
        public string user_gid { get; set; }

    }

    public class otpresponse
    {
        public string otp_flag { get; set; }
    }

    public class mdlIncomingMessage
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public Body body { get; set; }
        public string channelId { get; set; }
        public DateTime createdAt { get; set; }
        public string direction { get; set; }
        public DateTime lastStatusAt { get; set; }
        public string messageId { get; set; }
        public Meta meta { get; set; }
        public string platformId { get; set; }
        public string platformReferenceId { get; set; }
        public Receiver receiver { get; set; }
        public object reference { get; set; }
        public Sender sender { get; set; }
        public string status { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Body
    {
        public string type { get; set; }
        public Image image { get; set; }
        public Text text { get; set; }
        public list list { get; set; }
        public Files file { get; set; }
    }

    public class Files
    {
        public File1[] files { get; set; }
    }

    public class File1
    {
        public string contentType { get; set; }
        public string mediaUrl { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public bool isAnimated { get; set; }
    }

    public class list
    {
        public string text { get; set; }
        public string altText { get; set; }
    }

    public class Image
    {
        public string text { get; set; }
        public Image1[] images { get; set; }
    }

    public class Image1
    {
        public string mediaUrl { get; set; }
        public string altText { get; set; }
    }

    public class Text
    {
        public string text { get; set; }
    }

    public class Meta
    {
        public Extrainformation extraInformation { get; set; }
    }

    public class Extrainformation
    {
        public string conversation_id { get; set; }
        public string cost_request_id { get; set; }
        public string useCase { get; set; }
        public string use_wa_platform_account_id_approach { get; set; }
        public string timestamp { get; set; }
    }

    public class Receiver
    {
        public Contact1[] contacts { get; set; }
        public Connector1 connector { get; set; }
    }

    public class Connector1
    {
        public string id { get; set; }
        public string identifierValue { get; set; }
    }

    public class Contact1
    {
        public string id { get; set; }
        public string identifierKey { get; set; }
        public string identifierValue { get; set; }
        public string type { get; set; }
    }

    public class Sender
    {
        public Contact contact { get; set; }
    }

    public class Contact
    {
        public string contactId { get; set; }
        public string identifierKey { get; set; }
        public string identifierValue { get; set; }
    }


    public class incomingMail
    {
        public Msys msys { get; set; }
    }

    public class Msys
    {
        public Relay_Message relay_message { get; set; }
    }

    public class Relay_Message
    {
        public Content content { get; set; }
        public string customer_id { get; set; }
        public string friendly_from { get; set; }
        public string msg_from { get; set; }
        public string rcpt_to { get; set; }
        public string webhook_id { get; set; }
    }

    public class Content
    {
        public string email_rfc822 { get; set; }
        public bool email_rfc822_is_base64 { get; set; }
        public Header[] headers { get; set; }
        public string html { get; set; }
        public string subject { get; set; }
        public string text { get; set; }
        public string[] to { get; set; }
    }

    public class Header
    {
        public string ReturnPath { get; set; }
        public string MIMEVersion { get; set; }
        public string From { get; set; }
        public string Received { get; set; }
        public string Date { get; set; }
        public string MessageID { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
    }
}