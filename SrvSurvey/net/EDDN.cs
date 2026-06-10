using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Text;

// https://github.com/EDCD/EDDN/blob/live/README.md

namespace SrvSurvey.net
{
    /// <summary> For uploading to EDDN </summary>
    internal class EDDN
    {
        public static UploadPayloadHeader? header;
        private static HttpClient client;
        private static string useEnv = "dev";
        private static bool logAllUploads;
        private static Dictionary<string, string> urls = new()
        {
            { "dev", "https://dev.eddn.edcd.io:4432/upload/" },
            { "beta", "https://beta.eddn.edcd.io:4431/upload/" },
            { "live", "https://eddn.edcd.io:4430/upload/" }
        };

        static EDDN()
        {
            client = new HttpClient(); // Not yet --> Util.getResilienceHandler());
            client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        private async Task upload(JObject message, string schemaRef)
        {
            if (!Game.settings.eddnUpload || EDDN.header == null) return;

            if (logAllUploads)
            {
                Game.log($"Send to EDDN: {message.Value<string>("event")}\r\n" + JsonConvert.SerializeObject(new JObject
                {
                    ["$schemaRef"] = schemaRef,
                    ["header"] = JObject.FromObject(EDDN.header!),
                    ["message"] = message,
                }, Formatting.Indented));
            }

            var payload = JsonConvert.SerializeObject(new JObject
            {
                ["$schemaRef"] = schemaRef,
                ["header"] = JObject.FromObject(EDDN.header!),
                ["message"] = message,
            });
            var url = urls[Game.settings.eddnEnvironment ?? useEnv];
            if (url != urls["live"]) schemaRef += "/test";

            if (DateTime.Now.Year > 3000)
            {
                var response = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));

                Game.log($"EDDN upload response: {response.StatusCode} : {response.ReasonPhrase}");

                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    Game.log($"EDDN.upload: failed: payload:\r\n{payload}\r\nbody:\r\n{body}");
            }
        }

        private void trim(JObject obj, params List<string> names)
        {
            foreach (var name in names)
            {
                if (name.StartsWith("*"))
                {
                    // remove anything ending with the given name
                    foreach (var x in obj.Properties().ToList())
                        if (x.Name.EndsWith(name.Substring(1)))
                            obj.Remove(x.Name);
                }
                else
                {
                    obj.Remove(name);
                }
            }

            // recurse as needed
            foreach (var val in obj.Values())
            {
                if (val.Type == JTokenType.Object)
                {
                    trim((JObject)val, names);
                }
                else if (val.Type == JTokenType.Array)
                {
                    foreach (var item in (JArray)val)
                        if (item.Type == JTokenType.Object)
                            trim((JObject)item, names);
                }
            }
        }

        public void onJournalEntry(Game game, IJournalEntry entry, JObject raw) { /* ignore */ }

        public void onJournalEntry(Game game, CodexEntry _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim (BodyID will be put back below if conditions are met)
            trim(message, "*_Localised", nameof(CodexEntry.BodyID), nameof(CodexEntry.IsNewEntry), nameof(CodexEntry.NewTraitsDiscovered));

            // augment
            message["StarSystem"] = raw.Value<string>("System");
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            // Only set body name/ID if status.json has a BodyName and it matches the body we are tracking ...
            if (game.status.BodyName != null && game.systemBody != null && game.status.BodyName == game.systemBody.name)
            {
                message["BodyName"] = game.status.BodyName;

                // Set BodyID only if it matches our tracked body and that name matches status.json
                if (raw.Value<int>("BodyID") == game.systemBody.id)
                    message["BodyID"] = game.systemBody.id;
            }

            upload(message, "https://eddn.edcd.io/schemas/codexentry/1").justDoIt();
        }

        public void onJournalEntry(Game game, ApproachSettlement _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised");

            // augment
            message["StarSystem"] = game.systemData.name;
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/approachsettlement/1").justDoIt();
        }

        public void onJournalEntry(Game game, Market _, JObject raw)
        {
            if (raw.Value<string>("StarSystem") != game.systemData?.name || game.journals == null) return;
            var marketFile = game.marketFile;
            if (marketFile.Items.Count == 0) return;

            // TODO: ...

            //// serialize market.json
            //var message = new JObject()
            //{
            //    { "systemName", marketFile.StarSystem },
            //    { "stationName", marketFile.StationName },
            //    { "MarketId", game.marketFile.MarketId  },
            //    { "StationType", game.marketFile.StationType },
            //    { "CarrierDockingAccess", game.marketFile.CarrierDockingAccess },
            //};

            //// trim
            //trim(message, "*_Localised", nameof(MarketFile.StationType), nameof(MarketFile.Item.Producer), nameof(MarketFile.Item.Rare), nameof(MarketFile.Item.id));
            //// Skip commodities with "categoryname": "NonMarketable" (i.e. Limpets - not purchasable in station market) or a non-empty"legality": string (not normally traded at this station market).
            //var trimmedItems = ((JArray)message[nameof(MarketFile.Items)]!).Where(x => x.Value<string>(nameof(MarketFile.Item.Category)).Contains("NonMarketable") && );
            //message[nameof(MarketFile.Items)] = new JArray(trimmedItems);

            //upload(message, "https://eddn.edcd.io/schemas/commodity/3").justDoIt();
        }

