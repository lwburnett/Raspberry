using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Action = Nez.AI.GOAP.Action;

namespace Raspberry_Lib.Components
{
    internal class ProceduralRenderer : RenderableComponent, IBeginPlay/*, IUpdatable*/
    {
        public ProceduralRenderer()
        {
            _entities = new List<List<Entity>>();
            RenderLayer = 7;
        }
        
        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;

        public override void Render(Batcher batcher, Camera camera)
        {
            // This does nothing right now but I'm keeping it here in case I need to render tiles later
        }

        public override void OnAddedToEntity()
        {
            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();
        }

        public int BeginPlayOrder => 1;
        public void OnBeginPlay()
        {
            System.Diagnostics.Debug.Assert(_generator != null);
            System.Diagnostics.Debug.Assert(_generationJob == null);

            var tileIncrement = 32 * Entity.Transform.Scale.X;
            var character = Entity.Scene.FindEntity("character");
            foreach (var block in _generator.Blocks)
            {
                _entities.Add(new List<Entity>());

                _generationJob = new GenerationJob(
                    block,
                    OnTileGenerated,
                    tileIncrement,
                    Entity.Transform.Scale.X,
                    () => character.Position,
                    () => 200);

                _generationJob.Tick();
                System.Diagnostics.Debug.Assert(_generationJob.IsFinished);
            }
        }

        // public void Update()
        // {
        //     throw new System.NotImplementedException();
        // }

        public void OnNewGeneration(LevelBlock iNewBlock)
        {
            foreach (var entity in _entities.First())
            {
                entity.Destroy();
            }
            _entities.RemoveAt(0);

            System.Diagnostics.Debug.Assert(_generationJob == null || _generationJob.IsFinished);

            _entities.Add(new List<Entity>());

            var tileIncrement = 32 * Entity.Transform.Scale.X;
            var character = Entity.Scene.FindEntity("character");
            _generationJob = new GenerationJob(
                iNewBlock,
                OnTileGenerated,
                tileIncrement,
                Entity.Transform.Scale.X,
                () => character.Position,
                () => 200);

            _generationJob.Tick();
            System.Diagnostics.Debug.Assert(_generationJob.IsFinished);
        }
        
        private readonly List<List<Entity>> _entities;
        private ProceduralGeneratorComponent _generator;
        private GenerationJob _generationJob;

        private void OnTileGenerated(Entity iNewTile)
        {
            _entities.Last().Add(iNewTile);
            Entity.Scene.AddEntity(iNewTile);
        }

        private class GenerationJob
        {
            public GenerationJob(
                LevelBlock iBlock, 
                Action<Entity> iOnTileGenerated,
                float iIncrement,
                float iScale,
                Func<Vector2> iGetPlayerPosFunc,
                Func<float> iGetProximityRadius,
                int iNumToProcessPerFrame = int.MaxValue)
            {
                IsFinished = false;

                _levelBlock = iBlock;
                _onTileGenerated = iOnTileGenerated;
                _increment = iIncrement;
                _scale = iScale;
                _getPlayerPosFunc = iGetPlayerPosFunc;
                _getProximityRadius = iGetProximityRadius;
                _numToProcessPerFrame = iNumToProcessPerFrame;

                _currentXPos = _levelBlock.Function.DomainStart;
                _currentObstacleIndex = 0;
            }

            public void Tick()
            {
                var numProcessed = 0;

                while (numProcessed < _numToProcessPerFrame && !IsFinished)
                {
                    if (_currentXPos <= _levelBlock.Function.DomainEnd)
                    {
                        var theseTiles = TileGenerator.GenerateRiverTiles(
                            _currentXPos, 
                            _increment, 
                            _levelBlock, 
                            _scale, 
                            _getPlayerPosFunc);

                        foreach (var tile in theseTiles)
                        {
                            _onTileGenerated(tile);
                            numProcessed++;
                        }

                        _currentXPos += _increment;
                    }
                    else if (_currentObstacleIndex < _levelBlock.Obstacles.Count)
                    {
                        var thisObstacle = new RockObstacleEntity(_levelBlock.Obstacles[_currentObstacleIndex])
                        {
                            Scale = new Vector2(_scale)
                        };

                        _onTileGenerated(thisObstacle);
                        _currentObstacleIndex++;
                        numProcessed++;
                    }
                    else
                    {
                        IsFinished = true;
                    }
                }
            }

            public bool IsFinished { get; private set; }
            
            private readonly LevelBlock _levelBlock;
            private readonly Action<Entity> _onTileGenerated;
            private readonly float _increment;
            private readonly float _scale;
            private readonly Func<Vector2> _getPlayerPosFunc;
            private readonly Func<float> _getProximityRadius;
            private readonly int _numToProcessPerFrame;

            private float _currentXPos;
            private int _currentObstacleIndex;
        }
    }
}
