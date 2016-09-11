using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeezBot.MainWindowData
{
    public class PokemonNest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PokemonName { get; set; }
        public string LocationName { get; set; }
        public string id { get; set; }

        public string img
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "Images", "Models", id + ".png"); }
            set { img = value; }
        }
    }
}
