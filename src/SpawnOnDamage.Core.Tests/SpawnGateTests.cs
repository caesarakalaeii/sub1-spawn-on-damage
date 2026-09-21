using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpawnOnDamage.Core;

namespace SpawnOnDamage.Core.Tests;

[TestClass]
public class SpawnGateTests
{
    [TestMethod]
    public void DefaultCooldownIsSixtySeconds()
    {
        SpawnGate.DefaultCooldownSeconds.Should().Be(60);
    }

    [TestMethod]
    public void FirstTrySpawnIsGranted()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, SpawnGate.DefaultCooldownSeconds);

        gate.TrySpawn().Should().BeTrue();
    }

    [TestMethod]
    public void TrySpawnWithinCooldownIsRefused()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, 60);

        gate.TrySpawn().Should().BeTrue();
        clock.Advance(59.9);

        gate.TrySpawn().Should().BeFalse();
    }

    [TestMethod]
    public void TrySpawnAfterCooldownIsGranted()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, 60);

        gate.TrySpawn().Should().BeTrue();
        clock.Advance(60.1);

        gate.TrySpawn().Should().BeTrue();
    }

    [TestMethod]
    public void DisabledGateRefusesWithoutConsumingCooldown()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, 60) { Enabled = false };

        gate.TrySpawn().Should().BeFalse();
        clock.Advance(5);

        gate.Enabled = true;
        gate.TrySpawn().Should().BeTrue("a refused spawn while disabled must not start the cooldown");
    }

    [TestMethod]
    public void RefusedSpawnWithinCooldownDoesNotExtendIt()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, 60);

        gate.TrySpawn().Should().BeTrue();
        clock.Advance(30);
        gate.TrySpawn().Should().BeFalse();
        clock.Advance(30.1);

        gate.TrySpawn().Should().BeTrue("the refused attempt at t=30 must not restart the cooldown");
    }

    [TestMethod]
    public void ZeroCooldownGrantsEveryAttempt()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, 0);

        gate.TrySpawn().Should().BeTrue();
        gate.TrySpawn().Should().BeTrue();
    }

    [TestMethod]
    public void NegativeCooldownIsClampedToZero()
    {
        var clock = new FakeClock();
        var gate = new SpawnGate(clock.Now, -5);

        gate.TrySpawn().Should().BeTrue();
        gate.TrySpawn().Should().BeTrue();
    }

    private sealed class FakeClock
    {
        private double _seconds;

        public double Now() => _seconds;

        public void Advance(double seconds) => _seconds += seconds;
    }
}
