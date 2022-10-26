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
            System.Diagnostics.Debug.Assert(iOutsideSprite.SourceRect.Width == iInsideSprite.SourceRect.Width);
            System.Diagnostics.Debug.Assert(iOutsideSprite.SourceRect.Height == iInsideSprite.SourceRect.Height);

            _insideSprite = iInsideSprite;
            _outsideSprite = iOutsideSprite;

            _getPlayerPosFunc = iGetPlayerPos;
            _getProximityRadiusFunc = iGetProximityRadius;

            var fullTextureWidth = _insideSprite.Texture2D.Width;
            var fullTextureHeight = _insideSprite.Texture2D.Height;

            _insideU = new Vector2(iInsideSprite.SourceRect.Left, iInsideSprite.SourceRect.Right) / fullTextureWidth;
            _insideV = new Vector2(iInsideSprite.SourceRect.Top, iInsideSprite.SourceRect.Bottom) / fullTextureHeight;
            _outsideU = new Vector2(iOutsideSprite.SourceRect.Left, iOutsideSprite.SourceRect.Right) / fullTextureWidth;
            _outsideV = new Vector2(iOutsideSprite.SourceRect.Top, iOutsideSprite.SourceRect.Bottom) / fullTextureHeight;
        }

        public override RectangleF Bounds => _bounds;
        public override Material Material => _material;

        public override void OnAddedToEntity()
        {
            var spriteWidth = _insideSprite.SourceRect.Width;
            var spriteHeight = _insideSprite.SourceRect.Height;

            _bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _insideSprite.Origin,
                Entity.Transform.Scale, Entity.Transform.Rotation, _insideSprite.SourceRect.Width,
                _insideSprite.SourceRect.Height);

            // This seems weird to put the top left position at (0, 0), but this prevents floating point overflow on android whose max value is 2^14
            var topLeftPosition = Vector2.Zero;
            var spriteDimensions = new Vector2(spriteWidth, spriteHeight) * Entity.Transform.Scale;
            var screenDimensions = new Vector2(Entity.Scene.Camera.Bounds.Width, Entity.Scene.Camera.Bounds.Height);
            _material = new ProximityMaterial(
                _insideSprite.Texture2D, 
                _insideU, _insideV,
                _outsideSprite.Texture2D,
                _outsideU, _outsideV,
                spriteDimensions, 
                screenDimensions);

            _material.Effect.SetSpritePosition(topLeftPosition);


            _topLeftPos = Entity.Position + LocalOffset - new Vector2(spriteWidth / 2f, spriteHeight / 2f) * Entity.Transform.Scale;
        }

        public void Update()
        {
            // Expressing the player position relative to this sprite to avoid floating point overflow on android whose max value is 2^14
            var playerPos = _getPlayerPosFunc() - _topLeftPos;
            var radius = _getProximityRadiusFunc();

            _material.Effect.SetPlayerPosition(playerPos);
            _material.Effect.SetProximityRadius(radius);
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            iBatcher.Draw(_insideSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, _insideSprite.Origin, Entity.Transform.Scale, SpriteEffects.None, _layerDepth);
        }

        private readonly Sprite _insideSprite;
        private readonly Sprite _outsideSprite;
        private readonly Func<Vector2> _getPlayerPosFunc;
        private readonly Func<float> _getProximityRadiusFunc;
        private readonly Vector2 _insideU;
        private readonly Vector2 _insideV;
        private readonly Vector2 _outsideU;
        private readonly Vector2 _outsideV;

        private Vector2 _topLeftPos;

        private ProximityMaterial _material;
    }
}
