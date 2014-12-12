using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace meitubikanSite.Controllers
{
    public class ControllerHelper
    {
        public static string NormalizeString(string str)
        {
            return str == null ? null : str.ToLower().Trim();
        }

        public static string Crawl(string url, string encoding, bool isWarmup = false)
        {
            string content = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (isWarmup)
                {
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
                }
                else
                {
                    request.UserAgent = "Mozilla/5.0 (Linux; U; Android 4.2.2; zh-cn; HUAWEI G610-T11 Build/HuaweiG610-T11) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30 BingWeb/BingImage/1.0.0.20141130030010";
                }
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    BinaryReader responseReader = new BinaryReader(response.GetResponseStream());
                    int bytesRecvCount = 0;
                    List<byte> ListContent = new List<byte>();
                    do
                    {
                        byte[] bytes = new byte[1024 * 1024];
                        bytesRecvCount = responseReader.Read(bytes, 0, bytes.Length);
                        for (int i = 0; i < bytesRecvCount; i++)
                        {
                            ListContent.Add(bytes[i]);
                        }
                    } while (bytesRecvCount > 0);

                    if (string.IsNullOrEmpty(encoding) || encoding.ToLower().Equals("utf8") || encoding.ToLower().Equals("utf-8"))
                    {
                        content = Encoding.UTF8.GetString(ListContent.ToArray());
                    }
                    else if (encoding.ToLower().Equals("gb2312"))
                    {
                        content = Encoding.GetEncoding("gb2312").GetString(ListContent.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return content;
        }
    }
}
