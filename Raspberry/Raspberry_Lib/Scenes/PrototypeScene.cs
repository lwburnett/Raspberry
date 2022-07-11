using Microsoft.Xna.Framework;
using Raspberry_Lib.Components;

namespace Raspberry_Lib.Scenes
{
    internal class PrototypeScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting MapScale = new(3);
            public static readonly RenderSetting CharacterStartPositionX = new(64 * 4);
            public static readonly RenderSetting CharacterStartPositionY = new(256 * 4);
        }

        public PrototypeScene(System.Action iOnFatalCollision)
        {
            _onFatalCollision = iOnFatalCollision;
            ClearColor = new Color(69, 198, 88);
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

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value * 2);
            character.AddComponent(new PrototypeCharacterComponent(OnFatalCollision));
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));

            var uiEntity = CreateEntity("ui");
            uiEntity.AddComponent(new PlayUiCanvasComponent());

#if VERBOSE
            var debugMetricRenderer = CreateEntity("metrics");
            debugMetricRenderer.AddComponent(Verbose.GetRenderer());
#endif
        }

        private readonly System.Action _onFatalCollision;

        private void OnFatalCollision()
        {
            _onFatalCollision();
        }
    }
}