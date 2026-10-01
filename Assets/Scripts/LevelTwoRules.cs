using System;

// Deterministic rules, independent of rendering and frame rate.
public sealed class LevelTwoRules
{
    public enum Wave { Collect, Warning, Hunt }
    public int Energy { get; private set; }
    public int Streak { get; private set; }
    public float ComboLeft { get; private set; }
    public float Cooldown { get; private set; }
    public float WaveClock { get; private set; }
    public int Multiplier => Streak >= 25 ? 3 : Streak >= 10 ? 2 : 1;
    public Wave Phase => WaveClock < 20 ? Wave.Collect : WaveClock < 23 ? Wave.Warning : Wave.Hunt;
    public float PhaseLeft => (Phase == Wave.Collect ? 20 : Phase == Wave.Warning ? 23 : 30) - WaveClock;
    public void Tick(float seconds)
    {
        seconds = Math.Max(0, seconds);
        WaveClock = (WaveClock + seconds) % 30;
        Cooldown = Math.Max(0, Cooldown - seconds);
        ComboLeft = Math.Max(0, ComboLeft - seconds);
        if (ComboLeft <= 0) Streak = 0;
    }
    public int Collect(bool power)
    {
        Streak++;
        ComboLeft = 3;
        Energy = Math.Min(100, Energy + (power ? 25 : 5));
        return (power ? 50 : 10) * Multiplier;
    }
    public bool TryPulse(out bool super)
    {
        super = false;
        if (Cooldown > 0 || Energy < 50) return false;
        super = Energy == 100;
        Energy -= super ? 100 : 50;
        Cooldown = 4;
        return true;
    }
    public void ResetAfterHit()
    {
        Streak = 0; ComboLeft = 0; WaveClock = 0; Cooldown = 0;
    }
}
