using Raspberry_Lib.Components;
using System;

namespace Raspberry_Lib.Scenes
{
    internal class TutorialScene : GamePlayScene
    {
        public TutorialScene(Action iOnMainMenu) : base(iOnMainMenu)
        {
        }

        protected sealed override void InitializeUi()
        {
            var uiEntity = CreateEntity("ui");
            uiEntity.AddComponent(new TutorialUiCanvasComponent());
        }
    }
}
