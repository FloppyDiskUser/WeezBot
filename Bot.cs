using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic;
using System.Threading;
using System.Globalization;
using PoGo.NecroBot.Logic.State;
using PoGo.NecroBot.Logic.Utils;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Data.Player;
using POGOProtos.Data;
using PoGo.NecroBot.Logic.PoGoUtils;
using System.Windows;
using System.Windows.Threading;

namespace WeezBot
{
    public class Bot
    {

        /// <summary>
        /// Klasse um neuen Bot zu erstellen.
        /// </summary>
        
        // Globale Variablen
        public dynamic Informations = new Information();
        private string subPath = "";
        public Overlay GraphicalInterface = new Overlay();
        public Session session;
        public Boolean isLoaded = false;
        public getPokemonDisplayed pokemonDisplay;
        PoGo.NecroBot.Logic.Common.Translation Uebersetzer = new Translation();
        private Dictionary<string, string> pokemonNameToId = new Dictionary<string, string>();

        public Bot(string subPath)
        {
            this.subPath = subPath;
        }

        public void StartBot()
        {
            string strCulture = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            var culture = CultureInfo.CreateSpecificCulture("en");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
            var profilePath = Path.Combine(Directory.GetCurrentDirectory(), subPath);
            var profileConfigPath = Path.Combine(profilePath, "config");
            var configFile = Path.Combine(profileConfigPath, "config.json");
            PoGo.NecroBot.Logic.GlobalSettings Einstellungen = new PoGo.NecroBot.Logic.GlobalSettings();

            List<KeyValuePair<int, string>> PokemonTranslationStrings = Uebersetzer._PokemonNameToId;
            pokemonNameToId["missingno"] = "001";
            foreach (var pokemon in PokemonTranslationStrings)
            {
                string pomoName = pokemon.Key.ToString();
                string idC = "";
                if(pomoName.Length == 1)
                    idC = "00" + pomoName;
                if (pomoName.Length == 2)
                    idC = "0" + pomoName;
                if (pomoName.Length == 3)
                    idC = pomoName;
                pokemonNameToId[pokemon.Value.ToString()] = idC;
            }

            if (File.Exists(configFile))
            {
                Einstellungen = GlobalSettings.Load(subPath);
            }
            else
            {
                return;
            }

            session = new Session(new ClientSettings(Einstellungen), new LogicSettings(Einstellungen));
            session.Client.ApiFailure = new ApiFailureStrategy(session);
            var stats = new Statistics();


            stats.DirtyEvent += () =>
            {
                isLoaded = true;
                this.GraphicalInterface.updateData(stats.getNickname().ToString(), stats.getLevel().ToString(), stats.getNeedXp(), stats.getTotalXp(), stats.getStardust().ToString(), Math.Round((stats.GetRuntime()*60),3).ToString());
            };
                
            var aggregator = new StatisticsAggregator(stats);
            var machine = new StateMachine();
            session.EventDispatcher.EventReceived += evt => Informations.Listen(evt, session);
            session.EventDispatcher.EventReceived += evt => aggregator.Listen(evt, session);

            machine.SetFailureState(new LoginState());

            session.Navigation.UpdatePositionEvent += (lat,lng) => session.EventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng });
            session.Navigation.UpdatePositionEvent += Navigation_UpdatePositionEvent;
            machine.AsyncStart(new VersionCheckState(), session, subPath);
            pokemonDisplay = new getPokemonDisplayed(session);
            Einstellungen.checkProxy(session.Translation);
        }

        private async Task updatePokemonByServer(Boolean Iv = false)
        {
            await pokemonDisplay.RefreshPokemonList(Iv);
        }

        private void getPokemons(Boolean Iv = false)
        {
            List<PokemonListe> pokemonlist = new List<PokemonListe>();
            List<PokemonData> Pokemons = pokemonDisplay.Liste;
            if (Pokemons == null) return ;
            
            List<POGOProtos.Inventory.Candy> myPokemonFamilies = pokemonDisplay.FamilieListe;
            IEnumerable<POGOProtos.Settings.Master.PokemonSettings> myPokeSettings = pokemonDisplay.PokeSettingListe;
            foreach (var Poke in Pokemons)
            {
                PokemonListe pok = new PokemonListe();
                pok.setCp(Poke.Cp, PokemonInfo.CalculateMaxCp(Poke));
                pok.setId(Poke.Id);
                string pfad = pokemonNameToId[Poke.PokemonId.ToString()].ToString();
                pok.setIcon("Images/Models/" + pfad + ".png");
                pok.Name = session.Translation.GetPokemonTranslation(Poke.PokemonId).ToString();
                pok.Move1 = session.Translation.GetPokemonMovesetTranslation(Poke.Move1).ToString();
                pok.Move2 = session.Translation.GetPokemonMovesetTranslation(Poke.Move2).ToString();
                pok.setIv(PokemonInfo.CalculatePokemonPerfection(Poke));
                pok.Bonbon = PokemonInfo.GetCandy(Poke, myPokemonFamilies, myPokeSettings).ToString();
                pokemonlist.Add(pok);
            }
            GraphicalInterface.PokemonList = pokemonlist;
        }

        public void updatePokemons(Boolean Iv = false)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () => await updatePokemonByServer(Iv)));
            getPokemons(Iv);
        }

        public void updateSortList(bool SortByIv)
        {
            if (SortByIv == false)
                GraphicalInterface.PokemonList = GraphicalInterface.PokemonList.OrderByDescending(o => o.CpForOrder).ToList();
            else
                GraphicalInterface.PokemonList = GraphicalInterface.PokemonList.OrderByDescending(o => o.IV).ToList();
        }

        public Overlay updateUi(Boolean Iv)
        {
            updateEggs();
            this.GraphicalInterface.setTeam(session.Profile.PlayerData.Team.ToString());
            if (GraphicalInterface.PokemonList == null) updatePokemons();
            return GraphicalInterface;
        }

        public List<EggsSetting> eggs = new List<EggsSetting>();

        public async void updateEggs()
        {
            var playerStats = (await session.Inventory.GetPlayerStats()).FirstOrDefault();
            if (playerStats == null)
                return;

            var kmWalked = playerStats.KmWalked;
            var incubators = (await session.Inventory.GetEggIncubators())
                .Where(x => x.UsesRemaining > 0 || x.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncubatorBasicUnlimited)
                .OrderByDescending(x => x.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncubatorBasicUnlimited)
                .ToList();

            var unusedEggs = (await session.Inventory.GetEggs())
                .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                .OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart)
                .ToList();

            eggs = new List<EggsSetting>();

            foreach (var incubator in incubators)
            {
                if (incubator.PokemonId != 0)
                {
                    double kmRemaining = incubator.TargetKmWalked - kmWalked;
                    double targetKm = incubator.TargetKmWalked - incubator.StartKmWalked;
                    string cont = Math.Round((targetKm - kmRemaining), 2).ToString() + " / " + Math.Round(targetKm, 2).ToString() + " km";
                    EggsSetting newEgg = new EggsSetting(cont,kmRemaining,targetKm,System.Windows.Visibility.Visible, true);
                    eggs.Add(newEgg);
                }
            }
            foreach (var egg in unusedEggs)
            {
                if (egg.IsEgg)
                {
                    double targetKm = egg.EggKmWalkedTarget;
                    string cont = targetKm.ToString() + " km";
                    EggsSetting newEgg = new EggsSetting(cont , 0.00, targetKm , Visibility.Hidden,  false);
                    eggs.Add(newEgg);
                }
            }
            GraphicalInterface.EggLists = eggs;
            return;
        }
        

        private void Navigation_UpdatePositionEvent(double lat, double lng)
        {
            SaveLocationToDisk(lat, lng);
            GraphicalInterface.setCoords(lat, lng);
        }

        private void SaveLocationToDisk(double lat, double lng)
        {
            var coordsPath = Path.Combine(Directory.GetCurrentDirectory(), subPath, "Config", "LastPos.ini");

            File.WriteAllText(coordsPath, $"{lat}:{lng}");
        }

        private static void UnhandledExceptionEventHandler(object obj, UnhandledExceptionEventArgs args)
        {
            throw new Exception();
        }
    }
}
