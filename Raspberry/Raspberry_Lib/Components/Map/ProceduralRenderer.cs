using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class ProceduralRenderer : RenderableComponent, IBeginPlay
    {
        public ProceduralRenderer()
        {
            _tiles = new List<List<Tile>>();
            _colliders = new List<List<Collider>>();
        }

        public int PhysicsLayer = 1 << 0;
        public override float Width => 100000;
        public override float Height => 20000;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var tile in _tiles.SelectMany(s => s))
            {
                iBatcher.Draw(
                    tile.Texture,
                    tile.Position,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Entity.Transform.Scale.X,
                    SpriteEffects.None,
                    0.0f);
            }

#if VERBOSE
            foreach (var collider in _colliders.SelectMany(c => c))
            {
                collider.DebugRender(iBatcher);
            }
#endif
        }

        public int BeginPlayOrder => 1;

        public void OnBeginPlay()
        {
            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();
            foreach (var block in _generator.Blocks)
            {
                var theseTiles = GetTilesForLevelBlock(block);

                _tiles.Add(theseTiles);
            }
        }

        public void OnNewGeneration(LevelBlock iNewBlock)
        {
            _tiles.RemoveAt(0);

            foreach (var collider in _colliders.First())
            {
                Physics.RemoveCollider(collider);
            }
            _colliders.RemoveAt(0);

            _tiles.Add(GetTilesForLevelBlock(iNewBlock));
        }

        private class Tile
        {
            public Tile(Sprite iTexture, Vector2 iPosition)
            {
                Texture = iTexture;
                Position = iPosition;
            }

            public Sprite Texture { get; }
            public Vector2 Position { get; }
        }

        private readonly List<List<Tile>> _tiles;
        private readonly List<List<Collider>> _colliders;
        private ProceduralGeneratorComponent _generator;

        private List<Tile> GetTilesForLevelBlock(LevelBlock iBlock)
        {
            var tiles = new List<Tile>();
            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            var increment = texture.SourceRect.Width * Entity.Transform.Scale.X;

            var unscaledIncrement = texture.SourceRect.Width;

            var thisBlockColliders = new List<Collider>();
            var xPos = iBlock.Function.DomainStart;
            while (xPos <= iBlock.Function.DomainEnd)
            {
                var yPos = iBlock.Function.GetYForX(xPos);

                var upperTile = new Tile(texture, new Vector2(xPos, yPos - iBlock.RiverWidth / 2));
                var lowerTile = new Tile(texture, new Vector2(xPos, yPos + iBlock.RiverWidth / 2));

                tiles.Add(upperTile);
                tiles.Add(lowerTile);

                // BoxCollider reapplies the entity transform so I have to pass in position & size without that scaling
                var unscaledUpperTilePosition = new Vector2(upperTile.Position.X / Entity.Transform.Scale.X, upperTile.Position.Y / Entity.Transform.Scale.X);
                var unscaledLowerTilePosition = new Vector2(lowerTile.Position.X / Entity.Transform.Scale.X, lowerTile.Position.Y / Entity.Transform.Scale.X);
                
                var upperTileColliderRectangle = new Rectangle(unscaledUpperTilePosition.ToPoint(), new Point(unscaledIncrement, unscaledIncrement));
                var lowerTileColliderRectangle = new Rectangle(unscaledLowerTilePosition.ToPoint(), new Point(unscaledIncrement, unscaledIncrement));
                
                var upperCollider = new BoxCollider(upperTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity };
                var lowerCollider = new BoxCollider(lowerTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity };
                thisBlockColliders.Add(upperCollider);
                thisBlockColliders.Add(lowerCollider);
                
                Physics.AddCollider(upperCollider);
                Physics.AddCollider(lowerCollider);

                xPos += increment;
            }

            foreach (var iObstacleLocation in iBlock.Obstacles)
            {
                var obstacle = new Tile(texture, iObstacleLocation);
                tiles.Add(obstacle);

                var unscaledPosition = new Vector2(iObstacleLocation.X / Entity.Transform.Scale.X, iObstacleLocation.Y / Entity.Transform.Scale.X);

                var colliderRectangle = new Rectangle(unscaledPosition.ToPoint(), new Point(unscaledIncrement, unscaledIncrement));
                var collider = new BoxCollider(colliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity };

                thisBlockColliders.Add(collider);

                Physics.AddCollider(collider);
            }

            _colliders.Add(thisBlockColliders);

            return tiles;
        }
    }
}
