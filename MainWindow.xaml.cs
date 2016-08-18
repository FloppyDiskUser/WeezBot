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
        Thread Switch;
        int ausgewaehlterBotCache, ausgewaehlterBot = 0;

        // Button Variablen
        int anzahlMaximaleButtons = 0;


        // Other Variable
        private Boolean Iv = false;
        Boolean firstStart = true;
        BitmapImage imgLiveAction = new BitmapImage();
        Image myPushPin = new Image();
        public Overlay gui = new Overlay();
        public MessageDesign[] Messages = new MessageDesign[8];
        public Label[] CommandLineLabels = new Label[8];

        public MainWindow()
        {
            // Initialize Controls
            this.DataContext = this;
            InitializeComponent();

            CommandLineLabels[0] = Command1;
            CommandLineLabels[1] = Command2;
            CommandLineLabels[2] = Command3;
            CommandLineLabels[3] = Command4;
            CommandLineLabels[4] = Command5;
            CommandLineLabels[5] = Command6;
            CommandLineLabels[6] = Command7;
            CommandLineLabels[7] = Command8;


            // Starts The Bot with the Settings Tab
            OpenSettings();
        }

        #region SettingsTab
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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), "config/" + botNames[ausgewaehlterBotCache]);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");
            var authFile = Path.Combine(profileConfigPath, "auth.json");
            _set = Settings.GetGlobalSettingsObject(botNames[ausgewaehlterBotCache]);
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
            Loading.Opacity = 1;
            firstStart = true;
            anzahlMaximaleButtons++;
            BotAlreadyConfig[ausgewaehlterBot] = true;
            bots[ausgewaehlterBot] = new Bot("config\\" + Name);
            ThreadBot[ausgewaehlterBot] = new Thread(bots[ausgewaehlterBot].StartBot);
            ThreadBot[ausgewaehlterBot].Start();
            check = false;
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
        public void updateUi(){
            Nickname.Content = gui.Nickname;
            Level.Content = gui.Level;
            xp.Content = gui.xp;
            Stardust.Content = gui.Stardust;
            for(int f = 0; f < 8; ++f)
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
            while (true) {
                if (check == true) break;
                if (message != bots[ausgewaehlterBot].Informations.Message)
                {
                    Dispatcher.BeginInvoke(new Action(() => LoginFailed(false)));
                    Messages = bots[ausgewaehlterBot].Informations.readMessage();
                    message = Messages[0].Message;
                    
                    if (loaded[ausgewaehlterBot] == true)
                    {
                        bots[ausgewaehlterBot].updateEggs();
                        Dispatcher.BeginInvoke(new Action(() => { gui = bots[ausgewaehlterBot].updateUi(Iv); changeLive(bots[ausgewaehlterBot].Informations.liveHappening, bots[ausgewaehlterBot].Informations.PokemonName,
                                                                  bots[ausgewaehlterBot].Informations.PokemonID,bots[ausgewaehlterBot].Informations.coords);
                                                                  pokemonListBox.ItemsSource = gui.PokemonList; EggList.ItemsSource = gui.EggLists;updateUi();
                        }));
                        int norm = 0,ultra = 0,great = 0,master = 0;
                        var normPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemPokeBall);
                        var ultrPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemUltraBall);
                        var greatPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemGreatBall);
                        var masterPok = bots[ausgewaehlterBot].session.Inventory.GetItemAmountByType(POGOProtos.Inventory.Item.ItemId.ItemMasterBall);
                        Dispatcher.BeginInvoke(new Action(async () => { norm = await normPok; ultra = await ultrPok; great = await greatPok; master = await masterPok; }));
                        Dispatcher.BeginInvoke(new Action(() => { PokeballsTotal_Label.Content = "Total Pokéballs: " + (norm + great + ultra + master).ToString(); }));
                    }
                    if (firstStart == true && bots[ausgewaehlterBot].isLoaded == true)
                    {

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            tabs.SelectedIndex = 1;
                            Loading.Opacity = 0;
                            loaded[ausgewaehlterBot] = true;
                            firstStart = false;
                        }));
                    }
                    else
                    {
                        if (firstStart == true && bots[ausgewaehlterBot].Informations.ErrorHappen == true)
                        {
                            if(bots[ausgewaehlterBot].Informations.LoginError == true)
                            {
                                Dispatcher.BeginInvoke(new Action(() => LoginFailed(true)));
                            }
                            Dispatcher.BeginInvoke(new Action(() => { Loading.Opacity = 0; tabs.SelectedIndex = 0;
                                changeContent(check, true); ThreadBot[ausgewaehlterBot].Abort();
                            }));
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
        public void changeLive(string liveMessage, string pokemonName, string pokemonId,double[] Coords)
        {
            var uebersetzer = new PoGo.NecroBot.Logic.Common.Translation();
            switch (liveMessage)
            {
                case "caught":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Catch.gif")));
                    imgLiveAction_Label.Text =  uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.caught,pokemonName);
                    MapLayer imageLayer = new MapLayer();
                    Image image = new Image();
                    image.Height = 50;
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokemon", pokemonId + ".png"));
                    myBitmapImage.DecodePixelHeight = 50;
                    myBitmapImage.EndInit();
                    image.Source = myBitmapImage;
                    image.Opacity = 0.9;
                    image.Stretch = System.Windows.Media.Stretch.None;
                    Microsoft.Maps.MapControl.WPF.Location location = new Microsoft.Maps.MapControl.WPF.Location(Coords[0], Coords[1]);
                    PositionOrigin position = PositionOrigin.Center;
                    imageLayer.AddChild(image, location, position);
                    LoadingMap.Children.Add(imageLayer);
                    imgLiveActionPokemon.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img","Pokemon",pokemonId + ".png")));
                    break;
                case "moving":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Moving.gif")));
                    imgLiveAction_Label.Text = uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.moving);
                    imgLiveActionPokemon.Source = null;
                    break;
                case "hatched":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Egg_hatched.gif")));
                    imgLiveAction_Label.Text = imgLiveAction_Label.Text = uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.hatch, pokemonName); ;
                    imgLiveActionPokemon.Source = null;
                    break;
                case "fight":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Fight.gif")));
                    imgLiveAction_Label.Text = uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.fight, pokemonName);
                    imgLiveActionPokemon.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokemon", pokemonId + ".png")));
                    break;
                case "evolve":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Evolve.gif")));
                    imgLiveAction_Label.Text = imgLiveAction_Label.Text = uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.evolve, pokemonName);
                    imgLiveActionPokemon.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokemon", pokemonId + ".png")));
                    break;
                case "looting":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Looting.gif")));
                    imgLiveAction_Label.Text = "Looting PokeStop";
                    MapLayer imageLayerNew = new MapLayer();
                    Image imageNew = new Image();
                    imageNew.Height = 50;
                    BitmapImage myBitmapImageNew = new BitmapImage();
                    myBitmapImageNew.BeginInit();
                    myBitmapImageNew.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokestop.png"));
                    myBitmapImageNew.DecodePixelHeight = 50;
                    myBitmapImageNew.EndInit();
                    imageNew.Source = myBitmapImageNew;
                    imageNew.Opacity = 0.9;
                    imageNew.Stretch = System.Windows.Media.Stretch.None;
                    Microsoft.Maps.MapControl.WPF.Location locationNew = new Microsoft.Maps.MapControl.WPF.Location(Coords[0], Coords[1]);
                    PositionOrigin positionNew = PositionOrigin.Center;
                    imageLayerNew.AddChild(imageNew, locationNew, positionNew);
                    LoadingMap.Children.Add(imageLayerNew);
                    imgLiveActionPokemon.Source = null;
                    break;
                case "softban":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Error.gif")));
                    imgLiveAction_Label.Text = "Loading";
                    imgLiveActionPokemon.Source = null;
                    break;
                case "Error":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Error.gif")));
                    imgLiveAction_Label.Text = "Error";
                    imgLiveActionPokemon.Source = null;
                    break;
                case "lootingFailed":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Error.gif")));
                    imgLiveAction_Label.Text = "Looting Error";
                    imgLiveActionPokemon.Source = null;
                    break;
                case "transfer":
                    imgLiveAction = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "img/Event/Transfer.gif")));
                    if (HateSchreddertVersion.IsChecked == true) { imgLiveAction_Label.Text = pokemonName + " mit Hass Geschredert"; }
                    else imgLiveAction_Label.Text = uebersetzer.GetTranslation(PoGo.NecroBot.Logic.Common.TranslationString.transfer, pokemonName); ;
                    imgLiveActionPokemon.Source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokemon", pokemonId + ".png")));
                    break;
            }
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(localImage,imgLiveAction);
        }
        #endregion

        public void changeContent(ProgressBar object_Progress, double object_value, double object_maximum)
        {
            Dispatcher.BeginInvoke(new Action(() => { object_Progress.Value = object_value; object_Progress.Maximum = object_maximum; }));
        }

        public void changeContent(Grid object_Grid, double object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_Grid.Opacity = object_value));
        }

        public void changeContent(Image object_Image, System.Windows.Media.Imaging.BitmapImage object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_Image.Source = object_value));
        }

        public void changeContent(string object_String, string object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_String = object_value));
        }

        public void changeContent(Boolean object_Bool, bool object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_Bool = object_value));
        }

        public void changeContent(Label object_Label, string object_value){
            Dispatcher.BeginInvoke(new Action(() => object_Label.Content = object_value));
        }

        public void changeContent(TextBox object_TextBox, string object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_TextBox.Text = object_value));
        }

        public void changeContent(TextBlock object_TextBlock, string object_value)
        {
            Dispatcher.BeginInvoke(new Action(() => object_TextBlock.Text = object_value));
        }

        #region GenerellEvent

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int BotNr = ausgewaehlterBotCache;
            ausgewaehlterBot = ausgewaehlterBotCache;
            SaveButton_Click(this, new RoutedEventArgs());
            if (BotAlreadyConfig[ausgewaehlterBot] == true) switchBot_Click(this, new RoutedEventArgs());
            else createNewBot(botNames[ausgewaehlterBot],BotNr);
        }

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
                StartBotButton.Visibility = Visibility.Collapsed;
                panelUni.Columns = 3;
            } else
            {
                StartBotButton.Opacity = 1;
                panelUni.Columns = 4;
            }
        }

        private void switchBot_Click(object sender,RoutedEventArgs e)
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
            if (box.SelectedIndex == 0) Iv = false;
            else Iv = true;
            if (bots[ausgewaehlterBot] != null) ;
        }

        private void Window_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Switch != null)
            {
                Dispatcher.BeginInvoke(new Action(() => Switch.Abort()));
            }
            for(int i = 0; i < 8; i++)
            {
                Dispatcher.BeginInvoke(new Action(() => { if (ThreadBot[i] != null) ThreadBot[i].Abort(); }));
            }
        }
        #endregion
    }
}
