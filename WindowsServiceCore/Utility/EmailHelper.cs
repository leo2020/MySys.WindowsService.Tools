using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace WindowsServiceCore.Utility
{
    public class EmailHelper
    {
        #region [Construct]
        private EmailHelper()
        {
        }

        private static readonly EmailHelper MInstance = new EmailHelper();

        public static EmailHelper Instance
        {
            get
            {
                return MInstance;
            }
        }

        #endregion

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="strSendEmailAddr">发件人邮箱地址</param>
        /// <param name="strSendEmailPwd">发件人邮箱密码</param>
        /// <param name="strReceEmailAddr">收件人邮箱地址</param>
        /// <param name="strSmtp">SMTP 主机服务器</param>
        /// <param name="EmailTitle">邮件主题</param>
        /// <param name="EmailContent">邮件内容</param>
        public void SendEmail(string strSendEmailAddr, string strSendEmailPwd, string strReceEmailAddr, string strSmtp, string EmailTitle, string EmailContent)
        {
            try
            {
                List<string> list = new List<string>();
                if (!string.IsNullOrEmpty(strReceEmailAddr) && strReceEmailAddr.Contains(";"))
                {
                    list = strReceEmailAddr.Split(';').ToList();
                }
                else
                {
                    list.Add(strReceEmailAddr);
                }

                SmtpClient sc = new SmtpClient(strSmtp);//用来发送电子邮件的 SMTP 主机服务器
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(strSendEmailAddr, strSendEmailPwd);
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        MailMessage message = new MailMessage(strSendEmailAddr, item, EmailTitle, EmailContent);
                        message.IsBodyHtml = true;
                        sc.Send(message);
                    }

                }

            }
            catch (Exception ex)
            {
                
            }

        }
    }
}
