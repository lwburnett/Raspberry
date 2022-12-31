using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components.UI
{
    internal class PlayScreenStatsMenu : MenuBase
    {
        private static class MySettings
        {
            public static readonly RenderSetting CellPadding = new(20);
        }

        public PlayScreenStatsMenu(
            RectangleF iBounds,
            string iTitle,
            IEnumerable<string> iLoseLines,
            IEnumerable<string> iEndLines,
            Action<Button> iOnPlayAgain,
            Action<Button> iOnMainMenu) : base(iBounds)
        {
            _title = iTitle;
            _loseLines = iLoseLines;
            _endLines = iEndLines;
            _onPlayAgain = iOnPlayAgain;
            _onMainMenu = iOnMainMenu;
        }

        public void SetData(bool iLose, string iDistanceTraveledString, float iTime)
        {
            var lines = iLose ? _loseLines : _endLines;

            var str = string.Join("\n", lines);

            _descriptionLabel.SetText(str);
            _distanceLabel.SetText($"You Traveled {iDistanceTraveledString} in {TimeSpan.FromSeconds(iTime):mm':'ss}.");
        }

        private readonly string _title;
        private readonly IEnumerable<string> _loseLines;
        private readonly IEnumerable<string> _endLines;
        private readonly Action<Button> _onPlayAgain;
        private readonly Action<Button> _onMainMenu;
        private Label _distanceLabel;
        private Label _descriptionLabel;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var title = new Label(_title).
                SetFontScale(Settings.TitleFontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(title);

            _descriptionLabel = new Label(string.Empty).
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(_descriptionLabel);

            _distanceLabel = new Label(string.Empty).
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(_distanceLabel);

            var buttonTable = new Table();
            var playAgainButton = new TextButton("Play Again", Skin.CreateDefaultSkin());
            playAgainButton.OnClicked += _onPlayAgain;
            playAgainButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            playAgainButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            buttonTable.Add(playAgainButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value).
                Pad(MySettings.CellPadding.Value);

            var mainMenuButton = new TextButton("Main Menu", Skin.CreateDefaultSkin());
            mainMenuButton.OnClicked += _onMainMenu;
            mainMenuButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            mainMenuButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            buttonTable.Add(mainMenuButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value).
                Pad(MySettings.CellPadding.Value);

            elements.Add(buttonTable);

            return elements;
        }
    }
}
