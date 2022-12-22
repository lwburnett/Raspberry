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

            public static readonly RenderSetting FontScale = new(5);
            public static readonly RenderSetting MinButtonWidth = new(300);
            public static readonly RenderSetting MinButtonHeight = new(80);
        }

        protected MenuBase(RectangleF iBounds)
        {
            _menu = new Table();
            _menu.SetBounds(iBounds.X, iBounds.Y, iBounds.Width, iBounds.Height);
            _background = CreateBackgroundTexture(iBounds.Size);
        }

        public void Initialize()
        {
            var elements = InitializeTableElements();

            LayoutTable(elements, _background);
            AddElement(_menu);
        }

        public override void Draw(Batcher batcher, float parentAlpha)
        {
            base.Draw(batcher, parentAlpha);
            _menu.Draw(batcher, parentAlpha);
        }

        private readonly Table _menu;
        private readonly Texture2D _background;

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
