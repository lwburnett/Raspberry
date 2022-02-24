using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Renderers;

namespace Raspberry_Lib.Scenes
{
    internal class PrototypeScene : SceneBase
    {
        public override void Initialize()
        {
            base.Initialize();
            
            var map = CreateEntity("map");
            map.AddComponent(new LDtkMapRenderer(Raspberry_Lib.Content.Content.Prototype.Tilemap, Content));
            map.Transform.SetLocalScale(4);

            var character = CreateEntity("character", new Vector2(0,0));
            
            Camera.Entity.AddComponent(new FollowCamera(character));
        }
    }
}