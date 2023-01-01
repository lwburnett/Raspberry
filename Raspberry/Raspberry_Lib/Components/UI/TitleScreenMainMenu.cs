using System;
using System.Collections.Generic;
using Nez;
using Nez.UI;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib.Components.UI
{
    internal class TitleScreenMainMenu : MenuBase
    {
        public TitleScreenMainMenu(
            SceneBase iOwner,
            RectangleF iBounds, 
            Action<Button> iOnPlay, 
            Action<Button> iOnCredits, 
            Action<Button> iOnSettings, 
            Action<Button> iOnBack) : 
            base(iOwner, iBounds, iOnBack)
        {
            _onPlay = iOnPlay;
            _onCredits = iOnCredits;
            _onSettings = iOnSettings;
        }

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var playButton = new TextButton("Play", Skin.CreateDefaultSkin());
            playButton.OnClicked += _onPlay;
            playButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            playButton.SetWidth(Settings.MinButtonWidth.Value);
            playButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(playButton);

            var settingsButton = new TextButton("Settings", Skin.CreateDefaultSkin());
            settingsButton.OnClicked += _onSettings;
            settingsButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            settingsButton.SetWidth(Settings.MinButtonWidth.Value);
            settingsButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(settingsButton);

            var creditsButton = new TextButton("Credits", Skin.CreateDefaultSkin());
            creditsButton.OnClicked += _onCredits;
            creditsButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            creditsButton.SetWidth(Settings.MinButtonWidth.Value);
            creditsButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(creditsButton);

            return elements;
        }

        private readonly Action<Button> _onPlay;
        private readonly Action<Button> _onCredits;
        private readonly Action<Button> _onSettings;
    }
}
