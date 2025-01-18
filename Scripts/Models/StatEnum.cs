namespace Orcinus.Scripts.Models
{
    public enum StatEnum
    {
        Default = 99999,
        Points = 0,
        Fish = 1,
        Distance = 2,
        Powerup = 3,
        Biome = 4,
        Duration = 5,
        Penguin = 6,
        Seal = 7,
        NormalFish = 8,
        DodgedNet = 9,
        DodgedMine = 10,
        DodgedSpear = 11,
        DodgedDiver = 12,
        DodgedObstacle = 13,
        NearMissedNet = 14,
        NearMissedMine = 15,
        NearMissedSpear = 16,
        NearMissedDiver = 17,
        NearMissedObstacle = 18,
        FishBoost = 19,
        DestroyedObstacle = 20,
        DestroyedNet = 21,
        DestroyedMine = 22,
        DestroyedSpear = 23,
        DestroyedDiver = 24,
        LightFish = 25,
        Squid = 26,
        DodgedLightning = 27,
        NearMissedLightning = 28,
        DestroyedLightning = 29,
        DamageFreeBiomes = 30,
        MagnetizedFishCaught = 31,
        MinHealthDuration = 32,
        Yacht = 33,
        // TODO add tracked stat for each hat??
        // TODO add tracked stat for near fish misses (opens mouth without getting that fish)
        // TODO add tracked stat for time skimming surface ?
        // TODO add tracked stat for time skimming floor ?
        // TODO add tracked stats for lives lost in each biome?
        // TODO add tracked stat for duration spent with 0 extra lives
        // TODO add tracked stat for sparing the transition fishes
        // TODO add tracked stat for ghosted obstacles
        // TODO add tracked stat for max speed (works with increasing speed after each biome -- could have challenge exceed 1k speed or something)

        // TODO allow multi stat thresholds as part of a single mission (replace threshold, type, and progress with arrays)
    }
}
