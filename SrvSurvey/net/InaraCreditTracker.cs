using Newtonsoft.Json.Linq;

// Behavioral references:
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/monitor.py
// https://github.com/EDCD/EDMarketConnector/blob/2b6a0ce1ee3ba60c21f3f4e9fa093046da8825e4/plugins/inara.py
// Copyright (c) EDCD, licensed under GNU GPL v2 or later.
// API guidance: https://inara.cz/elite/inara-api-docs/

namespace SrvSurvey.net
{
    /// <summary>
    /// Maintains the best journal-derived commander balance available between
    /// authoritative LoadGame snapshots. Inara recommends reporting credits at
    /// session boundaries, on significant changes, or at hourly intervals rather
    /// than recording every transaction in the commander's credits log.
    /// </summary>
    internal sealed class InaraCreditTracker
    {
        internal static readonly TimeSpan ReportInterval = TimeSpan.FromHours(1);

        private long? credits;
        private long? loan;
        private long? assets;
        private DateTimeOffset? lastReportAt;

        public bool HasUnreportedChanges { get; private set; }

        public void Reset()
        {
            credits = null;
            loan = null;
            assets = null;
            lastReportAt = null;
            HasUnreportedChanges = false;
        }

        public void Observe(JObject entry, bool inMulticrew)
        {
            var eventName = entry.Value<string>("event");
            if (eventName == "LoadGame")
            {
                Reset();
                credits = value(entry, "Credits");
                loan = value(entry, "Loan");
                HasUnreportedChanges = credits.HasValue;
                return;
            }

            // Journal activity while serving on somebody else's ship must not alter
            // the tracked balance for the local commander.
            if (inMulticrew) return;

            if (eventName == "Statistics")
            {
                var currentAssets = value(entry["Bank_Account"] as JObject, "Current_Wealth");
                if (currentAssets.HasValue && currentAssets != assets)
                {
                    assets = currentAssets;
                    HasUnreportedChanges = true;
                }
                return;
            }

            if (eventName == "CarrierBankTransfer" && value(entry, "PlayerBalance") is long playerBalance)
            {
                if (playerBalance != credits)
                {
                    credits = playerBalance;
                    HasUnreportedChanges = true;
                }
                return;
            }

            if (!credits.HasValue) return;

            var delta = eventName switch
            {
                "ShipyardBuy" => -valueOrZero(entry, "ShipPrice"),
                "ModuleBuy" => -valueOrZero(entry, "BuyPrice"),
                "ModuleRetrieve" or "ModuleStore" => -valueOrZero(entry, "Cost"),
                "ModuleSell" or "ModuleSellRemote" => valueOrZero(entry, "SellPrice"),

                "BuyMicroResources" or "BuySuit" or "BuyWeapon" => -valueOrZero(entry, "Price"),
                "SellMicroResources" or "SellSuit" or "SellWeapon" => valueOrZero(entry, "Price"),
                "UpgradeSuit" or "UpgradeWeapon" => -valueOrZero(entry, "Cost"),
                "SellOrganicData" => organicDataValue(entry),
                "BookDropship" or "BookTaxi" => -valueOrZero(entry, "Cost"),
                "CancelDropship" or "CancelTaxi" => valueOrZero(entry, "Refund"),

                "BuyDrones" or "MarketBuy" => -valueOrZero(entry, "TotalCost"),
                "MarketSell" or "SellDrones" => valueOrZero(entry, "TotalSale"),
                "MissionCompleted" => valueOrZero(entry, "Reward") - valueOrZero(entry, "Donation"),
                "CommunityGoalReward" => valueOrZero(entry, "Reward"),
                "MultiSellExplorationData" or "SellExplorationData" => valueOrZero(entry, "TotalEarnings"),
                "BuyExplorationData" or "BuyTradeData" or "BuyAmmo" or "CrewHire" => -valueOrZero(entry, "Cost"),
                "FetchRemoteModule" => -valueOrZero(entry, "TransferCost"),
                "PayBounties" or "PayFines" or "PayLegacyFines" => -valueOrZero(entry, "Amount"),
                "RedeemVoucher" or "PowerplaySalary" => valueOrZero(entry, "Amount"),
                "RefuelAll" or "RefuelPartial" or "Repair" or "RepairAll" or "RestockVehicle" => -valueOrZero(entry, "Cost"),
                "SellShipOnRebuy" or "ShipyardSell" => valueOrZero(entry, "ShipPrice"),
                "ShipyardTransfer" => -valueOrZero(entry, "TransferPrice"),
                "PowerplayFastTrack" => -valueOrZero(entry, "Cost"),
                "CarrierBuy" => -valueOrZero(entry, "Price"),
                "NpcCrewPaidWage" => -valueOrZero(entry, "Amount"),
                "Resurrect" => -valueOrZero(entry, "Cost"),
                _ => 0,
            };

            if (delta != 0)
            {
                var updatedCredits = credits.Value + delta;
                if (updatedCredits < 0)
                {
                    // A missing or malformed journal delta means the reconstructed
                    // balance is no longer trustworthy. Wait for the next exact
                    // LoadGame or CarrierBankTransfer value instead of uploading it.
                    credits = null;
                    HasUnreportedChanges = false;
                    return;
                }

                credits = updatedCredits;
                HasUnreportedChanges = true;
            }
        }

        public InaraEvent? CreateReport(string timestamp, bool force, bool includeAssets = false)
        {
            if (!credits.HasValue) return null;

            var reportAt = parseTimestamp(timestamp);
            if (!force)
            {
                if (!HasUnreportedChanges) return null;
                if (lastReportAt.HasValue && reportAt - lastReportAt.Value < ReportInterval) return null;
            }

            var data = new JObject
            {
                ["commanderCredits"] = credits.Value,
            };
            if (loan.HasValue) data["commanderLoan"] = loan.Value;
            // Current_Wealth is authoritative only at the Statistics timestamp.
            // Omitting a later stale value lets Inara calculate assets from its data.
            if (includeAssets && assets.HasValue) data["commanderAssets"] = assets.Value;

            lastReportAt = reportAt;
            HasUnreportedChanges = false;
            return new InaraEvent("setCommanderCredits", timestamp, data, "credits");
        }

        private static DateTimeOffset parseTimestamp(string timestamp) =>
            DateTimeOffset.TryParse(timestamp, out var parsed) ? parsed : DateTimeOffset.UtcNow;

        private static long organicDataValue(JObject entry) =>
            (entry["BioData"] as JArray)?.OfType<JObject>()
                .Sum(item => valueOrZero(item, "Value") + valueOrZero(item, "Bonus")) ?? 0;

        private static long valueOrZero(JObject? entry, string property) => value(entry, property) ?? 0;

        private static long? value(JObject? entry, string property)
        {
            var token = entry?[property];
            if (token == null || token.Type is JTokenType.Null or JTokenType.Undefined) return null;
            return token.Value<long?>();
        }
    }
}
