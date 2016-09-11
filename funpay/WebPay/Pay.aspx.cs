using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebPay
{
    public partial class Pay : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var amount = Request["rechargeMoney"];
            var bankcode = Request["payType"];
            var username = Request["UserName"];

            if (string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(bankcode) || string.IsNullOrWhiteSpace(username))
                return;

            var returnUrl = ConfigurationManager.AppSettings["RETURN_URL"];
            var noticeUrl = ConfigurationManager.AppSettings["NOTIFY_URL"];
            var partnerID = ConfigurationManager.AppSettings["PAY_ID"];
            var partnerKey = ConfigurationManager.AppSettings["PAY_SECRET"];
            var amount1 = (Convert.ToDecimal(amount) * 100).ToString();
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");
            var dic = new Dictionary<string, string>
            {
                {"version", "1.0"},
                {"serialID", now},
                {"submitTime", now},
                {"failureTime", ""},
                {"customerIP", GetIP()},
                {"orderDetails", $"{now},{amount1},,【{username}】-充值,1"},
                {"totalAmount", amount1},
                {"type", "1000"},
                {"buyerMarked", ""},
                {"payType", "ALL"},
                {"orgCode", bankcode},
                {"currencyCode", "1"},
                {"directFlag", "0"},
                {"borrowingMarked", "0"},
                {"couponFlag", "1"},
                {"platformID", ""},
                {"returnUrl", returnUrl},
                {"noticeUrl", noticeUrl},
                {"partnerID", partnerID},
                {"remark", username},
                {"charset", "1"},
                {"signType", "2"},
                {"pkey", partnerKey}
            };

            var url = "";
            foreach (var item in dic)
            {
                if (url != "")
                    url += "&";

                url += $"{item.Key}={item.Value}";
            }

            const string bankUrl = "https://www.funpay.com/website/pay.htm";
            var signMsg = MD5(url);
            dic.Add("signMsg", signMsg);

            var html = "<!DOCTYPE HTML>";
            html += "<html>";
            html += "<head>";
            html += "<meta charset=\"utf-8\">";
            html += "<title>乐盈支付收银台</title>";
            html += "</head>";
            html += "<body onload=\"document.form1.submit()\">";
            html += $"<form method=\"post\" name=\"form1\" action=\"{bankUrl}\"> ";

            foreach (var item in dic)
                html += $"<input type=\"hidden\" name=\"{item.Key}\" value=\"{item.Value}\"/>";

            html += "<input type=\"submit\" style=\"display:none;\"/>";
            html += "</form>";
            html += "</body>";
            html += "</html>";

            Response.Write(html);
        }

        private static string GetIP()
        {
            return HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null
                ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0]
                : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        private static string MD5(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "").ToLower();
        }
    }
}