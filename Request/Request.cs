using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WeezBot.Request
{
    public class Request
    {
        public string requestKey()
        {
            string key = sendRequest("k=key");
            return key;
        }
        public void startBot()
        {
            string requestResponse = sendRequest("k=botstarted");
        }

        public void closeBot()
        {
            string requestResponse = sendRequest("k=botclosed");
        }
        public string checkForUpdate(string Version)
        {
            string newestVersion = sendRequest("k=update&v=" + Version);
            if (newestVersion == "noupdate")
                return "";
            else return newestVersion;
        }

        public string sendRequest(string paras)
        {
            string url = "http://www.antim8.de/request.php?"+ paras;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = "";
            using (Stream resStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                responseString = reader.ReadToEnd();
            }

            return responseString;
        }
    }
}
