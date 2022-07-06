﻿using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components
{
    internal class PlayUiCanvasComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting DistanceToMetersFactor = new(20);
        }

        public override void OnAddedToEntity()
        {
            var canvas = Entity.AddComponent(new UICanvas());
            _distanceLabel = canvas.Stage.AddElement(new Label("0 m"));
            _distanceLabel.SetPosition(100f, 100f);
            _distanceLabel.SetFontScale(4);

            // This needs to match the render layer of the ScreenSpaceRenderer in SceneBase ctor
            canvas.SetRenderLayer(-1);
        }

        public int BeginPlayOrder => 97;
        public void OnBeginPlay()
        {
            _movementComponent = Entity.Scene.FindEntity("character").GetComponent<CharacterMovementComponent>();

            System.Diagnostics.Debug.Assert(_movementComponent != null);
        }

        public void Update()
        {
            if (_movementComponent == null)
                return;

            var distanceTraveled = (int)Mathf.Round(_movementComponent.TotalDistanceTraveled / Settings.DistanceToMetersFactor.Value);
            _distanceLabel.SetText($"{distanceTraveled} m");
        }

        private CharacterMovementComponent _movementComponent;
        private Label _distanceLabel;
    }
}