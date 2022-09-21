using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class CharacterAnimationComponent : RenderableComponent
    {
        private static class Settings
        {
            public const int CellWidth = 80;
            public const int CellHeight = 30;
        }

        public CharacterAnimationComponent()
        {
            RenderLayer = 4;
        }

        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.CharacterSpriteSheet);
            _sprites = Sprite.SpritesFromAtlas(texture, Settings.CellWidth, Settings.CellHeight);

            _currentSprite = _sprites[0];
            _spriteEffect = SpriteEffects.None;
        }

        public override float Width => 600f;
        public override float Height => 600f;

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.Draw(_currentSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, _spriteEffect, _layerDepth);
        }

        private List<Sprite> _sprites;
        private Sprite _currentSprite;
        private SpriteEffects _spriteEffect;
    }
}