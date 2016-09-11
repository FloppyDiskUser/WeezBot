using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.IO;
using System.Windows.Input;
using Microsoft.Maps.MapControl.WPF;
using PoGo.NecroBot.Logic;
using PoGo.NecroBot.Logic.Utils;
using WeezBot.Model;
using System.Collections.Generic;
using PokemonGo.RocketAPI.Enums;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Imaging;
using PoGo.NecroBot.Logic.State;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Navigation;
using System.Threading.Tasks;
using WeezBot.GeocodeService;
using WeezBot.MainWindowData;
using XamlAnimatedGif;
using System.Windows.Media;

namespace WeezBot
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindowData.Multibot Bot = new MainWindowData.Multibot();
        public Overlay Gui = new Overlay();
        public MainWindowData.Variables Data = new MainWindowData.Variables();
        public EventShow[] eventList = new EventShow[4];

        public MainWindow()
        {
            // Initialize Controls
            this.DataContext = this;
            InitializeComponent();
            if(Data.Requester.checkKillSwitch() == true)
            {
                selectPage(KillSwitchEnabled);
                return;
            }
            ReloadButton_Click(this,new RoutedEventArgs());
            checkScreens();
            getKey();

            Data.Requester.startBot();

            initImgs();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "/u")
            {
                selectPage(updateFinishGrid);
            }
        }

        private void checkScreens()
        {
            checkScreens(typeof(MainWindow).Assembly.GetName().Version.ToString());
        }

        private void checkScreens(String newestVersion)
        {
            Version.Content = newestVersion;
            if (Data.Requester.checkForUpdate(newestVersion) != "")
                selectPage(updateWindow);
            else if (Settings.PtcUsername == null && Settings.GoogleUsername == null)
                startQuickSetup();
            else selectPage(startPage);
            if (Settings.AuthType == AuthType.Google)
                startPageUsername.Content = Settings.GoogleUsername;
            else
                startPageUsername.Content = Settings.PtcUsername;
        }

        private void startQuickSetup()
        {
            Data.quickGuidePos++;
            selectPage(quickStart);
        }

        private void clearAllGrids()
        {
            foreach (UIElement e in MainWindowGrid.GetChildObjects())
            {
                e.Visibility = Visibility.Collapsed;
            }
            ConfigPanel.Visibility = Visibility.Collapsed;
        }

        private void selectPage(UIElement e)
        {
            clearAllGrids();
            e.Visibility = Visibility.Visible;
        }

        private void getKey()
        {
            string KeyName;
            do
            {
                KeyName = Data.Requester.requestKey();
            } while (KeyName == "");
            BingMapsApi.Text = KeyName;
            LoadingMap.CredentialsProvider = new ApplicationIdCredentialsProvider(KeyName);
            choosingLocationMap.CredentialsProvider = new ApplicationIdCredentialsProvider(KeyName);
            LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(Settings.Latitude, Settings.Longitude);
        }

        private void initImgs()
        {
            AnimationBehavior.SetSourceUri(MenuButton, new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources","Images", "Pokeball.gif")));
            InventoryImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Inventory.png")));
            SettingsImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Settings.png")));
            ConsoleImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Pokedex.png")));
            RecentImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Pokeball.png")));
            EggImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Egg.png")));
            PokemonImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Pokemon.png")));
            ProfileBack.ImageSource = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Teams", "Team_Neutral.png")));
            LuckyEggImg.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "LuckyEgg.png"))); 
            StopBotImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Stop.png")));
            AddPokemonToMapImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Pokecoin.png")));
            updatePlayerLocation.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Map.png")));
            goToAllConfigsImage.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Settings.png")));
            Banner.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Banner.png")));
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(StartScreenLogo, new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "StartScreenLogo.gif")));
        }

        public void KeepThisVersion(object sender, RoutedEventArgs e)
        {
            selectPage(ConfigPanel);
        }

        public async void updateBot(object sender, RoutedEventArgs e)
        {
            selectPage(downloadingUpdate);
            var machine = new VersionCheckState();
            await machine.update();
            ProcessStartInfo restart = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
            restart.Arguments = "/u";
            Process.Start(restart);
            Environment.Exit(-1);
            return;
        }

        public void updateNests()
        {
            List<PokemonNest> PokeNest = Data.Requester.getPokeNests();
            Data.PokeNest = PokeNest;
            updateNests(PokeNest);
            return;
        }

        private void updateNests(List<PokemonNest> PokeNest) {
            foreach (PokemonNest nest in Data.PokeNest)
            {
                Image imageNew = new Image();
                string imgSource = nest.img;
                BitmapImage newBi = new BitmapImage(new Uri(imgSource));
                newBi.DecodePixelHeight = 50;
                imageNew.Source = newBi;
                imageNew.Stretch = System.Windows.Media.Stretch.None;
                Microsoft.Maps.MapControl.WPF.Location locationPoke = new Microsoft.Maps.MapControl.WPF.Location(nest.Latitude, nest.Longitude);
                PositionOrigin positionNew = PositionOrigin.Center;
                Data.PokemonNestLayer.AddChild(imageNew, locationPoke, positionNew);
            }
            choosingLocationMap.Children.Remove(Data.PokemonNestLayer);
            choosingLocationMap.Children.Add(Data.PokemonNestLayer);
            Microsoft.Maps.MapControl.WPF.Pushpin pin = new Microsoft.Maps.MapControl.WPF.Pushpin();
            pin.Location = new Microsoft.Maps.MapControl.WPF.Location(Settings.Latitude, Settings.Longitude);
            choosingLocationMap.ZoomLevel = 16;
            choosingLocationMap.Children.Add(pin);
            PokemonNests.ItemsSource = Data.PokeNest;
        }

        public void goToConfigs(object sender, RoutedEventArgs e)
        {
            selectPage(ConfigPanel);
        }

        public void Quick_Guide(object sender, RoutedEventArgs e)
        {
            SaveButton_Click(this, new RoutedEventArgs());
            switch (Data.quickGuidePos)
            {
                case 1:
                    if (Settings.AuthType == AuthType.Google)
                    {
                        Data.quickGuidePos++;
                        selectPage(quickStart2);
                    }
                    else
                    {
                        selectPage(quickStart3);
                        Data.quickGuidePos += 2;
                    }
                    break;
                case 2:
                    selectPage(quickStart3);
                    break;
                case 3:
                    ResetCoordsButton_Click(this, new RoutedEventArgs());
                    selectPage(ConfigPanel);
                    Data.quickGuidePos = 0;
                    break;
            }

        }

        public void startBotFromQuickGuide(object sender, RoutedEventArgs e)
        {
            ResetCoordsButton_Click(this, new RoutedEventArgs());
            SaveButton_Click(this, new RoutedEventArgs());
            button_Click(this, new RoutedEventArgs());
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            _set = new GlobalSettings();
            Settings = ObservableSettings.CreateFromGlobalSettings(_set);
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {

            _set = GlobalSettings.Load("config/" + Bot.BotName[Bot.chooseBotCache]);
            if (null == _set) _set = GlobalSettings.Load("config/" + Bot.BotName[Bot.chooseBotCache]);
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

            timePickerOn.SelectedTime = Settings.StartBotTime;
            timePickerOff.SelectedTime = Settings.StopBotTime;

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Settings == null) return;
            timePickerOn.SelectedTime = Settings.StartBotTime;
            timePickerOff.SelectedTime = Settings.StopBotTime;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), "config/" + Bot.BotName[Bot.chooseBotCache]);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");
            var authFile = Path.Combine(profileConfigPath, "auth.json");
            _set = Settings.GetGlobalSettingsObject(Bot.BotName[Bot.chooseBotCache]);
            
            LoadingMap.Center = new Microsoft.Maps.MapControl.WPF.Location(Settings.DefaultLatitude, Settings.DefaultLongitude);
            if (Data.quickGuidePos > 0)
            {
                _set.Auth.PtcPassword = Settings.PtcPassword = PasswordBoxAssistant.GetBoundPassword(ptcPasswordSetup);
                _set.Auth.GooglePassword = Settings.GooglePassword = PasswordBoxAssistant.GetBoundPassword(googlePasswordSetup);
                _set.TranslationLanguageCode = Settings.TranslationLanguageCode = Bot.LanguageDict[LanguageFieldSetup.SelectionBoxItem.ToString()];
            }
            else
            {
                _set.Auth.PtcPassword = Settings.PtcPassword = PasswordBoxAssistant.GetBoundPassword(ptcPassword);
                _set.Auth.GooglePassword = Settings.GooglePassword = PasswordBoxAssistant.GetBoundPassword(googlePassword);
                _set.TranslationLanguageCode = Settings.TranslationLanguageCode = Bot.LanguageDict[LanguageField.SelectionBoxItem.ToString()];
                if (timePickerOff.SelectedTime != null && timePickerOn.SelectedTime != null)
                {
                    _set.StartBotTime = Settings.StartBotTime = (DateTime)timePickerOn.SelectedTime;
                    _set.StopBotTime = Settings.StopBotTime = (DateTime)timePickerOff.SelectedTime;
                }
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

        public void OpenSettings()
        {
            selectPage(ConfigPanel);
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

        public void createNewBot(string Name, int Start)
        {
            if (Bot.BotAlreadyConfig[Start] == true) return;
            selectPage(Loading);
            addEvent("Started", "000");
            Data.firstStart = true;
            Bot.BotAlreadyConfig[Bot.chooseBot] = true;
            Bot.Bot[Bot.chooseBot] = new Bot("Resources\\config\\" + Name);
            Bot.ThreadBot[Bot.chooseBot] = new Thread(Bot.Bot[Bot.chooseBot].StartBot);
            Bot.ThreadBot[Bot.chooseBot].Start();
            
            if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config",Bot.BotName[Bot.chooseBot], "luckyEgg.ini")))
            {
                string inhalt = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", Bot.BotName[Bot.chooseBot], "luckyEgg.ini"));
                Data.luckyEggTime = DateTime.Parse(inhalt);
                Data.luckyEggEnabled = true;
            }

            Bot.Switch = new Thread(switchBot);
            Bot.Switch.Start();
        }

        public void updateUi()
        {
            if (Data.PokemonCounter > 998 || Data.PokestopCounter > 9998)
                stopBot_button(this, new RoutedEventArgs());
            Nickname.Content = Gui.Nickname;
            Level.Content = Gui.Level;
            xp.Content = Gui.xp;
            Stardust.Content = Gui.Stardust;
            ProfileBack.ImageSource = Gui.TeamImage;
            ProfileImage.Source = Gui.ProfileImage;
            if (Data.updatePlayerPosition == true)
                LoadingMap.Center = Gui.Position;
            pokemonListBox.ItemsSource = Gui.PokemonList;
            if (Gui.PokemonList != null)
            {
                string pokemonHave = Gui.PokemonList.Count().ToString();
                PokemonFromPokemon.Content = "Pokemon: " + pokemonHave + " / " + Bot.Bot[Bot.chooseBot].session.Profile.PlayerData.MaxPokemonStorage;
            }
            PokemonTab.Visibility = (Gui.PokemonList != null) ? Visibility.Visible : Visibility.Collapsed;
            ConsoleList.ItemsSource = Data.CommandLineBox;
            ConsoleList.Items.Refresh();
            ProgressBar.Maximum = Gui.NeedXp;
            EggList.ItemsSource = Gui.EggLists;
            ProgressBar.Value = Gui.HaveXp;
            RuntimeLabel.Content = Gui.Runtime.ToString();
        }

        public void switchBot()
        {
            string message = "";
            Bot.checkSwitch = false;
            while (true)
            {
                if (Bot.checkSwitch == true) break;
                if (message != Bot.Bot[Bot.chooseBot].Informations.Message)
                {
                    try
                    {
                        Dispatcher.BeginInvoke(new Action(() => LoginFailed(false)));
                        Data.CommandLineBox = Bot.Bot[Bot.chooseBot].Informations.readMessage();
                        MessageDesign[] arr = Data.CommandLineBox.ToArray();
                        message = arr[arr.Length - 1].Message;
                        if (Bot.BotLoaded[Bot.chooseBot] == true)
                        {
                            Dispatcher.BeginInvoke( new Action(() => Gui = Bot.Bot[Bot.chooseBot].updateUi(Data.SortByIv)));
                            Dispatcher.BeginInvoke( new Action(() => changeLive(Bot.Bot[Bot.chooseBot].Informations.liveHappening, Bot.Bot[Bot.chooseBot].Informations.PokemonName, Bot.Bot[Bot.chooseBot].Informations.PokemonID, Bot.Bot[Bot.chooseBot].Informations.coords, Bot.Bot[Bot.chooseBot].Informations.Pokemon)));
                            Dispatcher.BeginInvoke( new Action(() => updateUi()));
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (Gui.currentLat == 0.00) setPlayerPosition(_set.DefaultLatitude, _set.DefaultLongitude);
                                else setPlayerPosition(Gui.currentLat, Gui.currentLng);
                            }));
                            if (Data.LoginOnAntim8 == true)
                            {
                                Dispatcher.BeginInvoke( new Action(() => Data.Requester.updateDataIfLogin(Gui.Nickname, Gui.TeamName, Gui.Stardust, Gui.Level, Gui.xp, Gui.Runtime)));
                            }
                            Dispatcher.BeginInvoke( new Action(() =>
                            {
                                if (Settings.enableSchedule == true)
                                {
                                    DateTime time_now = DateTime.Now;
                                    if (Settings.StopBotTime.TimeOfDay > time_now.TimeOfDay)
                                        ClosingInMinutes.Content = (Settings.StopBotTime - time_now).TotalMinutes.ToString("0.00") + " Min.";
                                    else stopBot_button(this, new RoutedEventArgs());
                                }
                            }));
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (Data.luckyEggEnabled == true)
                                {
                                    DateTime now = DateTime.Now;
                                    if (Data.luckyEggTime < now)
                                    {
                                        luckyEggLabel.Content = "Lucky Egg ( finish )";
                                        Data.luckyEggEnabled = false;
                                        File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", Bot.BotName[Bot.chooseBot], "luckyEgg.ini"));
                                    }
                                    else
                                    {
                                        luckyEggLabel.Content = "Lucky Egg ( " + Math.Round(((Data.luckyEggTime - now).TotalMinutes), 1).ToString() + " Min. )";
                                    }
                                }
                                else
                                {
                                    Dispatcher.BeginInvoke(new Action(async () =>
                                    {
                                        int anzahlLucky = await Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemLuckyEgg);
                                        LuckyEggCount.Content = "( " + anzahlLucky.ToString() + " Eggs )";
                                    }));
                                    luckyEggLabel.Content = "Lucky Egg";
                                }
                            }));
                            int norm = 0, ultra = 0, great = 0, master = 0;
                            var normPok = Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemPokeBall);
                            var ultrPok = Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemUltraBall);
                            var greatPok = Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemGreatBall);
                            var masterPok = Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemMasterBall);
                            Dispatcher.BeginInvoke( new Action(async () => { norm = await normPok; ultra = await ultrPok; great = await greatPok; master = await masterPok; }));
                            Dispatcher.BeginInvoke( new Action(() => { PokeballsTotal_Label.Content = (norm + great + ultra + master).ToString();
                            }));
                        }
                        if (Data.firstStart == true && Bot.Bot[Bot.chooseBot].isLoaded == true)
                        {

                            Dispatcher.BeginInvoke( new Action(() => selectPage(Home)));
                            Dispatcher.BeginInvoke( new Action(() => Data.firstStart = false));
                            Dispatcher.BeginInvoke( new Action(() => Bot.BotLoaded[Bot.chooseBot] = true));
                        }
                        else
                        {
                            if (Data.firstStart == true && Bot.Bot[Bot.chooseBot].Informations.ErrorHappen == true)
                            {
                                if (Bot.Bot[Bot.chooseBot].Informations.LoginError == true)
                                    Dispatcher.BeginInvoke(new Action(() => LoginFailed(true)));
                                Dispatcher.BeginInvoke( new Action(() => {
                                    Dispatcher.BeginInvoke( new Action(() => selectPage(ConfigPanel)));
                                    Dispatcher.BeginInvoke( new Action(() => { Bot.checkSwitch = true; Bot.ThreadBot[Bot.chooseBot].Abort(); }));
                                }));
                            }
                            else Dispatcher.BeginInvoke( new Action(() => loadingMessages.Content = message.Split('>')[1]));
                        }
                    } catch (ArgumentNullException e)
                    {
                        //;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
            Dispatcher.BeginInvoke( new Action(() => Bot.checkSwitch = false));
            
            return;
        } 

        public void LoginFailed(Boolean wahr)
        {
            if (wahr == true) LoginFailedLabel.Visibility = Visibility.Visible;
            else LoginFailedLabel.Visibility = Visibility.Collapsed;
        }

        public void changeLive(string liveMessage, string pokemonName, string pokemonId, double[] Coords, PokemonListe Pokemon)
        {
            switch (liveMessage)
            {
                case "caught":
                    if (pokemonId == eventList[3].PokemonID) break;
                    Data.PokemonCounter++;
                    CaughtPokemon.Content =  Data.PokemonCounter;
                    addEvent("Caught", pokemonId);
                    if (Data.addThingsToMap == true) {
                        addToMap(Coords[0], Coords[1], false, pokemonId);
                    }
                    PokemonListe pok = new PokemonListe();
                    pok.setAll(Pokemon.Name, Pokemon.CP, Pokemon.IV, Pokemon.Bonbon, Pokemon.id, "", "","");
                    pok.setCatchTime();
                    pok.setIcon("Resources/Images/Models/" + pokemonId + ".png");
                    Data.RecentlyCaughtPokemon.Add(pok);
                    List<PokemonListe> tl = Data.RecentlyCaughtPokemon.OrderByDescending(o => o.catchTime).ToList();
                    while (tl.Count > 8)
                        tl.RemoveAt((tl.Count - 1));
                    recentlyCaughtPokemonList.ItemsSource = tl;
                    break;
                case "hatched":
                    if (pokemonId == eventList[3].PokemonID) break;
                    addEvent("Hatched", pokemonId);
                    break;
                case "fight":
                    break;
                case "evolve":
                    if (pokemonId == eventList[3].PokemonID) break;
                    addEvent("Evolved", pokemonId);
                    break;
                case "looting":
                    Data.PokestopCounter++;
                    VisitedPokestops.Content = Data.PokestopCounter;
                    if (Data.addThingsToMap == true)
                    {
                        addToMap(Coords[0], Coords[1]);
                    }
                    break;
                case "softban":
                    break;
                case "Error":
                    break;
                case "lootingFailed":
                    break;
                case "transfer":
                    if (pokemonId == eventList[3].PokemonID) break;
                    addEvent("Transfered", pokemonId);
                    break;    
                default:
                    break;
            }
        }

        public void setPlayerPosition(double lat, double lng)
        {
            if (Data.updatePlayerPosition == false) return;
            if (Data.PlayerPosition != null) LoadingMap.Children.Remove(Data.PlayerPosition);
            Data.PlayerPosition.Children.Clear() ;
            Data.PlayerPosition = new MapLayer();
            Microsoft.Maps.MapControl.WPF.Location locationNew = new Microsoft.Maps.MapControl.WPF.Location(lat, lng);
            PositionOrigin positionNew = PositionOrigin.Center;
            Data.PlayerPosition.AddChild(Data.imageNew, locationNew, positionNew);
            LoadingMap.Children.Add(Data.PlayerPosition);
        }

        private void addToMap(double lat, double lng, bool isPokestop = true,string pokeid = "")
        {
            if ( Data.PokemonMapPosition != null) LoadingMap.Children.Remove(Data.PokemonMapPosition);
            Image imageNew = new Image();
            String imgSource = "";
            if (isPokestop == false)
                imgSource = "Resources/Images/Models/" + pokeid + ".png";
            else
                imgSource = "Resources/Images/pokestop.png";
            imageNew.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), imgSource)));
            imageNew.Opacity = 0.9;
            imageNew.Stretch = System.Windows.Media.Stretch.None;
            Microsoft.Maps.MapControl.WPF.Location locationPoke = new Microsoft.Maps.MapControl.WPF.Location(lat, lng);
            PositionOrigin positionNew = PositionOrigin.Center;
            Data.PokemonMapPosition.AddChild(imageNew, locationPoke, positionNew);
            LoadingMap.Children.Add(Data.PokemonMapPosition);
        }

        #region GenerellEvent

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            int BotNr = Bot.chooseBotCache;
            Bot.chooseBot = Bot.chooseBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (Bot.BotAlreadyConfig[Bot.chooseBot] == true) switchBot_Click(this, new RoutedEventArgs());
            else
            {
                if (Settings.enableSchedule == true)
                {
                    ConfigPanel.Visibility = Visibility.Collapsed;
                    BotisStarting.Visibility = Visibility.Visible;
                    await sleepBot();
                    BotisStarting.Visibility = Visibility.Collapsed;
                }
                else {
                    ConfigPanel.Visibility = Visibility.Collapsed;
                }
                createNewBot(Bot.BotName[Bot.chooseBot], BotNr);
                StartBotButton.Click += BackToGame;
                StartBotButton.Content = "Go Back";
            }
        }

        private void BackToGame(object sender, RoutedEventArgs e)
        {
            selectPage(Home);
        }

        public void schedule_enabled()
        {
            ReloadButton_Click(this, new RoutedEventArgs());
            quickStart.Visibility = Visibility.Collapsed;
            ConfigPanel.Visibility = Visibility.Collapsed;
            startPage.Visibility = Visibility.Collapsed;
            updateWindow.Visibility = Visibility.Collapsed;
            button_Click(this,new RoutedEventArgs());
        }

        public async Task sleepBot()
        {         
                DateTime Now = DateTime.Now;
            if (Settings.StartBotTime < Now && Now > Settings.StopBotTime)
            {
                _set.StartBotTime = Settings.StartBotTime = Settings.StartBotTime.AddDays(1);
                _set.StopBotTime = Settings.StopBotTime = Settings.StopBotTime.AddDays(1);
            }
                while (Now.TimeOfDay < Settings.StartBotTime.TimeOfDay)
                {
                    Now = DateTime.Now;
                    startinginscheduleenabled.Content = "Starting in " + (Settings.StartBotTime - Now).TotalMinutes.ToString("0.00") + " Minutes";
                    await Task.Delay(600);
                }
        }

        
        private void useLuckyEgg(object sender, RoutedEventArgs e)
        {
                Dispatcher.BeginInvoke(new Action(async() =>
                {
                    int anzahlLucky = await Bot.Bot[Bot.chooseBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemLuckyEgg);
                    if (anzahlLucky < 1)
                    {
                        return;
                    }
                }));
            Bot.Bot[Bot.chooseBot].session.Client.Inventory.UseItemXpBoost();
            string configPfad = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", Bot.BotName[Bot.chooseBot],"luckyEgg.ini");
            Data.luckyEggTime = DateTime.Now;
            Data.luckyEggTime = Data.luckyEggTime.AddMinutes(30);
            Data.luckyEggEnabled = true;
            luckyEggLabel.Content = "Lucky Egg ( Active )";
            luckyEggLabel.Foreground = new SolidColorBrush(Colors.Green);
            File.WriteAllText(configPfad, $"{Data.luckyEggTime.ToString()}");
            BitmapImage img = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources","Images", "LuckyEgg.png")));
        }

        private void stopBot_MouseClick(object sender, MouseButtonEventArgs e)
        {
            stopBot_button(this, new RoutedEventArgs());
        }

        private void stopBot_button(object sender, RoutedEventArgs e)
        {
            Bot.chooseBot = Bot.chooseBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (Bot.Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Bot.Switch.Abort()));
            }
            Dispatcher.BeginInvoke(new Action(() => { if (Bot.ThreadBot[Bot.chooseBot] != null) Bot.ThreadBot[Bot.chooseBot].Abort(); }));
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();

        }

        private void stopBotSchedule_button(object sender, RoutedEventArgs e)
        {
            Bot.chooseBot = Bot.chooseBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (Bot.Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Bot.Switch.Abort()));
            }
            Dispatcher.BeginInvoke(new Action(() => { if (Bot.ThreadBot[Bot.chooseBot] != null) Bot.ThreadBot[Bot.chooseBot].Abort(); }));
            _set.enableSchedule = Settings.enableSchedule = false;
            SaveButton_Click(this,new RoutedEventArgs());
            ProcessStartInfo restart = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
            Process.Start(restart);
            Environment.Exit(-1);
        }

        private void whyuse(object sender, RoutedEventArgs e)
        {
            LoginToWebsite.Visibility = Visibility.Collapsed;
            whyUseWebsite.Visibility = Visibility.Visible;
        }

        private void whyuseBack(object sender, RoutedEventArgs e)
        {
            whyUseWebsite.Visibility = Visibility.Collapsed;
            LoginToWebsite.Visibility = Visibility.Visible;
        }

        private void backtoscreen(object sender, RoutedEventArgs e)
        {
            LoginToWebsite.Visibility = Visibility.Collapsed;
            startPage.Visibility = Visibility.Visible;
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

            if(Data.Requester.Login(username,password) == true)
            {
                Data.LoginOnAntim8 = true;
                _set.Auth.antim8username = Settings.antim8username = username;
                _set.Auth.antim8password = Settings.antim8password = password;
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
            if (Data.Requester.Register(antim8username.Text, PasswordBoxAssistant.GetBoundPassword(antim8Password)) == true)
            {
                LoginToWebsite.Visibility = Visibility.Collapsed;
                Data.LoginOnAntim8 = true;
                _set.Auth.antim8username = Settings.antim8username = antim8username.Text;
                _set.Auth.antim8password = Settings.antim8password = PasswordBoxAssistant.GetBoundPassword(antim8Password);
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
            ConfigPanel.Visibility = Visibility.Visible;
            button_Click(this, new RoutedEventArgs());
        }
        
        public void TotalAmountOfBerriesp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep = Settings.TotalAmountOfBerriesToKeep + 1; }
        public void TotalAmountOfPokeballsp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep = Settings.TotalAmountOfPokeballsToKeep + 1; }
        public void TotalAmountOfPotionsp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep = Settings.TotalAmountOfPotionsToKeep + 1; }
        public void TotalAmountOfRevicesp_Click(object sender, RoutedEventArgs e) { _set.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep = Settings.TotalAmountOfRevivesToKeep + 1; }
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
            Bot.chooseBotCache = botNr;
            ReloadButton_Click(this, new RoutedEventArgs());
            if (Bot.BotAlreadyConfig[Bot.chooseBotCache] == true)
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
                Data.quickGuidePos++;
                ConfigPanel.Visibility = Visibility.Collapsed;
                quickStart.Visibility = Visibility.Visible;
            } else
            {
                ConfigPanel.Visibility = Visibility.Visible;
            }
            
        }

        private void switchBot_Click(object sender, RoutedEventArgs e)
        {
            Bot.checkSwitch = true;
            Bot.Switch.Join();
            Bot.Switch = new Thread(switchBot);
            Bot.Switch.Start();
        }

        /***private void checkUsernameAvailable_Click(object sender, RoutedEventArgs e)
        {
            string nickname = tutorialUsername.Text;
            var resp = bots[ausgewaehlterBot].session.Client.Misc.CheckCodenameAvailable(nickname);
            if (resp.Result.IsAssignable == true)
            {
                codename = nickname;
                accountCreation = false;
                continueWithAccount = true;
                tutorialIsntComplete.Visibility = Visibility.Collapsed;
                dockPani.Visibility = Visibility.Visible;
            } else
            {
                tutorialUserError.Content = "Nickname is already taken";
            }
        } **/

        public void sendAway_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            ulong id = DataTemplate.id;
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                var response = await Bot.Bot[Bot.chooseBot].session.Client.Inventory.TransferPokemon(id);
            }));
            foreach(var poke in Gui.PokemonList)
            {
                if(poke.id == id)
                {
                    Dispatcher.BeginInvoke(new Action(() => Gui.PokemonList.Remove(poke)));
                }
            }
            updatePokeList();
            PokemonOverlay.Visibility = Visibility.Collapsed;
            PokemonTabItem.Visibility = Visibility.Visible;

        }

        public void levelUp_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            ulong id = DataTemplate.id;
            Dispatcher.BeginInvoke(new Action(async () =>
            {
               var response = await Bot.Bot[Bot.chooseBot].session.Inventory.UpgradePokemon(id);
            }));
            Dispatcher.BeginInvoke(new Action(() => Bot.Bot[Bot.chooseBot].updatePokemons(Data.SortByIv)));
        }

        public void Evolve_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            ulong id = DataTemplate.id;
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                        var EvolvePokemonResponse = await Bot.Bot[Bot.chooseBot].session.Client.Inventory.EvolvePokemon(id);
            }));
            Dispatcher.BeginInvoke(new Action(() => Bot.Bot[Bot.chooseBot].updatePokemons(Data.SortByIv)));
        }

        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (Bot.BotLoaded[Bot.chooseBot] == false) return;
            if ( box.SelectedIndex == 0)
                Data.SortByIv = false;
            else
                Data.SortByIv = true;
            Dispatcher.BeginInvoke(new Action(() => Bot.Bot[Bot.chooseBot].updateSortList(Data.SortByIv)));
            Thread updateList = new Thread(updatePokeList);
            updateList.Start();
        }

        public void updatePokeList()
        {
            if (Data.SortByIv == false)
                Dispatcher.BeginInvoke(new Action(() => pokemonListBox.ItemsSource = Gui.PokemonList.OrderByDescending(o => o.CpForOrder).ToList()));
            else
                Dispatcher.BeginInvoke(new Action(() => pokemonListBox.ItemsSource = Gui.PokemonList.OrderByDescending(o => o.IV).ToList()));
        }

        private void ToggleSwitch_IsCheckedChanged(object sender, EventArgs e)
        {
            if (Data.addThingsToMap == false)
                Data.addThingsToMap = true;
            else
            {
                Data.PokemonMapPosition.Children.Clear();
                Data.addThingsToMap = false;
            }
        }

        private void changeShowPlayer_IsCheckedChanged(object sender, EventArgs e)
        {
            if (Data.updatePlayerPosition == false)
            {
                LoadingMap.Children.Add(Data.PlayerPosition);
                Data.updatePlayerPosition = true;
            }
            else
            {
                LoadingMap.Children.Remove(Data.PlayerPosition);
                Data.updatePlayerPosition = false;
            }
        }

        private void enableschedule_checked(object sender, EventArgs e)
        {
            if (Data.enableSchedule == false)
            {
                scheduleplanervisibility.Visibility = Visibility.Visible;
                Data.enableSchedule = true;
            }
            else
            {
                scheduleplanervisibility.Visibility = Visibility.Collapsed;
                Data.enableSchedule = false;
            }
        }


        private Microsoft.Maps.MapControl.WPF.Location GeocodeAddress(string address)
        {
            Microsoft.Maps.MapControl.WPF.Location result = new Microsoft.Maps.MapControl.WPF.Location();
            string key = Data.Requester.Key;
            GeocodeRequest geocodeRequest = new GeocodeRequest();
            geocodeRequest.Credentials = new Microsoft.Maps.MapControl.WPF.Credentials();
            geocodeRequest.Credentials.ApplicationId = key;
            geocodeRequest.Query = address;
            ConfidenceFilter[] filters = new ConfidenceFilter[1];
            filters[0] = new ConfidenceFilter();
            filters[0].MinimumConfidence = GeocodeService.Confidence.High;
            GeocodeOptions geocodeOptions = new GeocodeOptions();
            geocodeOptions.Filters = filters;
            geocodeRequest.Options = geocodeOptions;
            GeocodeServiceClient geocodeService = new GeocodeServiceClient();
            GeocodeResponse geocodeResponse = geocodeService.Geocode(geocodeRequest);

            if (geocodeResponse.Results.Length > 0)
                result = new Microsoft.Maps.MapControl.WPF.Location(
                geocodeResponse.Results[0].Locations[0].Latitude,
                geocodeResponse.Results[0].Locations[0].Longitude);
            else
                return null;

            return result;
        }

        private void ChoosePokemonNest_Click(object sender, RoutedEventArgs e)
        {
            Button Button = sender as Button;
            PokemonNest DataTemplate = Button.DataContext as PokemonNest;
            choosingLocationMap.Center = new Microsoft.Maps.MapControl.WPF.Location(DataTemplate.Latitude, DataTemplate.Longitude);
            Settings.DefaultLatitude = DataTemplate.Latitude;
            Settings.DefaultLongitude = DataTemplate.Longitude;
            Microsoft.Maps.MapControl.WPF.Pushpin pin = new Microsoft.Maps.MapControl.WPF.Pushpin();
            pin.Location = new Microsoft.Maps.MapControl.WPF.Location(DataTemplate.Latitude, DataTemplate.Longitude);
            choosingLocationMap.Children.Clear();
            choosingLocationMap.Children.Add(Data.PokemonNestLayer);
            choosingLocationMap.ZoomLevel = 16;
            choosingLocationMap.Children.Add(pin);
        }

        private void search_click(object sender, RoutedEventArgs e)
        {
            string search = locationPickerSearch.Text;
            Microsoft.Maps.MapControl.WPF.Location position = GeocodeAddress(search);
            if (position == null) return;
            choosingLocationMap.Center = new Microsoft.Maps.MapControl.WPF.Location(position.Latitude, position.Longitude);
            Settings.DefaultLatitude = position.Latitude;
            Settings.DefaultLongitude = position.Longitude;
            Microsoft.Maps.MapControl.WPF.Pushpin pin = new Microsoft.Maps.MapControl.WPF.Pushpin();
            pin.Location = position;
            choosingLocationMap.Children.Clear();
            choosingLocationMap.Children.Add(Data.PokemonNestLayer);
            choosingLocationMap.Children.Add(pin);
        }

        private void MapWithPushpins_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            System.Windows.Point mousePosition = e.GetPosition(choosingLocationMap);
            Microsoft.Maps.MapControl.WPF.Location pinLocation = choosingLocationMap.ViewportPointToLocation(mousePosition);
            _set.DefaultLatitude = Settings.DefaultLatitude = Settings.Latitude = pinLocation.Latitude;
            _set.DefaultLongitude = Settings.DefaultLongitude = Settings.Longitude = pinLocation.Longitude;
            Microsoft.Maps.MapControl.WPF.Pushpin pin = new Microsoft.Maps.MapControl.WPF.Pushpin();
            pin.Location = pinLocation;
            choosingLocationMap.Children.Clear();
            choosingLocationMap.Children.Add(Data.PokemonNestLayer);
            choosingLocationMap.Children.Add(pin);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void openLocationPicker(object sender, RoutedEventArgs e)
        {
            updateNests();
            choosingLocationMap.Center = new Microsoft.Maps.MapControl.WPF.Location(Settings.Latitude, Settings.Longitude);
            quickStart3.Visibility = Visibility.Collapsed;
            ConfigPanel.Visibility = Visibility.Collapsed;
            PickALocation.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PickALocation.Visibility = Visibility.Collapsed;
            if (Data.quickGuidePos > 0)
            {
                quickStart3.Visibility = Visibility.Visible;
            }
            else
            {
                ConfigPanel.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Bot.Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Bot.Switch.Abort()));
            }
            for (int i = 0; i < 8; i++)
            {
                Dispatcher.BeginInvoke(new Action(() => { if (Bot.ThreadBot[i] != null) { Bot.ThreadBot[i].Abort(); } }));
            }
            Data.Requester.closeBot();
            Data.Requester.returnKey(BingMapsApi.Text);
        }
        #endregion

        private void openSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            profile.Visibility = Visibility.Collapsed;
            PokemonTabItem.Visibility = Visibility.Collapsed;
            EggsTabItem.Visibility = Visibility.Collapsed;
            PokemonOverlay.Visibility = Visibility.Collapsed;
            ConsoleTabItem.Visibility = Visibility.Collapsed;
            RecentlyTabItem.Visibility = Visibility.Collapsed;
            InventoryItemTab.Visibility = Visibility.Collapsed;
            SettingsItemTab.Visibility = Visibility.Collapsed;
            if (menu.Visibility == Visibility.Collapsed)
                menu.Visibility = Visibility.Visible;
            else
                menu.Visibility = Visibility.Collapsed;
        }

        private void openProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (profile.Visibility == Visibility.Collapsed)
                profile.Visibility = Visibility.Visible;
            else
                profile.Visibility = Visibility.Collapsed;
        }

        private void openRecent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (RecentlyTabItem.Visibility == Visibility.Collapsed)
                RecentlyTabItem.Visibility = Visibility.Visible;
            else
                RecentlyTabItem.Visibility = Visibility.Collapsed;
        }

        private void openPokemon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            Dispatcher.BeginInvoke(new Action(() => Bot.Bot[Bot.chooseBot].updatePokemons(Data.SortByIv)));
            if (PokemonTabItem.Visibility == Visibility.Collapsed)
                PokemonTabItem.Visibility = Visibility.Visible;
            else
                PokemonTabItem.Visibility = Visibility.Collapsed;
        }
        
        private void openEggs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (EggsTabItem.Visibility == Visibility.Collapsed)
                EggsTabItem.Visibility = Visibility.Visible;
            else
                EggsTabItem.Visibility = Visibility.Collapsed;
        }

        private void openConsole_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (ConsoleTabItem.Visibility == Visibility.Collapsed)
                ConsoleTabItem.Visibility = Visibility.Visible;
            else
                ConsoleTabItem.Visibility = Visibility.Collapsed;
        }

        private void openItems_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (InventoryItemTab.Visibility == Visibility.Collapsed)
                InventoryItemTab.Visibility = Visibility.Visible;
            else
                InventoryItemTab.Visibility = Visibility.Collapsed;
        }

        private void openSetting_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            if (SettingsItemTab.Visibility == Visibility.Collapsed)
                SettingsItemTab.Visibility = Visibility.Visible;
            else
                SettingsItemTab.Visibility = Visibility.Collapsed;
        }

        private void openAllSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
            OpenSettings();
        }

        private void useLucky_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            useLuckyEgg(this,new RoutedEventArgs());
        }

        private void openSinglePok(object sender, MouseButtonEventArgs e)
        {
            List<PokemonListe> pok = new List<PokemonListe>();
            Image Button = sender as Image;
            PokemonListe DataTemplate = Button.DataContext as PokemonListe;
            pok.Add(DataTemplate);
            pokemonCloseView.ItemsSource = pok;
            PokemonTabItem.Visibility = Visibility.Collapsed;
            PokemonOverlay.Visibility = Visibility.Visible;
        }

        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationBehavior.GetAnimator(MenuButton).Play();
        }

        private void MenuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationBehavior.GetAnimator(MenuButton).Pause();
        }

        private void addEvent(String eventName, string pokeId)
        {
            Uri path = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images","Pokemon", pokeId + ".gif"));
            for (int i = 0; i < 3; ++i)
            {
                eventList[i] = eventList[i + 1];
            }
            eventList[3] = new EventShow(eventName, path,pokeId);
            List<EventShow> Display = new List<EventShow>();
            foreach (var ev in eventList.Reverse())
            {
                if (ev != null)
                    Display.Add(ev);
            }
            EventHandler.ItemsSource = null;
            EventHandler.ItemsSource = Display;
            EventHandler.Items.Refresh();
        }

    }

    public class EventShow
    {
        public string EventMessage { get; set; }
        public Uri Path { get; set; }
        public string PokemonID { get; set; }
        public EventShow(string EventMessage, Uri Path, string PokemonId)
        {
            this.EventMessage = EventMessage;
            this.Path = Path;
            this.PokemonID = PokemonId;
        }

        public EventShow(EventShow e)
        {
            this.EventMessage = e.EventMessage;
            this.Path = e.Path;
            this.PokemonID = e.PokemonID;
        }
    }
}
