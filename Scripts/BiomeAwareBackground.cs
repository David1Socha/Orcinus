using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    [Tool]
    public class BiomeAwareBackground : Sprite
    {
        [Export]
        private readonly Texture[] BiomeTextures;

        private BiomeEnum _BiomeValue;

        public override void _Ready()
        {
            base._Ready();
            GlobalSignalBus.RegisterHandler(Signals.BiomeChanged, this, nameof(OnBiomeUpdated));
        }

        private void UpdateSpritesBasedOnActiveBiome()
        {
            Texture = BiomeTextures?[(int)_BiomeValue];
        }

        public void OnBiomeUpdated(BiomeEnum biome)
        {
            _BiomeValue = biome;
            UpdateSpritesBasedOnActiveBiome();
        }
    }
}
