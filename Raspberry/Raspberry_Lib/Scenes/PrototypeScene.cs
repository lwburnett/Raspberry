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

            CreateEntity("map").AddComponent(new LDtkMapRenderer(Raspberry_Lib.Content.Content.Prototype.Tilemap, Content));
            //mapEntity.RenderLayer = -1;
            
            var character = CreateEntity("character", new Vector2(0,0));
            
            Camera.Entity.AddComponent(new FollowCamera(character));
        }
    }
}