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
            Action<Button> iOnBack) : base(iBounds)
        {
            _iOnDistanceChallenge = iOnDistanceChallenge;
            _iOnTimeChallenge = iOnTimeChallenge;
            _iOnEndless = iOnEndless;
            _iOnBack = iOnBack;
        }

        private readonly Action<Button> _iOnDistanceChallenge;
        private readonly Action<Button> _iOnTimeChallenge;
        private readonly Action<Button> _iOnEndless;
        private readonly Action<Button> _iOnBack;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var distButton = new TextButton("Dist Challenge", Skin.CreateDefaultSkin());
            distButton.OnClicked += _iOnDistanceChallenge;
            distButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            distButton.SetWidth(Settings.MinButtonWidth.Value);
            distButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(distButton);

            var timeButton = new TextButton("Time Challenge", Skin.CreateDefaultSkin());
            timeButton.OnClicked += _iOnTimeChallenge;
            timeButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            timeButton.SetWidth(Settings.MinButtonWidth.Value);
            timeButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(timeButton);

            var endlessButton = new TextButton("Endless", Skin.CreateDefaultSkin());
            endlessButton.OnClicked += _iOnEndless;
            endlessButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            endlessButton.SetWidth(Settings.MinButtonWidth.Value);
            endlessButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(endlessButton);

            var backButton = new TextButton("Back", Skin.CreateDefaultSkin());
            backButton.OnClicked += _iOnBack;
            backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            backButton.SetWidth(Settings.MinButtonWidth.Value);
            backButton.SetHeight(Settings.MinButtonHeight.Value);
            elements.Add(backButton);

            return elements;
        }
    }
}
