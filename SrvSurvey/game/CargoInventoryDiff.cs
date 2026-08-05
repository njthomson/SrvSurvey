namespace SrvSurvey.game
{
    /// <summary>
    /// Pure inventory snapshot/diff helpers for squadron-FC cargo tracking.
    /// Kept free of UI/logging so unit tests can cover the arithmetic used by <see cref="CargoFile2"/>.
    /// Commodity names are matched case-insensitively to match Game journal mutation handlers.
    /// </summary>
    public static class CargoInventoryDiff
    {
        /// <summary>Comparer used for all cargo name maps (matches CollectCargo / CargoTransfer lookups).</summary>
        public static StringComparer NameComparer { get; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>Create an empty name→count map with case-insensitive keys.</summary>
        public static Dictionary<string, int> CreateCountMap() => new(NameComparer);

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
        /// Names are compared case-insensitively.
        /// </summary>
        public static Dictionary<string, int> Compute(IReadOnlyDictionary<string, int> before, IReadOnlyList<InventoryItem>? after)
        {
            var diffs = CreateCountMap();
            var inventory = after ?? Array.Empty<InventoryItem>();

            // O(after) name set so removed-commodity detection is O(before), not O(before×after).
            // This runs under CargoFile2.SyncRoot via getDiff — keep it linear.
            var afterNames = new HashSet<string>(inventory.Count, NameComparer);
            foreach (var entry in inventory)
            {
                afterNames.Add(entry.Name);
                var delta = entry.Count - before.GetValueOrDefault(entry.Name);
                if (delta != 0)
                    diffs[entry.Name] = delta;
            }

            foreach (var entry in before.Where(b => !afterNames.Contains(b.Key)))
                diffs[entry.Key] = -entry.Value;

            return diffs;
        }

        /// <summary>Map inventory items to name → count for logging/debug dumps.</summary>
        public static Dictionary<string, int> ToCountMap(IReadOnlyList<InventoryItem>? inventory)
        {
            var map = CreateCountMap();
            if (inventory == null || inventory.Count == 0)
                return map;

            foreach (var entry in inventory)
                map[entry.Name] = entry.Count;

            return map;
        }
    }
}
