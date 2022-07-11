using System.Collections.Generic;
using System.Linq;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class ProceduralRenderer : RenderableComponent, IBeginPlay
    {
        public ProceduralRenderer()
        {
            // _tiles = new List<List<Tile>>();
            // _colliders = new List<List<Collider>>();
            _entities = new List<List<Entity>>();
            RenderLayer = 5;
        }

        public int PhysicsLayer = 1;
        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            // foreach (var tile in _tiles.SelectMany(s => s))
            // {
            //     iBatcher.Draw(
            //         tile.Texture,
            //         tile.Position,
            //         Color.White,
            //         0.0f,
            //         Vector2.Zero,
            //         Entity.Transform.Scale.X,
            //         tile.Effects,
            //         0.0f);
            // }

// #if VERBOSE
//             foreach (var collider in _colliders.SelectMany(c => c))
//             {
//                 collider.DebugRender(iBatcher);
//             }
// #endif
        }

        public int BeginPlayOrder => 1;

        public void OnBeginPlay()
        {
            // var textureAtlas = Entity.Scene.Content.LoadTexture(Content.Content.LevelTileset);
            // var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);

            // _waterTexture = spriteList[0];
            // _waterBankTexture = spriteList[1];
            // _landBankTexture = spriteList[3];

            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();
            foreach (var block in _generator.Blocks)
            {
                GenerateLevelBlockRenderables(block);

                // var theseTiles = GetTilesForLevelBlock(block);
                //
                // _tiles.Add(theseTiles);
            }
        }

        public void OnNewGeneration(LevelBlock iNewBlock)
        {
            //_tiles.RemoveAt(0);

//             foreach (var collider in _colliders.First())
//             {
//                 Physics.RemoveCollider(collider);
//
// #if VERBOSE
//                 Verbose.RemoveColliderToRender(collider);
// #endif
//             }
//             _colliders.RemoveAt(0);

            foreach (var entity in _entities.First())
            {
                entity.Destroy();

// #if VERBOSE
//                 Verbose.RemoveColliderToRender(entity.GetComponent<Collider>());
// #endif
            }
            _entities.RemoveAt(0);

            GenerateLevelBlockRenderables(iNewBlock);
        }

        // private class Tile
        // {
        //     public Tile(Sprite iTexture, Vector2 iPosition, SpriteEffects iEffects = SpriteEffects.None)
        //     {
        //         Texture = iTexture;
        //         Position = iPosition;
        //         Effects = iEffects;
        //     }
        //
        //     public Sprite Texture { get; }
        //     public Vector2 Position { get; }
        //     public SpriteEffects Effects { get; }
        // }
        
        private readonly List<List<Entity>> _entities;
        private ProceduralGeneratorComponent _generator;
        // private Sprite _waterTexture;
        //private Sprite _landBankTexture;

        private void GenerateLevelBlockRenderables(LevelBlock iBlock)
        {
            var theseEntities = new List<Entity>();

            var increment = 32 * Entity.Transform.Scale.X;
            
            var xPos = iBlock.Function.DomainStart;
            while (xPos <= iBlock.Function.DomainEnd)
            {
                // var waterTileOffsetY = upperLandBankTile.Position.Y - numVerticallyStackedTiles / 2f;
                // for (var ii = 0; ii < numVerticallyStackedTiles; ii++)
                // {
                //     var thisWaterTilePosY = waterTileOffsetY + ii * increment;
                //     var thisWaterTilePos = new Vector2(upperLandBankTile.Position.X, thisWaterTilePosY);
                //     var thisWaterTile = new Tile(_waterTexture, thisWaterTilePos);
                //     tiles.Add(thisWaterTile);
                // }

                var theseBankTiles = TileGenerator.GenerateBankTiles(xPos, increment, iBlock, Entity.Scale.X);

                foreach (var tile in theseBankTiles)
                {
                    Entity.Scene.AddEntity(tile);
                    theseEntities.Add(tile);
                }

                xPos += increment;
            }
            
            foreach (var iObstacleLocation in iBlock.Obstacles)
            {
                var thisObstacle = new RockObstacleEntity(iObstacleLocation)
                {
                    Scale = Entity.Scale
                };

                theseEntities.Add(thisObstacle);
                Entity.Scene.AddEntity(thisObstacle);
            }

            _entities.Add(theseEntities);
        }
    }
}
