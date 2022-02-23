using Nez;

namespace Raspberry_Lib.Scenes
{
    internal class SceneBase : Scene
    {
        public SceneBase()
        {
            AddRenderer(new DefaultRenderer());
        }
    }
}