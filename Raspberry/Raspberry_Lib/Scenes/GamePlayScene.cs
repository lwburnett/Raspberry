using Microsoft.Xna.Framework;
using Raspberry_Lib.Components;
using Raspberry_Lib.Content;
using System;

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


        public GamePlayScene(Action<int?> iOnPlayAgain, Action iOnMainMenu, int? iSeed = null)
        {
            _onMainMenu = iOnMainMenu;
            _onPlayAgain = iOnPlayAgain;
            ClearColor = ContentData.ColorPallets.Desert.Color2;
            _seed = iSeed;

            PostConstructionInitialize();
        }

        protected virtual PlayUiCanvasComponent InitializeUi(Action iOnPlayAgain, Action iOnMainMenu)
        {
            var uiEntity = CreateEntity("ui");
            return uiEntity.AddComponent(new PlayUiCanvasComponent(iOnPlayAgain, iOnMainMenu));
        }

        private readonly Action<int?> _onPlayAgain;
        private readonly Action _onMainMenu;
        private PlayUiCanvasComponent _uiComponent;
        private BoatCharacterComponent _characterComponent;
        private readonly int? _seed;

        // Initialization needs to be after construction so that _seed is initialized
        private void PostConstructionInitialize()
        {
            base.Initialize();

            var characterStartingPos = new Vector2(Settings.CharacterStartPositionX.Value, Settings.CharacterStartPositionY.Value);

            var proceduralGenerator = new ProceduralGeneratorComponent(_seed);
            var map = CreateEntity("map");
            map.Transform.SetLocalScale(Settings.MapScale.Value);
            map.AddComponent(proceduralGenerator);
            map.AddComponent<ProceduralRenderer>();
            map.AddComponent<RiverParticleManager>();

            _uiComponent = InitializeUi(OnPlayAgain, OnMainMenu);

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * .85f);
            _characterComponent = character.AddComponent(new BoatCharacterComponent(OnPlayEnd));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));

            if (new Random().Next() % 2 == 0)
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

        private void OnPlayEnd()
        {
            _uiComponent.OnPlayEnd();
            _characterComponent.OnPlayEnd();
        }

        private void OnPlayAgain()
        {
#if VERBOSE
            Verbose.ClearCollidersToRender();
            Verbose.ClearMetrics();
#endif
            _onPlayAgain(_seed);

        }

        private void OnMainMenu()
        {
#if VERBOSE
            Verbose.ClearCollidersToRender();
            Verbose.ClearMetrics();
#endif

            _onMainMenu();
        }
    }
}