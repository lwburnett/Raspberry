using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class ProceduralRenderer : RenderableComponent, IBeginPlay
    {
        private static class Settings
        {
            public const bool RenderCollidersDebug = false;
        }

        public ProceduralRenderer()
        {
            _tiles = new List<List<Tile>>();
            //_colliders = new List<Collider>();
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

#if DEBUG
            // if (Settings.RenderCollidersDebug)
            // {
            //     foreach (var collider in _colliders)
            //     {
            //         collider.DebugRender(iBatcher);
            //     }
            // }
#endif
        }

        public int BeginPlayOrder => 1;

        public void OnBeginPlay()
        {
            //var unscaledIncrement = texture.SourceRect.Width;
            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();
            foreach (var function in _generator.Functions)
            {
                var theseTiles = GetTilesForFunction(function);

                _tiles.Add(theseTiles);
            }
        }

        public void OnNewGeneration(IFunction iNewFunction)
        {
            _tiles.RemoveAt(0);

            _tiles.Add(GetTilesForFunction(iNewFunction));
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
        //private readonly List<Collider> _colliders;
        private ProceduralGeneratorComponent _generator;

        private List<Tile> GetTilesForFunction(IFunction iFunc)
        {
            var tiles = new List<Tile>();
            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            var increment = texture.SourceRect.Width * Entity.Transform.Scale.X;

            var xPos = iFunc.DomainStart;
            while (xPos <= iFunc.DomainEnd)
            {
                var yPos = iFunc.GetYForX(xPos);

                var upperTile = new Tile(texture, new Vector2(xPos, yPos - increment * 4));
                var lowerTile = new Tile(texture, new Vector2(xPos, yPos + increment * 4));

                tiles.Add(upperTile);
                tiles.Add(lowerTile);

                // BoxCollider reapplies the entity transform so I have to pass in position & size without that scaling
                // var unscaledUpperTilePosition = new Vector2(upperTile.Position.X / Entity.Transform.Scale.X, upperTile.Position.Y / Entity.Transform.Scale.X);
                // var unscaledLowerTilePosition = new Vector2(lowerTile.Position.X / Entity.Transform.Scale.X, lowerTile.Position.Y / Entity.Transform.Scale.X);
                //
                // var upperTileColliderRectangle = new Rectangle(unscaledUpperTilePosition.ToPoint(), new Point(unscaledIncrement, unscaledIncrement));
                // var lowerTileColliderRectangle = new Rectangle(unscaledLowerTilePosition.ToPoint(), new Point(unscaledIncrement, unscaledIncrement));
                //
                // var upperCollider = new BoxCollider(upperTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity };
                // var lowerCollider = new BoxCollider(lowerTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity };
                // _colliders.Add(upperCollider);
                // _colliders.Add(lowerCollider);
                //
                // Physics.AddCollider(new BoxCollider(upperTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity });
                // Physics.AddCollider(new BoxCollider(lowerTileColliderRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity });

                xPos += increment;
            }

            return tiles;
        }
    }
}
