using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SrvSurvey.net
{
    internal sealed record EddnLocationContext(
        string systemName,
        long systemAddress,
        double[] starPosition);

    internal sealed record EddnMessageContext(
        EddnLocationContext? location,
        bool? horizons,
        bool? odyssey,
        string? statusBodyName = null,
        string? trackedBodyName = null,
        int? trackedBodyId = null,
        string? trackedBodyType = null);

    internal sealed record EddnPreparedMessage(
        string eventName,
        string schemaRef,
        JObject message);

    /// <summary>
    /// Builds schema-specific EDDN messages from journal events and the
    /// companion JSON files written by Elite Dangerous.
    /// </summary>
    internal static class EddnMessageSanitizer
    {
        private const string schemaRoot = "https://eddn.edcd.io/schemas/";
        private const string horizonsSku = "ELITE_HORIZONS_V_PLANETARY_LANDINGS";

        private static readonly HashSet<string> genericEvents = new(StringComparer.Ordinal)
        {
            "Docked",
            "FSDJump",
            "CarrierJump",
            "Scan",
            "Location",
            "SAASignalsFound",
        };

        private static readonly HashSet<string> companionEvents = new(StringComparer.Ordinal)
        {
            "Market",
            "Outfitting",
            "Shipyard",
            "FCMaterials",
            "NavRoute",
        };

        private static readonly Regex canonicalCommodityName = new(
            @"^\$(.+)_name;$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex moduleName = new(
            @"^Hpt_|^Int_|Armour_",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        internal static bool isCompanionEvent(string? eventName)
        {
            return eventName != null && companionEvents.Contains(eventName);
        }

        internal static EddnLocationContext? getLocation(JObject raw)
        {
            ArgumentNullException.ThrowIfNull(raw);

            var eventName = raw.Value<string>("event");
            if (eventName is not ("Location" or "FSDJump" or "CarrierJump"))
                return null;

            var systemName = raw.Value<string>("StarSystem");
            var systemAddress = raw.Value<long?>("SystemAddress");
            var position = raw["StarPos"] as JArray;
            if (string.IsNullOrWhiteSpace(systemName)
                || systemAddress is not > 0
                || position?.Count != 3
                || position.Any(value => value.Type is not (JTokenType.Float or JTokenType.Integer)))
            {
                return null;
            }

            return new EddnLocationContext(
                systemName,
                systemAddress.Value,
                position.Values<double>().ToArray());
        }

        internal static bool tryBuildJournal(
            JObject raw,
            EddnMessageContext context,
            out EddnPreparedMessage? prepared,
            out string reason)
        {
            ArgumentNullException.ThrowIfNull(raw);
            ArgumentNullException.ThrowIfNull(context);

            prepared = null;
            var eventName = raw.Value<string>("event");
            if (string.IsNullOrWhiteSpace(eventName))
            {
                reason = "the journal event name was missing";
                return false;
            }

            if (genericEvents.Contains(eventName))
                return tryBuildGenericJournal(raw, context, out prepared, out reason);

            JObject? message;
            string schema;
            switch (eventName)
            {
                case "CodexEntry":
                    schema = "codexentry/1";
                    if (!hasMatchingLocation(raw, context, "System"))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "System", "SystemAddress", "EntryID", "Name",
                        "Region", "Category", "Latitude", "Longitude", "SubCategory",
                        "NearestDestination", "VoucherAmount", "Traits", "BodyID", "BodyName");
                    message["StarPos"] = position(context.location!);
                    addFlags(message, context);

                    var bodyNamesAgree = !string.IsNullOrWhiteSpace(context.statusBodyName)
                        && string.Equals(
                            context.statusBodyName,
                            context.trackedBodyName,
                            StringComparison.Ordinal);
                    if (!message.ContainsKey("BodyName")
                        && bodyNamesAgree
                        && (!message.ContainsKey("BodyID")
                            || message.Value<int?>("BodyID") == context.trackedBodyId))
                    {
                        message["BodyName"] = context.statusBodyName;
                    }
                    if (!message.ContainsKey("BodyID")
                        && context.trackedBodyId.HasValue
                        && bodyNamesAgree
                        && (!message.ContainsKey("BodyName")
                            || string.Equals(
                                message.Value<string>("BodyName"),
                                context.statusBodyName,
                                StringComparison.Ordinal)))
                    {
                        message["BodyID"] = context.trackedBodyId.Value;
                    }
                    break;

                case "ApproachSettlement":
                    schema = "approachsettlement/1";
                    if (!hasMatchingLocation(raw, context))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "SystemAddress", "Name", "MarketID", "BodyID",
                        "BodyName", "Latitude", "Longitude", "StationGovernment",
                        "StationAllegiance", "StationEconomies", "StationFaction",
                        "StationServices", "StationEconomy");
                    message["StarSystem"] = context.location!.systemName;
                    message["StarPos"] = position(context.location);
                    addFlags(message, context);
                    break;

                case "DockingDenied":
                    schema = "dockingdenied/1";
                    message = select(raw,
                        "timestamp", "event", "MarketID", "StationName", "StationType", "Reason");
                    addFlags(message, context);
                    break;

                case "DockingGranted":
                    schema = "dockinggranted/1";
                    message = select(raw,
                        "timestamp", "event", "MarketID", "StationName", "StationType", "LandingPad");
                    addFlags(message, context);
                    break;

                case "FSSAllBodiesFound":
                    schema = "fssallbodiesfound/1";
                    if (!hasMatchingLocation(raw, context, "SystemName"))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "SystemName", "SystemAddress", "Count");
                    message["StarPos"] = position(context.location!);
                    addFlags(message, context);
                    break;

                case "FSSBodySignals":
                    schema = "fssbodysignals/1";
                    if (!hasMatchingLocation(raw, context))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "SystemAddress", "BodyID", "BodyName", "Signals");
                    message["StarSystem"] = context.location!.systemName;
                    message["StarPos"] = position(context.location);
                    addFlags(message, context);
                    break;

                case "FSSDiscoveryScan":
                    schema = "fssdiscoveryscan/1";
                    if (!hasMatchingLocation(raw, context, "SystemName"))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "SystemName", "SystemAddress", "BodyCount", "NonBodyCount");
                    message["StarPos"] = position(context.location!);
                    addFlags(message, context);
                    break;

                case "NavBeaconScan":
                    schema = "navbeaconscan/1";
                    if (!hasMatchingLocation(raw, context))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "SystemAddress", "NumBodies");
                    message["StarSystem"] = context.location!.systemName;
                    message["StarPos"] = position(context.location);
                    addFlags(message, context);
                    break;

                case "ScanBaryCentre":
                    schema = "scanbarycentre/1";
                    if (!hasMatchingLocation(raw, context, "StarSystem"))
                        return fail("the event did not match the tracked system", out prepared, out reason);
                    message = select(raw,
                        "timestamp", "event", "StarSystem", "SystemAddress", "BodyID",
                        "SemiMajorAxis", "Eccentricity", "OrbitalInclination", "Periapsis",
                        "OrbitalPeriod", "AscendingNode", "MeanAnomaly");
                    message["StarPos"] = position(context.location!);
                    addFlags(message, context);
                    break;

                default:
                    reason = "the event has no EDDN schema supported by SrvSurvey";
                    return false;
            }

            removeLocalised(message);
            removeNulls(message);
            if (eventName == "CodexEntry"
                && !hasValidCodexStrings(message, out reason))
            {
                return false;
            }
            if (!hasRequiredFields(message, eventName, out reason))
                return false;

            prepared = new EddnPreparedMessage(eventName, schemaRoot + schema, message);
            reason = string.Empty;
            return true;
        }

        internal static bool tryBuildCompanion(
            JObject companion,
            EddnMessageContext context,
            out EddnPreparedMessage? prepared,
            out string reason)
        {
            ArgumentNullException.ThrowIfNull(companion);
            ArgumentNullException.ThrowIfNull(context);

            prepared = null;
            var eventName = companion.Value<string>("event");
            JObject message;
            string schema;
            switch (eventName)
            {
                case "Market":
                    schema = "commodity/3";
                    message = buildCommodity(companion, context);
                    break;

                case "Outfitting":
                    schema = "outfitting/2";
                    message = buildOutfitting(companion, context);
                    break;

                case "Shipyard":
                    schema = "shipyard/2";
                    message = buildShipyard(companion, context);
                    break;

                case "FCMaterials":
                    schema = "fcmaterials_journal/1";
                    message = buildFleetCarrierMaterials(companion, context);
                    break;

                case "NavRoute":
                    schema = "navroute/1";
                    message = buildNavRoute(companion, context);
                    break;

                default:
                    reason = "the companion file event is not supported by EDDN";
                    return false;
            }

            removeNulls(message);
            if (!hasRequiredFields(message, eventName, out reason))
                return false;

            prepared = new EddnPreparedMessage(eventName, schemaRoot + schema, message);
            reason = string.Empty;
            return true;
        }

        internal static bool tryBuildSignalBatch(
            IReadOnlyList<JObject> pendingSignals,
            EddnLocationContext? location,
            bool? horizons,
            bool? odyssey,
            out EddnPreparedMessage? prepared,
            out string reason)
        {
            ArgumentNullException.ThrowIfNull(pendingSignals);
            prepared = null;
            if (pendingSignals.Count == 0)
            {
                reason = "the signal batch was empty";
                return false;
            }
            if (location == null)
            {
                reason = "the system location for the signal batch was unknown";
                return false;
            }

            var signals = new JArray();
            foreach (var raw in pendingSignals)
            {
                if (raw.Value<long?>("SystemAddress") != location.systemAddress
                    || raw.Value<string>("USSType") == "$USS_Type_MissionTarget;")
                {
                    continue;
                }

                var signal = select(raw,
                    "timestamp", "SignalName", "SignalType", "IsStation", "USSType",
                    "SpawningState", "SpawningFaction", "SpawningPower", "OpposingPower",
                    "ThreatLevel");
                removeLocalised(signal);
                if (hasValue(signal, "timestamp") && hasValue(signal, "SignalName"))
                    signals.Add(signal);
            }

            if (signals.Count == 0)
            {
                reason = "no public signals remained after filtering";
                return false;
            }

            var message = new JObject
            {
                ["event"] = "FSSSignalDiscovered",
                ["timestamp"] = signals[0]!["timestamp"]!.DeepClone(),
                ["SystemAddress"] = location.systemAddress,
                ["StarSystem"] = location.systemName,
                ["StarPos"] = position(location),
                ["signals"] = signals,
            };
            addFlags(message, new EddnMessageContext(location, horizons, odyssey));
            removeNulls(message);

            prepared = new EddnPreparedMessage(
                "FSSSignalDiscovered",
                schemaRoot + "fsssignaldiscovered/1",
                message);
            reason = string.Empty;
            return true;
        }

        private static bool tryBuildGenericJournal(
            JObject raw,
            EddnMessageContext context,
            out EddnPreparedMessage? prepared,
            out string reason)
        {
            prepared = null;
            var eventName = raw.Value<string>("event")!;
            if (!hasMatchingLocation(raw, context,
                    eventName is "FSDJump" or "CarrierJump" or "Location" or "Docked" or "Scan"
                        ? "StarSystem"
                        : null))
            {
                reason = "the event did not match the tracked system";
                return false;
            }

            var message = new JObject(raw);
            removeLocalised(message);
            switch (eventName)
            {
                case "Docked":
                    remove(message, "Wanted", "ActiveFine", "CockpitBreach");
                    if (!message.ContainsKey("Body")
                        && context.trackedBodyType == "Planet"
                        && !string.IsNullOrWhiteSpace(context.trackedBodyName))
                    {
                        message["Body"] = context.trackedBodyName;
                        message["BodyType"] = "Planet";
                    }
                    break;

                case "FSDJump":
                case "CarrierJump":
                    remove(message,
                        "Wanted", "BoostUsed", "FuelLevel", "FuelUsed", "JumpDist");
                    removeFactionPersonalData(message);
                    break;

                case "Location":
                    remove(message, "Wanted", "Latitude", "Longitude");
                    removeFactionPersonalData(message);
                    break;
            }

            message["StarSystem"] ??= context.location!.systemName;
            message["StarPos"] ??= position(context.location!);
            addFlags(message, context);
            removeNulls(message);
            if (!hasRequiredFields(message, eventName, out reason))
                return false;

            prepared = new EddnPreparedMessage(
                eventName,
                schemaRoot + "journal/1",
                message);
            reason = string.Empty;
            return true;
        }

        private static JObject buildCommodity(JObject source, EddnMessageContext context)
        {
            var commodities = new List<JObject>();
            foreach (var item in source["Items"] as JArray ?? [])
            {
                if (item is not JObject commodity
                    || commodity.Value<string>("Category")?.Contains(
                        "NonMarketable",
                        StringComparison.OrdinalIgnoreCase) == true
                    || !string.IsNullOrWhiteSpace(commodity.Value<string>("Legality")))
                {
                    continue;
                }

                var name = canonicalCommodity(commodity.Value<string>("Name"));
                if (string.IsNullOrWhiteSpace(name)) continue;
                var output = new JObject
                {
                    ["name"] = name,
                    ["meanPrice"] = commodity.Value<int?>("MeanPrice"),
                    ["buyPrice"] = commodity.Value<int?>("BuyPrice"),
                    ["stock"] = commodity.Value<int?>("Stock"),
                    ["stockBracket"] = commodity["StockBracket"]?.DeepClone(),
                    ["sellPrice"] = commodity.Value<int?>("SellPrice"),
                    ["demand"] = commodity.Value<int?>("Demand"),
                    ["demandBracket"] = commodity["DemandBracket"]?.DeepClone(),
                };
                if (new[]
                    {
                        "name", "meanPrice", "buyPrice", "stock", "stockBracket",
                        "sellPrice", "demand", "demandBracket",
                    }.Any(field => !hasValue(output, field)))
                {
                    continue;
                }
                var statusFlags = new JArray();
                foreach (var flag in new[] { "Producer", "Consumer", "Rare" })
                    if (commodity.Value<bool?>(flag) == true) statusFlags.Add(flag);
                if (statusFlags.Count > 0) output["statusFlags"] = statusFlags;
                commodities.Add(output);
            }

            var sorted = new JArray(commodities.OrderBy(
                item => item.Value<string>("name"),
                StringComparer.Ordinal));
            var message = new JObject
            {
                ["systemName"] = source.Value<string>("StarSystem"),
                ["stationName"] = source.Value<string>("StationName"),
                ["stationType"] = source.Value<string>("StationType"),
                ["marketId"] = source.Value<long?>("MarketID"),
                ["timestamp"] = source["timestamp"]?.DeepClone(),
                ["commodities"] = sorted,
            };
            var access = source.Value<string>("CarrierDockingAccess");
            if (!string.IsNullOrWhiteSpace(access))
                message["carrierDockingAccess"] = access;
            addFlags(message, context);
            removeNulls(message);
            return message;
        }

        private static JObject buildOutfitting(JObject source, EddnMessageContext context)
        {
            var modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source["Items"] as JArray ?? [])
            {
                if (item is not JObject module) continue;
                var name = module.Value<string>("Name");
                var sku = module.Value<string>("sku") ?? module.Value<string>("SKU");
                if (string.IsNullOrWhiteSpace(name)
                    || !moduleName.IsMatch(name)
                    || name.Equals("Int_PlanetApproachSuite", StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(sku)
                        && !sku.Equals(horizonsSku, StringComparison.Ordinal)))
                {
                    continue;
                }
                modules.Add(normalizeModuleName(name));
            }

            var message = new JObject
            {
                ["systemName"] = source.Value<string>("StarSystem"),
                ["stationName"] = source.Value<string>("StationName"),
                ["marketId"] = source.Value<long?>("MarketID"),
                ["timestamp"] = source["timestamp"]?.DeepClone(),
                ["modules"] = new JArray(modules.OrderBy(value => value, StringComparer.Ordinal)),
            };
            addFlags(message, context with
            {
                horizons = source.Value<bool?>("Horizons") ?? context.horizons,
            });
            removeNulls(message);
            return message;
        }

        private static JObject buildShipyard(JObject source, EddnMessageContext context)
        {
            var ships = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source["PriceList"] as JArray ?? [])
            {
                if (item is not JObject ship) continue;
                var type = ship.Value<string>("ShipType");
                if (!string.IsNullOrWhiteSpace(type)) ships.Add(type);
            }

            var message = new JObject
            {
                ["systemName"] = source.Value<string>("StarSystem"),
                ["stationName"] = source.Value<string>("StationName"),
                ["marketId"] = source.Value<long?>("MarketID"),
                ["timestamp"] = source["timestamp"]?.DeepClone(),
                ["ships"] = new JArray(ships.OrderBy(value => value, StringComparer.Ordinal)),
            };
            if (source.Value<bool?>("AllowCobraMkIV") is bool allowCobra)
                message["allowCobraMkIV"] = allowCobra;
            addFlags(message, context with
            {
                horizons = source.Value<bool?>("Horizons") ?? context.horizons,
            });
            removeNulls(message);
            return message;
        }

        private static JObject buildFleetCarrierMaterials(
            JObject source,
            EddnMessageContext context)
        {
            var items = new JArray();
            foreach (var item in source["Items"] as JArray ?? [])
            {
                if (item is not JObject material) continue;
                var output = select(material, "id", "Name", "Price", "Stock", "Demand");
                if (hasValue(output, "id")
                    && hasValue(output, "Name")
                    && hasValue(output, "Price")
                    && hasValue(output, "Stock")
                    && hasValue(output, "Demand"))
                {
                    items.Add(output);
                }
            }

            var message = select(
                source,
                "timestamp", "event", "MarketID", "CarrierName", "CarrierID");
            message["Items"] = items;
            addFlags(message, context);
            return message;
        }

        private static JObject buildNavRoute(JObject source, EddnMessageContext context)
        {
            var route = new JArray();
            foreach (var item in source["Route"] as JArray ?? [])
            {
                if (item is not JObject waypoint) continue;
                var output = select(
                    waypoint,
                    "StarSystem", "SystemAddress", "StarPos", "StarClass");
                if (new[] { "StarSystem", "SystemAddress", "StarPos", "StarClass" }
                    .All(field => hasValue(output, field)))
                {
                    route.Add(output);
                }
            }

            var message = select(source, "timestamp", "event");
            message["Route"] = route;
            addFlags(message, context);
            return message;
        }

        private static bool hasMatchingLocation(
            JObject raw,
            EddnMessageContext context,
            string? systemNameField = null)
        {
            var location = context.location;
            if (location == null
                || raw.Value<long?>("SystemAddress") != location.systemAddress)
            {
                return false;
            }

            if (systemNameField == null) return true;
            var eventName = raw.Value<string>(systemNameField);
            return !string.IsNullOrWhiteSpace(eventName)
                && eventName.Equals(location.systemName, StringComparison.Ordinal);
        }

        private static bool hasRequiredFields(
            JObject message,
            string eventName,
            out string reason)
        {
            string[] required = eventName switch
            {
                "CodexEntry" => ["timestamp", "event", "System", "StarPos", "SystemAddress", "EntryID"],
                "ApproachSettlement" => ["timestamp", "event", "StarSystem", "StarPos", "SystemAddress", "Name", "BodyID", "BodyName", "Latitude", "Longitude"],
                "DockingDenied" => ["timestamp", "event", "MarketID", "StationName", "Reason"],
                "DockingGranted" => ["timestamp", "event", "MarketID", "StationName"],
                "FSSAllBodiesFound" => ["timestamp", "event", "SystemName", "StarPos", "SystemAddress", "Count"],
                "FSSBodySignals" => ["timestamp", "event", "StarSystem", "StarPos", "SystemAddress", "BodyID", "Signals"],
                "FSSDiscoveryScan" => ["timestamp", "event", "SystemName", "StarPos", "SystemAddress", "BodyCount", "NonBodyCount"],
                "NavBeaconScan" => ["timestamp", "event", "StarSystem", "StarPos", "SystemAddress", "NumBodies"],
                "ScanBaryCentre" => ["timestamp", "event", "StarSystem", "StarPos", "SystemAddress", "BodyID"],
                "Market" => ["systemName", "stationName", "marketId", "timestamp", "commodities"],
                "Outfitting" => ["systemName", "stationName", "marketId", "timestamp", "modules"],
                "Shipyard" => ["systemName", "stationName", "marketId", "timestamp", "ships"],
                "FCMaterials" => ["timestamp", "event", "MarketID", "CarrierName", "CarrierID", "Items"],
                "NavRoute" => ["timestamp", "event", "Route"],
                _ when genericEvents.Contains(eventName) => ["timestamp", "event", "StarSystem", "StarPos", "SystemAddress"],
                _ => [],
            };

            var missing = required.Where(name => !hasValue(message, name)).ToArray();
            if (missing.Length > 0)
            {
                reason = "required field(s) were missing: " + string.Join(", ", missing);
                return false;
            }

            if (eventName is "Outfitting" or "Shipyard"
                && message[required.Last()] is JArray array
                && array.Count == 0)
            {
                reason = $"{required.Last()} was empty";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static bool hasValidCodexStrings(JObject message, out string reason)
        {
            foreach (var field in new[] { "System", "Name", "Region", "Category", "SubCategory" })
            {
                if (message[field]?.Type == JTokenType.String
                    && string.IsNullOrWhiteSpace(message.Value<string>(field)))
                {
                    reason = $"{field} was empty";
                    return false;
                }
            }

            if (message["Traits"] is JArray traits
                && traits.Any(value => value.Type != JTokenType.String
                    || string.IsNullOrWhiteSpace(value.Value<string>())))
            {
                reason = "Traits contained an empty or non-string value";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static bool hasValue(JObject obj, string name)
        {
            var value = obj[name];
            return value != null
                && value.Type != JTokenType.Null
                && (value.Type != JTokenType.String
                    || !string.IsNullOrWhiteSpace(value.Value<string>()));
        }

        private static JObject select(JObject source, params string[] names)
        {
            var result = new JObject();
            foreach (var name in names)
                if (source[name] != null) result[name] = source[name]!.DeepClone();
            removeLocalised(result);
            return result;
        }

        private static void addFlags(JObject message, EddnMessageContext context)
        {
            if (context.horizons.HasValue) message["horizons"] = context.horizons.Value;
            if (context.odyssey.HasValue) message["odyssey"] = context.odyssey.Value;
        }

        private static JArray position(EddnLocationContext location)
        {
            return new JArray(location.starPosition);
        }

        private static string? canonicalCommodity(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var match = canonicalCommodityName.Match(value);
            return match.Success ? match.Groups[1].Value : value;
        }

        private static string normalizeModuleName(string value)
        {
            return moduleName.Replace(value, match =>
            {
                var lower = match.Value.ToLowerInvariant();
                return char.ToUpperInvariant(lower[0]) + lower[1..];
            });
        }

        private static void removeFactionPersonalData(JObject message)
        {
            if (message["Factions"] is not JArray factions) return;
            foreach (var faction in factions.OfType<JObject>())
                remove(faction,
                    "HappiestSystem", "HomeSystem", "MyReputation", "SquadronFaction");
        }

        private static void remove(JObject message, params string[] names)
        {
            foreach (var name in names) message.Remove(name);
        }

        private static void removeNulls(JToken token)
        {
            if (token is JObject message)
            {
                foreach (var property in message.Properties().ToArray())
                {
                    if (property.Value.Type == JTokenType.Null)
                        property.Remove();
                    else
                        removeNulls(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array.ToArray())
                {
                    if (item.Type == JTokenType.Null)
                        item.Remove();
                    else
                        removeNulls(item);
                }
            }
        }

        private static void removeLocalised(JToken? token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToArray())
                {
                    if (property.Name.EndsWith("_Localised", StringComparison.Ordinal))
                        property.Remove();
                    else
                        removeLocalised(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array) removeLocalised(item);
            }
        }

        private static bool fail(
            string failure,
            out EddnPreparedMessage? prepared,
            out string reason)
        {
            prepared = null;
            reason = failure;
            return false;
        }
    }
}
