using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
            RenderLayer = 7;
        }

        public int PhysicsLayer = 1;
        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;

        public override void Render(Batcher batcher, Camera camera)
        {
            // This does nothing right now but I'm keeping it here in case I need to render tiles later
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
            }
        }

        public void OnNewGeneration(LevelBlock iNewBlock)
        {
            foreach (var entity in _entities.First())
            {
                entity.Destroy();
            }
            _entities.RemoveAt(0);

            GenerateLevelBlockRenderables(iNewBlock);
        }
        
        private readonly List<List<Entity>> _entities;
        private ProceduralGeneratorComponent _generator;

        private void GenerateLevelBlockRenderables(LevelBlock iBlock)
        {
            var theseEntities = new List<Entity>();

            var increment = 32 * Entity.Transform.Scale.X;

            var character = Entity.Scene.FindEntity("character");

            var xPos = iBlock.Function.DomainStart;
            while (xPos <= iBlock.Function.DomainEnd)
            {
                var theseBankTiles = TileGenerator.GenerateRiverTiles(xPos, increment, iBlock, Entity.Scale.X, () => character.Position);

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
