using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib.Components.UI
{
    internal class PlayScreenIntroMenu : MenuBase
    {
        private static class MySettings
        {
            public static readonly RenderSetting CellPadding = new(20);
        }

        public PlayScreenIntroMenu(
            SceneBase iOwner, 
            RectangleF iBounds, 
            string iTitle,
            IEnumerable<string> iLines,
            Action<Button> iOnBegin, 
            Action<Button> iOnMainMenu) : 
            base(iOwner, iBounds)
        {
            _title = iTitle;
            _lines = iLines;
            _onBegin = iOnBegin;
            _onMainMenu = iOnMainMenu;
        }

        private readonly string _title;
        private readonly IEnumerable<string> _lines;
        private readonly Action<Button> _onBegin;
        private readonly Action<Button> _onMainMenu;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            var title = new Label(_title).
                SetFontScale(Settings.TitleFontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            elements.Add(title);

            foreach (var line in _lines)
            {
                var descriptionLine = new Label(line).
                    SetFontScale(Settings.FontScale.Value).
                    SetFontColor(Color.White).
                    SetAlignment(Align.TopLeft);
                elements.Add(descriptionLine);
            }

            var buttonTable = new Table();
            var playAgainButton = new TextButton("Begin", Skin.CreateDefaultSkin());
            playAgainButton.OnClicked += _onBegin;
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
