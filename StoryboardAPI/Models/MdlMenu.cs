using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoryboardAPI.Models
{
    public class menu_response : result
    {
        public List<sys_menu> menu_list { get; set; }
    }

    public class sys_menu
    {
        //public string menu_code { get; set; }
        public string text { get; set; }
        public string sref { get; set; }
        public string icon { get; set; }
        public string label { get; set; }
        public List<sys_submenu> submenu { get; set; }
    }

    public class sys_submenu
    {
       // public string submenu_code { get; set; }
        public string text { get; set; }
        public string sref { get; set; }
     //   public string sub_icon { get; set; }
        public List<sys_sub1menu> submenu { get; set; }
    }
    public class sys_sub1menu
    {
       // public string submenu_code { get; set; }
        public string text { get; set; }
        public string sref { get; set; }
      //  public string sub_icon { get; set; }
    }
}
