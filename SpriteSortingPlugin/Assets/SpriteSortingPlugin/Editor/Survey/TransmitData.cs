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
        private const string MailAddress2 = "spriteswappingsurvey38@gmail.com";
        private const string Host = "smtp.mail.de";
        private const string Host2 = "smtp.gmail.com";
        private const int Port = 587;

        public delegate void OnMailSendCompleted(TransmitResult transmitResult);

        public event OnMailSendCompleted onMailSendCompleted;

        public void AddToMailingList(string mailAddress)
        {
            var mail = new MailMessage {From = new MailAddress(MailAddress2)};
            mail.To.Add(mail.From);
            mail.Subject = "Add to mailing list";
            mail.Body = mailAddress;

            var smtpClient = new SmtpClient(Host2)
            {
                Port = Port,
                Credentials = new NetworkCredential(MailAddress2, Password),
                EnableSsl = true,
            };

            // smtpClient.Send(mail);
            Debug.Log("send mail to add mail to mailing list");

            // smtpClient.SendCompleted += SendCompletedEventHandler;
            // smtpClient.SendAsync(mail, "mailingList");
        }

        public void SendMail(Guid surveyId, int progress, string zipFilePath, bool isResult = false)
        {
            var mail = new MailMessage {From = new MailAddress(MailAddress2)};
            mail.To.Add(mail.From);
            mail.Subject = "[" + surveyId + "] " + (isResult ? "Result" : progress.ToString());
            mail.Body = (isResult ? "Result\n" : "") + $"userId: {surveyId}\nprogress: {progress}";

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

            var smtpClient = new SmtpClient(Host2)
            {
                Port = Port,
                Credentials = new NetworkCredential(MailAddress2, Password),
                EnableSsl = true,
            };

            // smtpClient.Send(mail);
            // Debug.Log("send data with id " + surveyId);

            // smtpClient.SendCompleted += SendCompletedEventHandler;
            // smtpClient.SendAsync(mail, "sendingData");

            onMailSendCompleted?.Invoke(TransmitResult.Succeeded);
        }

        private void SendCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Debug.Log("mail canceled");
            }

            var transmitResult = TransmitResult.Succeeded;

            if (e.Error != null)
            {
                Debug.LogException(e.Error);
                transmitResult = TransmitResult.Failed;
            }
            else
            {
                Debug.Log("Message sent.");
            }

            onMailSendCompleted?.Invoke(transmitResult);
        }
    }
}