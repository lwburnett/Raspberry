using Microsoft.Xna.Framework;
using Raspberry_Lib.Components;
using Raspberry_Lib.Content;
using System;
using Nez;

namespace Raspberry_Lib.Scenes
{
    internal class GamePlayScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting MapScale = new(3);
            public static readonly RenderSetting CharacterStartPositionX = new(64 * 4);
            public static readonly RenderSetting CharacterStartPositionY = new(256 * 4);
        }


        public GamePlayScene(
            Action<Scenario> iOnPlayAgain, 
            Action iOnMainMenu,
            Scenario iScenario)
        {
            _onMainMenu = iOnMainMenu;
            _onPlayAgain = iOnPlayAgain;
            _scenario = iScenario;

            ClearColor = ContentData.ColorPallets.Desert.Color2;
            _isRunning = true;
            _runTime = 0f;
            _lost = false;

            PostConstructionInitialize();
        }

        public override void Update()
        {
            base.Update();

            if (_isRunning)
                _runTime += Time.DeltaTime;

            if (_movementComponent == null)
            {
                _movementComponent = _characterComponent.GetComponent<CharacterMovementComponent>();
                _characterComponent.TogglePause(true);
            }

            if (_scenario.HaveLost(_movementComponent.TotalDistanceTraveled, _runTime, !_lost))
            {
                OnPlayEnd(false);
            }
            else if (_scenario.HaveEnded(_movementComponent.TotalDistanceTraveled, _runTime, !_lost))
            {
                OnPlayEnd(true);
            }
        }

        protected virtual PlayUiCanvasComponent InitializeUi(Action iOnPlayAgain, Action iOnMainMenu)
        {
            var uiEntity = CreateEntity("ui");
            return uiEntity.AddComponent(
                new PlayUiCanvasComponent(iOnPlayAgain, iOnMainMenu, OnPause, OnResume, _scenario));
        }

        private readonly Action<Scenario> _onPlayAgain;
        private readonly Action _onMainMenu;
        private readonly Scenario _scenario;

        private PlayUiCanvasComponent _uiComponent;
        private BoatCharacterComponent _characterComponent;
        private bool _isRunning;
        private float _runTime;
        private bool _lost;
        private CharacterMovementComponent _movementComponent;

        // Initialization needs to be after construction so that _seed is initialized
        private void PostConstructionInitialize()
        {
            base.Initialize();

            var characterStartingPos = new Vector2(Settings.CharacterStartPositionX.Value, Settings.CharacterStartPositionY.Value);

            var proceduralGenerator = new ProceduralGeneratorComponent(_scenario.Seed);
            var map = CreateEntity("map");
            map.Transform.SetLocalScale(Settings.MapScale.Value);
            map.AddComponent(proceduralGenerator);
            map.AddComponent<ProceduralRenderer>();
            map.AddComponent<RiverParticleManager>();

            _uiComponent = InitializeUi(OnPlayAgain, OnMainMenu);

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * .85f);
            _characterComponent = character.AddComponent(new BoatCharacterComponent(OnLose));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));

            if (new System.Random().Next() % 2 == 0)
            {
                SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic1, .35f);
            }
            else
            {
                SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic2, 1.0f);
            }

#if VERBOSE
            var debugMetricRenderer = CreateEntity("metrics");
            debugMetricRenderer.AddComponent(Verbose.GetRenderer());
#endif
        }

        private void OnLose()
        {
            _lost = true;
        }

        private void OnPlayEnd(bool iUploadStats)
        {
            _isRunning = false;
            _uiComponent.OnPlayEnd(!iUploadStats, _runTime);
            _characterComponent.TogglePause(true);

            if (iUploadStats)
            {
                _scenario.RegisterStats(_movementComponent.TotalDistanceTraveled, _runTime);
            }
        }

        private void OnPlayAgain()
        {
#if VERBOSE
            Verbose.ClearCollidersToRender();
            Verbose.ClearMetrics();
#endif
            _onPlayAgain(_scenario);

        }

        private void OnMainMenu()
        {
#if VERBOSE
            Verbose.ClearCollidersToRender();
            Verbose.ClearMetrics();
#endif

            _onMainMenu();
        }

        protected void OnPause()
        {
            _characterComponent.TogglePause(true);
            _isRunning = false;
        }

        protected void OnResume()
        {
            _characterComponent.TogglePause(false);
            _isRunning = true;
        }
    }
}