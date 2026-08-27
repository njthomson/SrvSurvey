using Newtonsoft.Json.Linq;

// Behavioral references:
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/plugins/inara.py
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/monitor.py
// Copyright (c) EDCD, licensed under GNU GPL v2 or later.

namespace SrvSurvey.net
{
    /// <summary>
    /// Converts Elite journal events to the higher-level events accepted by Inara.
    /// The event shapes follow EDMarketConnector's Inara implementation and Inara's
    /// published API documentation; journal events are not forwarded verbatim.
    /// </summary>
    internal sealed class InaraEventMapper
    {
        private static readonly string[] materialCategories = ["Raw", "Manufactured", "Encoded"];
        private readonly Dictionary<string, int> cargo = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> materials = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> ranks = new(StringComparer.OrdinalIgnoreCase);
        private readonly InaraCreditTracker creditTracker = new();
        private bool hasCargoSnapshot;
        private bool hasMaterialsSnapshot;
        private bool sessionStarted;
        private string? sessionCommander;

        public bool InMulticrew { get; private set; }

        public void Reset()
        {
            cargo.Clear();
            materials.Clear();
            ranks.Clear();
            creditTracker.Reset();
            hasCargoSnapshot = false;
            hasMaterialsSnapshot = false;
            sessionStarted = false;
            sessionCommander = null;
            InMulticrew = false;
        }

        public IReadOnlyList<InaraEvent> Process(JObject entry, InaraContext context, bool collectEvents)
        {
            var name = entry.Value<string>("event");
            if (string.IsNullOrWhiteSpace(name))
                return [];

            if (name == "LoadGame")
                Reset();

            updateMulticrewState(name, entry);
            creditTracker.Observe(entry, InMulticrew);
            var inventoryChanged = updateInventoryState(name, entry);
            updateRankState(name, entry);

            if (!collectEvents || InMulticrew)
            {
                sessionStarted = false;
                return [];
            }

            var timestamp = entry.Value<string>("timestamp") ?? DateTime.UtcNow.ToString("O");
            var events = new List<InaraEvent>();
            var sessionStarting = !sessionStarted
                || !string.Equals(sessionCommander, context.Commander, StringComparison.OrdinalIgnoreCase);

            if (sessionStarting)
            {
                sessionStarted = true;
                sessionCommander = context.Commander;
                events.Add(new("getCommanderProfile", timestamp, new JObject(), "profile"));

                var ship = currentShip(context);
                if (ship != null)
                    events.Add(new("setCommanderShip", timestamp, ship, $"ship:{context.ShipId}"));

                addInventorySnapshots(events, timestamp, true, true);
            }

            mapEvent(name, timestamp, entry, context, events);
            addInventorySnapshots(events, timestamp, inventoryChanged.cargo, inventoryChanged.materials);

            // The common Statistics event supplies the authoritative assets value.
            // Otherwise, coalesce transaction deltas to Inara's recommended hourly
            // cadence and flush any remaining change at session shutdown.
            var forceCreditReport = sessionStarting
                || (name == "Statistics" && creditTracker.HasUnreportedChanges)
                || (name == "Shutdown" && creditTracker.HasUnreportedChanges);
            var creditReport = creditTracker.CreateReport(timestamp, forceCreditReport, name == "Statistics");
            if (creditReport != null) events.Add(creditReport);
            return events;
        }

