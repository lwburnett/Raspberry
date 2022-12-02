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

            _uiComponent = InitializeUi(OnMainMenu);

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * .85f);
            _characterComponent = character.AddComponent(new BoatCharacterComponent(OnPlayEnd));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));

            SetBackgroundSong(ContentData.AssetPaths.PlayScreenMusic, .35f);

#if VERBOSE
            var debugMetricRenderer = CreateEntity("metrics");
            debugMetricRenderer.AddComponent(Verbose.GetRenderer());
#endif
        }

        protected virtual PlayUiCanvasComponent InitializeUi(System.Action iOnMainMenu)
        {
            var uiEntity = CreateEntity("ui");
            return uiEntity.AddComponent(new PlayUiCanvasComponent(iOnMainMenu));
        }

        private readonly Action _onMainMenu;
        private PlayUiCanvasComponent _uiComponent;
        private BoatCharacterComponent _characterComponent;

        private void OnPlayEnd()
        {
            _uiComponent.OnPlayEnd();
            _characterComponent.OnPlayEnd();
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