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

            var parentTable = new Table();

            var buttonTable = CreateButtonTable();
            var recordTable = CreateRecordTable();

            parentTable.Add(buttonTable).
                SetAlign(Align.TopRight).
                SetPadRight(Settings.LabelTopPadding.Value);
            parentTable.Add(recordTable).
                SetAlign(Align.TopRight).
                SetPadLeft(Settings.LabelTopPadding.Value).
                SetPadRight(Settings.LabelTopPadding.Value);

            elements.Add(parentTable);

            return elements;
        }

        private Element CreateButtonTable()
        {
            var buttonTable = new Table();

            var distButton = new TextButton("Distance", Skin.CreateDefaultSkin());
            distButton.OnClicked += _onDistanceChallenge;
            distButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            buttonTable.Add(distButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            buttonTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var timeButton = new TextButton("Time", Skin.CreateDefaultSkin());
            timeButton.OnClicked += _onTimeChallenge;
            timeButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            buttonTable.Add(timeButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            buttonTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var endlessButton = new TextButton("Endless", Skin.CreateDefaultSkin());
            endlessButton.OnClicked += _onEndless;
            endlessButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            buttonTable.Add(endlessButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            buttonTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var tutorialButton = new TextButton("Tutorial", Skin.CreateDefaultSkin());
            tutorialButton.OnClicked += _onTutorial;
            tutorialButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            buttonTable.Add(tutorialButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            buttonTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            return buttonTable;
        }

        private Element CreateRecordTable()
        {
            var recordTable = new Table();

            var data = DataManager.GetGameData();

            string distRecordString;
            if (data.DistChallengeRecordDateTime.HasValue && 
                data.DistChallengeRecord.HasValue && 
                data.DistChallengeRecordDateTime.Value.Date == DateTime.Today.Date)
            {
                distRecordString = data.DistChallengeRecord.Value.ToString();
            }
            else
            {
                distRecordString = "No Record";
            }
            var distRecord = new Label(distRecordString);
            distRecord.SetFontScale(Settings.FontScale.Value);
            recordTable.Add(distRecord).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            recordTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            string timeRecordString;
            if (data.TimeChallengeRecordDateTime.HasValue &&
                data.TimeChallengeRecord.HasValue &&
                data.TimeChallengeRecordDateTime.Value.Date == DateTime.Today.Date)
            {
                timeRecordString = data.TimeChallengeRecord.Value.ToString();
            }
            else
            {
                timeRecordString = "No Record";
            }
            var timeButton = new Label(timeRecordString);
            timeButton.SetFontScale(Settings.FontScale.Value);
            recordTable.Add(timeButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            recordTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            string endlessRecordString;
            if (data.EndlessChallengeRecord.HasValue)
            {
                endlessRecordString = data.EndlessChallengeRecord.Value.ToString();
            }
            else
            {
                endlessRecordString = "No Record";
            }
            var endlessButton = new Label(endlessRecordString);
            endlessButton.SetFontScale(Settings.FontScale.Value);
            recordTable.Add(endlessButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            recordTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var tutorialButton = new Label(string.Empty);
            tutorialButton.SetFontScale(Settings.FontScale.Value);
            recordTable.Add(tutorialButton).
                SetMinWidth(Settings.MinButtonWidth.Value).
                SetMinHeight(Settings.MinButtonHeight.Value);
            recordTable.Row().SetPadTop(Settings.LabelTopPadding.Value);

            return recordTable;
        }
    }
}
