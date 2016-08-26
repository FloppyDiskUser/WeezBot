using Microsoft.Maps.MapControl.WPF;
using PoGo.NecroBot.Logic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WeezBot.MainWindowData
{
    public class Variables
    {
        public Boolean SortByIv { get; set; }
        public bool LoginOnAntim8 { get; set; }
        public latestAction[] RecentActions { get; set; }
        public int PokestopCounter { get; set; }
        public int PokemonCounter { get; set; }
        public int quickGuidePos { get; set; }
        public Boolean firstStart { get; set; }
        public BitmapImage TopLeftImage { get; set; }
        public MessageDesign[] CommandLineBox { get; set; }
        public Label[] CommandLineLabels { get; set; }
        public Request.Request Requester { get; set; }
        public DateTime stopBotTime { get; set; }
        public Dictionary<string, string> LanguageDict { get;set;}
        public List<PokemonListe> RecentlyCaughtPokemon { get; set; }
        public MapLayer PlayerPosition { get; set; }
        public Translation translator { get; set; }
        public DateTime luckyEggTime { get; set; }
        public bool luckyEggEnabled { get; set; }
        public bool addThingsToMap { get; set; }
        public bool updatePlayerPosition { get; set; }
        public MapLayer PokemonMapPosition { get; set; }
        public bool enableSchedule { get; set; }
        public bool accountCreation { get; set; }
        public bool continueWithAccount { get; set; }
        public string codename { get; set; }
        public DateTime StopBotSchedule { get; set; }
        public DateTime StartBotSchedule { get; set; }
        public bool setDateByField { get; set; }
        public DateTime NewStartBotTime { get; set; }
        public DateTime NewStopBotTime { get; set; }

        public Variables()
        {
            SortByIv = false;
            LoginOnAntim8 = false;
            RecentActions = new latestAction[100];
            PokemonCounter = 0;
            PokestopCounter = 0;
            quickGuidePos = 0;
            TopLeftImage = new BitmapImage();
            firstStart = true;
            CommandLineBox = new MessageDesign[8];
            CommandLineLabels = new Label[8];
            Requester = new Request.Request();
            stopBotTime = new DateTime();
            RecentlyCaughtPokemon = new List<PokemonListe>();
            PlayerPosition = new MapLayer();
            translator = new Translation();
            luckyEggEnabled = false;
            luckyEggTime = new DateTime();
            addThingsToMap = true;
            updatePlayerPosition = true;
            PokemonMapPosition = new MapLayer();
            enableSchedule = false;
            accountCreation = false;
            continueWithAccount = false;
            codename = "";
            StopBotSchedule = new DateTime();
            StartBotSchedule = new DateTime();
            setDateByField = true;
            NewStartBotTime = new DateTime();
            NewStopBotTime = new DateTime();
        }
    }
}
