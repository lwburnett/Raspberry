using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal static class TileGenerator
    {
        private static class Settings
        {
            public const float DarkWaterWidthAsPercentOfRiverWidth = .5f;
            public const float MediumWaterWidthAsPercentOfRiverWidth = .9f;

            public static readonly RenderSetting GrassTileHeight = new(1000);
        }

        public static List<Entity> GenerateRiverTiles(float iXPos, float iTileWidth, LevelBlock iBlock, float iScale)
        {
            var tiles = new List<Entity>();

            const int desiredDataWidth = 32;

            var pixelWidth = iTileWidth / desiredDataWidth;
            var pixelHeight = pixelWidth;

            var yPos = iBlock.Function.GetYForX(iXPos);
            var numExtraUpTotal = 0;
            var numExtraDownTotal = 0;
            var columnData = new List<ColumnDatum> { new(0, 0) };

            // Do math to figure out how many pixels are above the shoreline and how many are below the shoreline for each column of pixels
            var actualDataWidth = 1;
            for (var ii = 1; ii < desiredDataWidth; ii++)
            {
                var thisColumnX = iXPos + ii * pixelWidth;
                if (thisColumnX > iBlock.Function.DomainEnd)
                    break;

                actualDataWidth++;

                var thisIterationY = iBlock.Function.GetYForX(iXPos + ii * pixelWidth);

                var yDiff = thisIterationY - yPos;

                var numPixelDiff = (int)Math.Round(yDiff / pixelHeight);

                if (numPixelDiff > 0)
                {
                    var extraDownDiff = numPixelDiff - numExtraDownTotal;

                    if (extraDownDiff <= 0)
                    {
                        columnData.Add(new ColumnDatum(numPixelDiff + numExtraUpTotal, -extraDownDiff));
                    }
                    else
                    {
                        foreach (var column in columnData)
                        {
                            column.NumBelow += extraDownDiff;
                        }

                        columnData.Add(new ColumnDatum(numPixelDiff + numExtraUpTotal, 0));

                        numExtraDownTotal += extraDownDiff;
                    }
                }
                else
                {
                    var extraUpDiff = -numPixelDiff - numExtraUpTotal;

                    if (extraUpDiff <= 0)
                    {
                        columnData.Add(new ColumnDatum(-extraUpDiff, -numPixelDiff + numExtraDownTotal));
                    }
                    else
                    {
                        foreach (var column in columnData)
                        {
                            column.NumAbove += extraUpDiff;
                        }

                        columnData.Add(new ColumnDatum(0, -numPixelDiff + numExtraDownTotal));

                        numExtraUpTotal += extraUpDiff;
                    }
                }
            }

            var dataHeight = numExtraUpTotal + 10 + numExtraDownTotal;

            // Fill out color data arrays based on the column calculations above
            var upperData = new Color[dataHeight * actualDataWidth];
            var lowerData = new Color[dataHeight * actualDataWidth];

            for (var ii = 0; ii < actualDataWidth; ii++)
            {
                var thisColumn = columnData[ii];

                for (var jj = 0; jj < dataHeight; jj++)
                {
                    var index = actualDataWidth * jj + ii;

                    if (jj < thisColumn.NumAbove)
                    {
                        upperData[index] = GetGrassColor();
                        lowerData[index] = Content.ContentData.ColorPallets.Meadow.Water1; // Settings.WaterColor;
                    }
                    else if (jj < thisColumn.NumAbove + 5)
                    {
                        upperData[index] = Content.ContentData.ColorPallets.Meadow.Mud; //Settings.MudColor;
                        lowerData[index] = Content.ContentData.ColorPallets.Meadow.Water1; //Settings.FoamColor;
                    }
                    else if (jj < thisColumn.NumAbove + 10)
                    {
                        upperData[index] = Content.ContentData.ColorPallets.Meadow.Water1; //Settings.FoamColor;
                        lowerData[index] = Content.ContentData.ColorPallets.Meadow.Mud; //Settings.MudColor;
                    }
                    else
                    {
                        upperData[index] = Content.ContentData.ColorPallets.Meadow.Water1; // Settings.WaterColor;
                        lowerData[index] = GetGrassColor();
                    }
                }
            }

            // Instantiate the shoreline sprites
            var riverWidth = iBlock.GetRiverWidth(iXPos);

            var upperTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            upperTexture.SetData(upperData);
            var lowerTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            lowerTexture.SetData(lowerData);

            var upperSprite = new Sprite(upperTexture);
            var lowerSprite = new Sprite(lowerTexture);

            var additionalYOffset = (numExtraDownTotal - numExtraUpTotal) * pixelHeight / 2;
            var upperBankTileYPos = yPos - (riverWidth / 2f) + additionalYOffset;
            var lowerBankTileYPos = yPos + (riverWidth / 2f) + additionalYOffset;
            
            var upperPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), upperBankTileYPos);
            var upperEntity = new Entity();
            upperEntity.SetPosition(upperPosition);
            upperEntity.SetScale(iScale);
            upperEntity.AddComponent(new SpriteRenderer(upperSprite) { RenderLayer = 5 });

            tiles.Add(upperEntity);

            var lowerPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), lowerBankTileYPos);
            var lowerEntity = new Entity();
            lowerEntity.SetPosition(lowerPosition);
            lowerEntity.SetScale(iScale);
            lowerEntity.AddComponent(new SpriteRenderer(lowerSprite) { RenderLayer = 5 });

            tiles.Add(lowerEntity);

            // Make a sprite for the water between the shorelines
            var riverSpriteHeightPixels = (int)(1f + riverWidth / pixelHeight);

            var waterData = new Color[riverSpriteHeightPixels * actualDataWidth];

            var waterSpriteYPos = yPos + additionalYOffset;

            for (var xx = 0; xx < actualDataWidth; xx++)
            {
                var thisXPos = iXPos + xx * pixelWidth;
                var thisYPos = iBlock.Function.GetYForX(thisXPos);
                var thisRiverWidth = iBlock.GetRiverWidth(thisXPos);
                var darkWidth = thisRiverWidth * Settings.DarkWaterWidthAsPercentOfRiverWidth;
                var midWidth = thisRiverWidth * Settings.MediumWaterWidthAsPercentOfRiverWidth;

                for (var yy = 0; yy < riverSpriteHeightPixels; yy++)
                {
                    var dataIndex = xx + actualDataWidth * yy;
                    var worldYPos = waterSpriteYPos - riverWidth / 2 + pixelHeight * yy;

                    var diff = Math.Abs(thisYPos - worldYPos);
                    if (diff < darkWidth / 2)
                        waterData[dataIndex] = Content.ContentData.ColorPallets.Meadow.Water3;
                    else if (diff < midWidth / 2)
                        waterData[dataIndex] = Content.ContentData.ColorPallets.Meadow.Water2;
                    else
                        waterData[dataIndex] = Content.ContentData.ColorPallets.Meadow.Water1;
                }
            }

            var waterTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, riverSpriteHeightPixels);
            waterTexture.SetData(waterData);
            var waterSprite = new Sprite(waterTexture);

            var waterPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), waterSpriteYPos);
            var waterEntity = new Entity();
            waterEntity.SetPosition(waterPosition);
            waterEntity.SetScale(iScale);
            waterEntity.AddComponent(new SpriteRenderer(waterSprite){RenderLayer = 6});

            tiles.Add(waterEntity);

            // Make sprites for the grass outside the shorelines
            var grassHeight = Settings.GrassTileHeight.Value;
            var grassHeightPixels = (int)(1f + grassHeight / pixelHeight);

            var grassDataSize = grassHeightPixels * actualDataWidth;
            var upperGrassData = new Color[grassDataSize];
            var lowerGrassData = new Color[grassDataSize];
            for (var ii = 0; ii < grassDataSize; ii++)
            {
                upperGrassData[ii] = GetGrassColor();
                lowerGrassData[ii] = GetGrassColor();
            }

            var upperGrassTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            upperGrassTexture.SetData(upperGrassData);
            var upperGrassSprite = new Sprite(upperGrassTexture);

            var upperGrassPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), upperBankTileYPos - dataHeight * pixelHeight / 2f - grassHeight / 2f);
            var upperGrassEntity = new Entity();
            upperGrassEntity.SetPosition(upperGrassPosition);
            upperGrassEntity.SetScale(iScale);
            upperGrassEntity.AddComponent(new SpriteRenderer(upperGrassSprite) { RenderLayer = 6 });

            var lowerGrassTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            lowerGrassTexture.SetData(lowerGrassData);
            var lowerGrassSprite = new Sprite(lowerGrassTexture);

            var lowerGrassPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), lowerBankTileYPos + dataHeight * pixelHeight / 2f + grassHeight / 2f);
            var lowerGrassEntity = new Entity();
            lowerGrassEntity.SetPosition(lowerGrassPosition);
            lowerGrassEntity.SetScale(iScale);
            lowerGrassEntity.AddComponent(new SpriteRenderer(lowerGrassSprite) { RenderLayer = 6 });

            tiles.Add(upperGrassEntity);
            tiles.Add(lowerGrassEntity);

            return tiles;
        }

        private class ColumnDatum
        {
            public ColumnDatum(int iNumAbove, int iNumBelow)
            {
                NumAbove = iNumAbove;
                NumBelow = iNumBelow;
            }

            public int NumAbove { get; set; }
            public int NumBelow { get; set; }
        }

        private static readonly System.Random sRng = new();

        private static Color GetGrassColor()
        {
            var num = sRng.Next(80);

            if (num < 76) return Content.ContentData.ColorPallets.Meadow.Grass3;
            if (num < 78) return Content.ContentData.ColorPallets.Meadow.Grass2;
            return Content.ContentData.ColorPallets.Meadow.Grass1;
        }

        private static Color GetSandColor()
        {
            var num = sRng.Next(80);

            if (num < 76) return Content.ContentData.ColorPallets.Desert.Color2;
            if (num < 78) return Content.ContentData.ColorPallets.Desert.Color3;
            return Content.ContentData.ColorPallets.Desert.Color4;
        }
    }
}