        private void mapEvent(string name, string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            switch (name)
            {
                case "Progress":
                    mapProgress(timestamp, entry, events);
                    break;
                case "Promotion":
                    mapPromotion(timestamp, entry, events);
                    break;
                case "EngineerProgress":
                    mapEngineer(timestamp, entry, events);
                    break;
                case "Reputation":
                    mapMajorFactionReputation(timestamp, entry, events);
                    break;
                case "PowerplayJoin":
                    addRequired(events, "setCommanderRankPower", timestamp, obj(
                        ("powerName", entry["Power"]), ("rankValue", 1)), "power");
                    break;
                case "PowerplayLeave":
                    addRequired(events, "setCommanderRankPower", timestamp, obj(
                        ("powerName", entry["Power"]), ("rankValue", -1)), "power");
                    break;
                case "PowerplayDefect":
                    addRequired(events, "setCommanderRankPower", timestamp, obj(
                        ("powerName", entry["ToPower"]), ("rankValue", 1)), "power");
                    break;
                case "Powerplay":
                    addRequired(events, "setCommanderRankPower", timestamp, obj(
                        ("powerName", entry["Power"]), ("rankValue", entry["Rank"]), ("meritsValue", entry["Merits"])), "power");
                    break;
                case "PowerplayRank":
                    addRequired(events, "setCommanderRankPower", timestamp, obj(
                        ("powerName", entry["Power"]), ("rankValue", entry["Rank"])), "power");
                    break;
                case "Docked":
                    mapDocked(timestamp, entry, context, events);
                    break;
                case "FSDJump":
                    mapJump("addCommanderTravelFSDJump", timestamp, entry, context, events);
                    mapMinorFactionReputation(timestamp, entry, events);
                    break;
                case "CarrierJump":
                    mapJump("addCommanderTravelCarrierJump", timestamp, entry, context, events);
                    mapMinorFactionReputation(timestamp, entry, events);
                    break;
                case "Location":
                    mapLocation(timestamp, entry, events);
                    mapMinorFactionReputation(timestamp, entry, events);
                    break;
                case "SupercruiseExit":
                    mapSupercruiseExit(timestamp, entry, events);
                    break;
                case "ApproachSettlement":
                    mapSettlement(timestamp, entry, context, events);
                    break;
                case "DropshipDeploy":
                    addRequired(events, "addCommanderTravelLand", timestamp, obj(
                        ("starsystemName", entry["StarSystem"]),
                        ("starsystemBodyName", entry["Body"]),
                        ("isTaxiDropship", true)));
                    break;
                case "Touchdown":
                    mapTouchdown(timestamp, entry, context, events);
                    break;
                case "Statistics":
                    mapStatistics(timestamp, entry, events);
                    break;
                case "ShipyardNew":
                    addRequired(events, "addCommanderShip", timestamp, obj(
                        ("shipType", entry["ShipType"]),
                        ("shipGameID", entry["NewShipID"] ?? entry["ShipID"])));
                    break;
                case "ShipyardBuy":
                case "ShipyardSell":
                case "SellShipOnRebuy":
                case "ShipyardSwap":
                    mapShipyard(timestamp, entry, context, events);
                    break;
                case "SetUserShipName":
                    var namedShip = currentShip(context);
                    if (namedShip != null)
                        events.Add(new("setCommanderShip", timestamp, namedShip, $"ship:{context.ShipId}"));
                    break;
                case "ShipyardTransfer":
                    addRequired(events, "setCommanderShipTransfer", timestamp, obj(
                        ("shipType", entry["ShipType"]), ("shipGameID", entry["ShipID"]),
                        ("starsystemName", context.SystemName), ("stationName", context.StationName),
                        ("transferTime", entry["TransferTime"])), $"ship-transfer:{entry["ShipID"]}");
                    break;
                case "StoredShips":
                    mapStoredShips(timestamp, entry, events);
                    break;
                case "Loadout":
                    mapLoadout(timestamp, entry, context, events);
                    break;
                case "StoredModules":
                    mapStoredModules(timestamp, entry, events);
                    break;
                case "MissionAccepted":
                    mapMissionAccepted(timestamp, entry, context, events);
                    break;
                case "MissionAbandoned":
                    addRequired(events, "setCommanderMissionAbandoned", timestamp,
                        obj(("missionGameID", entry["MissionID"])), $"mission:{entry["MissionID"]}");
                    break;
                case "MissionCompleted":
                    mapMissionCompleted(timestamp, entry, events);
                    break;
                case "MissionFailed":
                    addRequired(events, "setCommanderMissionFailed", timestamp,
                        obj(("missionGameID", entry["MissionID"])), $"mission:{entry["MissionID"]}");
                    break;
                case "Died":
                case "Interdicted":
                case "Interdiction":
                case "EscapeInterdiction":
                case "PVPKill":
                    mapCombat(name, timestamp, entry, context, events);
                    break;
                case "ShipLocker":
                    mapShipLocker(timestamp, entry, events);
                    break;
                case "CreateSuitLoadout":
                case "SuitLoadout":
                    mapSuitLoadout("setCommanderSuitLoadout", timestamp, entry, events);
                    break;
                case "DeleteSuitLoadout":
                    addRequired(events, "delCommanderSuitLoadout", timestamp,
                        obj(("loadoutGameID", entry["LoadoutID"])), $"suit:{entry["LoadoutID"]}");
                    break;
                case "RenameSuitLoadout":
                    addRequired(events, "updateCommanderSuitLoadout", timestamp, obj(
                        ("loadoutGameID", entry["LoadoutID"]), ("loadoutName", entry["LoadoutName"]),
                        ("suitType", entry["SuitName"]), ("suitGameID", entry["SuitID"])), $"suit:{entry["LoadoutID"]}");
                    break;
                case "LoadoutEquipModule":
                    mapSuitModule(timestamp, entry, events);
                    break;
                case "CommunityGoal":
                    mapCommunityGoals(timestamp, entry, events);
                    break;
                case "Friends":
                    mapFriend(timestamp, entry, events);
                    break;
            }
        }

        private void updateMulticrewState(string name, JObject entry)
        {
            if (name == "QuitACrew")
                InMulticrew = false;
            else if (name is "JoinACrew" or "ChangeCrewRole")
                InMulticrew = true;
            else if (entry["Multicrew"]?.Type == JTokenType.Boolean && entry["Multicrew"]?.Value<bool?>() == true)
                InMulticrew = true;
            else if (name == "LoadGame")
                InMulticrew = false;
        }

