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
            Action<Button> iOnPlayAgain,
            Action<Button> iOnMainMenu) : base(iBounds)
        {
            _iOnPlayAgain = iOnPlayAgain;
            _iOnMainMenu = iOnMainMenu;
        }

        public void SetDistanceTraveled(string iDistanceTraveledString)
        {
            _distanceLabel.SetText($"You Traveled {iDistanceTraveledString}.");
        }

        private readonly Action<Button> _iOnPlayAgain;
        private readonly Action<Button> _iOnMainMenu;
        private Label _distanceLabel;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var title = new Label("You are lost in the desert.").
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(title);

            _distanceLabel = new Label(string.Empty).
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(_distanceLabel);

            var buttonTable = new Table();
            var playAgainButton = new TextButton("Play Again", Skin.CreateDefaultSkin());
            playAgainButton.OnClicked += _iOnPlayAgain;
            playAgainButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            playAgainButton.SetSize(Settings.MinButtonHeight.Value, Settings.MinButtonWidth.Value);
            buttonTable.Add(playAgainButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value).
                Pad(MySettings.CellPadding.Value);

            var mainMenuButton = new TextButton("Main Menu", Skin.CreateDefaultSkin());
            mainMenuButton.OnClicked += _iOnMainMenu;
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
