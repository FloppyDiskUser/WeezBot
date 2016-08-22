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
        string username = "";
        string password = "";
        Boolean isLogin = false;
        public string requestKey()
        {
            string key = sendRequest("k=keyNew");
            return key;
        }
        public void returnKey(string key)
        {
            string response = sendRequest("k=finishKeyNew&key=" + key);
            return;
        }
        public void startBot()
        {
            string requestResponse = sendRequest("k=botstartedNew");
            return;
        }

        public Boolean Login(string username, string Password)
        {
            string response = sendRequest("k=Login&user=" + username + "&pass=" + Password);
            if(response == "True") {
                this.username = username;
                this.password = Password;
                this.isLogin = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean Register(string Username, string Password, string email = "")
        {
            string response = sendRequest("k=Register&user=" + Username + "&pass=" + Password + "&email=" + email);
            if (response == "true") { this.username = Username; this.password = Password; this.isLogin = true; return true; }
            else { return false; }
        }

        public void updateDataIfLogin(string Nickname,string Team, string Stardust, string Level, string xp, string runtime)
        {
            string response = sendRequest("k=updateData&user=" + username + "&pass=" + password + "&Nick=" + Nickname + "&Team=" + Team + "&star=" + Stardust + "&lvl=" + Level + "&xp=" + xp + "&runtime=" + runtime);
            return;
        }

        public string getOnlineBots()
        {
            string requestResponse = sendRequest("k=getOnline");
            return requestResponse;
        }

        public void closeBot()
        {
            string requestResponse = sendRequest("k=botclosedNew");
            return;
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
