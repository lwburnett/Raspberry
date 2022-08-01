using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
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

        public static List<Entity> GenerateRiverTiles(float iXPos, float iTileWidth, LevelBlock iBlock, float iScale, Func<Vector2> iGetPlayerPosFunc)
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
            var upperInsideData = new Color[dataHeight * actualDataWidth];
            var upperOutsideData = new Color[dataHeight * actualDataWidth];
            var lowerInsideData = new Color[dataHeight * actualDataWidth];
            var lowerOutsideData = new Color[dataHeight * actualDataWidth];

            for (var ii = 0; ii < actualDataWidth; ii++)
            {
                var thisColumn = columnData[ii];

                for (var jj = 0; jj < dataHeight; jj++)
                {
                    var index = actualDataWidth * jj + ii;

                    if (jj < thisColumn.NumAbove)
                    {
                        upperInsideData[index] = GetGrassColor();
                        upperOutsideData[ii] = GetSandColor();
                        lowerInsideData[index] = Content.ContentData.ColorPallets.Meadow.Water1;
                        lowerOutsideData[ii] = Content.ContentData.ColorPallets.Desert.Color1;
                    }
                    else if (jj < thisColumn.NumAbove + 5)
                    {
                        upperInsideData[index] = Content.ContentData.ColorPallets.Meadow.Mud;
                        upperOutsideData[index] = GetSandColor();
                        lowerInsideData[index] = Content.ContentData.ColorPallets.Meadow.Water1;
                        lowerOutsideData[index] = Content.ContentData.ColorPallets.Desert.Color1;
                    }
                    else if (jj < thisColumn.NumAbove + 10)
                    {
                        upperInsideData[index] = Content.ContentData.ColorPallets.Meadow.Water1;
                        upperOutsideData[index] = Content.ContentData.ColorPallets.Desert.Color1;
                        lowerInsideData[index] = Content.ContentData.ColorPallets.Meadow.Mud;
                        lowerOutsideData[index] = GetSandColor();
                    }
                    else
                    {
                        upperInsideData[index] = Content.ContentData.ColorPallets.Meadow.Water1;
                        upperOutsideData[index] = Content.ContentData.ColorPallets.Desert.Color1;
                        lowerInsideData[index] = GetGrassColor();
                        lowerOutsideData[index] = GetSandColor();
                    }
                }
            }

            // Instantiate the shoreline sprites
            var riverWidth = iBlock.GetRiverWidth(iXPos);

            var upperTextureInside = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            upperTextureInside.SetData(upperInsideData);
            var upperTextureOutside = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            upperTextureOutside.SetData(upperOutsideData);
            var lowerTextureInside = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            lowerTextureInside.SetData(lowerInsideData);
            var lowerTextureOutside = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            lowerTextureOutside.SetData(lowerOutsideData);

            var upperSpriteInside = new Sprite(upperTextureInside);
            var upperSpriteOutside = new Sprite(upperTextureOutside);
            var lowerSpriteInside = new Sprite(lowerTextureInside);
            var lowerSpriteOutside = new Sprite(lowerTextureOutside);

            var additionalYOffset = (numExtraDownTotal - numExtraUpTotal) * pixelHeight / 2;
            var upperBankTileYPos = yPos - (riverWidth / 2f) + additionalYOffset;
            var lowerBankTileYPos = yPos + (riverWidth / 2f) + additionalYOffset;

            var upperPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), upperBankTileYPos);
            var upperEntity = new Entity();
            upperEntity.SetPosition(upperPosition);
            upperEntity.SetScale(iScale);
            upperEntity.AddComponent(new ProximitySpriteRenderer(upperSpriteInside, upperSpriteOutside, iGetPlayerPosFunc, () => 100) { RenderLayer = 5 });

            tiles.Add(upperEntity);

            var lowerPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), lowerBankTileYPos);
            var lowerEntity = new Entity();
            lowerEntity.SetPosition(lowerPosition);
            lowerEntity.SetScale(iScale);
            lowerEntity.AddComponent(new ProximitySpriteRenderer(lowerSpriteInside, lowerSpriteOutside, iGetPlayerPosFunc, () => 100) { RenderLayer = 5 });

            tiles.Add(lowerEntity);

            // Make a sprite for the water between the shorelines
            var riverSpriteHeightPixels = (int)(1f + riverWidth / pixelHeight);

            var waterData = new Color[riverSpriteHeightPixels * actualDataWidth];
            var riverBedData = new Color[riverSpriteHeightPixels * actualDataWidth];

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

                    riverBedData[dataIndex] = Content.ContentData.ColorPallets.Desert.Color1;
                }
            }

            var waterTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, riverSpriteHeightPixels);
            waterTexture.SetData(waterData);
            var riverBedTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, riverSpriteHeightPixels);
            riverBedTexture.SetData(riverBedData);

            var waterSprite = new Sprite(waterTexture);
            var riverBedSprite = new Sprite(riverBedTexture);

            var waterPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), waterSpriteYPos);
            var waterEntity = new Entity();
            waterEntity.SetPosition(waterPosition);
            waterEntity.SetScale(iScale);
            waterEntity.AddComponent(new ProximitySpriteRenderer(waterSprite, riverBedSprite, iGetPlayerPosFunc, () => 100) {RenderLayer = 6});

            tiles.Add(waterEntity);

            // Make sprites for the grass and sand outside the shorelines
            var grassHeight = Settings.GrassTileHeight.Value;
            var grassHeightPixels = (int)(1f + grassHeight / pixelHeight);

            var grassDataSize = grassHeightPixels * actualDataWidth;
            var upperGrassData = new Color[grassDataSize];
            var upperSandData = new Color[grassDataSize];
            var lowerGrassData = new Color[grassDataSize];
            var lowerSandData = new Color[grassDataSize];
            for (var ii = 0; ii < grassDataSize; ii++)
            {
                upperGrassData[ii] = GetGrassColor();
                upperSandData[ii] = GetSandColor();
                lowerGrassData[ii] = GetGrassColor();
                lowerSandData[ii] = GetSandColor();
            }

            var upperGrassTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            upperGrassTexture.SetData(upperGrassData);
            var upperSandTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            upperSandTexture.SetData(upperSandData);

            var upperGrassSprite = new Sprite(upperGrassTexture);
            var upperSandSprite = new Sprite(upperSandTexture);

            var upperGrassPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), upperBankTileYPos - dataHeight * pixelHeight / 2f - grassHeight / 2f);
            var upperGrassEntity = new Entity();
            upperGrassEntity.SetPosition(upperGrassPosition);
            upperGrassEntity.SetScale(iScale);
            upperGrassEntity.AddComponent(new ProximitySpriteRenderer(upperGrassSprite, upperSandSprite, iGetPlayerPosFunc, () => 100) { RenderLayer = 6 });

            var lowerGrassTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            lowerGrassTexture.SetData(lowerGrassData);
            var lowerSandTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, grassHeightPixels);
            lowerSandTexture.SetData(lowerSandData);

            var lowerGrassSprite = new Sprite(lowerGrassTexture);
            var lowerSandSprite = new Sprite(lowerSandTexture);

            var lowerGrassPosition = new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), lowerBankTileYPos + dataHeight * pixelHeight / 2f + grassHeight / 2f);
            var lowerGrassEntity = new Entity();
            lowerGrassEntity.SetPosition(lowerGrassPosition);
            lowerGrassEntity.SetScale(iScale);
            lowerGrassEntity.AddComponent(new ProximitySpriteRenderer(lowerGrassSprite, lowerSandSprite, iGetPlayerPosFunc, () => 100) { RenderLayer = 6 });

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