        private (bool cargo, bool materials) updateInventoryState(string name, JObject entry)
        {
            var cargoChanged = false;
            var materialsChanged = false;

            if (name == "Cargo"
                && entry.Value<string>("Vessel") == "Ship"
                && entry["Inventory"] is JArray inventory)
            {
                cargo.Clear();
                foreach (var item in inventory.OfType<JObject>())
                    setCount(cargo, item.Value<string>("Name"), item.Value<int?>("Count") ?? 0);
                hasCargoSnapshot = true;
                cargoChanged = true;
            }
            else if (hasCargoSnapshot)
            {
                cargoChanged = updateCargoDelta(name, entry);
            }

            if (name == "Materials")
            {
                materials.Clear();
                foreach (var category in materialCategories)
                {
                    if (entry[category] is not JArray items) continue;
                    foreach (var item in items.OfType<JObject>())
                        setCount(materials, item.Value<string>("Name"), item.Value<int?>("Count") ?? 0);
                }
                hasMaterialsSnapshot = true;
                materialsChanged = true;
            }
            else if (hasMaterialsSnapshot)
            {
                materialsChanged = updateMaterialDelta(name, entry);
            }

            return (cargoChanged, materialsChanged);
        }

        private void updateRankState(string name, JObject entry)
        {
            if (name != "Rank") return;
            foreach (var property in entry.Properties().Where(p => p.Name is not "timestamp" and not "event"))
                if (property.Value.Type == JTokenType.Integer)
                    ranks[property.Name] = property.Value.Value<int>();
        }

        private bool updateCargoDelta(string name, JObject entry)
        {
            switch (name)
            {
                case "CollectCargo":
                case "MarketBuy":
                case "BuyDrones":
                case "MiningRefined":
                    return changeCount(cargo, itemName(entry), itemCount(entry, 1));
                case "EjectCargo":
                case "MarketSell":
                case "SellDrones":
                    return changeCount(cargo, itemName(entry), -itemCount(entry, 1));
                case "CargoTransfer":
                    var transferred = false;
                    foreach (var item in (entry["Transfers"] as JArray)?.OfType<JObject>() ?? [])
                    {
                        var direction = item.Value<string>("Direction");
                        var amount = itemCount(item, 0);
                        transferred |= changeCount(cargo, itemName(item), direction == "toship" ? amount : -amount);
                    }
                    return transferred;
                case "SearchAndRescue":
                    var changed = false;
                    foreach (var item in (entry["Items"] as JArray)?.OfType<JObject>() ?? [])
                        changed |= changeCount(cargo, itemName(item), -itemCount(item, 1));
                    return changed;
                case "MissionCompleted":
                    return changeMany(cargo, entry["CommodityReward"] as JArray, 1);
                case "EngineerContribution":
                    return changeCount(cargo, entry.Value<string>("Commodity"), -entry.Value<int?>("Quantity") ?? 0);
                case "TechnologyBroker":
                    var brokerCargoChanged = changeMany(cargo, entry["Ingredients"] as JArray, -1);
                    return changeMany(cargo, entry["Commodities"] as JArray, -1) || brokerCargoChanged;
                default:
                    return false;
            }
        }

        private bool updateMaterialDelta(string name, JObject entry)
        {
            switch (name)
            {
                case "MaterialCollected":
                    return changeCount(materials, itemName(entry), itemCount(entry, 1));
                case "MaterialDiscarded":
                case "ScientificResearch":
                    return changeCount(materials, itemName(entry), -itemCount(entry, 1));
                case "Synthesis":
                    return changeMany(materials, entry["Materials"] as JArray, -1);
                case "EngineerCraft":
                case "EngineerLegacyConvert":
                    if (entry.Value<bool?>("IsPreview") == true) return false;
                    return changeMany(materials, entry["Ingredients"] as JArray, -1);
                case "MaterialTrade":
                    var changed = changeItem(materials, entry["Paid"] as JObject, -1);
                    return changeItem(materials, entry["Received"] as JObject, 1) || changed;
                case "TechnologyBroker":
                    var brokerMaterialsChanged = changeMany(materials, entry["Ingredients"] as JArray, -1);
                    return changeMany(materials, entry["Materials"] as JArray, -1) || brokerMaterialsChanged;
                case "MissionCompleted":
                    return changeMany(materials, entry["MaterialsReward"] as JArray, 1);
                case "EngineerContribution":
                    return changeCount(materials, entry.Value<string>("Material"), -entry.Value<int?>("Quantity") ?? 0);
                default:
                    return false;
            }
        }

        private void addInventorySnapshots(List<InaraEvent> events, string timestamp, bool cargoChanged, bool materialsChanged)
        {
            if (cargoChanged && hasCargoSnapshot)
            {
                var data = new JArray(cargo.OrderBy(item => item.Key).Select(item => obj(
                    ("itemName", item.Key), ("itemCount", item.Value))));
                events.Add(new("setCommanderInventoryCargo", timestamp, data, "inventory:cargo"));
            }

            if (materialsChanged && hasMaterialsSnapshot)
            {
                var data = new JArray(materials.OrderBy(item => item.Key).Select(item => obj(
                    ("itemName", item.Key), ("itemCount", item.Value))));
                events.Add(new("setCommanderInventoryMaterials", timestamp, data, "inventory:materials"));
            }
        }

