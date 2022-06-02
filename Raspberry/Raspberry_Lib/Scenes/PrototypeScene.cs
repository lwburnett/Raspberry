using Microsoft.Xna.Framework;
using Raspberry_Lib.Components;

namespace Raspberry_Lib.Scenes
{
    internal class PrototypeScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting MapScale = new(6);
            public static readonly RenderSetting CharacterStartPositionX = new(64 * 4);
            public static readonly RenderSetting CharacterStartPositionY = new(256 * 4);
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
            character.Transform.SetLocalScale(Settings.MapScale.Value);
            character.AddComponent(new PrototypeCharacterComponent());
            Camera.Entity.AddComponent(new RiverFollowCamera(character, proceduralGenerator));
        }
    }
}