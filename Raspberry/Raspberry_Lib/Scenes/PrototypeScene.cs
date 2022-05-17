using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Components;
using LDtkNez;

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
            
            var map = CreateEntity("map");
            map.AddComponent(new LDtkNezRenderer(Raspberry_Lib.Content.Content.Prototype.Tilemap, Content));
            map.Transform.SetLocalScale(Settings.MapScale.Value);

            var character = CreateEntity("character", new Vector2(Settings.CharacterStartPositionX.Value, Settings.CharacterStartPositionY.Value));
            character.Transform.SetLocalScale(Settings.MapScale.Value);
            character.AddComponent(new PrototypeCharacterComponent());
            Camera.Entity.AddComponent(new FollowCamera(character));
        }
    }
}