        private void mapProgress(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var values = new JArray();
            foreach (var property in entry.Properties().Where(p =>
                p.Name is not "timestamp" and not "event" && p.Value.Type == JTokenType.Integer))
            {
                var rankName = normalizeRank(property.Name);
                var data = obj(("rankName", rankName), ("rankProgress", property.Value.Value<double>() / 100d));
                if (ranks.TryGetValue(property.Name, out var rank)) data["rankValue"] = rank;
                values.Add(data);
            }
            if (values.Count > 0) events.Add(new("setCommanderRankPilot", timestamp, values, "ranks"));
        }

        private void mapPromotion(string timestamp, JObject entry, List<InaraEvent> events)
        {
            foreach (var property in entry.Properties().Where(p =>
                p.Name is not "timestamp" and not "event" && p.Value.Type == JTokenType.Integer))
            {
                var value = property.Value.Value<int>();
                ranks[property.Name] = value;
                events.Add(new("setCommanderRankPilot", timestamp, obj(
                    ("rankName", normalizeRank(property.Name)), ("rankValue", value), ("rankProgress", 0d)),
                    $"rank:{property.Name.ToLowerInvariant()}"));
            }
        }

        private static void mapEngineer(string timestamp, JObject entry, List<InaraEvent> events)
        {
            if (entry["Engineer"] == null) return;
            addRequired(events, "setCommanderRankEngineer", timestamp, obj(
                ("engineerName", entry["Engineer"]), ("rankValue", entry["Rank"]),
                ("rankStage", entry["Progress"])), $"engineer:{entry["Engineer"]}");
        }

        private static void mapMajorFactionReputation(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var data = new JArray(entry.Properties()
                .Where(property => property.Name is not "timestamp" and not "event" && property.Value.Type is JTokenType.Integer or JTokenType.Float)
                .Select(property => obj(
                    ("majorfactionName", property.Name.ToLowerInvariant()),
                    ("majorfactionReputation", property.Value.Value<double>() / 100d))));
            if (data.Count > 0)
                events.Add(new("setCommanderReputationMajorFaction", timestamp, data, "reputation:major"));
        }

        private static void mapDocked(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var data = obj(
                ("starsystemName", entry["StarSystem"] ?? context.SystemName),
                ("stationName", entry["StationName"] ?? context.StationName),
                ("marketID", entry["MarketID"]));
            addShipIdentity(data, context, entry.Value<bool?>("Taxi") ?? context.IsTaxi);
            addRequired(events, "addCommanderTravelDock", timestamp, data);
        }

        private static void mapJump(string eventName, string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var data = obj(
                ("starsystemName", entry["StarSystem"]),
                ("starsystemCoords", entry["StarPos"]),
                ("jumpDistance", entry["JumpDist"]),
                ("stationName", entry["StationName"]),
                ("marketID", entry["MarketID"]));
            addShipIdentity(data, context, entry.Value<bool?>("Taxi") ?? context.IsTaxi);
            addRequired(events, eventName, timestamp, data);
        }

        private static void mapLocation(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var data = obj(
                ("starsystemName", entry["StarSystem"]),
                ("starsystemCoords", entry["StarPos"]));
            if (entry.Value<bool?>("Docked") == true)
            {
                copy(data, entry, ("stationName", "StationName"), ("marketID", "MarketID"));
                if (entry.Value<string>("BodyType") == "Planet") copy(data, entry, ("starsystemBodyName", "Body"));
            }
            if (entry["Latitude"] != null && entry["Longitude"] != null)
            {
                copy(data, entry, ("starsystemBodyName", "Body"));
                data["starsystemBodyCoords"] = new JArray(entry["Latitude"]!.DeepClone(), entry["Longitude"]!.DeepClone());
            }
            addRequired(events, "setCommanderTravelLocation", timestamp, data, "location");
        }

        private static void mapSupercruiseExit(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var data = obj(("starsystemName", entry["StarSystem"]));
            if (entry.Value<string>("BodyType") == "Planet") copy(data, entry, ("starsystemBodyName", "Body"));
            addRequired(events, "setCommanderTravelLocation", timestamp, data, "location");
        }

        private static void mapSettlement(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var data = obj(
                ("starsystemName", entry["StarSystem"] ?? context.SystemName),
                ("stationName", entry["Name"]),
                ("starsystemBodyName", entry["BodyName"]),
                ("marketID", entry["MarketID"]));
            if (entry["Latitude"] != null && entry["Longitude"] != null)
                data["starsystemBodyCoords"] = new JArray(entry["Latitude"]!.DeepClone(), entry["Longitude"]!.DeepClone());
            addRequired(events, "setCommanderTravelLocation", timestamp, data, "location");
        }

