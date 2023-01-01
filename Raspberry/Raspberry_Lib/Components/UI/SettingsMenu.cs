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

        public SettingsMenu(RectangleF iBounds, Action<Button> iOnBack) : base(iBounds, b => OnBack(b, iOnBack))
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
            var gameSettings = SettingsManager.GetGameSettings();
            AddCheckboxSettings(table, new[]
            {
                new SettingConfig("Vibrate", gameSettings.Vibrate,
                    iVal =>
                    {
                        SettingsManager.SetGameSettings(new GameSettings(iVal, gameSettings.ScreenShake,
                            gameSettings.Music, gameSettings.Sfx));
                    }),
                new SettingConfig("Screen Shake", gameSettings.ScreenShake,
                    iVal =>
                    {
                        SettingsManager.SetGameSettings(new GameSettings(gameSettings.Vibrate, iVal, gameSettings.Music,
                            gameSettings.Sfx));
                    })
            });
            AddCheckboxSettings(table, new[]
            {
                new SettingConfig("Music", gameSettings.Music,
                    iVal =>
                    {
                        SettingsManager.SetGameSettings(new GameSettings(gameSettings.Vibrate, gameSettings.ScreenShake,
                            iVal, gameSettings.Sfx));
                    }),
                new SettingConfig("Sounds Effects", gameSettings.Sfx,
                    iVal =>
                    {
                        SettingsManager.SetGameSettings(new GameSettings(gameSettings.Vibrate, gameSettings.ScreenShake,
                            gameSettings.Music, iVal));
                    })
            });

            elements.Add(table);

            return elements;
        }

        private class SettingConfig
        {
            public SettingConfig(string iName, bool iInitialValue, Action<bool> iOnChange)
            {
                Name = iName;
                InitialValue = iInitialValue;
                Change = iOnChange;
            }

            public string Name { get; }
            public bool InitialValue { get; }
            public Action<bool> Change { get; }
        }

        private void AddCheckboxSettings(Table iTable, IEnumerable<SettingConfig> iSettings)
        {
            var subTable = new Table();

            foreach (var setting in iSettings)
            {
                var checkbox = new CheckBox(setting.Name, Skin.CreateDefaultSkin());
                checkbox.GetLabel().
                    SetFontScale(MySettings.SettingFontScale.Value).
                    SetFontColor(Color.White);
                checkbox.IsChecked = setting.InitialValue;
                checkbox.OnClicked += _ => setting.Change(checkbox.IsChecked);
                
                checkbox.Pad(Settings.LabelTopPadding.Value);

                subTable.Add(checkbox).SetAlign(Align.Left);

                subTable.Row();
            }

            iTable.Add(subTable);
        }

        private static void OnBack(Button iButton, Action<Button> iParentOnBack)
        {
            SettingsManager.SaveSettings();

            iParentOnBack(iButton);
        }
    }
}
