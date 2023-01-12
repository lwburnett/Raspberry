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
            public const string TitleScreenMusic = @"Content/Audio/ocean-of-ice.ogg";
            public const string PlayScreenMusic1 = @"Content/Audio/the-soul-crushing-monotony-of-isolation.ogg";
            public const string StreamSound = @"Content/Audio/Stream1.wav";
            public const string Row1 = @"Content/Audio/Row1.wav";
            public const string Row2 = @"Content/Audio/Row2.wav";
            public const string Row3 = @"Content/Audio/Row3.wav";
            public const string Row4 = @"Content/Audio/Row4.wav";
            public const string Row5 = @"Content/Audio/Row5.wav";
            public const string Row6 = @"Content/Audio/Row6.wav";
            public const string Row7 = @"Content/Audio/Row7.wav";
            public const string Energy = @"Content/Audio/Energy.wav";
            public const string Collision1 = @"Content/Audio/Collision1.wav";
            public const string Collision2 = @"Content/Audio/Collision2.wav";
            public const string Collision3 = @"Content/Audio/Collision3.wav";
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

            internal static class Ui
            {
                // https://colorhunt.co/palette/321f28734046a05344e79e4f
                // {
                public static readonly Color Menu = new(231, 158, 79);
                public static readonly Color ButtonUp = new(50, 31, 40);
                public static readonly Color ButtonOver = new(160, 83, 68);
                public static readonly Color ButtonDown = new(115, 64, 70);
                // }
            }
        }
    }
}
