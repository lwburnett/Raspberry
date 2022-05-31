using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib
{
    internal class ProceduralRenderer : RenderableComponent
    {
        public ProceduralRenderer(Vector2 iCharacterStartingPos)
        {
            _characterStartingPos = iCharacterStartingPos;
            _tiles = new List<Tile>();
        }

        public override void OnAddedToEntity()
        {
            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            var increment = texture.SourceRect.Width * Entity.Transform.Scale.X;
            _generator = new ProceduralGenerator(_characterStartingPos, increment);
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

        public int PhysicsLayer = 1 << 0; 
        public override float Width => _generator.Functions.Last().DomainEnd;
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
        private readonly Vector2 _characterStartingPos;
        private ProceduralGenerator _generator;
    }
}
