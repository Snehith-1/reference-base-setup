using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ems.utilities.Models
{

    public class logintoken
    {
        public string token { get; set; }
        public string employee_gid { get; set; }
        public string user_gid { get; set; }
        public string department_gid { get; set; }
        public string branch_gid { get; set; }
    }

    public class adminlogin
    {
        public string user_code { get; set; }
        public string user_password { get; set; }
        public string company_code { get; set; }
        public bool status { get; set; }
    }
}
