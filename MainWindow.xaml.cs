using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.IO;
using Bing.Maps;
using System.Windows.Input;
using MapControl;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Media;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Utils;
using WeezBot.Model;
using System.Collections.Generic;
using PokemonGo.RocketAPI.Enums;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.ComponentModel;
using PoGo.NecroBot.Logic.State;
using System.Windows.Threading;
using System.Linq;
using PoGo.NecroBot.Logic.Common;

namespace WeezBot
{
    /// <summary>
    /// Pokemon Go Bot mit GUI
    /// </summary>

    public partial class MainWindow : MetroWindow
    {

        // Globale Variablen
        // Bot Variable
        Bot[] bots = new Bot[10];
        Thread[] ThreadBot = new Thread[9];

        // MultiBot Variablen
        bool check = true;
        Boolean[] loaded = { false, false, false, false, false, false, false, false, false, false };
        string[] botNames = { "Bot0", "Bot1", "Bot2", "Bot3", "Bot4", "Bot5", "Bot6", "Bot7", "Bot8" };
        Boolean[] BotAlreadyConfig = { false, false, false, false, false, false, false, false, false };
        private Dictionary<string, string> LanguageDict = new Dictionary<string, string>();
        Thread Switch;
        int ausgewaehlterBotCache, ausgewaehlterBot = 0;

        // Button Variablen
        int anzahlMaximaleButtons = 0;


        // Other Variable
        private Boolean Iv = false;
        public bool isLogedInOnTheWeb = false;
        private latestAction[] show = new latestAction[7];
        public int PokestopCounter = 0;
        int quickGuidePos = 0;
        Boolean firstStart = true;
        BitmapImage imgLiveAction = new BitmapImage();
        Image myPushPin = new Image();
        public Overlay gui = new Overlay();
        public MessageDesign[] Messages = new MessageDesign[8];
        public Label[] CommandLineLabels = new Label[8];
        private Languages translationTranslater = new Languages();
        public List<string> translationLanguages;
        public Request.Request Requester = new Request.Request();
        public DateTime stopBotTime = new DateTime();
        MapLayer imageLayerNew = new MapLayer();
        Translation uebersetzer = new PoGo.NecroBot.Logic.Common.Translation();
        DateTime luckyEggDauer = new DateTime();
        bool luckyEggEnabled = false;

        public MainWindow()
        {

            // Initialize Controls
            this.DataContext = this;
            InitializeComponent();
            translationLanguages = translationTranslater.getLang();
            ReloadButton_Click(this, new RoutedEventArgs());



            // Check For Update
            string newestVersion = "";
            if ((newestVersion = Requester.checkForUpdate(typeof(MainWindow).Assembly.GetName().Version.ToString())) != "")
            {
                Version.Content = newestVersion;
                dockPani.Visibility = Visibility.Collapsed;
                updateWindow.Visibility = Visibility.Visible;
            }
            else
            {
                Version.Content = typeof(MainWindow).Assembly.GetName().Version.ToString();
                //Quick Setting
                if (Settings.PtcUsername == null && Settings.GoogleUsername == null)
                {
                    quickGuidePos++;
                    dockPani.Visibility = Visibility.Collapsed;
                    quickStart.Visibility = Visibility.Visible;
                }
                else
                {
                    dockPani.Visibility = Visibility.Collapsed;
                    if (Settings.AuthType == AuthType.Google)
                        startPageUsername.Content = Settings.GoogleUsername;
                    else
                        startPageUsername.Content = Settings.PtcUsername;
                    startPage.Visibility = Visibility.Visible;
                }
            }


            // Maps Api Key
            string KeyName;
            do
            {
                KeyName = Requester.requestKey();
            } while (KeyName == "");
            BingMapsApi.Text = KeyName;
            LoadingMap.CredentialsProvider = new ApplicationIdCredentialsProvider(KeyName);
            LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(Settings.Latitude, Settings.Longitude);

            CommandLineLabels[0] = Command1;
            CommandLineLabels[1] = Command2;
            CommandLineLabels[2] = Command3;
            CommandLineLabels[3] = Command4;
            CommandLineLabels[4] = Command5;
            CommandLineLabels[5] = Command6;
            CommandLineLabels[6] = Command7;
            CommandLineLabels[7] = Command8;
            LanguageDict["English"] = "en";
            LanguageDict["German"] = "de";
            LanguageDict["French"] = "fr";
            LanguageDict["Dutch"] = "nl";
            LanguageDict["Spanish"] = "es";
            LanguageDict["Italian"] = "it";
            Requester.startBot();
            // Starts The Bot with the Settings Tab
            OpenSettings();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "/u")
            {
                quickStart.Visibility = Visibility.Collapsed;
                dockPani.Visibility = Visibility.Collapsed;
                startPage.Visibility = Visibility.Collapsed;
                updateWindow.Visibility = Visibility.Collapsed;
                updateFinishGrid.Visibility = Visibility.Visible;
            }
            if (args.Length > 1 && args[1] == "/s")
            {
                quickStart.Visibility = Visibility.Collapsed;
                dockPani.Visibility = Visibility.Collapsed;
                startPage.Visibility = Visibility.Collapsed;
                updateWindow.Visibility = Visibility.Collapsed;
                timeElapsed.Visibility = Visibility.Visible;
            }
        }