        public void onJournalEntry(Game game, DockingGranted _, JObject raw)
        {
            if (game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/dockinggranted/1").justDoIt();
        }

        public void onJournalEntry(Game game, DockingDenied _, JObject raw)
        {
            if (game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/dockingdenied/1").justDoIt();
        }

        public void onJournalEntry(Game game, FSSAllBodiesFound _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/fssallbodiesfound/1").justDoIt();
        }

        public void onJournalEntry(Game game, FSSBodySignals _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised");

            // augment
            message["StarSystem"] = game.systemData.name;
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/fssbodysignals/1").justDoIt();
        }

        public void onJournalEntry(Game game, FSSDiscoveryScan _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised", nameof(FSSDiscoveryScan.Progress));

            // augment
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/fssdiscoveryscan/1").justDoIt();
        }

        public void onJournalEntry(Game game, FSSSignalDiscovered entry, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // TODO: ... batching ...

            //// serialize
            //raw.Value<long>("SystemAddress")

            //// trim
            //trim(message, "*_Localised", nameof(FSSDiscoveryScan.Progress));

            //// augment
            //message["StarPos"] = new JArray(game.systemData.starPos);
            //if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            //if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            //upload(message, "https://eddn.edcd.io/schemas/fsssignaldiscovered/1").justDoIt();
        }

        public void onJournalEntry(Game game, NavBeaconScan _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            message["StarSystem"] = game.systemData.name;
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/navbeaconscan/1").justDoIt();
        }

        public void onJournalEntry(Game game, NavRoute entry, JObject raw)
        {
            if (game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/navroute/1").justDoIt();
        }

        // TODO: Outfitting ?

        // TODO: Shipyard ?

        public void onJournalEntry(Game game, ScanBaryCentre _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // augment
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/scanbarycentre/1").justDoIt();
        }


        // The following use the same schemaRef

        public void onJournalEntry(Game game, Docked _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised", nameof(Docked.Wanted), nameof(Docked.ActiveFine), nameof(Docked.CockpitBreach)); // StationEconomyKeys?

            // augment
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }

        public void onJournalEntry(Game game, FSDJump entry, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised", "Wanted", nameof(FSDJump.BoostUsed), nameof(FSDJump.FuelLevel), nameof(FSDJump.FuelUsed), nameof(FSDJump.JumpDist), "HappiestSystem", "HomeSystem", nameof(SystemFaction.MyReputation), "SquadronFaction");

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }

        public void onJournalEntry(Game game, CarrierJump _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised", "Wanted", nameof(FSDJump.BoostUsed), nameof(FSDJump.FuelLevel), nameof(FSDJump.FuelUsed), nameof(FSDJump.JumpDist), "HappiestSystem", "HomeSystem", nameof(SystemFaction.MyReputation), "SquadronFaction");

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }

        public void onJournalEntry(Game game, Scan _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised");

            // augment
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }

        public void onJournalEntry(Game game, Location _, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised", "Wanted", nameof(Location.Latitude), nameof(Location.Longitude), "HappiestSystem", "HomeSystem", nameof(SystemFaction.MyReputation), "SquadronFaction");

            // augment
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }

        public void onJournalEntry(Game game, SAASignalsFound entry, JObject raw)
        {
            if (raw.Value<long>("SystemAddress") != game.systemData?.address || game.journals == null) return;

            // serialize
            var message = new JObject(raw);

            // trim
            trim(message, "*_Localised");

            // augment
            message["StarSystem"] = game.systemData.name;
            message["StarPos"] = new JArray(game.systemData.starPos);
            if (game.journals.isGameOdyssey.HasValue) message["odyssey"] = game.journals.isGameOdyssey.Value;
            if (game.journals.isGameHorizons.HasValue) message["horizons"] = game.journals.isGameHorizons.Value;

            upload(message, "https://eddn.edcd.io/schemas/journal/1").justDoIt();
        }
    }

    class UploadPayloadHeader
    {
        public string uploaderID;
        public string softwareName;
        public string softwareVersion;
        public string gameVersion;
        public string gamebuild;

        public UploadPayloadHeader(string uploaderID, string gameVersion, string gameBuild)
        {
            this.uploaderID = uploaderID;
            this.gameVersion = gameVersion;
            this.gamebuild = gameBuild;

            this.softwareName = "SrvSurvey";
            this.softwareVersion = Program.releaseVersion;
        }
    }
}
