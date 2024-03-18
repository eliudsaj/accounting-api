using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using pyme_finance_api.Models.Authentication;
using pyme_finance_api.Models.Sales;
using pyme_finance_api.Models.StockInv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pyme_finance_api.Service.MailService
{

    public interface ImailService
    {
            void SendCompanyTrailRegistrationEmail(Company company,string webrootpath,string license,string companyref,string username, string password);

        void SendPurchaseRequest( string webrootpath,  Users userdata, string receiptRef);

        void EmailCustomerQuotation(List<InventoryItem> pr_details , string webrootpath, License lic,string email);




    }

    public class MailService : ImailService
    {
        public void EmailCustomerQuotation(List<InventoryItem> inv_details, string webrootpath, License lic,string customeremail)
        {
            string textBody = " <table border=" + 1 + " cellpadding=" + 1 + " cellspacing=" + 1 + "  style=\"font-family: Arial, Helvetica, sans-serif;\" >" +
                "<tr >" +
                "  <td> <b>Product Name</b></td>   <td><b>Unit Price</b></td>  <td> <b> Quantity </b> </td> <td> <b> Vat(%) </b> </td>  <td> <b>Vat(Amt) </b> </td>   <td> <b>Total Amt </b> </td>" +
                "</tr>";

            double subtotal = 0;
            double vatTotal = 0;
            double totals = 0;
            string img_inv_path = Path.Combine(webrootpath, "Images", "inventory");
            foreach (var item in inv_details)
            {
                subtotal = (subtotal + item.pd_unitprice * item.pd_qty);
                vatTotal = vatTotal + item.pd_vat_amt;
                totals = vatTotal + subtotal;
                string base64ImageRepresentation1 = "";
                string full_inv_path = "";

                if (!String.IsNullOrEmpty(item.image_path))
                {
                     full_inv_path = Path.Combine(img_inv_path, item.image_path);
                    byte[] imageArray1 = System.IO.File.ReadAllBytes(full_inv_path);
                base64ImageRepresentation1 = Convert.ToBase64String(imageArray1);

                }
            
     



                string src = "data:image/jpg;base64,"+base64ImageRepresentation1;
                string src1 = "http://pyme.ngenx.io/" + full_inv_path;

                textBody += $"<tr>  </td> <td>" + item.pd_item_name + "</td>  <td>" + String.Format("{0:n}", item.pd_unitprice) + "</td> <td>" + item.pd_qty + "</td>    <td> " + item.pd_vat_perc+ "</td>   <td> " + String.Format("{0:n}", item.pd_vat_amt)  + "</td> <td> " + String.Format("{0:n}", (item.pd_vat_amt + (item.pd_unitprice * item.pd_qty))) + "</td>    </tr>";
            }
            textBody += "<tr>    <td colspan="+5+ " style=\"text-align:right;\"> Subtotal</td> <td> " + String.Format("{0:n}", subtotal) + "</td>    </tr>";
            textBody += "<tr>    <td colspan=" + 5 + " style=\"text-align:right;\"> Vat total</td> <td> " + String.Format("{0:n}", vatTotal) + "</td>    </tr>";
            textBody += "<tr>   <td colspan=" + 5+ " style=\"text-align:right;\"> Totals</td> <td> " + String.Format("{0:n}", totals) + "</td>    </tr>";
            textBody += "</table>";




            string img_path = Path.Combine(webrootpath, "Images", "company_profile");
            string full_imgPath = "";
            if (String.IsNullOrEmpty(lic.CompanyLogo))
            {
                full_imgPath = Path.Combine(img_path, "invoice_default.jpg");

            }
            else
            {
                full_imgPath = Path.Combine(img_path, lic.CompanyLogo);
            }
          
            
            byte[] imageArray = System.IO.File.ReadAllBytes(full_imgPath);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            lic.CompanyLogo = base64ImageRepresentation;
            string compimage = "data:image/png;base64,"+ lic.CompanyLogo;



            var path = Path.Combine(webrootpath, "EmailTemplates", "QuotationEmailTemplate.html");
            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(path))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }

            //{0} : additional details  
            //{1} : tabledata  
            //{2} : CompanyName  
            //{3} : Addresss
            //{4} : Logo
            //{4} : Date


            string messageBody = string.Format(builder.HtmlBody,
                "",
                textBody.Trim(),
                lic.CompanyName,
                lic.CompanyPostal,
                lic.CompanyContact,
                DateTime.Today.ToString("dd-MM-yyyy")

            );



            //send email
            var email = new MimeMessage();
            // email.From.Add(MailboxAddress.Parse("bwangocho@gmail.com"));
            email.From.Add(new MailboxAddress("PYME", "noreply@ngenx.io"));
            email.To.Add(MailboxAddress.Parse(customeremail));
            email.Subject = "Quotation on items requested";
            email.Body = new TextPart("html")
            {
                Text =
                    messageBody
            };

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.Connect("Smtp.munshiram.com", 587, SecureSocketOptions.Auto);
                    smtp.Authenticate("email@xample.com", "1234");
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }




        }

        public void SendCompanyTrailRegistrationEmail(Company company, string webrootpath, string license, string companyref, string username, string password)
        {

            //send Email
            var path = Path.Combine(webrootpath, "EmailTemplates", "Company_Trial_Register.html");

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(path))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            //{0} : Subject  
            //{1} : DateTime  
            //{2} : Email  
            //{3} : Password  
            //{4} : Message  
            //{5} : callbackURL  

            string messageBody = string.Format(builder.HtmlBody,
                "Welcome to PYME Finance",
                string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(company.CpName.ToLower()),
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase((company.CpAdminFirstname + " " + company.CpAdminLastname).ToLower()),
                string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                string.Format("{0:dddd, d MMMM yyyy}", DateTime.Now.AddDays(30)),
                 license,
                companyref.Trim(),
                username.Trim(),
                password.Trim()
            );


            //send email
            var email = new MimeMessage();
           // email.From.Add(MailboxAddress.Parse("bwangocho@gmail.com"));
            email.From.Add(new MailboxAddress("PYME", "noreply@ngenx.io"));
            email.To.Add(MailboxAddress.Parse(company.CpAdminEmail));
            email.Subject = "Welcome " + company.CpAdminFirstname + " " + company.CpAdminLastname;
            email.Body = new TextPart("html")
            {
                Text =
               messageBody
            };

            // send email
            //using var smtp = new SmtpClient();
            //smtp.Connect("in-v3.mailjet.com", 587, SecureSocketOptions.StartTls);
            //smtp.Authenticate("cab7a809219d11a0cae71576d16596a1", "5395b324384107eb1e80c3ffa076327b");
            //smtp.Send(email);
            //smtp.Disconnect(true);

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.Connect("Smtp.munshiram.com", 587, SecureSocketOptions.Auto);
                    smtp.Authenticate("email@example.com", "1234");
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        



        }

        public void SendPurchaseRequest( string webrootpath, Users userdata, string receiptRef)
        {

            //send Email
            var path = Path.Combine(webrootpath, "EmailTemplates", "PurchaseRequest.html");
            var imgsnipp_path = Path.Combine(webrootpath, "Images", "email_images", "purchaseRequest_snipp.png");

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(path))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            //{0} : Subject  
            //{1} : DateTime  
            //{2} : Email  
            //{3} : Password  
            //{4} : Message  
            //{5} : callbackURL  

            string messageBody = string.Format(builder.HtmlBody,
               String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
               userdata.UFirstName + " " + userdata.ULastName,
               receiptRef
              );


            //send email
            var email = new MimeMessage();
            // email.From.Add(MailboxAddress.Parse("bwangocho@gmail.com"));
            email.From.Add(new MailboxAddress("PYME", "noreply@ngenx.io"));
            email.To.Add(MailboxAddress.Parse(userdata.UEmail));
            email.Subject = "Purchase Request " + receiptRef;
            email.Body = new TextPart("html")
            {
                Text =
               messageBody
            };

            // send email
            //using var smtp = new SmtpClient();
            //smtp.Connect("in-v3.mailjet.com", 587, SecureSocketOptions.StartTls);
            //smtp.Authenticate("cab7a809219d11a0cae71576d16596a1", "5395b324384107eb1e80c3ffa076327b");
            //smtp.Send(email);
            //smtp.Disconnect(true);

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.Connect("Smtp.munshiram.com", 587, SecureSocketOptions.Auto);
                    smtp.Authenticate("email@example.com", "1234");
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
