using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib
{
    internal abstract class MenuBase : Group
    {
        protected static class Settings
        {
            public static readonly Color TextBoxBackgroundTextureColor = new(112, 128, 144, 200);
            public static readonly RenderSetting LabelTopPadding = new(20);

            public static readonly RenderSetting TitleFontScale = new(6);
            public static readonly RenderSetting FontScale = new(5);
            public static readonly RenderSetting MinButtonWidth = new(300);
            public static readonly RenderSetting MinButtonHeight = new(80);


            public static readonly RenderSetting BackButtonXOffset = new(20);
            public static readonly RenderSetting BackButtonYOffset = new(20);
        }

        protected MenuBase(SceneBase iOwner, RectangleF iBounds, Action<Button> iOnBack = null)
        {
            Owner = iOwner;
            _menu = new Table();
            _menu.SetBounds(iBounds.X, iBounds.Y, iBounds.Width, iBounds.Height);
            _background = SkinManager.GetMenuBackgroundSprite();
            _skin = SkinManager.GetGameUiSkin();

            _onBack = iOnBack;
        }

        public void Initialize()
        {
            var elements = InitializeTableElements();

            LayoutTable(elements, _background);
            AddElement(_menu);

            if (_onBack != null)
            {
                _backButton = new TextButton("Back", _skin);
                _backButton.OnClicked += _onBack;
                _backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
                _backButton.SetBounds(
                    _menu.GetX() + Settings.BackButtonXOffset.Value,
                    _menu.GetY() + Settings.BackButtonYOffset.Value,
                    Settings.MinButtonWidth.Value / 2,
                    Settings.MinButtonHeight.Value);

                AddElement(_backButton);
            }
        }

        public override void Draw(Batcher batcher, float parentAlpha)
        {
            base.Draw(batcher, parentAlpha);
            _menu.Draw(batcher, parentAlpha);
            _backButton?.Draw(batcher, parentAlpha);
        }

        private readonly Table _menu;
        private TextButton _backButton;
        private readonly NinePatchDrawable _background;
        private readonly Action<Button> _onBack;
        private readonly Skin _skin;

        protected SceneBase Owner;

        protected abstract IEnumerable<Element> InitializeTableElements();

        private void LayoutTable(IEnumerable<Element> iElements, Nez.UI.IDrawable iBackground)
        {
            _menu.SetBackground(iBackground);
            _menu.Row().SetPadTop(Settings.LabelTopPadding.Value);

            foreach (var element in iElements)
            {
                _menu.Add(element).
                    SetMinHeight(Settings.MinButtonHeight.Value).
                    SetMinWidth(Settings.MinButtonWidth.Value);
                _menu.Row().SetPadTop(Settings.LabelTopPadding.Value);
            }
        }
    }
}
