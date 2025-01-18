using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

[Tool]
public class OrcaBubbleEmitter : Particles2D
{
    [Export]
    public Texture[] BubbleTextures { get; set; }

    private BiomeEnum _BiomeValue;
    private void UpdateSpritesBasedOnActiveBiome()
    {
        Texture = BubbleTextures[(int)_BiomeValue];
    }

    public override void _Ready()
    {
        base._Ready();
        GlobalSignalBus.RegisterHandler(Signals.BiomeChanged, this, nameof(OnBiomeUpdated));
    }

    public void OnBiomeUpdated(BiomeEnum biome)
    {
        _BiomeValue = biome;
        UpdateSpritesBasedOnActiveBiome();
    }
}