using System;
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
            System.Diagnostics.Debug.Assert(iOutsideSprite.Texture2D.Bounds == iInsideSprite.Texture2D.Bounds);

            _insideSprite = iInsideSprite;
            _outsideSprite = iOutsideSprite;
            
            _spriteHeight = iInsideSprite.Texture2D.Height;
            _spriteWidth = iInsideSprite.Texture2D.Width;

            _getPlayerPosFunc = iGetPlayerPos;
            _getProximityRadiusFunc = iGetProximityRadius;

            _areBoundsDirty = true;
        }

        public override RectangleF Bounds => _bounds;
        
        public override void OnAddedToEntity()
        {
            _shader = Entity.Scene.Content.Load<Effect>(Content.ContentData.AssetPaths.ProximityShader);

            _bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _insideSprite.Origin,
                Entity.Transform.Scale, Entity.Transform.Rotation, _insideSprite.SourceRect.Width,
                _insideSprite.SourceRect.Height);
        }

        public void Update()
        {
            var playerPos = _getPlayerPosFunc();
            var radius = _getProximityRadiusFunc();

            var spriteDimension = new[] { _spriteWidth, _spriteHeight };


            _shader.Parameters["InsideTexture"].SetValue(_insideSprite.Texture2D);
            _shader.Parameters["OutsideTexture"].SetValue(_outsideSprite.Texture2D);
            _shader.Parameters["SpritePositionTopLeft"].SetValue(Entity.Position + LocalOffset - new Vector2(_spriteWidth / 2f, _spriteHeight / 2f) * Entity.Transform.Scale);
            _shader.Parameters["SpriteDimensions"].SetValue(spriteDimension);
            _shader.Parameters["ScreenDimensions"].SetValue(new Vector2(Entity.Scene.Camera.Bounds.Width, Entity.Scene.Camera.Bounds.Height));
            _shader.Parameters["PlayerPosition"].SetValue(playerPos);
            _shader.Parameters["ProximityRadius"].SetValue(radius);
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            // iBatcher.End();
            // iBatcher.Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, _shader);

            iBatcher.Draw(_insideSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, _insideSprite.Origin, Entity.Transform.Scale, SpriteEffects.None, _layerDepth);

            // iBatcher.End();
            // iBatcher.Begin();
        }

        private readonly Sprite _insideSprite;
        private readonly Sprite _outsideSprite;
        private readonly int _spriteHeight;
        private readonly int _spriteWidth;
        private readonly Func<Vector2> _getPlayerPosFunc;
        private readonly Func<float> _getProximityRadiusFunc;

        private Effect _shader;
    }
}
