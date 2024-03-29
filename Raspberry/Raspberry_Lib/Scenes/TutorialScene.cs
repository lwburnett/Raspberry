﻿using Raspberry_Lib.Components;
using System;

namespace Raspberry_Lib.Scenes
{
    internal class TutorialScene : GamePlayScene
    {
        public TutorialScene(Action iOnPlayAgain, Action iOnMainMenu) : base(_ => iOnPlayAgain(), iOnMainMenu, Scenario.CreateTutorialScenario())
        {
        }

        protected sealed override PlayUiCanvasComponent InitializeUi(Action iOnPlayAgain, Action iOnMainMenu)
        {
            var uiEntity = CreateEntity("ui");
            return uiEntity.AddComponent(new TutorialUiCanvasComponent(iOnPlayAgain, iOnMainMenu, OnPause, OnResume));
        }
    }
}
