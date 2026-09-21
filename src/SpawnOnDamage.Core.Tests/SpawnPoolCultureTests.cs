using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpawnOnDamage.Core;

namespace SpawnOnDamage.Core.Tests;

[TestClass]
public class SpawnPoolCultureTests
{
    [TestMethod]
    public void WeightParsingIsLocaleIndependent()
    {
        var original = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            // Under de-DE a naive TryParse reads "1.5" as 15 and "0.5" as 5,
            // silently reweighting the pool tenfold.
            var pool = SpawnPool.Parse("GhostLeviathan:1.5,ReaperLeviathan:0.5");

            pool.Entries.Should().HaveCount(2);
            pool.Entries[0].Weight.Should().Be(1.5);
            pool.Entries[1].Weight.Should().Be(0.5);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void CommaIsAlwaysTheEntrySeparator()
    {
        var original = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            // The comma is the entry separator in every locale; a
            // decimal-comma weight cannot be expressed in this format. The
            // stray "5" is caught later by TechType validation at spawn time.
            var pool = SpawnPool.Parse("GhostLeviathan:2,5");

            pool.Entries.Select(e => e.Name).Should().Equal("GhostLeviathan", "5");
            pool.Entries[0].Weight.Should().Be(2);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void InfiniteWeightIsSkipped()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:Infinity,ReaperLeviathan:1");

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan");
    }

    [TestMethod]
    public void NanWeightIsSkipped()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:NaN,ReaperLeviathan:1");

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan");
    }
}
