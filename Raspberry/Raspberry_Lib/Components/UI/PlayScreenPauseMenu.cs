using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components.UI
{
    internal class PlayScreenPauseMenu : MenuBase
    {

        public PlayScreenPauseMenu(
            RectangleF iBounds,
            Action<Button> iOnResume,
            Action<Button> iOnRestart,
            Action<Button> iOnMainMenu) : base(iBounds)
        {
            _onResume = iOnResume;
            _onRestart = iOnRestart;
            _onMainMenu = iOnMainMenu;
        }

        private readonly Action<Button> _onResume;
        private readonly Action<Button> _onRestart;
        private readonly Action<Button> _onMainMenu;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var pauseTitle = new Label("Pause").
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            pauseTitle.SetAlignment(Align.Center);
            elements.Add(pauseTitle);

            var resumeButton = new TextButton("Resume", Skin.CreateDefaultSkin());
            resumeButton.OnClicked += _onResume;
            resumeButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            resumeButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            elements.Add(resumeButton);

            var restartButton = new TextButton("Restart", Skin.CreateDefaultSkin());
            restartButton.OnClicked += _onRestart;
            restartButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            restartButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            elements.Add(restartButton);

            var mainMenuButton = new TextButton("Main Menu", Skin.CreateDefaultSkin());
            mainMenuButton.OnClicked += _onMainMenu;
            mainMenuButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            mainMenuButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            elements.Add(mainMenuButton);

            return elements;
        }
    }
}