        #region SettingsTab
        public void KeepThisVersion(object sender, RoutedEventArgs e)
        {
            updateWindow.Visibility = Visibility.Collapsed;
            SuccessfullyRegistered.Visibility = Visibility.Collapsed;
            dockPani.Visibility = Visibility.Visible;
        }

        public async void updateBot(object sender, RoutedEventArgs e)
        {
            updateWindow.Visibility = Visibility.Collapsed;
            downloadingUpdate.Visibility = Visibility.Visible;
            var machine = new VersionCheckState();
            await machine.update();
            ProcessStartInfo restart = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
            restart.Arguments = "/u";
            Process.Start(restart);
            Environment.Exit(-1);
            return;
        }

        public void goToConfigs(object sender, RoutedEventArgs e)
        {
            startPage.Visibility = Visibility.Collapsed;
            dockPani.Visibility = Visibility.Visible;
        }

        public void Quick_Guide(object sender, RoutedEventArgs e)
        {
            SaveButton_Click(this, new RoutedEventArgs());
            switch (quickGuidePos)
            {
                case 1:
                    quickStart.Visibility = Visibility.Collapsed;
                    if (Settings.AuthType == AuthType.Google)
                    {
                        quickGuidePos++;
                        quickStart2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        quickStart3.Visibility = Visibility.Visible;
                        quickGuidePos += 2;
                    }
                    break;
                case 2:
                    quickStart2.Visibility = Visibility.Collapsed;
                    quickStart3.Visibility = Visibility.Visible;
                    break;
                case 3:
                    ResetCoordsButton_Click(this, new RoutedEventArgs());

                    quickStart3.Visibility = Visibility.Collapsed;
                    dockPani.Visibility = Visibility.Visible;
                    quickGuidePos = 0;
                    break;
            }

        }

