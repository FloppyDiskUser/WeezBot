using Microsoft.Maps.MapControl.WPF;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WeezBot.MainWindowData;

namespace WeezBot.Request
{
    public class Request
    {
        string username = "";
        public string Key = "";
        string password = "";
        private string iv = "45287112549354892144548565456541";
        private string key_ = "anjueolkdiwpoida";

        public string requestKey()
        {
            string _key = sendRequest("k=getKeyCrypt");
            string key = decrypt(_key);
            this.Key = key;
            return key;
        }
        public bool checkKillSwitch()
        {
            string resp = sendRequest("k=checkKillSwitch");
            if (resp == "true") return true;
            else return false;
        }

        public void returnKey(string key)
        {
            string response = sendRequest("k=finishKey124&key=" + key);
            return;
        }
        public void startBot()
        {
            string requestResponse = sendRequest("k=botstarted124");
            return;
        }

        public List<PokemonNest> getPokeNests()
        {
            string _Nests = sendRequest("k=getPokemonNestCrypt");
            string Nests = decrypt(_Nests);
            System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<PokemonNest> _pokeNests = ser.Deserialize<List<PokemonNest>>(Nests);
            return _pokeNests;
        }

        public Boolean Login(string username, string Password)
        {
            string response = sendRequest("k=Login&user=" + username + "&pass=" + Password);
            if(response == "True") {
                this.username = username;
                this.password = Password;
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
            if (response == "true") { this.username = Username; this.password = Password; return true; }
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
            string online = decrypt(requestResponse);
            return online;
        }

        public void closeBot()
        {
            string requestResponse = sendRequest("k=botclosed124");
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

        public byte[] Decode(string str)
        {
            var decbuff = Convert.FromBase64String(str);
            return decbuff;
        }

        static public String DecryptRJ256(byte[] cypher, string KeyString, string IVString)
        {
            var sRet = "";

            var encoding = new UTF8Encoding();
            var Key = encoding.GetBytes(KeyString);
            var IV = encoding.GetBytes(IVString);

            using (var rj = new RijndaelManaged())
            {
                try
                {
                    rj.Padding = PaddingMode.PKCS7;
                    rj.Mode = CipherMode.CBC;
                    rj.KeySize = 256;
                    rj.BlockSize = 256;
                    rj.Key = Key;
                    rj.IV = IV;
                    var ms = new MemoryStream(cypher);

                    using (var cs = new CryptoStream(ms, rj.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            sRet = sr.ReadLine();
                        }
                    }
                }
                finally
                {
                    rj.Clear();
                }
            }

            return sRet;
        }

        public string decrypt(string text)
        {
            return DecryptRJ256(Decode(text), key_, iv);
        }
    }
}
