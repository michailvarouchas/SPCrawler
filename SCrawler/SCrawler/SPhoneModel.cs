using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCrawler
{
    //json model
    public class SPhoneModel
    {
        public SPhoneData data { get; set; }
    }

    public class SPhoneData
    {
        public bool flag { get; set; }
        public string phone { get; set; }
    } 
}
