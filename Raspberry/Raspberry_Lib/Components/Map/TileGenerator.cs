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

        public static List<Entity> GenerateBankTiles(float iXPos, float iTileWidth, LevelBlock iBlock, float iScale)
        {
            var tiles = new List<Entity>();

            const int dataWidth = 32;

            var pixelWidth = iTileWidth / dataWidth;
            var pixelHeight = pixelWidth;

            var yPos = iBlock.Function.GetYForX(iXPos);
            var numExtraUpTotal = 0;
            var numExtraDownTotal = 0;
            var columnData = new List<ColumnDatum> { new(0, 0) };

            for (var ii = 1; ii < dataWidth; ii++)
            {
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

            var upperData = new Color[dataHeight * dataWidth];
            var lowerData = new Color[dataHeight * dataWidth];

            for (var ii = 0; ii < dataWidth; ii++)
            {
                var thisColumn = columnData[ii];

                for (var jj = 0; jj < dataHeight; jj++)
                {
                    var index = dataWidth * jj + ii;

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

            var upperTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, dataWidth, dataHeight);
            upperTexture.SetData(upperData);
            var lowerTexture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, dataWidth, dataHeight);
            lowerTexture.SetData(lowerData);

            var upperSprite = new Sprite(upperTexture);
            var lowerSprite = new Sprite(lowerTexture);

            var upperBankTileYPos = yPos - (iBlock.RiverWidth / 2f);
            var lowerBankTileYPos = yPos + (iBlock.RiverWidth / 2f);
            
            var upperPosition = new Vector2(iXPos, upperBankTileYPos);
            var upperEntity = new Entity();
            upperEntity.SetPosition(upperPosition);
            upperEntity.SetScale(iScale);
            upperEntity.AddComponent(new SpriteRenderer(upperSprite));

            tiles.Add(upperEntity);

            var lowerPosition = new Vector2(iXPos, lowerBankTileYPos);
            var lowerEntity = new Entity();
            lowerEntity.SetPosition(lowerPosition);
            lowerEntity.SetScale(iScale);
            lowerEntity.AddComponent(new SpriteRenderer(lowerSprite));

            tiles.Add(lowerEntity);

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
