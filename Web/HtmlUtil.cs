using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Nistec.Web
{
    public class HtmlUtil
    {
        public static string HtmlDecode(string message)
        {
            if (message == null)
                return "";
            message = System.Web.HttpUtility.HtmlDecode(message);
            message = message.Replace("&quot;", "\"");
            message = message.Replace("&apos;", "'");
            message = message.Replace("&amp;", "&");

            return message;
        }
        public static string HtmlEncode(string message)
        {
            if (message == null)
                return "";
            message = System.Web.HttpUtility.HtmlEncode(message);

            return message;
        }

        public static string ToHtmlTable(NameValueCollection data, params string[] excludeKeys)
        {

            StringBuilder sb = new StringBuilder("<table>");

            var totalRows = data.GetValues(0).Count();
            for (int i = 0; i < totalRows; i++)
            {
                sb.AppendLine("<tr>");
                foreach (var key in data.AllKeys)
                {
                    if (excludeKeys != null && excludeKeys.Contains(key))
                        continue;
                    sb.AppendLine("<td>" + key + "</td>");
                    sb.AppendLine("<td>" + data.GetValues(key)[i] + "</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }
    }
}