        private static void mapTouchdown(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            if (entry.Value<bool?>("PlayerControlled") == false || entry.Value<bool?>("OnPlanet") == false) return;
            var data = obj(
                ("starsystemName", entry["StarSystem"] ?? context.SystemName),
                ("starsystemBodyName", entry["Body"] ?? context.BodyName));
            if (entry["Latitude"] != null && entry["Longitude"] != null)
                data["starsystemBodyCoords"] = new JArray(entry["Latitude"]!.DeepClone(), entry["Longitude"]!.DeepClone());
            addShipIdentity(data, context, entry.Value<bool?>("Taxi") ?? context.IsTaxi);
            addRequired(events, "addCommanderTravelLand", timestamp, data);
        }

        private static void mapMinorFactionReputation(string timestamp, JObject entry, List<InaraEvent> events)
        {
            if (entry["Factions"] is not JArray factions) return;
            var data = new JArray(factions.OfType<JObject>()
                .Where(f => f["Name"] != null && f["MyReputation"] != null)
                .Select(f => obj(
                    ("minorfactionName", f["Name"]),
                    ("minorfactionReputation", f.Value<double>("MyReputation") / 100d))));
            if (data.Count > 0) events.Add(new("setCommanderReputationMinorFaction", timestamp, data, "reputation:minor"));
        }

        private static void mapStatistics(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var stats = (JObject)entry.DeepClone();
            stats.Remove("timestamp");
            stats.Remove("event");
            if (stats.Count > 0) events.Add(new("setCommanderGameStatistics", timestamp, stats, "statistics"));
        }

        private static void mapShipyard(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            if (entry["StoreShipID"] != null)
                addRequired(events, "setCommanderShip", timestamp, obj(
                    ("shipType", entry["StoreOldShip"]), ("shipGameID", entry["StoreShipID"]),
                    ("starsystemName", context.SystemName), ("stationName", context.StationName)), $"ship:{entry["StoreShipID"]}");
            if (entry["SellShipID"] != null)
                addRequired(events, "delCommanderShip", timestamp, obj(
                    ("shipType", entry["SellOldShip"] ?? entry["ShipType"]),
                    ("shipGameID", entry["SellShipID"])), $"ship:{entry["SellShipID"]}");
        }

        private static void mapStoredShips(string timestamp, JObject entry, List<InaraEvent> events)
        {
            foreach (var ship in (entry["ShipsHere"] as JArray)?.OfType<JObject>() ?? [])
            {
                addRequired(events, "setCommanderShip", timestamp, obj(
                    ("shipType", ship["ShipType"]), ("shipGameID", ship["ShipID"]),
                    ("shipName", ship["Name"]), ("isHot", ship["Hot"]),
                    ("starsystemName", entry["StarSystem"]), ("stationName", entry["StationName"]),
                    ("marketID", entry["MarketID"])), $"ship:{ship["ShipID"]}");
            }
            foreach (var ship in (entry["ShipsRemote"] as JArray)?.OfType<JObject>() ?? [])
            {
                addRequired(events, "setCommanderShip", timestamp, obj(
                    ("shipType", ship["ShipType"]), ("shipGameID", ship["ShipID"]),
                    ("shipName", ship["Name"]), ("isHot", ship["Hot"]),
                    ("starsystemName", ship["StarSystem"]), ("marketID", ship["ShipMarketID"])),
                    $"ship:{ship["ShipID"]}");
            }
        }

        private static void mapLoadout(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var shipType = entry["Ship"] ?? context.ShipType;
            var shipId = entry["ShipID"] ?? context.ShipId;
            var modules = new JArray();
            foreach (var module in (entry["Modules"] as JArray)?.OfType<JObject>() ?? [])
                modules.Add(mapModule(module));

            addRequired(events, "setCommanderShipLoadout", timestamp, obj(
                ("shipType", shipType), ("shipGameID", shipId), ("shipLoadout", modules)), $"loadout:{shipId}");

            var ship = obj(
                ("shipType", shipType), ("shipGameID", shipId),
                ("shipName", entry["ShipName"] ?? context.ShipName),
                ("shipIdent", entry["ShipIdent"] ?? entry["ShipIDent"] ?? context.ShipIdent),
                ("isCurrentShip", true), ("shipMaxJumpRange", entry["MaxJumpRange"]),
                ("shipCargoCapacity", entry["CargoCapacity"]), ("shipHullValue", entry["HullValue"]),
                ("shipModulesValue", entry["ModulesValue"]), ("shipRebuyCost", entry["Rebuy"]));
            addRequired(events, "setCommanderShip", timestamp, ship, $"ship:{shipId}");
        }

