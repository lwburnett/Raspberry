﻿using System;
using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Scenes;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Raspberry_Lib
{
    public class GameMaster : Core
    {
        private static class Settings
        {
            public static readonly Vector2 TargetScreenSize = new(2204, 1014);
        }

        public GameServiceContainer Service => Core.Services;

        public GameMaster(bool iFullScreen, bool iIsTouch) : base(windowTitle: "Concurrent Streams")
        {
            _fullScreen = iFullScreen;
            _isTouch = iIsTouch;
            IsMouseVisible = !iIsTouch;
        }
        
        private readonly bool _fullScreen;
        private readonly bool _isTouch;

        protected override void Initialize()
        {
            base.Initialize();

            if (_isTouch)
                Input.Touch.EnableTouchSupport();

#if VERBOSE
            DebugRenderEnabled = true;
#endif

            var windowBounds = GraphicsDevice.DisplayMode;
            var renderScaleFactor = windowBounds.Width / Settings.TargetScreenSize.X;
            PlatformUtils.SetRenderScale(renderScaleFactor);

            if (_fullScreen)
            {
                Window.AllowUserResizing = false;
                ExitOnEscapeKeypress = false;

                Screen.IsFullscreen = true;
                Screen.SetSize(windowBounds.Width, windowBounds.Height);
                Screen.ApplyChanges();
            }
            else
            {
                Window.AllowUserResizing = true;
                ExitOnEscapeKeypress = true;
                PauseOnFocusLost = false;

                // Dumb work around to make it so the debug window isn't so annoyingly obtrusive
                Screen.SetSize((int)(renderScaleFactor * Settings.TargetScreenSize.X * .95f), (int)(renderScaleFactor * Settings.TargetScreenSize.Y * .95f));
            }

            SettingsManager.Initialize();
            DataManager.Initialize();
            SkinManager.Initialize(Content);

            Batcher.UseFnaHalfPixelMatrix = true;
            Scene = new MainMenuScene(OnPlay, OnTutorial, OnCredits);
        }

        private void OnMainMenu()
        {
            Scene = new MainMenuScene(OnPlay, OnTutorial, OnCredits);
        }

        private void OnPlay(Scenario iScenario)
        {
            Scene = new GamePlayScene(OnPlay, OnMainMenu, iScenario);
        }

        private void OnTutorial()
        {
            Scene = new TutorialScene(OnTutorial, OnMainMenu);
        }

        private void OnCredits()
        {
            Scene = new CreditsScene(OnMainMenu);
        }
    }
}