        public void startBotFromQuickGuide(object sender, RoutedEventArgs e)
        {
            ResetCoordsButton_Click(this, new RoutedEventArgs());
            SaveButton_Click(this, new RoutedEventArgs());
            quickStart3.Visibility = Visibility.Collapsed;
            button_Click(this, new RoutedEventArgs());
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            _set = new GlobalSettings();
            Settings = ObservableSettings.CreateFromGlobalSettings(_set);
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {

            _set = GlobalSettings.Load("config/" + botNames[ausgewaehlterBotCache]);
            if (null == _set) _set = GlobalSettings.Load("config/" + botNames[ausgewaehlterBotCache]);
            if (null == _set) throw new Exception("There was an error attempting to build default config files - may be a file permissions issue! Cannot proceed.");
            Settings = ObservableSettings.CreateFromGlobalSettings(_set);
            string langCode = Settings.TranslationLanguageCode;
            string[] languageCodeToString = { "nl", "en", "fr", "de", "it", "es" };
            for (int i = 0; i < 6; ++i)
            {
                if (languageCodeToString[i] == langCode)
                {
                    LanguageField.SelectedIndex = i;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), "config/" + botNames[ausgewaehlterBotCache]);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");
            var authFile = Path.Combine(profileConfigPath, "auth.json");
            _set = Settings.GetGlobalSettingsObject(botNames[ausgewaehlterBotCache]);
            LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(Settings.DefaultLatitude, Settings.DefaultLongitude);
            if (quickGuidePos > 0)
            {
                _set.Auth.PtcPassword = Settings.PtcPassword = PasswordBoxAssistant.GetBoundPassword(ptcPasswordSetup);
                _set.Auth.GooglePassword = Settings.GooglePassword = PasswordBoxAssistant.GetBoundPassword(googlePasswordSetup);
                string lang = LanguageDict[LanguageFieldSetup.SelectionBoxItem.ToString()];
                _set.TranslationLanguageCode = Settings.TranslationLanguageCode = lang;
            }
            else
            {
                _set.Auth.PtcPassword = Settings.PtcPassword = PasswordBoxAssistant.GetBoundPassword(ptcPassword);
                _set.Auth.GooglePassword = Settings.GooglePassword = PasswordBoxAssistant.GetBoundPassword(googlePassword);
                string lang = LanguageDict[LanguageField.SelectionBoxItem.ToString()];
                _set.TranslationLanguageCode = Settings.TranslationLanguageCode = lang;
            }
            _set.Save(configFile);
            _set.Auth.Save(authFile);
        }

        private void AuthType_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsGoogleAuthShowing = (AuthType.Google == (AuthType)e.AddedItems[0]);
        }