        private static JObject mapModule(JObject module)
        {
            var data = obj(
                ("slotName", module["Slot"]), ("itemName", module["Item"]),
                ("itemHealth", module["Health"]), ("isOn", module["On"]),
                ("itemPriority", module["Priority"]), ("itemAmmoClip", module["AmmoInClip"]),
                ("itemAmmoHopper", module["AmmoInHopper"]), ("itemValue", module["Value"]),
                ("isHot", module["Hot"]));
            if (module["Engineering"] is JObject engineering)
            {
                var mapped = obj(
                    ("blueprintName", engineering["BlueprintName"]),
                    ("blueprintLevel", engineering["Level"]),
                    ("blueprintQuality", engineering["Quality"]),
                    ("experimentalEffect", engineering["ExperimentalEffect"]));
                if (engineering["Modifiers"] is JArray modifiers)
                {
                    mapped["modifiers"] = new JArray(modifiers.OfType<JObject>().Select(modifier => obj(
                        ("name", modifier["Label"]), ("value", modifier["Value"] ?? modifier["ValueStr"]),
                        ("originalValue", modifier["OriginalValue"]), ("lessIsGood", modifier["LessIsGood"]))));
                }
                data["engineering"] = mapped;
            }
            return data;
        }

        private static void mapStoredModules(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var modules = new JArray();
            var storedModules = (entry["Items"] as JArray)?.OfType<JObject>() ?? [];
            foreach (var item in storedModules.OrderBy(i => i.Value<int?>("StorageSlot")))
            {
                var module = obj(
                    ("itemName", item["Name"]), ("itemValue", item["BuyPrice"]), ("isHot", item["Hot"]),
                    ("starsystemName", item["StarSystem"]), ("marketID", item["MarketID"]));
                if (item["EngineerModifications"] != null)
                    module["engineering"] = obj(
                        ("blueprintName", item["EngineerModifications"]),
                        ("blueprintLevel", item["Level"]), ("blueprintQuality", item["Quality"]));
                modules.Add(module);
            }
            events.Add(new("setCommanderStorageModules", timestamp, modules, "stored-modules"));
        }

        private static void mapMissionAccepted(string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var data = obj(
                ("missionName", entry["Name"]), ("missionGameID", entry["MissionID"]),
                ("influenceGain", entry["Influence"]), ("reputationGain", entry["Reputation"]),
                ("starsystemNameOrigin", context.SystemName), ("stationNameOrigin", context.StationName),
                ("minorfactionNameOrigin", entry["Faction"]));
            copy(data, entry,
                ("missionExpiry", "Expiry"), ("starsystemNameTarget", "DestinationSystem"),
                ("stationNameTarget", "DestinationStation"), ("minorfactionNameTarget", "TargetFaction"),
                ("commodityName", "Commodity"), ("commodityCount", "Count"), ("targetName", "Target"),
                ("targetType", "TargetType"), ("killCount", "KillCount"), ("passengerType", "PassengerType"),
                ("passengerCount", "PassengerCount"), ("passengerIsVIP", "PassengerVIPs"),
                ("passengerIsWanted", "PassengerWanted"));
            addRequired(events, "addCommanderMission", timestamp, data, $"mission:{entry["MissionID"]}");
        }

        private static void mapMissionCompleted(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var data = obj(
                ("missionGameID", entry["MissionID"]), ("donationCredits", entry["Donation"]),
                ("rewardCredits", entry["Reward"]));
            if (entry["PermitsAwarded"] is JArray permits)
            {
                data["rewardPermits"] = new JArray(permits.Select(permit => obj(("starsystemName", permit))));
                foreach (var permit in permits)
                    events.Add(new("addCommanderPermit", timestamp, obj(("starsystemName", permit))));
            }
            if (entry["CommodityReward"] is JArray commodities)
                data["rewardCommodities"] = mapRewards(commodities);
            if (entry["MaterialsReward"] is JArray materialsReward)
                data["rewardMaterials"] = mapRewards(materialsReward);

            if (entry["FactionEffects"] is JArray factionEffects)
            {
                var effects = new JArray();
                foreach (var faction in factionEffects.OfType<JObject>())
                {
                    var effect = obj(
                        ("minorfactionName", faction["Faction"]),
                        ("reputationGain", faction["Reputation"]));
                    var influence = (faction["Influence"] as JArray)?.OfType<JObject>()
                        .Select(value => value.Value<string>("Influence"))
                        .Where(value => value != null)
                        .OrderByDescending(value => value!.Length)
                        .FirstOrDefault();
                    if (influence != null) effect["influenceGain"] = influence;
                    effects.Add(effect);
                }
                if (effects.Count > 0) data["minorfactionEffects"] = effects;
            }
            addRequired(events, "setCommanderMissionCompleted", timestamp, data, $"mission:{entry["MissionID"]}");
        }

        private static JArray mapRewards(JArray rewards) => new(rewards.OfType<JObject>().Select(item => obj(
            ("itemName", item["Name"]), ("itemCount", item["Count"]))));

