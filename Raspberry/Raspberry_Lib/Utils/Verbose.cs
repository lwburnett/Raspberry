#if VERBOSE
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.BitmapFonts;

namespace Raspberry_Lib
{
    internal static class Verbose
    {
        private static Renderer sRendererInstance;
        public static RenderableComponent GetRenderer()
        {
            return sRendererInstance ??= new Renderer();
        }

        public static void TrackMetric<T>(Func<T> iSampleFunc, Func<T, string> iToTextFunc) where T : new()
        {
            sMetrics.Add(new Metric<T>(iSampleFunc, iToTextFunc));
        }

        private static readonly List<IMetric> sMetrics = new();

        private interface IMetric
        {
            string GetText();
            void Sample();
        }

        private class Metric<T> : IMetric where T : new()
        {

            public Metric(Func<T> iSampleFunc, Func<T, string> iToTextFunc)
            {
                _metric = new T();
                _sampleFunc = iSampleFunc;
                _toTextFunc = iToTextFunc;
            }

            public string GetText()
            {
                return _toTextFunc(_metric);
            }

            public void Sample()
            {
                _metric = _sampleFunc();
            }

            private T _metric;
            private readonly Func<T> _sampleFunc;
            private readonly Func<T, string> _toTextFunc;
        }
        

        private class Renderer : RenderableComponent
        {
            private static class Settings
            {
                public static readonly RenderSetting MarginX = new(10);
                public static readonly RenderSetting MarginY = new(20);

                public const int Scale = 3;
            }

            public Renderer()
            {
                _font = Graphics.Instance.BitmapFont;
            }
            public override float Width => float.MaxValue;
            public override float Height => float.MaxValue;

            public override void Render(Batcher iBatcher, Camera iCamera)
            {
                var lastStringBottomY = iCamera.Bounds.Y;

                foreach (var metric in sMetrics)
                {
                    metric.Sample();

                    var size = _font.MeasureString(metric.GetText()) * Settings.Scale;
                    var thisStringPos = new Vector2(
                        iCamera.Bounds.X + Settings.MarginX.Value, 
                        lastStringBottomY + Settings.MarginY.Value);

                    lastStringBottomY = thisStringPos.Y + size.Y;

                    iBatcher.DrawString(_font, metric.GetText(), thisStringPos, Color.White, 0f, Vector2.Zero, Settings.Scale, SpriteEffects.None, 0f);
                }
            }

            private readonly BitmapFont _font;
        }
    }
}
#endif
