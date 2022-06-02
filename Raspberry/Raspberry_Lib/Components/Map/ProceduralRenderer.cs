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
            _tiles = new List<Tile>();
        }

        public int PhysicsLayer = 1 << 0;
        public override float Width => 100000;
        public override float Height => 20000;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var tile in _tiles)
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
        }

        public int BeginPlayOrder => 1;

        public void OnBeginPlay()
        {
            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            var increment = texture.SourceRect.Width * Entity.Transform.Scale.X;
            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();
            foreach (var function in _generator.Functions)
            {
                var xPos = function.DomainStart;
                while (xPos <= function.DomainEnd)
                {
                    var yPos = function.GetYForX(xPos);

                    var upperTile = new Tile(texture, new Vector2(xPos, yPos - increment * 2));
                    var lowerTile = new Tile(texture, new Vector2(xPos, yPos + increment * 2));

                    _tiles.Add(upperTile);
                    _tiles.Add(lowerTile);

                    var upperTileRectangle = new Rectangle(upperTile.Position.ToPoint(), new Point((int)increment, (int)increment));
                    var lowerTileRectangle = new Rectangle(upperTile.Position.ToPoint(), new Point((int)increment, (int)increment));
                    Physics.AddCollider(new BoxCollider(upperTileRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity });
                    Physics.AddCollider(new BoxCollider(lowerTileRectangle) { PhysicsLayer = PhysicsLayer, Entity = Entity });

                    xPos += increment;
                }
            }
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

        private readonly List<Tile> _tiles;
        private ProceduralGeneratorComponent _generator;
    }
}
