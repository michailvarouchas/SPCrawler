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
                    Url = $"https://www.spitogatos.gr/search/results/residential/sale/r100/m100m101m102m103m104m2011m2015m2022m2025m2033m2036m2038m6001m6002m6003m",
                    Name = ""
                }
            };
        }
    }
}
