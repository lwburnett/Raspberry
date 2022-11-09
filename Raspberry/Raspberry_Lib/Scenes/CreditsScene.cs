using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Scenes
{
    internal class CreditsScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting HeaderFontScale = new(6f);
            public static readonly RenderSetting SubHeaderFontScale = new(5f);
            public static readonly RenderSetting ContentFontScale = new(3.5f);

            public static readonly RenderSetting MarginBig = new(75);
            public static readonly RenderSetting MarginMedium = new(50);
            public static readonly RenderSetting MarginSmall = new(25);

            public static readonly Color FontColor = Color.White;

            public static readonly List<string> PlayTesters = new()
            {
                "Sarah Lizarraga",
                "Mark Robinson",
                "Nolan Perry-Arnold",
                "Greg Burnett"
            };

            public static readonly RenderSetting MinButtonHeight = new(80);
        }

        public CreditsScene(System.Action iOnBack)
        {
            _onBack = iOnBack;

            ClearColor = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            var canvas = CreateEntity("UiCanvas").AddComponent(new UICanvas());
            canvas.IsFullScreen = true;
            
            var mainTable = new Table().SetFillParent(true).PadTop(Settings.MarginBig.Value);
            mainTable.Add(CreateLabel("Credits", Settings.HeaderFontScale.Value));

            mainTable.Row().SetPadTop(Settings.MarginBig.Value);

            mainTable.Add(CreateLabel("Development", Settings.SubHeaderFontScale.Value).SetAlignment(Align.Center));

            var devTable = new Table();
            devTable.Add(CreateLabel("Created by", Settings.ContentFontScale.Value).SetAlignment(Align.Right)).
                SetPadRight(Settings.MarginSmall.Value);
            devTable.Add(CreateLabel("Luke Burnett", Settings.ContentFontScale.Value).SetAlignment(Align.Left)).
                SetPadLeft(Settings.MarginSmall.Value);

            AddPlayTesters(ref devTable);
            //devTable.SetPosition(0, 0);

            mainTable.Row().SetPadTop(Settings.MarginMedium.Value);
            mainTable.Add(devTable);//.SetAlign(Align.Center);
            mainTable.Row().SetPadTop(Settings.MarginBig.Value).SetPadBottom(Settings.MarginBig.Value);

            var backButton = new TextButton("Back", Skin.CreateDefaultSkin());
            backButton.OnClicked += OnBack;
            backButton.GetLabel().SetFontScale(Settings.SubHeaderFontScale.Value);
            mainTable.Add(backButton).SetMinHeight(Settings.MinButtonHeight.Value);

            var scrollPane = new ScrollPane(mainTable);
            scrollPane.SetBounds(0, 0, Screen.Width, Screen.Height);
            canvas.Stage.AddElement(scrollPane);
        }

        private readonly System.Action _onBack;

        private void OnBack(Button iButton)
        {
            _onBack();
        }

        private Label CreateLabel(string iText, float iFontScale) =>
            new Label(iText).SetFontScale(iFontScale).SetFontColor(Settings.FontColor);

        private void AddPlayTesters(ref Table ioTable)
        {
            for (var ii = 0; ii < Settings.PlayTesters.Count; ii++)
            {
                var playTester = Settings.PlayTesters[ii];

                if (ii == 0) 
                    ioTable.Row().SetPadTop(Settings.MarginSmall.Value);

                ioTable.Add(CreateLabel("Play Tester", Settings.ContentFontScale.Value).SetAlignment(Align.Right)).
                    SetPadRight(Settings.MarginSmall.Value);
                ioTable.Add(CreateLabel(playTester, Settings.ContentFontScale.Value).SetAlignment(Align.Left)).
                    SetPadLeft(Settings.MarginSmall.Value);

                if (ii != Settings.PlayTesters.Count - 1)
                    ioTable.Row().SetPadTop(Settings.MarginSmall.Value);
            }
        }
    }
}
