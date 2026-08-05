using SrvSurvey.game;
using Xunit;

namespace SrvSurvey.Tests
{
    public class CargoInventoryDiffTests
    {
        [Fact]
        public void CopyFromInventory_clears_and_copies_counts()
        {
            var dest = new Dictionary<string, int> { ["old"] = 9 };
            var inventory = new List<InventoryItem>
            {
                new("iron", "Iron") { Count = 3 },
                new("nickel", "Nickel") { Count = 7 },
            };

            CargoInventoryDiff.CopyFromInventory(dest, inventory);

            Assert.Equal(2, dest.Count);
            Assert.Equal(3, dest["iron"]);
            Assert.Equal(7, dest["nickel"]);
            Assert.False(dest.ContainsKey("old"));
        }

        [Fact]
        public void CopyFromInventory_handles_null_inventory()
        {
            var dest = new Dictionary<string, int> { ["iron"] = 1 };
            CargoInventoryDiff.CopyFromInventory(dest, null);
            Assert.Empty(dest);
        }

        [Fact]
        public void Compute_returns_positive_delta_when_ship_gains_cargo()
        {
            var before = new Dictionary<string, int> { ["iron"] = 2 };
            var after = new List<InventoryItem>
            {
                new("iron", "Iron") { Count = 5 },
            };

            var diff = CargoInventoryDiff.Compute(before, after);

            Assert.Equal(new Dictionary<string, int> { ["iron"] = 3 }, diff);
        }

        [Fact]
        public void Compute_returns_negative_delta_when_ship_loses_cargo()
        {
            var before = new Dictionary<string, int> { ["steel"] = 100 };
            var after = new List<InventoryItem>
            {
                new("steel", "Steel") { Count = 25 },
            };

            var diff = CargoInventoryDiff.Compute(before, after);

            Assert.Equal(new Dictionary<string, int> { ["steel"] = -75 }, diff);
        }

        [Fact]
        public void Compute_includes_removed_commodities_as_negative()
        {
            var before = new Dictionary<string, int>
            {
                ["iron"] = 4,
                ["nickel"] = 2,
            };
            var after = new List<InventoryItem>
            {
                new("iron", "Iron") { Count = 4 },
            };

            var diff = CargoInventoryDiff.Compute(before, after);

            Assert.Equal(new Dictionary<string, int> { ["nickel"] = -2 }, diff);
        }

        [Fact]
        public void Compute_returns_empty_when_unchanged()
        {
            var before = new Dictionary<string, int> { ["iron"] = 4 };
            var after = new List<InventoryItem>
            {
                new("iron", "Iron") { Count = 4 },
            };

            var diff = CargoInventoryDiff.Compute(before, after);

            Assert.Empty(diff);
        }

        [Fact]
        public void Compute_handles_null_after_inventory()
        {
            var before = new Dictionary<string, int> { ["iron"] = 4 };
            var diff = CargoInventoryDiff.Compute(before, null);
            Assert.Equal(new Dictionary<string, int> { ["iron"] = -4 }, diff);
        }

        [Fact]
        public void ToCountMap_maps_names_and_counts()
        {
            var inventory = new List<InventoryItem>
            {
                new("iron", "Iron") { Count = 1 },
                new("nickel", "Nickel") { Count = 2 },
            };

            var map = CargoInventoryDiff.ToCountMap(inventory);

            Assert.Equal(2, map.Count);
            Assert.Equal(1, map["iron"]);
            Assert.Equal(2, map["nickel"]);
        }

        [Fact]
        public void ToCountMap_handles_null_or_empty()
        {
            Assert.Empty(CargoInventoryDiff.ToCountMap(null));
            Assert.Empty(CargoInventoryDiff.ToCountMap(new List<InventoryItem>()));
        }

        [Fact]
        public void Compute_inverted_matches_squadron_fc_supply_delta()
        {
            // Ship transferred 10 steel to carrier: ship before 50 → after 40
            var before = new Dictionary<string, int> { ["steel"] = 50 };
            var after = new List<InventoryItem>
            {
                new("steel", "Steel") { Count = 40 },
            };

            var shipDiff = CargoInventoryDiff.Compute(before, after);
            var fcDiff = shipDiff.ToDictionary(x => x.Key, x => x.Value * -1);

            Assert.Equal(-10, shipDiff["steel"]);
            Assert.Equal(10, fcDiff["steel"]);
        }
    }
}
