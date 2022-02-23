using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Scenes
{
    internal class PrototypeScene : SceneBase
    {
        public override void Initialize()
        {
            base.Initialize();

            // var mapEntity = CreateEntity("map").AddComponent(new LDtkMapRenderer(Raspberry_Lib.Content.Content.Prototype.Tilemap, Content));
            // mapEntity.RenderLayer = -1;
            //
            // var character = CreateEntity("character", new Vector2(0,0));
            //
            // Camera.Entity.AddComponent(new FollowCamera(character));

            var mapEntity = CreateEntity("tiled-map-entity");
            var map = Content.LoadTiledMap(Raspberry_Lib.Content.Content.Prototype.TiledMap);
            var renderer = mapEntity.AddComponent(new TiledMapRenderer(map));

            var character = CreateEntity("character", new Vector2(0,0));
            
            Camera.Entity.AddComponent(new FollowCamera(character));
        }
    }
}