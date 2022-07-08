using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class RockObstacleEntity : Entity
    {
        public RockObstacleEntity(string iName, Vector2 iPosition) :
            base(iName)
        {
            Position = iPosition;
        }


        public int PhysicsLayer = 1;

        public override void OnAddedToScene()
        {
            var textureAtlas = Scene.Content.LoadTexture(Content.Content.LevelTileset);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);
            var texture = spriteList[4];

            _renderer = AddComponent<SpriteRenderer>();
            _renderer.RenderLayer = 4;
            _renderer.Sprite = texture;

            _collider = new CircleCollider(texture.SourceRect.Width / 2f) {PhysicsLayer = PhysicsLayer, Entity = this};
            Physics.AddCollider(_collider);

#if VERBOSE
            Verbose.RenderCollider(_collider);
#endif
        }

        public override void OnRemovedFromScene()
        {
            Physics.RemoveCollider(_collider);
        }

        private SpriteRenderer _renderer;
        private Collider _collider;
    }
}
