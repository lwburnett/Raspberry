using Microsoft.Xna.Framework;

namespace Raspberry_Lib.Content
{
    internal static class ContentData
    {
        internal static class AssetPaths
        {
            public const string IconsTileset = @"Content/Levels/Icons.png";
            public const string ObjectsTileset = @"Content/Levels/Objects.png";
            public const string TitleScreenBackground = @"Content/Levels/TitleScreen.png";
            public const string CharacterSpriteSheet = @"Content/Characters/Character.png";
            public const string ProximityShader = @"Content/Shaders/ProximityShader.mgfxo";
            public const string TitleScreenMusic = @"Content/Audio/ocean-of-ice.wav";
            public const string PlayScreenMusic1 = @"Content/Audio/the-soul-crushing-monotony-of-isolation.wav";
            public const string PlayScreenMusic2 = @"Content/Audio/Hymn-To-The-Gods.wav";
            public const string StreamSound = @"Content/Audio/Stream1.wav";
        }

        internal static class ColorPallets
        {
            internal static class Meadow
            {
                // https://www.colorcombos.com/color-schemes/121/ColorCombo121.html
                // {
                public static readonly Color Water1 = new(116, 194, 225);
                public static readonly Color Water2 = new(1, 145, 200);
                public static readonly Color Water3 = new(0, 91, 154);
                // }

                // https://www.colorcombos.com/color-schemes/245/ColorCombo245.html
                // {
                public static readonly Color Grass1 = new(199, 235, 110);
                public static readonly Color Grass2 = new(157, 206, 92);
                public static readonly Color Grass3 = new(121, 184, 55);
                // }

                public static readonly Color Mud = new(89, 62, 26);
            }

            internal static class Desert
            {
                // https://www.colorcombos.com/color-schemes/288/ColorCombo288.html
                // {
                public static readonly Color Color1 = new(133, 98, 42);
                public static readonly Color Color2 = new(230, 166, 68);
                public static readonly Color Color3 = new(225, 131, 57);
                public static readonly Color Color4 = new(182, 87, 29);
                public static readonly Color Color5 = new(140, 51, 19);
                // }
            }
        }
    }
}
