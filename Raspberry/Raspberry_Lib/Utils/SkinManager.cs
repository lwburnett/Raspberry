using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;
using Nez.Textures;
using Nez.UI;
using Raspberry_Lib.Content;

namespace Raspberry_Lib
{
    internal static class SkinManager
    {
        public static void Initialize(NezContentManager iContent)
        {
            var uiTexture = iContent.LoadTexture(ContentData.AssetPaths.IconsTileset);


            sGameUiSkin = CreateGameUiSkin(uiTexture);
            sIsInitialized = true;
        }
        private static bool sIsInitialized;

        public static NinePatchDrawable GetMenuBackgroundSprite()
        {
            System.Diagnostics.Debug.Assert(sIsInitialized);

            return sMenuSprite;
        }
        private static NinePatchDrawable sMenuSprite;

        public static Skin GetGameUiSkin()
        {
            System.Diagnostics.Debug.Assert(sIsInitialized);

            return sGameUiSkin;
        }
        private static Skin sGameUiSkin;

        private static Skin CreateGameUiSkin(Texture2D iSpriteAtlas)
        {
            const string styleName = "default";

            var menuSprite = new Sprite(iSpriteAtlas, 0, 96, 105, 50);
            sMenuSprite = new NinePatchDrawable(menuSprite, 0, 0, 0, 0)
            {
                TintColor = ContentData.ColorPallets.Ui.Menu
            };
            var buttonSprite = new Sprite(iSpriteAtlas, 64, 39, 105, 50);
            var buttonDrawable = new NinePatchDrawable(buttonSprite, 0, 0, 0, 0);
            var buttonUpDrawable = buttonDrawable.NewTintedDrawable(ContentData.ColorPallets.Ui.ButtonUp);
            var buttonOverDrawable = buttonDrawable.NewTintedDrawable(ContentData.ColorPallets.Ui.ButtonOver);
            var buttonDownDrawable = buttonDrawable.NewTintedDrawable(ContentData.ColorPallets.Ui.ButtonDown);

            var skin = new Skin();
            var textButtonStyle = new TextButtonStyle()
            {
                Up = buttonUpDrawable,
                Over = buttonOverDrawable,
                Down = buttonDownDrawable,
                OverFontColor = Color.White,
                DownFontColor = Color.White,
                PressedOffsetX = 1,
                PressedOffsetY = 1
            };
            skin.Add(styleName, textButtonStyle);

            return skin;
        }
    }
}
