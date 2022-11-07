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

        public GamePlayScene(Action iOnMainMenu)
        {
            _onMainMenu = iOnMainMenu;
            ClearColor = ContentData.ColorPallets.Desert.Color2;
        }

        public override void Initialize()
        {
            base.Initialize();

            var characterStartingPos = new Vector2(Settings.CharacterStartPositionX.Value, Settings.CharacterStartPositionY.Value);

            var proceduralGenerator = new ProceduralGeneratorComponent();
            var map = CreateEntity("map");
            map.Transform.SetLocalScale(Settings.MapScale.Value);
            map.AddComponent(proceduralGenerator);
            map.AddComponent<ProceduralRenderer>();
            map.AddComponent<RiverParticleManager>();

            InitializeUi();

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * .85f);
            character.AddComponent(new BoatCharacterComponent(OnMainMenu));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));

            SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic, .35f);

#if VERBOSE
            var debugMetricRenderer = CreateEntity("metrics");
            debugMetricRenderer.AddComponent(Verbose.GetRenderer());
#endif
        }

        protected virtual void InitializeUi()
        {
            var uiEntity = CreateEntity("ui");
            uiEntity.AddComponent(new PlayUiCanvasComponent());
        }

        private readonly Action _onMainMenu;

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