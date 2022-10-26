using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class BranchEntity : Entity
    {
        private static class Settings
        {
            public const float ScaleAdjustmentMultiplier = .75f;
        }

        public BranchEntity(Vector2 iPosition)
        {
            Position = iPosition;
        }

        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            Scale *= Settings.ScaleAdjustmentMultiplier;

            var textureAtlas = Scene.Content.LoadTexture(Content.ContentData.AssetPaths.ObjectsTileset, true);
            var texture = new Sprite(textureAtlas, new Rectangle(0, 144, 36, 36));

            _renderer = AddComponent<SpriteRenderer>();
            _renderer.RenderLayer = 4;
            _renderer.Sprite = texture;

            _collider = AddComponent(new CircleCollider(texture.SourceRect.Width / 2f) { PhysicsLayer = PhysicsLayer, Entity = this });
            Physics.AddCollider(_collider);

#if VERBOSE
            Verbose.RenderCollider(_collider);
#endif
        }

        public override void OnRemovedFromScene()
        {
            Physics.RemoveCollider(_collider);
        }

        public void OnPlayerHit()
        {
            Physics.RemoveCollider(_collider);
            _renderer.Enabled = false;
        }

        private SpriteRenderer _renderer;
        private Collider _collider;
    }
}
