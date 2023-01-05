using Microsoft.Xna.Framework;
using Raspberry_Lib.Components;
using Raspberry_Lib.Content;
using System;
using System.Linq;
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

            public const float StreamSoundVolume = .25f;
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
            
            _isRunning = !iScenario.IntroLines.Any();
            _runTime = 0f;
            _lost = false;

            _riverParticleManager = new RiverParticleManager();

            PostConstructionInitialize();
        }

        public override void Update()
        {
            base.Update();

            if (_isRunning && _uiComponent.ShouldBeAggregatingTime())
            {
                _runTime += Time.DeltaTime;
                _uiComponent.SetPlayTime(_runTime);
            }

            if (_movementComponent == null)
            {
                _movementComponent = _characterComponent.GetComponent<CharacterMovementComponent>();
                _characterComponent.IsPaused = true;
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

        public override void OnStart()
        {
            if (_streamSoundId != null)
                AudioManager.PlaySound(_streamSoundId.Value, true, Settings.StreamSoundVolume, SoundStrategy.Overwrite);

            base.OnStart();
        }

        public override void End()
        {
            if (_streamSoundId != null)
                AudioManager.Unload(_streamSoundId.Value);

            base.End();
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
        private readonly RiverParticleManager _riverParticleManager;

        private int? _streamSoundId;
        private GameSettings _gameSettings;

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
            map.AddComponent(_riverParticleManager);

            _uiComponent = InitializeUi(OnPlayAgain, OnMainMenu);

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * .85f);
            _characterComponent = character.AddComponent(new BoatCharacterComponent(OnLose));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));
            Camera.Entity.AddComponent(new RiverCameraShake());

            if (new System.Random().Next() % 2 == 0)
            {
                SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic1, .25f);
            }
            else
            {
                SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic2, .8f);
            }

            _gameSettings = SettingsManager.GetGameSettings();
            if (_gameSettings.Sfx)
                _streamSoundId = AudioManager.Load(Content, ContentData.AssetPaths.StreamSound);

#if VERBOSE
            var debugMetricRenderer = CreateEntity("metrics");
            debugMetricRenderer.AddComponent(Verbose.GetRenderer());
#endif
        }

        private void OnLose()
        {
            _lost = true;

            if (_streamSoundId != null)
                AudioManager.StopSound(_streamSoundId.Value);
        }

        private void OnPlayEnd(bool iUploadStats)
        {
            _isRunning = false;
            _uiComponent.OnPlayEnd(!iUploadStats, _runTime);
            _characterComponent.IsPaused = true;

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
            _characterComponent.IsPaused = true;
            _riverParticleManager.IsPaused = true;

            if (_streamSoundId != null)
                AudioManager.PauseSound(_streamSoundId.Value);

            ToggleWakeAndEnergyAnimationPause(true);
            _isRunning = false;
        }

        protected void OnResume()
        {
            _characterComponent.IsPaused = false;
            _riverParticleManager.IsPaused = false;

            if (_streamSoundId != null)
                AudioManager.ResumeSound(_streamSoundId.Value);

            ToggleWakeAndEnergyAnimationPause(false);
            _isRunning = true;
        }

        private void ToggleWakeAndEnergyAnimationPause(bool iPause)
        {
            var wakeEmitters =  Entities.FindComponentsOfType<WakeParticleEmitter>();

            foreach (var wakeEmitter in wakeEmitters)
            {
                wakeEmitter.IsPaused = iPause;
            }

            var energyAnimationComponents = Entities.FindComponentsOfType<EnergyAnimationComponent>();

            foreach (var energyAnimationComponent in energyAnimationComponents)
            {
                energyAnimationComponent.IsPaused = iPause;
            }
        }
    }
}