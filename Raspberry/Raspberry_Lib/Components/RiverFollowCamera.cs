using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class RiverFollowCamera : Component, IUpdatable
    {
        private static class Settings
        {
            public static readonly RenderSetting LeadingDeltaX = new(600f);
        }

        public RiverFollowCamera(Entity iTargetEntity, ProceduralGeneratorComponent iProceduralGenerator)
        {
            UpdateOrder = 99;
            _targetEntity = iTargetEntity;
            _proceduralGenerator = iProceduralGenerator;
        }
        
        public void Update()
        {
            var playerPosition = _targetEntity.Position;

            var setCameraPos = false;
            foreach (var function in _proceduralGenerator.Functions ?? new List<IFunction>())
            {
                var cameraDesiredX = playerPosition.X + Settings.LeadingDeltaX.Value;

                if (function.DomainStart <= cameraDesiredX &&
                    function.DomainEnd >= cameraDesiredX)
                {
                    var targetCollider = _targetEntity.GetComponent<Collider>();

                    RectangleF playerBounds;
                    if (targetCollider != null)
                    {
                        playerBounds = targetCollider.Bounds;
                    }
                    else
                    {
                        playerBounds = RectangleF.Empty;
                        System.Diagnostics.Debug.Fail("Failed to get character collider.");
                    }

                    var desiredFocusPoint = new Vector2(cameraDesiredX, function.GetYForX(cameraDesiredX));
                    var focusOffset = new Vector2(
                        playerBounds.X + playerBounds.Width / 2 - desiredFocusPoint.X,
                        playerBounds.Y + playerBounds.Height / 2 - desiredFocusPoint.Y);

                    _camera.FocusOffset = focusOffset;
                    setCameraPos = true;
                }
            }

            if (!setCameraPos)
            {
                _camera.FocusOffset = new Vector2(-Settings.LeadingDeltaX.Value, 0.0f);
                //System.Diagnostics.Debug.Fail("Failed to set camera position.");
            }
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _camera = new FollowCamera(_targetEntity);
            Entity.AddComponent(_camera);
        }

        private readonly Entity _targetEntity;
        private readonly ProceduralGeneratorComponent _proceduralGenerator;
        private FollowCamera _camera;
    }
}