        private static void mapCombat(string name, string timestamp, JObject entry, InaraContext context, List<InaraEvent> events)
        {
            var data = obj(("starsystemName", entry["StarSystem"] ?? context.SystemName));
            string eventName;
            switch (name)
            {
                case "Died":
                    eventName = "addCommanderCombatDeath";
                    if (entry["Killers"] is JArray killers)
                        data["wingOpponentNames"] = new JArray(killers.OfType<JObject>().Select(k => k["Name"]?.DeepClone()));
                    else
                        data["opponentName"] = entry["KillerName"] ?? entry["KillerShip"];
                    break;
                case "Interdicted":
                    eventName = "addCommanderCombatInterdicted";
                    copy(data, entry, ("isPlayer", "IsPlayer"), ("isSubmit", "Submitted"));
                    data["opponentName"] = opponent(entry, "Interdictor");
                    break;
                case "Interdiction":
                    eventName = "addCommanderCombatInterdiction";
                    copy(data, entry, ("isPlayer", "IsPlayer"), ("isSuccess", "Success"));
                    data["opponentName"] = opponent(entry, "Interdicted");
                    break;
                case "EscapeInterdiction":
                    eventName = "addCommanderCombatInterdictionEscape";
                    copy(data, entry, ("isPlayer", "IsPlayer"));
                    data["opponentName"] = opponent(entry, "Interdictor");
                    break;
                default:
                    eventName = "addCommanderCombatKill";
                    data["opponentName"] = entry["Victim"];
                    break;
            }

            var hasOpponent = !string.IsNullOrWhiteSpace(data.Value<string>("opponentName"))
                || data["wingOpponentNames"] is JArray { Count: > 0 };
            if (hasOpponent) events.Add(new(eventName, timestamp, data));
        }

        private static JToken? opponent(JObject entry, string primary)
        {
            if (entry[primary] != null) return entry[primary]!.DeepClone();
            if (entry["Faction"] != null) return entry["Faction"]!.DeepClone();
            if (entry["Power"] != null) return entry["Power"]!.DeepClone();
            if (entry.Value<bool?>("IsThargoid") == true || entry.Value<bool?>("isThargoid") == true) return "Thargoid";
            return null;
        }

        private static void mapShipLocker(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var types = new[] { "Items", "Components", "Data", "Consumables" };
            if (types.Any(type => entry[type] is not JArray)) return;

            events.Add(new("resetCommanderInventory", timestamp,
                new JArray(types.Select(type => obj(("itemType", type)))), "locker:reset"));
            var data = new JArray();
            foreach (var type in types)
            {
                foreach (var item in ((JArray)entry[type]!).OfType<JObject>())
                    data.Add(obj(("itemName", item["Name"]), ("itemCount", item["Count"]),
                        ("itemType", type), ("itemLocation", "ShipLocker")));
            }
            events.Add(new("setCommanderInventory", timestamp, data, "locker:items"));
        }

        private static void mapSuitLoadout(string eventName, string timestamp, JObject entry, List<InaraEvent> events)
        {
            var modules = new JArray();
            foreach (var module in (entry["Modules"] as JArray)?.OfType<JObject>() ?? [])
            {
                modules.Add(obj(
                    ("slotName", module["SlotName"]), ("itemName", module["ModuleName"]),
                    ("itemClass", module["Class"]), ("itemGameID", module["SuitModuleID"]),
                    ("engineering", new JArray((module["WeaponMods"] as JArray)?.Select(mod => obj(("blueprintName", mod))) ?? []))));
            }
            addRequired(events, eventName, timestamp, obj(
                ("loadoutGameID", entry["LoadoutID"]), ("loadoutName", entry["LoadoutName"]),
                ("suitGameID", entry["SuitID"]), ("suitType", entry["SuitName"]),
                ("suitMods", entry["SuitMods"]), ("suitLoadout", modules)), $"suit:{entry["LoadoutID"]}");
        }

        private static void mapSuitModule(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var module = obj(
                ("slotName", entry["SlotName"]), ("itemName", entry["ModuleName"]),
                ("itemClass", entry["Class"]), ("itemGameID", entry["SuitModuleID"]),
                ("engineering", new JArray((entry["WeaponMods"] as JArray)?.Select(mod => obj(("blueprintName", mod))) ?? [])));
            addRequired(events, "updateCommanderSuitLoadout", timestamp, obj(
                ("loadoutGameID", entry["LoadoutID"]), ("loadoutName", entry["LoadoutName"]),
                ("suitGameID", entry["SuitID"]), ("suitType", entry["SuitName"]),
                ("suitLoadout", new JArray(module))), $"suit:{entry["LoadoutID"]}");
        }

