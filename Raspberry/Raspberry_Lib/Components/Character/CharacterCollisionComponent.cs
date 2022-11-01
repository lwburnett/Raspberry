using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;
#if VERBOSE
using Microsoft.Xna.Framework;
#endif

namespace Raspberry_Lib.Components
{
    internal class CharacterCollisionComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public const float ColliderXOffset = -9f;
            public const float ColliderYOffset = 0f;

            public const float ColliderWidth = 60;
            public const float ColliderHeight = 15;
        }

        public CharacterCollisionComponent()
        {
            _collider = new BoxCollider(
                -Settings.ColliderWidth / 2 + Settings.ColliderXOffset,
                -Settings.ColliderHeight / 2 + Settings.ColliderYOffset,
                Settings.ColliderWidth,
                Settings.ColliderHeight);
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(_collider);
            _playerProximityComponent = Entity.GetComponent<PlayerProximityComponent>();
            _playerMovementComponent = Entity.GetComponent<CharacterMovementComponent>();

#if VERBOSE
            Verbose.RenderCollider(_collider);
#endif
        }

        public int BeginPlayOrder => 100;
        public void OnBeginPlay()
        {
            _proceduralGenerator = Entity.Scene.FindEntity("map").GetComponent<ProceduralGeneratorComponent>();
        }

        public void Update()
        {
            if (_proceduralGenerator == null)
                return;

            if (_collider.Shape is not Polygon polygon)
                return;

            var points = polygon.Points;
            var scaledLocalOffset = _collider.LocalOffset * Entity.Scale;
            var vertices = points.Select(p => Entity.Position + scaledLocalOffset + p);

            foreach (var vertex in vertices)
            {
                var thisBlock = _proceduralGenerator.GetBlockForPosition(vertex);

                var riverY = thisBlock.Function.GetYForX(vertex.X);

                var riverWidth = thisBlock.GetRiverWidth(vertex.X);

                var upperBankY = riverY - riverWidth / 2;
                var lowerBankY = riverY + riverWidth / 2;

#if VERBOSE
                //Debug.DrawPixel(vertex, 4, Color.Red);
                Debug.DrawPixel(new Vector2(vertex.X, upperBankY), 4, Color.Yellow);
                Debug.DrawPixel(new Vector2(vertex.X, lowerBankY), 4, Color.Yellow);
#endif

                if (upperBankY > vertex.Y)
                {
                    var slope = new Vector2(1f, thisBlock.Function.GetYPrimeForX(vertex.X));
                    var minYDistToMove = upperBankY - vertex.Y;
                    HandleShorelineCollision(slope, minYDistToMove);
                    break;
                }
                else if (lowerBankY < vertex.Y)
                {
                    var slope = new Vector2(1f, thisBlock.Function.GetYPrimeForX(vertex.X));
                    var minYDistToMove = lowerBankY - vertex.Y;
                    HandleShorelineCollision(slope, minYDistToMove);
                    break;
                }
            }
        }

        public void HandleCollision(CollisionResult collisionResult)
        {
            if (collisionResult.Collider == null) 
                return;

            if (collisionResult.Collider.Entity is RockObstacleEntity)
            {
                _playerMovementComponent.AdjustMovementAfterCollision(-collisionResult.Normal, collisionResult.MinimumTranslationVector);
            }
            else if (collisionResult.Collider.Entity is BranchEntity branch)
            {
                _playerProximityComponent.OnBranchHit();
                branch.OnPlayerHit();
            }
        }

        private readonly BoxCollider _collider;
        private ProceduralGeneratorComponent _proceduralGenerator;
        private PlayerProximityComponent _playerProximityComponent;
        private CharacterMovementComponent _playerMovementComponent;

        private void HandleShorelineCollision(Vector2 iShorelineSlope, float iMinYDistToMove)
        {
            var normal = new Vector2(-iShorelineSlope.Y, iShorelineSlope.X);

            if (iMinYDistToMove < 0)
                normal *= -1;

            normal.Normalize();

            var minTranslation = normal * Math.Abs(iMinYDistToMove);

            _playerMovementComponent.AdjustMovementAfterCollision(normal, minTranslation);
        }
    }
}