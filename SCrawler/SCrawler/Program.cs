using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfPages = Int32.Parse(ConfigurationManager.AppSettings["numberOfPages"]);

            var urls = UrlModel.SelectedUrls();

            var fileNameList = new List<string>();
            var pagesNewIds = new List<string>();
            var tasks = new List<Task>();
            using (var client = new HttpClient())
            {
                var applicationTask = Task.Run(async () =>
                {
                    foreach (var url in urls)
                    {
                        Console.WriteLine($"Searching {url.Url}...");
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; B-l-i-t-z-B-O-T)");

                        var searchNewIds = await CrawlingFunctions.GetNewPropertyIds(numberOfPages, client, url.Url);
                        pagesNewIds.AddRange(searchNewIds);
                        if (searchNewIds.Count() > 0)
                        {
                            using (var driver = new ChromeDriver())
                            {
                                List<PropertyModel> properties = await CrawlingFunctions.GetProperties(searchNewIds, client, driver);

                                foreach (var item in properties)
                                {
                                    Console.WriteLine(item.ToString());
                                }

                                string fileName = ExcelExport.ExportToFile(properties, url);
                                fileNameList.Add(fileName);
                                Console.WriteLine($"{searchNewIds.Count()} new properties found for {url.Name}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No new properties found for {url.Name}.");

                        }

                        await EmailSender.SendMail(fileNameList, pagesNewIds.Count());
                        SaveIds(pagesNewIds);

                        Console.WriteLine($"{pagesNewIds.Count()} new properties --> END");
                            //Console.ReadKey();
                            Thread.Sleep(10000);
                    }
                });
                applicationTask.Wait();
            }
        }

        public static void SaveIds(List<string> ids)
        {
            try
            {
                using (var context = new SCrawlerEntities())
                {
                    var exportedIds = ids.Select(id => new ExportedProperties() { ExportedPropertyId = id });
                    context.ExportedProperties.AddRange(exportedIds);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
            }

        }
    }
}
