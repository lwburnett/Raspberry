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
            public static readonly Color GrassColor = new(69, 198, 88);
            public static readonly Color MudColor = new(69, 52, 29);
            public static readonly Color FoamColor = new(182, 211, 211);
            public static readonly Color WaterColor = new(35, 101, 100);
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
                        upperData[index] = Settings.GrassColor;
                        lowerData[index] = Settings.WaterColor;
                    }
                    else if (jj < thisColumn.NumAbove + 5)
                    {
                        upperData[index] = Settings.MudColor;
                        lowerData[index] = Settings.FoamColor;
                    }
                    else if (jj < thisColumn.NumAbove + 10)
                    {
                        upperData[index] = Settings.FoamColor;
                        lowerData[index] = Settings.MudColor;
                    }
                    else
                    {
                        upperData[index] = Settings.WaterColor;
                        lowerData[index] = Settings.GrassColor;
                    }
                }
            }

            var riverWidth = iBlock.GetRiverWidth(iXPos);

            var upperTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            upperTexture.SetData(upperData);
            var lowerTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, dataHeight);
            lowerTexture.SetData(lowerData);

            var upperSprite = new Sprite(upperTexture);
            var lowerSprite = new Sprite(lowerTexture);

            var upperBankTileYPos = yPos - (riverWidth / 2f);
            var lowerBankTileYPos = yPos + (riverWidth / 2f);
            
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

            var riverSpriteHeightPixels = (int)(1f + riverWidth / pixelHeight);

            var waterData = new Color[riverSpriteHeightPixels * actualDataWidth];
            for (var ii = 0; ii < riverSpriteHeightPixels * actualDataWidth; ii++)
            {
                waterData[ii] = Settings.WaterColor;
            }

            var waterTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, actualDataWidth, riverSpriteHeightPixels);
            waterTexture.SetData(waterData);
            var waterSprite = new Sprite(waterTexture);

            //var waterYPos = upperBankTileYPos + (numExtraUpTotal + 10 + numExtraDownTotal) * pixelHeight;
            var waterEntity = new Entity();
            waterEntity.SetPosition(new Vector2(iXPos + (actualDataWidth * pixelWidth / 2), yPos));
            waterEntity.SetScale(iScale);
            waterEntity.AddComponent(new SpriteRenderer(waterSprite){RenderLayer = 6});

            tiles.Add(waterEntity);

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
    }
}
