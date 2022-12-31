using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.UI;

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

        protected MenuBase(RectangleF iBounds, Action<Button> iOnBack = null)
        {
            _menu = new Table();
            _menu.SetBounds(iBounds.X, iBounds.Y, iBounds.Width, iBounds.Height);
            _background = CreateBackgroundTexture(iBounds.Size);

            _onBack = iOnBack;
        }

        public void Initialize()
        {
            var elements = InitializeTableElements();

            LayoutTable(elements, _background);
            AddElement(_menu);

            if (_onBack != null)
            {
                _backButton = new TextButton("Back", Skin.CreateDefaultSkin());
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
        private readonly Texture2D _background;
        private readonly Action<Button> _onBack;

        protected abstract IEnumerable<Element> InitializeTableElements();

        private Texture2D CreateBackgroundTexture(Vector2 iSize)
        {
            var size = new Point((int)iSize.X, (int)iSize.Y);

            var textureData = new Color[size.X * size.Y];
            for (var ii = 0; ii < size.X * size.Y; ii++)
            {
                textureData[ii] = Settings.TextBoxBackgroundTextureColor;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, size.X, size.Y);
            texture.SetData(textureData);

            return texture;
        }

        private void LayoutTable(IEnumerable<Element> iElements, Texture2D iBackground)
        {
            _menu.SetBackground(new SpriteDrawable(iBackground));
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
