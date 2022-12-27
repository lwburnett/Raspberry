using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components.UI
{
    internal class SettingsMenu : MenuBase
    {
        private static class MySettings
        {
            public static readonly RenderSetting Title1FontScale = new(6);
            public static readonly RenderSetting Title2FontScale = new(5.5f);

            public static readonly RenderSetting SettingFontScale = new(5);
        }

        public SettingsMenu(RectangleF iBounds, Action<Button> iOnBack) : base(iBounds, iOnBack)
        {
            _bounds = iBounds;
        }

        private readonly RectangleF _bounds;

        protected override IEnumerable<Element> InitializeTableElements()
        {
            var elements = new List<Element>();

            // Settings title
            var settingsTitle = new Label("Settings").
                SetFontScale(MySettings.Title1FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.Center);
            settingsTitle.SetAlignment(Align.Center);
            elements.Add(settingsTitle);

            // Category labels
            var table = new Table();
            table.SetWidth(_bounds.Width);

            var gameplayLabel = new Label("Gameplay").
                SetFontScale(MySettings.Title2FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.Center);
            settingsTitle.SetAlignment(Align.Center);
            table.Add(gameplayLabel);

            var audioLabel = new Label("Audio").
                SetFontScale(MySettings.Title2FontScale.Value).
                SetFontColor(Color.White).
                SetAlignment(Align.Center);
            settingsTitle.SetAlignment(Align.Center);
            table.Add(audioLabel);

            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            // Settings

            AddCheckboxSettings(table, new[] { "Vibrate", "Screen Shake" });
            AddCheckboxSettings(table, new[] { "Music", "Sound Effects" });

            elements.Add(table);

            return elements;
        }

        private void AddCheckboxSettings(Table iTable, IEnumerable<string> iNames)
        {
            var subTable = new Table();

            foreach (var name in iNames)
            {
                var checkbox = new CheckBox(name, Skin.CreateDefaultSkin());
                checkbox.GetLabel().
                    SetFontScale(MySettings.SettingFontScale.Value).
                    SetFontColor(Color.White);
                
                checkbox.Pad(Settings.LabelTopPadding.Value);

                subTable.Add(checkbox).SetAlign(Align.Left);

                subTable.Row();
            }

            iTable.Add(subTable);
        }
    }
}
