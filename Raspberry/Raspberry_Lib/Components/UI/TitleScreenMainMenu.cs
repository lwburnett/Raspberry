using System;
using System.Collections.Generic;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components.UI
{
    internal class TitleScreenMainMenu : MenuBase
    {
        public TitleScreenMainMenu(
            RectangleF iBounds, 
            Action<Button> iOnPlay, 
            Action<Button> iOnTutorial, 
            Action<Button> iOnCredits, 
            Action<Button> iOnBack) : base(iBounds)
        {
            _onPlay = iOnPlay;
            _onTutorial = iOnTutorial;
            _onCredits = iOnCredits;
            _onBack = iOnBack;
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

            var tutorialButton = new TextButton("Tutorial", Skin.CreateDefaultSkin());
            tutorialButton.OnClicked += _onTutorial;
            tutorialButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            tutorialButton.SetWidth(Settings.MinButtonWidth.Value);
            tutorialButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(tutorialButton);

            var creditsButton = new TextButton("Credits", Skin.CreateDefaultSkin());
            creditsButton.OnClicked += _onCredits;
            creditsButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            creditsButton.SetWidth(Settings.MinButtonWidth.Value);
            creditsButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(creditsButton);

            var backButton = new TextButton("Back", Skin.CreateDefaultSkin());
            backButton.OnClicked += _onBack;
            backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            backButton.SetWidth(Settings.MinButtonWidth.Value);
            backButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(backButton);

            return elements;
        }

        private readonly Action<Button> _onPlay;
        private readonly Action<Button> _onTutorial;
        private readonly Action<Button> _onCredits;
        private readonly Action<Button> _onBack;
    }
}
