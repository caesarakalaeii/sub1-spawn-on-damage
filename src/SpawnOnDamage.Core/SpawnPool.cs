using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpawnOnDamage.Core;

/// <summary>A pool of creature names with optional weights, parsed from a config string.</summary>
public sealed class SpawnPool
{
    public const string DefaultPool = "ReaperLeviathan,GhostLeviathan,SeaDragon";

    public sealed record Entry(string Name, double Weight);

    public IReadOnlyList<Entry> Entries { get; }

    private SpawnPool(IReadOnlyList<Entry> entries)
    {
        Entries = entries;
    }

    public static SpawnPool Parse(string? config)
    {
        var entries = new List<Entry>();
        if (!string.IsNullOrWhiteSpace(config))
        {
            foreach (var raw in config!.Split(','))
            {
                var token = raw.Trim();
                if (token.Length == 0)
                {
                    continue;
                }

                var separator = token.LastIndexOf(':');
                if (separator > 0)
                {
                    // Only a name:weight entry has the separator; a bad weight
                    // skips the entry rather than guessing a default, so the
                    // operator's typo never silently reweights the pool.
                    var name = token[..separator].Trim();
                    var weightText = token[(separator + 1)..].Trim();
                    if (double.TryParse(weightText, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight)
                        && weight > 0 && !double.IsInfinity(weight) && !double.IsNaN(weight) && name.Length > 0)
                    {
                        entries.Add(new Entry(name, weight));
                    }
                }
                else
                {
                    entries.Add(new Entry(token, 1));
                }
            }
        }

        return new SpawnPool(entries);
    }

    public string? Pick(Random rng)
    {
        var total = Entries.Sum(e => e.Weight);
        if (total <= 0)
        {
            return null;
        }

        var roll = rng.NextDouble() * total;
        foreach (var entry in Entries)
        {
            roll -= entry.Weight;
            if (roll < 0)
            {
                return entry.Name;
            }
        }

        // Only reachable through floating-point rounding at the boundary;
        // the last entry is the honest answer there.
        return Entries[^1].Name;
    }
}
