using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class ProximitySpriteRenderer : RenderableComponent, IUpdatable
    {
        public ProximitySpriteRenderer(Sprite iInsideSprite, Sprite iOutsideSprite, Func<Vector2> iGetPlayerPos, Func<float> iGetProximityRadius)
        {
            System.Diagnostics.Debug.Assert(iInsideSprite != null);
            System.Diagnostics.Debug.Assert(iOutsideSprite != null);
            System.Diagnostics.Debug.Assert(iInsideSprite.SourceRect == iOutsideSprite.SourceRect);

            _insideSprite = iInsideSprite;

            _outsideSpriteHeight = iOutsideSprite.Texture2D.Height;
            _outsideSpriteWidth = iOutsideSprite.Texture2D.Width;
            var outsideSpriteData = new Color[_outsideSpriteWidth * _outsideSpriteHeight];
            iOutsideSprite.Texture2D.GetData(outsideSpriteData);

            _outsideSpriteData = outsideSpriteData.Select(c => new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f)).ToArray();

            _getPlayerPosFunc = iGetPlayerPos;
            _getProximityRadiusFunc = iGetProximityRadius;
        }

        public override float Width => _insideSprite.SourceRect.Width;
        public override float Height => _insideSprite.SourceRect.Height;

        public override void OnAddedToEntity()
        {
            _shader = Entity.Scene.Content.LoadEffect(Content.ContentData.AssetPaths.ProximityShader);
        }

        public void Update()
        {
            var playerPos = _getPlayerPosFunc();
            var radius = _getProximityRadiusFunc();
            
            _shader.Parameters["SpritePositionTopLeft"].SetValue(Entity.Position + LocalOffset - new Vector2(_outsideSpriteWidth / 2f, _outsideSpriteHeight / 2f) * Entity.Transform.Scale);
            _shader.Parameters["SpriteDimensions"].SetValue(new Vector2(_outsideSpriteWidth, _outsideSpriteHeight));
            _shader.Parameters["PlayerPosition"].SetValue(playerPos);
            _shader.Parameters["ProximityRadius"].SetValue(radius);
            _shader.Parameters["OutsideSpriteData"].SetValue(_outsideSpriteData);
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            iBatcher.Begin(_shader);

            iBatcher.Draw(_insideSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, Vector2.Zero, Entity.Transform.Scale, SpriteEffects.None, _layerDepth);

            iBatcher.End();
        }

        private readonly Sprite _insideSprite;
        private readonly Vector4[] _outsideSpriteData;
        private readonly int _outsideSpriteHeight;
        private readonly int _outsideSpriteWidth;
        private readonly Func<Vector2> _getPlayerPosFunc;
        private readonly Func<float> _getProximityRadiusFunc;

        private Effect _shader;
    }
}
