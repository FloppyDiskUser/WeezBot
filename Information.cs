using Newtonsoft.Json;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeezBot
{
    class Information
    {

        PoGo.NecroBot.Logic.Common.Translation Uebersetzer = new Translation();
        private Dictionary<string, string> pokemonNameToId = new Dictionary<string, string>();
        public Boolean ErrorHappen;
        int counter = 0;
        string Color = "White";

        public Information()
        {
            for(int i = 0; i < 8; ++i)
            {
                Messages[i] = new MessageDesign("","White");
            }
            List<KeyValuePair<int, string>> PokemonTranslationStrings = Uebersetzer._PokemonNameToId;
            foreach (var pokemon in PokemonTranslationStrings)
            {
                string pomoName = pokemon.Key.ToString();
                string idC = "";
                if (pomoName.Length == 1)
                    idC = "00" + pomoName;
                if (pomoName.Length == 2)
                    idC = "0" + pomoName;
                if (pomoName.Length == 3)
                    idC = pomoName;
                pokemonNameToId[pokemon.Value.ToString()] = idC;
            }
        }
        internal void Listen(IEvent evt, ISession session)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve, session);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
        public string Message = "";
        public string PokemonID = "000";
        public string PokemonName = "";

        public bool LuckyEggActive = false;
        public string liveHappening = "moving";
        public double[] coords = new double[2];
        public bool LoginError = false;
        public MessageDesign[] Messages = new MessageDesign[8];

        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            if (level == LogLevel.Pokestop) Color = "Blue";
            else if (level == LogLevel.Caught) Color = "Green";
            else if (level == LogLevel.Egg) Color = "Yellow";
            else if (level != LogLevel.Flee) Color = "White";
              
            if (message == "User credentials are invalid") LoginError = true;
            else
                if (level != LogLevel.Error) ErrorHappen = false;
                else LoginError = false;
            if (level == LogLevel.Flee) liveHappening = "fight";
            if (liveHappening == "moving") counter = 0;
            if (counter > 0) { liveHappening = "moving"; counter = 0; }

            for(int h = 6;h >= 0; --h)
            {
                Messages[h + 1] = Messages[h];
            }
            MessageDesign obj = new MessageDesign(this.Message = level.ToString() + " > " + message, Color);
            Messages[0] = obj;
        }

        public MessageDesign[] readMessage()
        {
            return Messages;
        }

        private void HandleEvent(ProfileEvent profileEvent, ISession session)
        {
             Write(session.Translation.GetTranslation(TranslationString.EventProfileLogin,
                profileEvent.Profile.PlayerData.Username ?? ""));
        }

        private void HandleEvent(ErrorEvent errorEvent, ISession session)
        {
             ErrorHappen = true;
            liveHappening = "Error";
             Write(errorEvent.ToString(), LogLevel.Error);
        }

        private void HandleEvent(NoticeEvent noticeEvent, ISession session)
        {
             Write(noticeEvent.ToString());
        }

        private void HandleEvent(WarnEvent warnEvent, ISession session)
        {
            liveHappening = "softban";
             Write(warnEvent.ToString(), LogLevel.Warning);
            // If the event requires no input return.
            if (!warnEvent.RequireInput) return;
        }

        private void HandleEvent(UseLuckyEggEvent useLuckyEggEvent, ISession session)
        {
            LuckyEggActive = true;
             Write(session.Translation.GetTranslation(TranslationString.EventUsedLuckyEgg, useLuckyEggEvent.Count),
                LogLevel.Egg);
        }

        private void HandleEvent(PokemonEvolveEvent pokemonEvolveEvent, ISession session)
        {
            liveHappening = "evolve";
            PokemonID = pokemonNameToId[pokemonEvolveEvent.Id.ToString()];
            string strPokemon = session.Translation.GetPokemonTranslation(pokemonEvolveEvent.Id);
             Write(pokemonEvolveEvent.Result == EvolvePokemonResponse.Types.Result.Success
                ? session.Translation.GetTranslation(TranslationString.EventPokemonEvolvedSuccess, strPokemon, pokemonEvolveEvent.Exp)
                : session.Translation.GetTranslation(TranslationString.EventPokemonEvolvedFailed, pokemonEvolveEvent.Id, pokemonEvolveEvent.Result,
                    strPokemon),
                LogLevel.Evolve);
        }

        private void HandleEvent(TransferPokemonEvent transferPokemonEvent, ISession session)
        {
            liveHappening = "transfer";
            PokemonID = pokemonNameToId[transferPokemonEvent.Id.ToString()];
            PokemonName = session.Translation.GetPokemonTranslation(transferPokemonEvent.Id);
             Write(
                session.Translation.GetTranslation(TranslationString.EventPokemonTransferred,
                session.Translation.GetPokemonTranslation(transferPokemonEvent.Id).PadRight(12, ' '),
                transferPokemonEvent.Cp.ToString().PadLeft(4, ' '),
                transferPokemonEvent.Perfection.ToString("0.00").PadLeft(6, ' '),
                transferPokemonEvent.BestCp.ToString().PadLeft(4, ' '),
                transferPokemonEvent.BestPerfection.ToString("0.00").PadLeft(6, ' '),
                transferPokemonEvent.FamilyCandies),
                LogLevel.Transfer);
        }

        private   void HandleEvent(ItemRecycledEvent itemRecycledEvent, ISession session)
        {
             Write(session.Translation.GetTranslation(TranslationString.EventItemRecycled, itemRecycledEvent.Count, itemRecycledEvent.Id),
                LogLevel.Recycling);
        }

        private   void HandleEvent(EggIncubatorStatusEvent eggIncubatorStatusEvent, ISession session)
        {
             Write(eggIncubatorStatusEvent.WasAddedNow
                ? session.Translation.GetTranslation(TranslationString.IncubatorPuttingEgg, eggIncubatorStatusEvent.KmRemaining)
                : session.Translation.GetTranslation(TranslationString.IncubatorStatusUpdate, eggIncubatorStatusEvent.KmRemaining),
                LogLevel.Egg);
        }

        private   void HandleEvent(EggHatchedEvent eggHatchedEvent, ISession session)
        {
            liveHappening = "hatched";
             Write(session.Translation.GetTranslation(TranslationString.IncubatorEggHatched,
                session.Translation.GetPokemonTranslation(eggHatchedEvent.PokemonId), eggHatchedEvent.Level, eggHatchedEvent.Cp, eggHatchedEvent.MaxCp, eggHatchedEvent.Perfection),
                LogLevel.Egg);
        }

        private   void HandleEvent(FortUsedEvent fortUsedEvent, ISession session)
        {
            liveHappening = "looting";
            coords[0] = fortUsedEvent.Latitude;
            coords[1] = fortUsedEvent.Longitude;
            var itemString = fortUsedEvent.InventoryFull
                ? session.Translation.GetTranslation(TranslationString.InvFullPokestopLooting)
                : fortUsedEvent.Items;
             Write(
                session.Translation.GetTranslation(TranslationString.EventFortUsed, fortUsedEvent.Name, fortUsedEvent.Exp, fortUsedEvent.Gems,
                    itemString, fortUsedEvent.Latitude, fortUsedEvent.Longitude),
                LogLevel.Pokestop);
        }

        private   void HandleEvent(FortFailedEvent fortFailedEvent, ISession session)
        {
            liveHappening = "lootingFailed";
            if (fortFailedEvent.Try != 1 && fortFailedEvent.Looted == false)
            {
                 
            }

            if (fortFailedEvent.Looted == true)
            {
                 Write(
                session.Translation.GetTranslation(TranslationString.SoftBanBypassed),
                LogLevel.SoftBan, ConsoleColor.Green);
            }
            else
            {
                 Write(
                session.Translation.GetTranslation(TranslationString.EventFortFailed, fortFailedEvent.Name, fortFailedEvent.Try, fortFailedEvent.Max),
                LogLevel.SoftBan);
            }
        }

        private   void HandleEvent(FortTargetEvent fortTargetEvent, ISession session)
        {
            liveHappening = "moving";
            int intTimeForArrival = (int)(fortTargetEvent.Distance / (session.LogicSettings.WalkingSpeedInKilometerPerHour * 0.5));

             Write(
                session.Translation.GetTranslation(TranslationString.EventFortTargeted, fortTargetEvent.Name,
                     Math.Round(fortTargetEvent.Distance), intTimeForArrival),
                LogLevel.Info, ConsoleColor.Gray);
        }

        private   void HandleEvent(PokemonCaptureEvent pokemonCaptureEvent, ISession session)
        {
            Func<ItemId, string> returnRealBallName = a =>
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (a)
                {
                    case ItemId.ItemPokeBall:
                        return session.Translation.GetTranslation(TranslationString.Pokeball);
                    case ItemId.ItemGreatBall:
                        return session.Translation.GetTranslation(TranslationString.GreatPokeball);
                    case ItemId.ItemUltraBall:
                        return session.Translation.GetTranslation(TranslationString.UltraPokeball);
                    case ItemId.ItemMasterBall:
                        return session.Translation.GetTranslation(TranslationString.MasterPokeball);
                    default:
                        return session.Translation.GetTranslation(TranslationString.CommonWordUnknown);
                }
            };
            PokemonID = pokemonNameToId[pokemonCaptureEvent.Id.ToString()];

            var catchType = pokemonCaptureEvent.CatchType;

            string strStatus;
            switch (pokemonCaptureEvent.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusError);
                    liveHappening = "fight";
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:
                    liveHappening = "fight";
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusEscape);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    liveHappening = "fight";
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusFlee);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusMissed);
                    liveHappening = "fight";
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    liveHappening = "caught";
                    strStatus = session.Translation.GetTranslation(TranslationString.CatchStatusSuccess);
                    break;
                default:
                    strStatus = pokemonCaptureEvent.Status.ToString();
                    break;
            }

            var catchStatus = pokemonCaptureEvent.Attempt > 1
                ? session.Translation.GetTranslation(TranslationString.CatchStatusAttempt, strStatus, pokemonCaptureEvent.Attempt)
                : session.Translation.GetTranslation(TranslationString.CatchStatus, strStatus);

            var familyCandies = pokemonCaptureEvent.FamilyCandies > 0
                ? session.Translation.GetTranslation(TranslationString.Candies, pokemonCaptureEvent.FamilyCandies)
                : "";

            string message;

            if (pokemonCaptureEvent.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
            {
                coords[0] = pokemonCaptureEvent.Latitude;
                coords[1] = pokemonCaptureEvent.Longitude;
                PokemonName = session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id);
                message = session.Translation.GetTranslation(TranslationString.EventPokemonCaptureSuccess, catchStatus, catchType, session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id),
                pokemonCaptureEvent.Level, pokemonCaptureEvent.Cp, pokemonCaptureEvent.MaxCp, pokemonCaptureEvent.Perfection.ToString("0.00"), pokemonCaptureEvent.Probability,
                pokemonCaptureEvent.Distance.ToString("F2"),
                returnRealBallName(pokemonCaptureEvent.Pokeball), pokemonCaptureEvent.BallAmount,
                pokemonCaptureEvent.Exp, familyCandies, pokemonCaptureEvent.Latitude.ToString("0.000000"), pokemonCaptureEvent.Longitude.ToString("0.000000"));
                 Write(message, LogLevel.Caught);
            }
            else
            {
                message = session.Translation.GetTranslation(TranslationString.EventPokemonCaptureFailed, catchStatus, catchType, session.Translation.GetPokemonTranslation(pokemonCaptureEvent.Id),
                pokemonCaptureEvent.Level, pokemonCaptureEvent.Cp, pokemonCaptureEvent.MaxCp, pokemonCaptureEvent.Perfection.ToString("0.00"), pokemonCaptureEvent.Probability,
                pokemonCaptureEvent.Distance.ToString("F2"),
                returnRealBallName(pokemonCaptureEvent.Pokeball), pokemonCaptureEvent.BallAmount,
                pokemonCaptureEvent.Latitude.ToString("0.000000"), pokemonCaptureEvent.Longitude.ToString("0.000000"));
                 Write(message, LogLevel.Flee);
            }

        }

        private   void HandleEvent(NoPokeballEvent noPokeballEvent, ISession session)
        {
             Write(session.Translation.GetTranslation(TranslationString.EventNoPokeballs, noPokeballEvent.Id, noPokeballEvent.Cp),
                LogLevel.Caught);
        }

        private   void HandleEvent(UseBerryEvent useBerryEvent, ISession session)
        {
            string strBerry;
            switch (useBerryEvent.BerryType)
            {
                case ItemId.ItemRazzBerry:
                    strBerry = session.Translation.GetTranslation(TranslationString.ItemRazzBerry);
                    break;
                default:
                    strBerry = useBerryEvent.BerryType.ToString();
                    break;
            }

             Write(session.Translation.GetTranslation(TranslationString.EventUseBerry, strBerry, useBerryEvent.Count),
                LogLevel.Berry);
        }

        private   void HandleEvent(SnipeEvent snipeEvent, ISession session)
        {
             Write(snipeEvent.ToString(), LogLevel.Sniper);
        }

        private void HandleEvent(SnipeScanEvent snipeScanEvent, ISession session)
        {
             Write(snipeScanEvent.PokemonId == PokemonId.Missingno
                ? ((snipeScanEvent.Source != null) ? "(" + snipeScanEvent.Source + ") " : null) + session.Translation.GetTranslation(TranslationString.SnipeScan,
                    $"{snipeScanEvent.Bounds.Latitude},{snipeScanEvent.Bounds.Longitude}")
                : ((snipeScanEvent.Source != null) ? "(" + snipeScanEvent.Source + ") " : null) + session.Translation.GetTranslation(TranslationString.SnipeScanEx, session.Translation.GetPokemonTranslation(snipeScanEvent.PokemonId),
                    snipeScanEvent.Iv > 0 ? snipeScanEvent.Iv.ToString(CultureInfo.InvariantCulture) : session.Translation.GetTranslation(TranslationString.CommonWordUnknown),
                    $"{snipeScanEvent.Bounds.Latitude},{snipeScanEvent.Bounds.Longitude}"), LogLevel.Sniper);
        }

        private   void HandleEvent(DisplayHighestsPokemonEvent displayHighestsPokemonEvent, ISession session)
        {
            string strHeader;
            //PokemonData | CP | IV | Level | MOVE1 | MOVE2 | Candy
            switch (displayHighestsPokemonEvent.SortedBy)
            {
                case "Level":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsLevelHeader);
                    break;
                case "IV":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsPerfectHeader);
                    break;
                case "CP":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsCpHeader);
                    break;
                case "MOVE1":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestMove1Header);
                    break;
                case "MOVE2":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestMove2Header);
                    break;
                case "Candy":
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestCandy);
                    break;
                default:
                    strHeader = session.Translation.GetTranslation(TranslationString.DisplayHighestsHeader);
                    break;
            }
            var strPerfect = session.Translation.GetTranslation(TranslationString.CommonWordPerfect);
            var strName = session.Translation.GetTranslation(TranslationString.CommonWordName).ToUpper();
            var move1 = session.Translation.GetTranslation(TranslationString.DisplayHighestMove1Header);
            var move2 = session.Translation.GetTranslation(TranslationString.DisplayHighestMove2Header);
            var candy = session.Translation.GetTranslation(TranslationString.DisplayHighestCandy);

             Write($"====== {strHeader} ======", LogLevel.Info, ConsoleColor.Yellow);
            foreach (var pokemon in displayHighestsPokemonEvent.PokemonList)
            {
                string strMove1 = session.Translation.GetPokemonMovesetTranslation(pokemon.Item5);
                string strMove2 = session.Translation.GetPokemonMovesetTranslation(pokemon.Item6);

                 Write(
                    $"# CP {pokemon.Item1.Cp.ToString().PadLeft(4, ' ')}/{pokemon.Item2.ToString().PadLeft(4, ' ')} | ({pokemon.Item3.ToString("0.00")}% {strPerfect})\t| Lvl {pokemon.Item4.ToString("00")}\t {strName}: {session.Translation.GetPokemonTranslation(pokemon.Item1.PokemonId).PadRight(10, ' ')}\t {move1}: {strMove1.PadRight(20, ' ')} {move2}: {strMove2.PadRight(20, ' ')} {candy}: {pokemon.Item7}",
                    LogLevel.Info, ConsoleColor.Yellow);
            }
        }

        private   void HandleEvent(EvolveCountEvent evolveCountEvent, ISession session)
        {
             Write(session.Translation.GetTranslation(TranslationString.PkmPotentialEvolveCount, evolveCountEvent.Evolves), LogLevel.Evolve);
        }

        private   void HandleEvent(UpdateEvent updateEvent, ISession session)
        {
             Write(updateEvent.ToString(), LogLevel.Update);
        }
    }
}
