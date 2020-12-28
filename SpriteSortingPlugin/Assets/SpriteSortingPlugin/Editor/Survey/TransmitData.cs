#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using UnityEngine;
using MailMessage = System.Net.Mail.MailMessage;
using Random = System.Random;

namespace SpriteSortingPlugin.Survey
{
    public class TransmitData
    {
        private const int ShuffleThreshold = 5;

        private const string Password = "f3-A)zL[6*s57jW3";
        private const string MailAddress = "spriteswappingsurvey38@mail.de";
        private const string MailAddress2 = "spriteswappingsurvey38@gmail.com";
        private const string Host = "smtp.mail.de";
        private const string Host2 = "smtp.gmail.com";
        private const int Port = 587;

        private static readonly MailData[] MailDataArray = new MailData[]
        {
            // new MailData()
            // {
            //     mailAddress = "spriteswappingsurvey38@gmail.com", password = "f3-A)zL[6*s57jW3",
            //     host = "smtp.gmail.com",
            //     port = 587
            // },

            new MailData()
            {
                mailAddress = "spriteswappingsurvey38@mail.de", password = "f3-A)zL[6*s57jW3",
                host = "smtp.mail.de",
                port = 587
            },
            new MailData()
            {
                mailAddress = "spriteswappingsurvey39@mail.de", password = "7NnF*ftH44#SU32",
                host = "smtp.mail.de",
                port = 587
            },
            new MailData()
            {
                mailAddress = "spriteswappingsurvey40@mail.de", password = "Qz#+ZrZV6wv4N-p",
                host = "smtp.mail.de",
                port = 587
            },
        };

        public delegate void OnMailSendCompleted(TransmitResult transmitResult);

        public event OnMailSendCompleted onMailSendCompleted;

        static TransmitData()
        {
            ShuffleCredentialArray();
        }

        public void SendMail(Guid surveyId, int progress, string zipFilePath, bool isResult = false)
        {
            foreach (var credentialData in MailDataArray)
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(credentialData.mailAddress),
                    Subject = $"[{surveyId}] {(isResult ? "Result" : progress.ToString())}",
                    Body = $"{(isResult ? "Result\n" : "")}userId: {surveyId}\nprogress: {progress}",
                };
                mail.To.Add(mail.From);

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


                var smtpClient = new SmtpClient(credentialData.host)
                {
                    Port = credentialData.port,
                    Credentials = new NetworkCredential(credentialData.mailAddress, credentialData.password),
                    EnableSsl = true,
                };

                try
                {
                    smtpClient.Send(mail);
                    // onMailSendCompleted?.Invoke(TransmitResult.Failed);

                    // smtpClient.SendCompleted += SendCompletedEventHandler;
                    // smtpClient.SendAsync(mail, "sendingData");

                    onMailSendCompleted?.Invoke(TransmitResult.Succeeded);
                    return;
                }
                catch (SmtpFailedRecipientException)
                {
                    //next mail address
                }
                catch (Exception)
                {
                    onMailSendCompleted?.Invoke(TransmitResult.Failed);
                    throw;
                }
            }

            onMailSendCompleted?.Invoke(TransmitResult.Failed);
        }

        private void SendCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Debug.Log("mail canceled");
            }

            var transmitResult = TransmitResult.Succeeded;

            if (e.Error != null)
            {
                Debug.LogException(e.Error);
                transmitResult = TransmitResult.Failed;
            }
            else
            {
                // Debug.Log("Message sent.");
            }

            onMailSendCompleted?.Invoke(transmitResult);
        }

        private static void ShuffleCredentialArray()
        {
            var size = MailDataArray.Length;
            var random = new Random();
            if (size < ShuffleThreshold)
            {
                for (var i = size; i > 1; i--)
                {
                    Swap(MailDataArray, i - 1, random.Next(i));
                }

                return;
            }

            var arr = MailDataArray.ToArray();

            // Shuffle array
            for (var i = size; i > 1; i--)
            {
                Swap(arr, i - 1, random.Next(i));
            }

            //Copy items back to list
            for (var i = 0; i < arr.Length; i++)
            {
                MailDataArray[i] = arr[i];
            }
        }

        private static void Swap(IList<MailData> arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        private struct MailData
        {
            public string mailAddress;
            public string password;
            public string host;
            public int port;
        }
    }
}