using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCrawler
{
    class EmailSender
    {
        public static async Task SendMail(List<string> fileNameList, int newProperties)
        {
            string sendGridKey = ConfigurationManager.AppSettings["sendGridKey"];
            string filePath = ConfigurationManager.AppSettings["filePath"];

            string[] mailFrom = ConfigurationManager.AppSettings["mailFrom"].ToString().Split(';'); //[email, name]

            var client = new SendGridClient(sendGridKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(mailFrom[0], mailFrom[1]),
                Subject = "Export new properties",
            };

            //add attachments
            if (newProperties > 0)
            {
                foreach (var fileName in fileNameList)
                {
                    var bytes = File.ReadAllBytes(filePath + $"\\{fileName}");
                    var file = Convert.ToBase64String(bytes);
                    msg.AddAttachment(fileName, file);
                }
            }

            //add recipients
            List<EmailName> mailList = GetMailList();
            foreach (var item in mailList)
            {
                msg.AddTo(new EmailAddress(item.Email, item.Name));
            }

            //add html content
            msg.HtmlContent = GetHtml(newProperties);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                Console.WriteLine("Email Sent.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Email will not be delivered!");
                Console.ReadKey();
            }
            else if (response.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine("You have exceeded your daily limit!");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"Error while sending the email! \nERROR CODE:{response.StatusCode.ToString()}, \nHEADERS: {response.Headers.ToString()}, \nBODY: {response.Body.ToString()}");
                Console.ReadKey();
            }

        }

        private static string GetHtml(int newProperties)
        {
            string templatePath = ConfigurationManager.AppSettings["templatePath"];

            string title, paragraph, date;
            if (newProperties > 0)
            {
                title = $"Βρέθηκαν {newProperties} νέα ακίνητα.";
                paragraph = "Βρείτε το αρχείο excel σε επισύναψη";
            }
            else
            {
                title = "Δεν βρέθηκαν νέα ακίνητα.";
                paragraph = "";
            }
            date = DateTime.Now.ToString("dd/MM/yyyy");

            var sbMail = new StringBuilder();
            using (var sReader = new StreamReader(templatePath))
            {
                sbMail.Append(sReader.ReadToEnd());
                sbMail.Replace("{title}", title);
                sbMail.Replace("{paragraph}", paragraph);
                sbMail.Replace("{date}", date);
            }
            return sbMail.ToString();
        }

        private static List<EmailName> GetMailList()
        {
            var mailToEmailList = ConfigurationManager.AppSettings["mailTo"].ToString().Split(';').ToList();
            var mailToNameList = ConfigurationManager.AppSettings["mailToName"].ToString().Split(';').ToList();
            var mailList = new List<EmailName>();
            for (int i = 0; i < mailToEmailList.Count(); i++)
            {
                var mailItem = new EmailName
                {
                    Email = mailToEmailList[i],
                    Name = mailToNameList[i]
                };
                mailList.Add(mailItem);
            }

            return mailList;
        }
    }

    public class EmailName
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
