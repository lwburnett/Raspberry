using System;
using System.Collections.Generic;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components.UI
{
    internal class TitleScreenPlayMenu : MenuBase
    {
        public TitleScreenPlayMenu(
            RectangleF iBounds,
            Action<Button> iOnDistanceChallenge,
            Action<Button> iOnTimeChallenge,
            Action<Button> iOnEndless,
            Action<Button> iOnTutorial,
            Action<Button> iOnBack) : base(iBounds, iOnBack)
        {
            _onDistanceChallenge = iOnDistanceChallenge;
            _onTimeChallenge = iOnTimeChallenge;
            _onEndless = iOnEndless;
            _onTutorial = iOnTutorial;
        }

        private readonly Action<Button> _onDistanceChallenge;
        private readonly Action<Button> _onTimeChallenge;
        private readonly Action<Button> _onEndless;
        private readonly Action<Button> _onTutorial;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var distButton = new TextButton("Dist Challenge", Skin.CreateDefaultSkin());
            distButton.OnClicked += _onDistanceChallenge;
            distButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            distButton.SetWidth(Settings.MinButtonWidth.Value);
            distButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(distButton);

            var timeButton = new TextButton("Time Challenge", Skin.CreateDefaultSkin());
            timeButton.OnClicked += _onTimeChallenge;
            timeButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            timeButton.SetWidth(Settings.MinButtonWidth.Value);
            timeButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(timeButton);

            var endlessButton = new TextButton("Endless", Skin.CreateDefaultSkin());
            endlessButton.OnClicked += _onEndless;
            endlessButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            endlessButton.SetWidth(Settings.MinButtonWidth.Value);
            endlessButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(endlessButton);

            var tutorialButton = new TextButton("Tutorial", Skin.CreateDefaultSkin());
            tutorialButton.OnClicked += _onTutorial;
            tutorialButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            tutorialButton.SetWidth(Settings.MinButtonWidth.Value);
            tutorialButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(tutorialButton);

            return elements;
        }
    }
}
