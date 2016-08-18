using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeezBot
{
    public class PokeStopListe
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }

        public PokeStopListe (double Latitude, double Longitude, string Name)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Name = Name;
        }
    }

    public class PokemonListe
    {
        public System.Windows.Media.Imaging.BitmapImage Icon { get; set; }
        public string Name { get; set; }
        public string CP { get; set; }
        public string IV { get; set; }
        public string Bonbon { get; set; }
        public ulong id { get; set; }
        public string Move1 { get; set; }
        public string Move2 { get; set; }

        public void setAll(string Name, string CP, string IV, string Bonbon, ulong id, string move1, string move2, string pfad)
        {
            setIcon(pfad);
            this.Name = Name;
            this.CP = CP;
            this.IV = IV;
            this.Bonbon = Bonbon;
            this.id = id;
            this.Move1 = move1;
            this.Move2 = move2;
        }

        public void setIcon(string pfad)
        {
            if(File.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(), pfad)))
                Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), pfad)));
        }

        public void setCp(int Cp, int maxCp)
        {
            CP = Cp.ToString() + " / " + maxCp.ToString();
        }

        public void setIv(double Iv)
        {
            IV = Math.Round(Iv,2).ToString() + " % ";
        }

        public void setId(ulong id)
        {
            this.id = id;
        }
    }
}
