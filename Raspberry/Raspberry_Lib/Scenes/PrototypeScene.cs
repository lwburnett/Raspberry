using LDtkNez;
using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Components;

namespace Raspberry_Lib.Scenes
{
    internal class PrototypeScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting MapScale = new(4);
            public static readonly RenderSetting CharacterStartPositionX = new(64 * 4);
            public static readonly RenderSetting CharacterStartPositionY = new(256 * 4);
        }

        public override void Initialize()
        {
            base.Initialize();

            var characterStartingPos = new Vector2(Settings.CharacterStartPositionX.Value, Settings.CharacterStartPositionY.Value);
            _generator = new ProceduralGenerator(characterStartingPos);

            var map = CreateEntity("map");
            map.AddComponent(new ProceduralRenderer(_generator));
            map.Transform.SetLocalScale(Settings.MapScale.Value);

            var character = CreateEntity("character", characterStartingPos);
            character.Transform.SetLocalScale(Settings.MapScale.Value);
            character.AddComponent(new PrototypeCharacterComponent());
            Camera.Entity.AddComponent(new FollowCamera(character));
        }

        private ProceduralGenerator _generator;
    }
}