using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
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
    class CrawlingFunctions
    {
        private static Random _random = new Random();

        /// <summary>
        /// Σκανάρει τη σελίδες αναζήτησης του spitogatos.gr
        /// </summary>
        /// <returns>Tα ids που δεν έχουμε ήδη κατεβάσει</returns>
        public async static Task<List<string>> GetNewPropertyIds(int pages, HttpClient client, string url)
        {
            var allPropertyIds = new List<string>();

            for (int i = 1; i <= pages; i++)
            {
                Console.WriteLine("Loading page " + i + " from " + pages);

                try
                {
                    var html = await client.GetStringAsync(new Uri(url + $"/offset_{i * 10}"));
                    //wait
                    int s = _random.Next(1, 2);
                    Thread.Sleep(s * 1000);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    //for each property on the page get the contents of media div
                    try
                    {
                        var mediaDivs = htmlDocument.DocumentNode.Descendants("div")
                            .Where(id => id.GetAttributeValue("id", "").Contains("searchDetailsListings"))
                            .SingleOrDefault()
                            .SelectNodes("div/div")
                            .Where(n => n.GetAttributeValue("class", "").Contains("media"))
                            .ToList();

                        var notFromRealEstateHrefs = mediaDivs
                            //.Where(p => !p.SelectSingleNode("div/div/a[1]").GetAttributeValue("href", "").Contains("Κτηματομεσίτης"))
                            .Select(n => n.SelectSingleNode("a[1]").GetAttributeValue("href", ""))
                            .ToList();

                        //split href string on - and get the last part which is the property id
                        var pageItemIds = new List<string>();
                        foreach (var pageItemAnchor in notFromRealEstateHrefs)
                        {
                            var parts = pageItemAnchor.Split('-');
                            int length = parts.Length;
                            pageItemIds.Add(parts[length - 1].Substring(1));
                        }

                        if (pageItemIds.Count() > 0)
                        {
                            allPropertyIds.AddRange(pageItemIds);
                        }
                    }
                    catch (Exception)
                    {
                        //this exception is thrown every time the program reaches a page with no properties
                        //no reason to continue searching subsequent pages
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                    throw;
                }
            }

            //select only non-exported propertyIds
            var newPropertyIds = new List<string>();

            try
            {
                using (var context = new SCrawlerEntities())
                {
                    var expoertedPropertyIds = context.ExportedProperties.Select(p => p.ExportedPropertyId).ToList();

                    newPropertyIds = allPropertyIds.Where(i => !expoertedPropertyIds.Contains(i)).ToList();
                }

                return newPropertyIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                throw;
            }
        }

        /// <summary>
        /// Σκανάρει τη σελίδες πωλήσεις ακινήτων του spitogatos.gr επιστρέφει το αντικείμενο που μας ενδιαφέρει
        /// </summary>
        /// <param name="idList"></param>
        /// <returns>Τα χαρακτηρηστικά των ακινήτων</returns>
        public async static Task<List<PropertyModel>> GetProperties(List<string> idList, HttpClient client, ChromeDriver driver)
        {
            string url = @"https://www.spitogatos.gr/";

            var list = new List<HtmlNode>();
            var propertyList = new List<PropertyModel>();

            int z = 1;
            foreach (var propertyId in idList)
            {
                int nProperties = idList.Count;
                Console.WriteLine($"Receiving property {z} from {nProperties}. Id: {propertyId}");
                var propertyItem = new PropertyModel();

                try
                {
                    var html = await client.GetStringAsync(new Uri(url + "-l" + propertyId));

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    //attributes div
                    var attributes = htmlDocument.DocumentNode.DescendantsAndSelf("div")
                        .Where(n => n.GetAttributeValue("class", "").Equals("padding-phone-only"))
                        .FirstOrDefault()
                        .ChildNodes
                        .Where(n => n.HasChildNodes)
                        .Select(n => new PropertyAttribute
                        {
                            Key = n.ChildNodes[1].InnerText.Replace('\t', ' ').Replace('\n', ' ').TrimStart(' ').TrimEnd(' '),
                            Value = n.ChildNodes[3].InnerText.Replace('\t', ' ').Replace('\n', ' ').TrimStart(' ').TrimEnd(' ')
                        })
                        .ToList();

                    //breadcrumbs div
                    var breadcrumbs = htmlDocument.DocumentNode.DescendantsAndSelf("div")
                        .Where(n => n.GetAttributeValue("id", "").Equals("breadCrumbs"))
                        .FirstOrDefault()
                        .ChildNodes
                        .Where(n => n.HasChildNodes && n.Name == "a")
                        .ToList();

                    //get the id
                    propertyItem.Id = propertyId;

                    //property type
                    propertyItem.PropertyType = attributes.GetValue("Τύπος");

                    //sq meters
                    var sqmeters = attributes.GetValue("Εμβαδό");
                    if (Double.TryParse(sqmeters.TrimEnd('τ', '.', 'μ', '.'), out double sqm))
                    {
                        propertyItem.SqMeteters = sqm;
                    }

                    //location
                    if (breadcrumbs.Count() > 1)
                        propertyItem.Locationp1 = breadcrumbs[1].InnerText;
                    if (breadcrumbs.Count() > 2)
                        propertyItem.Locationp2 = breadcrumbs[2].InnerText;
                    if (breadcrumbs.Count() > 3)
                        propertyItem.Locationp3 = breadcrumbs[3].InnerText;

                    //get floor(s)
                    propertyItem.Floor = attributes.GetValue("Όροφος");
                    if (!propertyItem.Floor.Contains("Ισόγειο")
                        && !propertyItem.Floor.Contains("Υπόγειο")
                        && !String.IsNullOrEmpty(propertyItem.Floor)
                        && propertyItem.PropertyType.Contains("Διαμέρισμα"))
                    {
                        propertyItem.Floor += "ος";
                    }

                    //get price
                    var pricetext = attributes.GetValue("Τιμή");
                    if (Double.TryParse(pricetext.TrimStart('&', '#', '8', '3', '6', '4', ';'), out double price))
                        propertyItem.Price = price;

                    //get price per sq meter
                    var pricePerSqMeterText = attributes.GetValue("Τιμή ανά τ.μ.");
                    if (Double.TryParse(pricePerSqMeterText.TrimStart('&', '#', '8', '3', '6', '4', ';'), out double pricePerSqMeter))
                        propertyItem.PricePerSqMeter = pricePerSqMeter;

                    //get year
                    string year = attributes.GetValue("Έτος κατασκευής");
                    if (Int32.TryParse(year, out int propYear))
                        propertyItem.Year = propYear;

                    //get number of bedrooms
                    string bedrooms = attributes.GetValue("Υπνοδωμάτια");
                    if (Int32.TryParse(bedrooms, out int propBed))
                        propertyItem.Bedroms = propBed;

                    //get number of bathrooms
                    string bathrooms = attributes.GetValue("Μπάνια");
                    if (Int32.TryParse(bathrooms, out int propRooms))
                        propertyItem.Toilets = propRooms;

                    //get parking availability
                    propertyItem.Parking = attributes.GetValue("Θέση στάθμευσης");

                    //get fireplace availability
                    string additionalAttr = attributes.GetValue("Εσωτερικά Χαρακτηριστικά");
                    propertyItem.Fireplace = additionalAttr.Contains("Τζάκι: Ναι") ? "Ναι" : "Όχι";

                    //get autonomous heating system attr
                    propertyItem.AutonomousHeat = attributes.GetValue("Σύστημα Θέρμανσης");

                    #region GetPhone
                    //get phone using chrome driver
                    driver.Url = url + "-l" + propertyId;

                    //run script on the browser to reveal phone
                    var js = (IJavaScriptExecutor)driver;

                    string clickPhoneAnchor = @"document.getElementById('propertyListingButton').getElementsByTagName('a')[0].click();";
                    string returnPhones = @"return document.getElementById('agentPhone').getElementsByTagName('span');";

                    js.ExecuteScript(clickPhoneAnchor);

                    //wait for the results of the click event
                    int s = _random.Next(4, 6);
                    Thread.Sleep(s * 1000);

                    //and then get the phones from the recently updated dom
                    dynamic phoneSpans = js.ExecuteScript(returnPhones);

                    if (phoneSpans != null && phoneSpans.Count > 0)
                    {
                        propertyItem.Phone = phoneSpans[1].Text;
                        if (phoneSpans.Count == 4)
                        {
                            propertyItem.Phone += "/" + phoneSpans[3].Text;
                        }
                    }
                    #endregion GetPhone

                    //add to list
                    propertyList.Add(propertyItem);
                    z++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                    z++;
                }
            }
            return propertyList;
        }
    }
}
