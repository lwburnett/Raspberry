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
        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;

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
                    tile.Effects,
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
            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.Content.LevelTileset);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);

            _waterTexture = spriteList[0];
            _obstacleTexture = spriteList[1];
            _bankTexture = spriteList[2];

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
            public Tile(Sprite iTexture, Vector2 iPosition, SpriteEffects iEffects = SpriteEffects.None)
            {
                Texture = iTexture;
                Position = iPosition;
                Effects = iEffects;
            }

            public Sprite Texture { get; }
            public Vector2 Position { get; }
            public SpriteEffects Effects { get; }
        }

        private readonly List<List<Tile>> _tiles;
        private readonly List<List<Collider>> _colliders;
        private ProceduralGeneratorComponent _generator;
        private Sprite _waterTexture;
        private Sprite _bankTexture;
        private Sprite _obstacleTexture;

        private List<Tile> GetTilesForLevelBlock(LevelBlock iBlock)
        {
            var tiles = new List<Tile>();

            var increment = _bankTexture.SourceRect.Width * Entity.Transform.Scale.X;

            var unscaledIncrement = _bankTexture.SourceRect.Width;

            var thisBlockColliders = new List<Collider>();
            var xPos = iBlock.Function.DomainStart;
            while (xPos <= iBlock.Function.DomainEnd)
            {
                var yPos = iBlock.Function.GetYForX(xPos);

                var upperTile = new Tile(_bankTexture, new Vector2(xPos - increment / 2, yPos - increment / 2 - iBlock.RiverWidth / 2));
                var lowerTile = new Tile(_bankTexture, new Vector2(xPos - increment / 2, yPos - increment / 2 + iBlock.RiverWidth / 2), SpriteEffects.FlipVertically);

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
                var adjustedLocation = new Vector2(iObstacleLocation.X - increment / 2, iObstacleLocation.Y - increment / 2);

                var obstacle = new Tile(_obstacleTexture, adjustedLocation);
                tiles.Add(obstacle);

                var unscaledPosition = new Vector2(adjustedLocation.X / Entity.Transform.Scale.X, adjustedLocation.Y / Entity.Transform.Scale.X);

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
