using Nez;

namespace Raspberry_Lib.Scenes
{
    public class SceneBase : Scene
    {
        public SceneBase()
        {
            AddRenderer(new DefaultRenderer());
        }
    }
}