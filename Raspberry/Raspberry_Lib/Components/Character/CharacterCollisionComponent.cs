using System.Linq;
using Nez;
using Nez.PhysicsShapes;

namespace Raspberry_Lib.Components
{
    internal class CharacterCollisionComponent : Component, IUpdatable, IBeginPlay
    {
        public CharacterCollisionComponent(System.Action iOnFatalCollision)
        {
            _onFatalCollision = iOnFatalCollision;
            _collider = new BoxCollider(20, 6);
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(_collider);
            _playerProximityComponent = Entity.GetComponent<PlayerProximityComponent>();

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
            var vertices = points.Select(p => Entity.Position + p);

            foreach (var vertex in vertices)
            {
                var thisBlock = _proceduralGenerator.GetBlockForPosition(vertex);

                var riverY = thisBlock.Function.GetYForX(vertex.X);

                var riverWidth = thisBlock.GetRiverWidth(vertex.X);

                var upperBankY = riverY - riverWidth / 2;
                var lowerBankY = riverY + riverWidth / 2;

#if VERBOSE
                Debug.DrawPixel(vertex, 4, Color.Red);
                Debug.DrawPixel(new Vector2(vertex.X, upperBankY), 4, Color.Yellow);
                Debug.DrawPixel(new Vector2(vertex.X, lowerBankY), 4, Color.Yellow);
#endif

                if (upperBankY >= vertex.Y || lowerBankY <= vertex.Y)
                {
                    OnFatalCollision();
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
                OnFatalCollision();
            }
            else if (collisionResult.Collider.Entity is BranchEntity branch)
            {
                _playerProximityComponent.OnBranchHit();
                branch.OnPlayerHit();
            }
        }

        private readonly BoxCollider _collider;
        private readonly System.Action _onFatalCollision;
        private ProceduralGeneratorComponent _proceduralGenerator;
        private PlayerProximityComponent _playerProximityComponent;

        private void OnFatalCollision()
        {
#if VERBOSE
            Verbose.ClearCollidersToRender();
            Verbose.ClearMetrics();
#endif

            _onFatalCollision();
        }
    }
}