        private static void mapCommunityGoals(string timestamp, JObject entry, List<InaraEvent> events)
        {
            foreach (var goal in (entry["CurrentGoals"] as JArray)?.OfType<JObject>() ?? [])
            {
                var id = goal["CGID"];
                var data = obj(
                    ("communitygoalGameID", id), ("communitygoalName", goal["Title"]),
                    ("starsystemName", goal["SystemName"]), ("stationName", goal["MarketName"]),
                    ("goalExpiry", goal["Expiry"]), ("isCompleted", goal["IsComplete"]),
                    ("contributorsNum", goal["NumContributors"]), ("contributionsTotal", goal["CurrentTotal"]),
                    ("topRankSize", goal["TopRankSize"]));
                events.Add(new("setCommunityGoal", timestamp, data, $"community-goal:{id}"));

                var progress = obj(
                    ("communitygoalGameID", id), ("contribution", goal["PlayerContribution"]),
                    ("percentileBand", goal["PlayerPercentileBand"]),
                    ("percentileBandReward", goal["Bonus"]), ("isTopRank", goal["PlayerInTopRank"]));
                events.Add(new("setCommanderCommunityGoalProgress", timestamp, progress, $"community-progress:{id}"));
            }
        }

        private static void mapFriend(string timestamp, JObject entry, List<InaraEvent> events)
        {
            var status = entry.Value<string>("Status");
            var eventName = status is "Added" or "Online" ? "addCommanderFriend"
                : status is "Declined" or "Lost" ? "delCommanderFriend" : null;
            if (eventName != null)
                addRequired(events, eventName, timestamp, obj(
                    ("commanderName", entry["Name"]), ("gamePlatform", "pc")), $"friend:{entry["Name"]}");
        }

        private static JObject? currentShip(InaraContext context)
        {
            if (context.ShipId is null or < 0 || string.IsNullOrWhiteSpace(context.ShipType)) return null;
            return obj(
                ("shipType", context.ShipType), ("shipGameID", context.ShipId),
                ("shipName", context.ShipName), ("shipIdent", context.ShipIdent),
                ("isCurrentShip", true));
        }

        private static void addShipIdentity(JObject data, InaraContext context, bool? isTaxi)
        {
            if (isTaxi == true)
            {
                data["isTaxiShuttle"] = true;
                return;
            }

            // Shared Status.json cannot be associated with a commander while multi-boxing.
            // An unknown taxi state must not claim that a travel event used the commander's ship.
            if (isTaxi == null) return;

            if (!string.IsNullOrWhiteSpace(context.ShipType)) data["shipType"] = context.ShipType;
            if (context.ShipId is >= 0) data["shipGameID"] = context.ShipId;
        }

        private static void addRequired(List<InaraEvent> events, string name, string timestamp, JObject data, string? replaceKey = null)
        {
            if (data.Properties().Any(property => property.Value.Type is not JTokenType.Null and not JTokenType.Undefined))
                events.Add(new(name, timestamp, data, replaceKey));
        }

        private static JObject obj(params (string name, object? value)[] properties)
        {
            var result = new JObject();
            foreach (var (name, value) in properties)
            {
                if (value == null) continue;
                if (value is JToken token)
                {
                    if (token.Type is not JTokenType.Null and not JTokenType.Undefined)
                        result[name] = token.DeepClone();
                }
                else
                    result[name] = JToken.FromObject(value);
            }
            return result;
        }

        private static void copy(JObject target, JObject source, params (string target, string source)[] properties)
        {
            foreach (var (targetName, sourceName) in properties)
                if (source[sourceName] is JToken value && value.Type is not JTokenType.Null and not JTokenType.Undefined)
                    target[targetName] = value.DeepClone();
        }

        private static string normalizeRank(string rank) => rank.Equals("Exploration", StringComparison.OrdinalIgnoreCase)
            ? "explore"
            : rank.ToLowerInvariant();

        private static string? itemName(JObject entry) =>
            entry.Value<string>("Type") ?? entry.Value<string>("Name") ?? entry.Value<string>("Material") ?? entry.Value<string>("Commodity");

        private static int itemCount(JObject entry, int fallback) =>
            entry.Value<int?>("Count") ?? entry.Value<int?>("Amount") ?? entry.Value<int?>("Quantity") ?? fallback;

        private static void setCount(Dictionary<string, int> inventory, string? name, int count)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (count <= 0) inventory.Remove(name);
            else inventory[name] = count;
        }

        private static bool changeCount(Dictionary<string, int> inventory, string? name, int delta)
        {
            if (string.IsNullOrWhiteSpace(name) || delta == 0) return false;
            var oldCount = inventory.GetValueOrDefault(name);
            var newCount = Math.Max(0, oldCount + delta);
            if (oldCount == newCount) return false;
            setCount(inventory, name, newCount);
            return true;
        }

        private static bool changeItem(Dictionary<string, int> inventory, JObject? item, int direction)
        {
            if (item == null) return false;
            return changeCount(inventory, itemName(item), direction * itemCount(item, 1));
        }

        private static bool changeMany(Dictionary<string, int> inventory, JArray? items, int direction)
        {
            var changed = false;
            foreach (var item in items?.OfType<JObject>() ?? [])
                changed |= changeItem(inventory, item, direction);
            return changed;
        }
    }
}
