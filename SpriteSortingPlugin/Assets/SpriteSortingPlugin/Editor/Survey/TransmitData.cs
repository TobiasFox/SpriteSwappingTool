using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using UnityEngine;
using MailMessage = System.Net.Mail.MailMessage;

namespace SpriteSortingPlugin.Survey
{
    public class TransmitData
    {
        private const string Password = "f3-A)zL[6*s57jW3";
        private const string MailAddress = "spriteswappingsurvey38@mail.de";
        private const string Host = "smtp.mail.de";
        private const int Port = 587;

        public void SendMail(Guid surveyId, int progress, string zipFilePath)
        {
            var mail = new MailMessage {From = new MailAddress(MailAddress)};
            mail.To.Add(mail.From);
            mail.Subject = "[" + surveyId + "] " + progress;
            mail.Body = "test message";

            if (!string.IsNullOrEmpty(zipFilePath))
            {
                // Create  the file attachment for this email message.
                var data = new Attachment(zipFilePath, MediaTypeNames.Application.Octet);

                var disposition = data.ContentDisposition;
                disposition.CreationDate = File.GetCreationTime(zipFilePath);
                disposition.ModificationDate = File.GetLastWriteTime(zipFilePath);
                disposition.ReadDate = File.GetLastAccessTime(zipFilePath);

                mail.Attachments.Add(data);
            }

            var smtpClient = new SmtpClient(Host)
            {
                Port = Port,
                Credentials = new NetworkCredential(MailAddress, Password),
                EnableSsl = true,
            };

            // smtpClient.Send(mail);
            Debug.Log("send data with id " + surveyId);

            smtpClient.SendCompleted += SendCompletedEventHandler;
            // smtpClient.SendAsync(mail, "asyncToken");
        }

        private void SendCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Debug.Log("mail canceled");
            }

            if (e.Error != null)
            {
                Debug.LogException(e.Error);
            }
            else
            {
                Debug.Log("Message sent.");
            }
        }
    }
}