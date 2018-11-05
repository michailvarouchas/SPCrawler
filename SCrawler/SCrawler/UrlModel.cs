using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCrawler
{
    public class UrlModel
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public static List<UrlModel> SelectedUrls()
        {
            return new List<UrlModel>
            {
                new UrlModel()
                {
                    Url = "https://www.spitogatos.gr/search/results/residential/sale/r101/m2103m",
                    Name = "page1"
                }
            };
        }
    }
}
