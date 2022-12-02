using Raspberry_Lib.Components;
using System;

namespace Raspberry_Lib.Scenes
{
    internal class TutorialScene : GamePlayScene
    {
        public TutorialScene(Action iOnMainMenu) : base(iOnMainMenu)
        {
        }

        protected sealed override PlayUiCanvasComponent InitializeUi(System.Action iOnMainMenu)
        {
            var uiEntity = CreateEntity("ui");
            return uiEntity.AddComponent(new TutorialUiCanvasComponent(iOnMainMenu));
        }
    }
}
