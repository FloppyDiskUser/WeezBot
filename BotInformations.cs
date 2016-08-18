using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WeezBot
{
    public class MessageDesign
    {
        public string Message { get; set; }
        public Brush Farbe { get; set; }

        public MessageDesign(string Message,string ColorName)
        {
            this.Message = Message;
            this.setColor(ColorName);
        }

        public void setColor(string ColorName)
        {
            Farbe = (Brush)new BrushConverter().ConvertFromString(ColorName);
        }
    }

    public class Overlay : INotifyPropertyChanged
    {
        public List<EggsSetting> EggLists { get; set; }
        public List<PokemonListe> PokemonList { get; set; }
        public string Nickname { get; set; }
        public string Level { get; set; }
        public double NeedXp { get; set; }
        public double HaveXp { get; set; }
        public string xp { get; set; }
        public string TeamName { get; set; }
        public BitmapImage TeamImage { get; set; }
        public string liveActionLabel { get; set; }
        public BitmapImage PokemonImage { get; set; }
        public double currentLat { get; set; }
        public double currentLng { get; set; }
        public Location Position { get; set; }
        public string MapLabel { get; set; }
        public string Stardust { get; set; }

        public Overlay()
        {
            Position = new Location(0.00, 0.00);
        }

        public void updateData(string Nickname, string Level, double needXp,double haveXp, string stardust)
        {
            this.Nickname = Nickname;
            this.Level = "Level " + Level;
            this.xp = (this.HaveXp = haveXp) + " / " + (this.NeedXp = needXp);
            this.Stardust = "Stardust: " + stardust;
        }

        public void setCoords(double lat, double lng)
        {
            MapLabel = "Latitude: " + (this.currentLat = lat) + " , Longitude: " + (this.currentLng = lng);
        }

        public void setPokeImg(string PokemonName)
        {
            PokemonImage = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "Pokemon", PokemonName+ ".png")));
        }

        public void setTeam(string TeamName)
        {
            TeamImage = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img", "teams", "team_" + TeamName + ".png")));
            if (TeamName == "Red") this.TeamName = "Team Valor";
            if (TeamName == "Yellow") this.TeamName = "Team Instinct";
            if (TeamName == "Blue") this.TeamName = "Team Mystic";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class EggsSetting
    {
        public string Content { get; set; }
        public double kmRemaining { get; set; }
        public double targetKm { get; set; }
        public Visibility Visibility { get; set; }
        public bool incubator { get; set; }
        public BitmapImage image { get; set; }

        public EggsSetting(string Content,double kmRemaining, double targetKm, Visibility Visibility, bool incubator)
        {
            this.Content = Content;
            this.kmRemaining = kmRemaining;
            this.targetKm = targetKm;
            this.Visibility = Visibility;
            this.incubator = incubator;
            if (incubator == true) this.image = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(),"img/Pokemon_incubator.png")));
            else this.image = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "img/Pokemon_egg.png")));
        }
    }
}
