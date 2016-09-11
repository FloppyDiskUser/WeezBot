using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeezBot.MainWindowData
{
    public class Multibot
    {
        public bool checkSwitch { get; set; }
        public Boolean[] BotLoaded { get; set; }
        public string[] BotName { get; set; }
        public Boolean[] BotAlreadyConfig { get; set; }
        public Dictionary<string, string> LanguageDict { get; set; }
        public Thread Switch { get; set; }
        public int chooseBotCache { get; set; }
        public int chooseBot { get; set; }
        public Bot[] Bot { get; set; }
        public Thread[] ThreadBot { get; set; }

        public Multibot()
        {
            this.checkSwitch = true;
            BotLoaded = new bool[9];
            BotAlreadyConfig = new bool[9];
            BotName = new string[9];
            for(int i = 0; i < 9; ++i)
            {
                this.BotLoaded[i] = false;
                this.BotAlreadyConfig[i] = false;
                this.BotName[i] = "Bot" + i;
            }
            this.LanguageDict = new Dictionary<string, string>();
            this.Bot = new Bot[9];
            this.ThreadBot = new Thread[9];
            LanguageDict["English"] = "en";
            LanguageDict["German"] = "de";
            LanguageDict["French"] = "fr";
            LanguageDict["Dutch"] = "nl";
            LanguageDict["Spanish"] = "es";
            LanguageDict["Italian"] = "it";
        }
    }
}