        private void DevicePackage_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsCustomDevicePackage = e.AddedItems[0].Equals("custom");
        }

        private void ResetCoordsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Latitude = Settings.DefaultLatitude;
            Settings.Longitude = Settings.DefaultLongitude;
        }

        private GlobalSettings _set = new GlobalSettings();

        public ObservableSettings Settings
        {
            get { return (ObservableSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(ObservableSettings), typeof(MainWindow), new PropertyMetadata(null));

        public bool IsGoogleAuthShowing
        {
            get { return (bool)GetValue(IsGoogleAuthShowingProperty); }
            set { SetValue(IsGoogleAuthShowingProperty, value); }
        }
        public static readonly DependencyProperty IsGoogleAuthShowingProperty =
            DependencyProperty.Register("IsGoogleAuthShowing", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public bool IsCustomDevicePackage
        {
            get { return (bool)GetValue(IsCustomDevicePackageProperty); }
            set { SetValue(IsCustomDevicePackageProperty, value); }
        }
        public static readonly DependencyProperty IsCustomDevicePackageProperty =
            DependencyProperty.Register("IsCustomDevicePackage", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public List<string> DevicePackageCollection
        {
            get { return (List<string>)GetValue(DevicePackageCollectionProperty); }
            set { SetValue(DevicePackageCollectionProperty, value); }
        }
        public static readonly DependencyProperty DevicePackageCollectionProperty =
            DependencyProperty.Register("DevicePackageCollection", typeof(List<string>), typeof(MainWindow), new PropertyMetadata(new List<string>()));

        public List<string> OperatorsCollection
        {
            get { return (List<string>)GetValue(OperatorsCollectionProperty); }
            set { SetValue(OperatorsCollectionProperty, value); }
        }
        public static readonly DependencyProperty OperatorsCollectionProperty =
            DependencyProperty.Register("OperatorsCollection", typeof(List<string>), typeof(MainWindow), new PropertyMetadata(new List<string>() { "or", "and" }));

        public List<string> IpCvCollection
        {
            get { return (List<string>)GetValue(IpCvCollectionProperty); }
            set { SetValue(IpCvCollectionProperty, value); }
        }
        public static readonly DependencyProperty IpCvCollectionProperty =
            DependencyProperty.Register("IpCvCollection", typeof(List<string>), typeof(MainWindow), new PropertyMetadata(new List<string>() { "iv", "cp" }));

        #endregion

        public void createNewBot(string Name, int Start)
        {
            if (BotAlreadyConfig[Start] == true) return;
            dockPani.Visibility = Visibility.Collapsed;
            startPage.Visibility = Visibility.Collapsed;
            loadingMessages.Content = "Starting...";
            Loading.Visibility = Visibility.Visible;
            firstStart = true;
            anzahlMaximaleButtons++;
            BotAlreadyConfig[ausgewaehlterBot] = true;
            bots[ausgewaehlterBot] = new Bot("config\\" + Name);
            ThreadBot[ausgewaehlterBot] = new Thread(bots[ausgewaehlterBot].StartBot);
            ThreadBot[ausgewaehlterBot].Start();
            check = false;
            
            if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(),"Config",botNames[ausgewaehlterBot], "luckyEgg.ini")))
            {
                string inhalt = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config", botNames[ausgewaehlterBot], "luckyEgg.ini"));
                luckyEggDauer = DateTime.Parse(inhalt);
                luckyEggEnabled = true;
                ButtonUseLuckyEgg.Visibility = Visibility.Collapsed;
                BitmapImage img = new BitmapImage(new Uri((Path.Combine(Directory.GetCurrentDirectory(), "image", "Lucky_Egg.png"))));
                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(LuckyEggImage,img);
                LuckyEgg.Visibility = Visibility.Visible;
            }

            Switch = new Thread(switchBot);
            Switch.Start();
        }

        private void updateGUIBot(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string titel = button.Content.ToString();
            int botNr = (titel[0] - '0') - 1;
            ausgewaehlterBotCache = botNr;
            ReloadButton_Click(this, new RoutedEventArgs());
            if (BotAlreadyConfig[ausgewaehlterBotCache] == true)
                button_Click(this, new RoutedEventArgs());
        }

        public void OpenSettings()
        {
            tabs.SelectedIndex = 0;
            MainWindow_Loaded(this, new RoutedEventArgs());
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DevicePackageCollection.Add("custom");
            DevicePackageCollection.Add("random");
            foreach (string key in DeviceInfoHelper.DeviceInfoSets.Keys)
                DevicePackageCollection.Add(key);
            ReloadButton_Click(this, null);
        }

        #region 
        public void updateUi()
        {
            Nickname.Content = gui.Nickname;
            Level.Content = gui.Level;
            xp.Content = gui.xp;
            Stardust.Content = gui.Stardust;
            for (int f = 0; f < 8; ++f)
            {
                CommandLineLabels[f].Content = Messages[f].Message;
                CommandLineLabels[f].Foreground = Messages[f].Farbe;
            }
            TeamName.Content = gui.TeamName;
            locationUnder.Content = gui.MapLabel;
            LoadingMap.Center = gui.Position;
            ProgressBar.Maximum = gui.NeedXp;
            ProgressBar.Value = gui.HaveXp;
            TeamImage.Source = gui.TeamImage;
        }
        #endregion

        #region SwitchBot

        public void switchBot()
        {
            string message = "";
            check = false;
            while (true)
            {
                if (check == true) break;
                if (message != bots[ausgewaehlterBot].Informations.Message)
                {
                    Dispatcher.BeginInvoke(new Action(() => LoginFailed(false)));
                    Messages = bots[ausgewaehlterBot].Informations.readMessage();
                    message = Messages[0].Message;

                    if (loaded[ausgewaehlterBot] == true)
                    {
                        bots[ausgewaehlterBot].updateEggs();
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                        gui = bots[ausgewaehlterBot].updateUi(Iv); changeLive(bots[ausgewaehlterBot].Informations.liveHappening, bots[ausgewaehlterBot].Informations.PokemonName,
bots[ausgewaehlterBot].Informations.PokemonID, bots[ausgewaehlterBot].Informations.coords, bots[ausgewaehlterBot].Informations.Pokemon);
                        pokemonListBox.ItemsSource = gui.PokemonList; EggList.ItemsSource = gui.EggLists; updateUi(); if (gui.currentLat == 0.00) { setPlayerPosition( _set.DefaultLatitude,_set.DefaultLongitude); LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(_set.DefaultLatitude, _set.DefaultLongitude); } else { LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(gui.currentLat, gui.currentLng); }
                            if (isLogedInOnTheWeb == true)
                            {
                                Dispatcher.BeginInvoke(new Action(() => Requester.updateDataIfLogin(gui.Nickname, gui.TeamName, gui.Stardust, gui.Level, gui.xp, gui.Runtime)));
                            }
                            if (EnableAutomatedBotStop.IsChecked == true)
                            {
                                DateTime time_now = DateTime.Now;
                                if (time_now > stopBotTime)
                                {
                                    stopBot_button(this, new RoutedEventArgs());
                                    dockPani.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    TimeSpan span = stopBotTime - time_now;
                                    double totalMinutes = span.TotalMinutes;
                                    ClosingInMinutes.Content = totalMinutes.ToString("0.00");
                                }
                            }
                            if(luckyEggEnabled == true)
                            {
                                DateTime jetzt = DateTime.Now;
                                if(luckyEggDauer < jetzt)
                                {
                                    LuckyEgg.Visibility = Visibility.Collapsed;
                                    luckyEggEnabled = false;
                                    ButtonUseLuckyEgg.Visibility = Visibility.Visible;
                                    File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Config", botNames[ausgewaehlterBot], "luckyEgg.ini"));
                                } else
                                {
                                    ButtonUseLuckyEggWarning.Content = "";
                                    LuckyEggLabel.Content = Math.Round(((luckyEggDauer - jetzt).TotalMinutes),1).ToString() + " Min.";
                                }
                            } else
                            {
                                ButtonUseLuckyEgg.Visibility = Visibility.Visible;
                                LuckyEgg.Visibility = Visibility.Collapsed;
                            }
                            RuntimeLabel.Content = gui.Runtime.ToString();
                        }));
                        int norm = 0, ultra = 0, great = 0, master = 0;
                        var normPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemPokeBall);
                        var ultrPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemUltraBall);
                        var greatPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemGreatBall);
                        var masterPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemMasterBall);
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () => { norm = await normPok; ultra = await ultrPok; great = await greatPok; master = await masterPok; }));
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { PokeballsTotal_Label.Content = "Total Pokéballs: " + (norm + great + ultra + master).ToString(); }));
                    }
                    if (firstStart == true && bots[ausgewaehlterBot].isLoaded == true)
                    {
                        Dispatcher.BeginInvoke(new Action(() => { BotTabItem.Visibility = Visibility.Visible; EggsTabItem.Visibility = Visibility.Visible; PokemonTabItem.Visibility = Visibility; Loading.Visibility = Visibility.Collapsed; dockPani.Visibility = Visibility.Visible; }));
                        Dispatcher.BeginInvoke(new Action(() => tabs.SelectedIndex = 1));
                        Dispatcher.BeginInvoke(new Action(() => Loading.Opacity = 0));
                        Dispatcher.BeginInvoke(new Action(() => loaded[ausgewaehlterBot] = true));
                        Dispatcher.BeginInvoke(new Action(() => firstStart = false));
                    }
                    else
                    {
                        if (firstStart == true && bots[ausgewaehlterBot].Informations.ErrorHappen == true)
                        {
                            if (bots[ausgewaehlterBot].Informations.LoginError == true)
                            {
                                Dispatcher.BeginInvoke(new Action(() => LoginFailed(true)));
                            }
                            Dispatcher.BeginInvoke(new Action(() => {
                                Loading.Visibility = Visibility.Collapsed; dockPani.Visibility = Visibility.Visible; tabs.SelectedIndex = 0;
                                check = true; ThreadBot[ausgewaehlterBot].Abort();
                            }));
                        } else
                        if( _set.Auth.ProxyFailed == true)
                        {
                            Dispatcher.BeginInvoke(new Action(() => loadingMessages.Content = uebersetzer.GetTranslation(TranslationString.FixProxySettings))) ;
                            Thread.Sleep(2000);
                            Loading.Visibility = Visibility.Collapsed;
                            dockPani.Visibility = Visibility.Visible;
                            tabs.SelectedIndex = 0;
                        }
                        else
                        {
                            string[] loadMsg = message.Split('>');
                            Dispatcher.BeginInvoke(new Action(() => loadingMessages.Content = loadMsg[1]));
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
            Dispatcher.BeginInvoke(new Action(() => check = false));
            return;
        }
        #endregion

        public void LoginFailed(Boolean wahr)
        {
            if (wahr == true) LoginFailedLabel.Visibility = Visibility.Visible;
            else LoginFailedLabel.Visibility = Visibility.Collapsed;
        }

        #region changeLive
        public void changeLive(string liveMessage, string pokemonName, string pokemonId, double[] Coords, PokemonListe Pokemon)
        {
            latestAction zs = show[6];
            BitmapImage animatedSrc = new BitmapImage();
            bool reset = false;
            for (int h = 5; h >= 0; --h)
            {
                show[h + 1] = show[h];
            }
            switch (liveMessage)
            {
                case "caught":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.caught, pokemonName))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.caught, pokemonName), "Catch", Visibility.Visible);
                    else reset = true;
                    break;
                case "moving":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.moving))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.moving), "Moving", Visibility.Visible);
                    else reset = true;
                    break;
                case "hatched":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.hatch, pokemonName))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.hatch, pokemonName), "Egg_hatched", Visibility.Visible);
                    else reset = true;
                    break;
                case "fight":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.fight, pokemonName))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.fight, pokemonName), "Fight", Visibility.Visible); 
                    else reset = true;
                    break;
                case "evolve":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.evolve, pokemonName))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.evolve, pokemonName), "Evolve", Visibility.Visible);
                    else reset = true;
                    break;
                case "looting":
                    PokestopCounter++;
                    if (show[1] == null || show[1].latestAction_label != "Looting")
                    {
                        show[0] = new latestAction("Looting", "Looting", Visibility.Visible);
                        VisitedPokestops.Content = "Visited Pokestops: " + PokestopCounter;
                    } else
                    {
                        reset = true;
                    }
                    break;
                case "softban":
                    if (show[1] == null || show[1].latestAction_label != "Loading")
                        show[0] = new latestAction("Loading", "Error", Visibility.Visible);
                    else reset = true;
                    break;
                case "Error":
                    if (show[1] == null || show[1].latestAction_label != "Loading")
                        show[0] = new latestAction("Loading", "Error", Visibility.Visible);
                    else reset = true;
                    break;
                case "lootingFailed":
                    if (show[1] == null || show[1].latestAction_label != "Looting Error")
                        show[0] = new latestAction("Looting Error", "Error", Visibility.Visible);
                    else reset = true;
                    break;
                case "transfer":
                    if (show[1] == null || show[1].latestAction_label != uebersetzer.GetTranslation(TranslationString.transfer, pokemonName))
                        show[0] = new latestAction(uebersetzer.GetTranslation(TranslationString.transfer, pokemonName), "Transfer", Visibility.Visible);
                    else reset = true;
                    break;    
                default:
                    show[0] = new latestAction("", "", Visibility.Collapsed);
                    reset = true;
                    break;
            }
            if(reset == true)
            {
                for(int i = 0; i < 6; ++i)
                {
                    show[i] = show[i + 1];
                }
                show[6] = zs;
            }
            List<latestAction> showList = new List<latestAction>();
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(localImage, show[0].MovingImage);
            localImage_label.Content = show[0].latestAction_label;
            for (int k = 1; k < 7; ++k)
                showList.Add(show[k]);
            latestActionShow.ItemsSource = showList;
        }
        #endregion

        #region createPushpin

        public void setPlayerPosition(double lat, double lng)
        {
            if (imageLayerNew != null) LoadingMap.Children.Remove(imageLayerNew);
            Image imageNew = new Image();
            string imgSource = "img/player_symbol.png";
            imageNew.Height = 50;
            BitmapImage myBitmapImageNew = new BitmapImage();
            myBitmapImageNew.BeginInit();
            myBitmapImageNew.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), imgSource));
            myBitmapImageNew.DecodePixelHeight = 50;
            myBitmapImageNew.EndInit();
            imageNew.Source = myBitmapImageNew;
            imageNew.Opacity = 0.9;
            imageNew.Stretch = System.Windows.Media.Stretch.None;
            Microsoft.Maps.MapControl.WPF.Location locationNew = new Microsoft.Maps.MapControl.WPF.Location(lat, lng);
            PositionOrigin positionNew = PositionOrigin.Center;
            imageLayerNew.AddChild(imageNew, locationNew, positionNew);
            Dispatcher.BeginInvoke(new Action(() => LoadingMap.Children.Add(imageLayerNew)));
        }
        #endregion

        #region GenerellEvent

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int BotNr = ausgewaehlterBotCache;
            if (EnableAutomatedBotStop.IsChecked == true)
            {
                stopBotTime = DateTime.Now;
                stopBotTime = stopBotTime.AddMinutes(Convert.ToDouble(stopBot.Text.ToString()));
            }
            ausgewaehlterBot = ausgewaehlterBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (BotAlreadyConfig[ausgewaehlterBot] == true) switchBot_Click(this, new RoutedEventArgs());
            else
            {
                createNewBot(botNames[ausgewaehlterBot], BotNr);
                StartBotButton.Click += stopBot_button;
                StartBotButton.Content = "Stop Bot";
            }
        }

        private void useLuckyEgg(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async()=> {
                int anzahlLucky = await bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemLuckyEgg);
                if (anzahlLucky == 0)
                {
                    ButtonUseLuckyEggWarning.Content = "No Lucky Eggs";
                    return;
                }
                await bots[ausgewaehlterBot].session.Client.Inventory.UseItemXpBoost();
            }));
            string configPfad = Path.Combine(Directory.GetCurrentDirectory(), "Config", botNames[ausgewaehlterBot],"luckyEgg.ini");
            luckyEggDauer = DateTime.Now;
            luckyEggDauer = luckyEggDauer.AddMinutes(30);
            luckyEggEnabled = true;
            File.WriteAllText(configPfad, $"{luckyEggDauer.ToString()}");
            BitmapImage img = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "image", "Lucky_Egg.png")));
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(LuckyEggImage, img);
            ButtonUseLuckyEgg.Visibility = Visibility.Collapsed;
            LuckyEgg.Visibility = Visibility.Visible;
        }

        private void stopBot_button(object sender, RoutedEventArgs e)
        {
            ausgewaehlterBot = ausgewaehlterBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Switch.Abort()));
            }
            Dispatcher.BeginInvoke(new Action(() => { if (ThreadBot[ausgewaehlterBot] != null) ThreadBot[ausgewaehlterBot].Abort(); }));
            ProcessStartInfo restart = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
            restart.Arguments = "/s";
            Process.Start(restart);
            Environment.Exit(-1);
        }

        private void loginToAntim8(object sender, RoutedEventArgs e)
        {
            startPage.Visibility = Visibility.Collapsed;
            LoginToWebsite.Visibility = Visibility.Visible;
        }

        private void LoginAntim8(object sender, RoutedEventArgs e)
        {
            string username = antim8username.Text;
            string password = PasswordBoxAssistant.GetBoundPassword(antim8Password);
            if(Requester.Login(username,password) == true)
            {
                isLogedInOnTheWeb = true;
                LoginToWebsite.Visibility = Visibility.Collapsed;
                SuccessMessage.Content = "Login Complete.";
                SuccessfullyRegistered.Visibility = Visibility.Visible;
            }
            else
            {
                LoginError.Content = "Username / Password wrong";
            }
        }

        private void RegisterFinalle(object sender, RoutedEventArgs e)
        {
            bool error = false;
            if(antim8username.Text == "")
            {
                LoginError.Content = "Username must be set";
                error = true;
            }
            if (PasswordBoxAssistant.GetBoundPassword(antim8Password) == "")
            {
                LoginError.Content = "Password must be set";
                error = true;
            }
            if (error == true) return;
            if (Requester.Register(antim8username.Text, PasswordBoxAssistant.GetBoundPassword(antim8Password)) == true)
            {
                LoginToWebsite.Visibility = Visibility.Collapsed;
                isLogedInOnTheWeb = true;
                SuccessfullyRegistered.Visibility = Visibility.Visible;
            } else
            {
                LoginError.Content = "Error: Username already used";
            }

        }

        public void startitagain_Click(object sender, RoutedEventArgs e)
        {
            SuccessfullyRegistered.Visibility = Visibility.Collapsed;
            ResetCoordsButton_Click(this, new RoutedEventArgs());
            SaveButton_Click(this, new RoutedEventArgs());
            timeElapsed.Visibility = Visibility.Collapsed;
            dockPani.Visibility = Visibility.Visible;
            button_Click(this, new RoutedEventArgs());
        }

        public void TotalItemsBeingKeptp_Click(object sender, RoutedEventArgs e) { Settings.TotalItemsBeingKept = Settings.TotalItemsBeingKept + 1; }
        public void TotalAmountOfBerriesp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep + 1; }
        public void TotalAmountOfPokeballsp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep + 1; }
        public void TotalAmountOfPotionsp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep + 1; }
        public void TotalAmountOfRevicesp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep + 1; }
        public void TotalItemsBeingKeptm_Click(object sender, RoutedEventArgs e) { if (Settings.TotalItemsBeingKept > 0) Settings.TotalItemsBeingKept = Settings.TotalItemsBeingKept - 1; }
        public void TotalAmountOfBerriesm_Click(object sender, RoutedEventArgs e) { if (Settings.TotalAmountOfBerriesToKeep > 0) _set.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep - 1; }
        public void TotalAmountOfPokeballsm_Click(object sender, RoutedEventArgs e) { if (Settings.TotalAmountOfPokeballsToKeep > 0) _set.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep - 1; }
        public void TotalAmountOfPotionsm_Click(object sender, RoutedEventArgs e) { if (Settings.TotalAmountOfPotionsToKeep > 0) _set.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep - 1; }
        public void TotalAmountOfRevicesm_Click(object sender, RoutedEventArgs e) { if (Settings.TotalAmountOfRevivesToKeep > 0) _set.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep - 1; }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;
            if (button == null) return;
            string Title = button.Name.ToString();
            int botNr = (Title[3] - '0') - 1;
            ausgewaehlterBotCache = botNr;
            ReloadButton_Click(this, new RoutedEventArgs());
            if (BotAlreadyConfig[ausgewaehlterBotCache] == true)
            {
                button_Click(this, new RoutedEventArgs());
            }
        }

        public void eventRestart(object sender, RoutedEventArgs e)
        {
            updateFinishGrid.Visibility = Visibility.Collapsed;
            timeElapsed.Visibility = Visibility.Collapsed;
            if (Settings.PtcUsername == null && Settings.GoogleUsername == null)
            {
                quickGuidePos++;
                dockPani.Visibility = Visibility.Collapsed;
                quickStart.Visibility = Visibility.Visible;
            } else
            {
                dockPani.Visibility = Visibility.Visible;
            }
            
        }

        private void switchBot_Click(object sender, RoutedEventArgs e)
        {
            check = true;
            Switch.Join();
            Switch = new Thread(switchBot);
            Switch.Start();
        }

        public void sendAway_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            ulong id = DataTemplate.id;
            Dispatcher.BeginInvoke(new Action(async () => await bots[ausgewaehlterBot].session.Inventory.DeletePokemonFromInvById(id)));
        }

        public void levelUp_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            ulong id = DataTemplate.id;
            Dispatcher.BeginInvoke(new Action(async () => await bots[ausgewaehlterBot].session.Inventory.UpgradePokemon(id)));
        }

        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (loaded[ausgewaehlterBot] == false) return;
            if ( box.SelectedIndex == 0)
                Iv = false;
            else
                Iv = true;
            Thread updateList = new Thread(updatePokeList);
            updateList.Start();
        }

        public void updatePokeList()
        {
            if (Iv == false)
                Dispatcher.BeginInvoke(new Action(() => pokemonListBox.ItemsSource = gui.PokemonList.OrderByDescending(o => o.CpForOrder).ToList()));
            else
                Dispatcher.BeginInvoke(new Action(() => pokemonListBox.ItemsSource = gui.PokemonList.OrderByDescending(o => o.IV).ToList()));
        }

        private void Window_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Switch.Abort()));
            }
            for (int i = 0; i < 8; i++)
            {
                Dispatcher.BeginInvoke(new Action(() => { if (ThreadBot[i] != null) { ThreadBot[i].Abort(); } }));
            }
            Requester.closeBot();
            Requester.returnKey(BingMapsApi.Text);
        }
        #endregion
    }
}
