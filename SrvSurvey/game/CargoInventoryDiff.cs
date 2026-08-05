namespace SrvSurvey.game
{
    /// <summary>
    /// Pure inventory snapshot/diff helpers for squadron-FC cargo tracking.
    /// Kept free of UI/logging so unit tests can cover the arithmetic used by <see cref="CargoFile2"/>.
    /// </summary>
    public static class CargoInventoryDiff
    {
        /// <summary>Copy commodity counts from inventory items into a destination dictionary (cleared first).</summary>
        public static void CopyFromInventory(Dictionary<string, int> destination, IReadOnlyList<InventoryItem>? inventory)
        {
            destination.Clear();
            if (inventory == null)
                return;

            foreach (var entry in inventory)
                destination[entry.Name] = entry.Count;
        }

        /// <summary>
        /// Ship cargo delta: <paramref name="after"/> − <paramref name="before"/> (non-zero entries only).
        /// Missing commodities in <paramref name="after"/> contribute a negative delta equal to their before count.
        /// </summary>
        public static Dictionary<string, int> Compute(IReadOnlyDictionary<string, int> before, IReadOnlyList<InventoryItem>? after)
        {
            var diffs = new Dictionary<string, int>();
            var inventory = after ?? Array.Empty<InventoryItem>();

            // O(after) name set so removed-commodity detection is O(before), not O(before×after).
            // This runs under CargoFile2.SyncRoot via getDiff — keep it linear.
            var afterNames = new HashSet<string>(inventory.Count, StringComparer.Ordinal);
            foreach (var entry in inventory)
            {
                afterNames.Add(entry.Name);
                var delta = entry.Count - before.GetValueOrDefault(entry.Name);
                if (delta != 0)
                    diffs[entry.Name] = delta;
            }

            foreach (var entry in before)
            {
                if (!afterNames.Contains(entry.Key))
                    diffs[entry.Key] = -entry.Value;
            }

            return diffs;
        }

        /// <summary>Map inventory items to name → count for logging/debug dumps.</summary>
        public static Dictionary<string, int> ToCountMap(IReadOnlyList<InventoryItem>? inventory)
        {
            if (inventory == null || inventory.Count == 0)
                return new Dictionary<string, int>();

            return inventory.ToDictionary(i => i.Name, i => i.Count);
        }
    }
}
