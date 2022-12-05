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
            public static readonly RenderSetting ButtonPositionX = new(100);
            public static readonly RenderSetting ButtonPositionY = new(75);
            public static readonly RenderSetting ButtonWidth = new(300);
            public static readonly RenderSetting ButtonHeight = new(80);

            public static readonly RenderSetting HeaderFontScale = new(6f);
            public static readonly RenderSetting SubHeaderFontScale = new(5f);
            public static readonly RenderSetting ContentFontScale = new(3.5f);

            public static readonly RenderSetting MarginBig = new(75);
            public static readonly RenderSetting MarginMedium = new(50);
            public static readonly RenderSetting MarginSmall = new(30);

            public static readonly Color FontColor = Color.White;

            public static readonly List<string> PlayTesters = new()
            {
                "Sarah Lizarraga",
                "Mark Robinson",
                "Nolan Perry-Arnold",
                "Greg Burnett",
                "Donny Sison",
                "Stephen Davick",
                "Nick Nooney",

            };

            public static readonly List<string> AssetCredits = new()
            {
                "Ocean Of Ice by McFunkypants",
                "The Soul-Crushing Monotony Of Isolation (Instrumental Mix) by Punch Deck"
            };

            public static readonly RenderSetting CreditScrollPerSecond = new(20f);
        }

        public CreditsScene(System.Action iOnBack)
        {
            _onBack = iOnBack;
            _scrollDown = true;
            ClearColor = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            var canvas = CreateEntity("UiCanvas").AddComponent(new UICanvas());
            canvas.IsFullScreen = true;
            
            var mainTable = new Table().PadTop(Settings.MarginBig.Value).PadBottom(Settings.MarginBig.Value);
            mainTable.Row();
            mainTable.Add(CreateLabel("Credits", Settings.HeaderFontScale.Value));

            mainTable.Row().SetPadTop(Settings.MarginBig.Value);

            mainTable.Add(CreateLabel("Development", Settings.SubHeaderFontScale.Value).SetAlignment(Align.Center));

            var devTable = new Table();
            devTable.Add(CreateLabel("Created by", Settings.ContentFontScale.Value).SetAlignment(Align.Right)).
                SetPadRight(Settings.MarginSmall.Value);
            devTable.Add(CreateLabel("Luke Burnett", Settings.ContentFontScale.Value).SetAlignment(Align.Left)).
                SetPadLeft(Settings.MarginSmall.Value);

            AddPlayTestersToTable(ref devTable);

            mainTable.Row().SetPadTop(Settings.MarginMedium.Value);
            mainTable.Add(devTable);
            mainTable.Row().SetPadTop(Settings.MarginBig.Value);

            mainTable.Add(CreateLabel("Assets", Settings.SubHeaderFontScale.Value).SetAlignment(Align.Center));
            mainTable.Row().SetPadTop(Settings.MarginMedium.Value);
            
            var assetTable = new Table();
            AddAssetsToTable(ref assetTable);

            mainTable.Add(assetTable);
            mainTable.Row().SetPadTop(Settings.MarginBig.Value);

            _creditPane = new ScrollPane(mainTable);
            _creditPane.SetBounds(0, 0, Screen.Width, Screen.Height);
            _creditPane.SetScrollY(0f);
            canvas.Stage.AddElement(_creditPane);

            var backButton = canvas.Stage.AddElement(new TextButton("Back", Skin.CreateDefaultSkin()));
            backButton.OnClicked += OnBack;
            backButton.GetLabel().SetFontScale(Settings.SubHeaderFontScale.Value);
            backButton.SetBounds(
                Settings.ButtonPositionX.Value,
                Settings.ButtonPositionY.Value,
                Settings.ButtonWidth.Value,
                Settings.ButtonHeight.Value);
        }

        public override void Update()
        {
            base.Update();

            var currentScroll = _creditPane.GetScrollY();
            if (_scrollDown)
            {
                var potentialScrollY = currentScroll + Settings.CreditScrollPerSecond.Value * Time.DeltaTime;
                var maxY = _creditPane.GetMaxY();
                if (potentialScrollY <= maxY)
                    _creditPane.SetScrollY(potentialScrollY);
                else
                    _scrollDown = false;
            }
            else
            {
                var potentialScrollY = currentScroll - Settings.CreditScrollPerSecond.Value * Time.DeltaTime;
                if (potentialScrollY >= 0f)
                    _creditPane.SetScrollY(potentialScrollY);
                else
                    _scrollDown = true;
            }
        }

        private readonly System.Action _onBack;
        private ScrollPane _creditPane;
        private bool _scrollDown;

        private void OnBack(Button iButton)
        {
            _onBack();
        }

        private Label CreateLabel(string iText, float iFontScale) =>
            new Label(iText).SetFontScale(iFontScale).SetFontColor(Settings.FontColor);

        private void AddPlayTestersToTable(ref Table ioTable)
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

        private void AddAssetsToTable(ref Table ioTable)
        {
            for (var ii = 0; ii < Settings.AssetCredits.Count; ii++)
            {
                var assetCredit = Settings.AssetCredits[ii];

                ioTable.Add(CreateLabel(assetCredit, Settings.ContentFontScale.Value));

                if (ii != Settings.AssetCredits.Count - 1)
                    ioTable.Row().SetPadTop(Settings.MarginSmall.Value);
            }
        }
    }
}
