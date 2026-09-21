using System;

namespace SpawnOnDamage.Core;

/// <summary>Decides whether a damage event may spawn a creature: enabled, and
/// the cooldown since the last granted spawn has elapsed. The clock is
/// injected so tests need no real time.</summary>
public sealed class SpawnGate
{
    public const double DefaultCooldownSeconds = 60;

    private readonly Func<double> _clock;
    private readonly double _cooldownSeconds;
    private double _lastSpawn = double.MinValue;

    public SpawnGate(Func<double> clock, double cooldownSeconds)
    {
        _clock = clock;
        _cooldownSeconds = Math.Max(0, cooldownSeconds);
    }

    public bool Enabled { get; set; } = true;

    public bool TrySpawn()
    {
        if (!Enabled)
        {
            return false;
        }

        var now = _clock();
        if (now - _lastSpawn < _cooldownSeconds)
        {
            // Refused: the pending cooldown keeps running from the last
            // granted spawn, it does not restart.
            return false;
        }

        _lastSpawn = now;
        return true;
    }
}
