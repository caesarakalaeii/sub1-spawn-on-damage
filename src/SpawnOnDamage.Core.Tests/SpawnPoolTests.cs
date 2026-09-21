using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpawnOnDamage.Core;

namespace SpawnOnDamage.Core.Tests;

[TestClass]
public class SpawnPoolTests
{
    [TestMethod]
    public void ParsesSingleUnweightedEntry()
    {
        var pool = SpawnPool.Parse("ReaperLeviathan");

        pool.Entries.Should().ContainSingle().Which.Name.Should().Be("ReaperLeviathan");
    }

    [TestMethod]
    public void ParsesCommaSeparatedEntriesWithWhitespace()
    {
        var pool = SpawnPool.Parse(" ReaperLeviathan , GhostLeviathan ,SeaDragon ");

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan", "GhostLeviathan", "SeaDragon");
    }

    [TestMethod]
    public void ParsesWeightedEntry()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:2,ReaperLeviathan:1");

        pool.Entries.Should().HaveCount(2);
        pool.Entries[0].Name.Should().Be("GhostLeviathan");
        pool.Entries[0].Weight.Should().Be(2);
    }

    [TestMethod]
    public void SkipsEntryWithInvalidWeight()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:notanumber,ReaperLeviathan");

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan");
    }

    [TestMethod]
    public void SkipsEmptyEntry()
    {
        var pool = SpawnPool.Parse("ReaperLeviathan,,GhostLeviathan");

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan", "GhostLeviathan");
    }

    [TestMethod]
    public void EmptyPoolPicksNothing()
    {
        var pool = SpawnPool.Parse("  ");

        pool.Entries.Should().BeEmpty();
        pool.Pick(new Random(1)).Should().BeNull();
    }

    [TestMethod]
    public void DefaultPoolIsTheThreeLeviathans()
    {
        var pool = SpawnPool.Parse(SpawnPool.DefaultPool);

        pool.Entries.Select(e => e.Name).Should().Equal("ReaperLeviathan", "GhostLeviathan", "SeaDragon");
    }

    [TestMethod]
    public void WeightedPickFavoursHeavierEntry()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:9,ReaperLeviathan:1");

        var rng = new Random(1);
        var picks = Enumerable.Range(0, 100).Select(_ => pool.Pick(rng)).ToList();

        picks.Count(n => n == "GhostLeviathan").Should().BeGreaterThan(80);
    }

    [TestMethod]
    public void UnweightedPickIsUniform()
    {
        var pool = SpawnPool.Parse("ReaperLeviathan,GhostLeviathan");

        var rng = new Random(1);
        var counts = Enumerable.Range(0, 1000).Select(_ => pool.Pick(rng))
            .GroupBy(n => n).ToDictionary(g => g.Key!, g => g.Count());
        counts["ReaperLeviathan"].Should().BeInRange(430, 570);
        counts["GhostLeviathan"].Should().BeInRange(430, 570);
    }

    [TestMethod]
    public void PickOfEmptyWeightPoolIsNull()
    {
        var pool = SpawnPool.Parse("GhostLeviathan:0");

        pool.Pick(new Random(1)).Should().BeNull();
    }
}
