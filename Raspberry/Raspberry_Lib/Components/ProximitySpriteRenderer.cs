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
        }

        public override RectangleF Bounds => _bounds;
        public override Material Material => _material;

        public override void OnAddedToEntity()
        {
            _bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _insideSprite.Origin,
                Entity.Transform.Scale, Entity.Transform.Rotation, _insideSprite.SourceRect.Width,
                _insideSprite.SourceRect.Height);

            // This seems weird to put the top left position at (0, 0), but this prevents floating point overflow on android whose max value is 2^14
            var topLeftPosition = Vector2.Zero;
            var spriteDimensions = new Vector2(_spriteWidth, _spriteHeight) * Entity.Transform.Scale;
            var screenDimensions = new Vector2(Entity.Scene.Camera.Bounds.Width, Entity.Scene.Camera.Bounds.Height);
            _material = new ProximityMaterial(_insideSprite.Texture2D, _outsideSprite.Texture2D, topLeftPosition, spriteDimensions, screenDimensions);


            _topLeftPos = Entity.Position + LocalOffset - new Vector2(_spriteWidth / 2f, _spriteHeight / 2f) * Entity.Transform.Scale;
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
        private readonly int _spriteHeight;
        private readonly int _spriteWidth;
        private readonly Func<Vector2> _getPlayerPosFunc;
        private readonly Func<float> _getProximityRadiusFunc;
        private Vector2 _topLeftPos;

        private ProximityMaterial _material;
    }

    internal class ProximityMaterial : Material<ProximityEffect>
    {
        public ProximityMaterial(Texture2D iInsideTexture, Texture2D iOutsideTexture, Vector2 iPositionTopLeft, Vector2 iSpriteDimensions, Vector2 iScreenDimension)
        {
            Effect = new ProximityEffect(iInsideTexture, iOutsideTexture, iPositionTopLeft, iSpriteDimensions, iScreenDimension);
        }
    }

    internal class ProximityEffect : Effect
    {
        public ProximityEffect(Texture2D iInsideTexture, Texture2D iOutsideTexture, Vector2 iPositionTopLeft, Vector2 iSpriteDimensions, Vector2 iScreenDimensions) : 
            base(Core.GraphicsDevice, EffectResource.GetFileResourceBytes(Content.ContentData.AssetPaths.ProximityShader))
        {
            Parameters["InsideTexture"].SetValue(iInsideTexture);
            
            Parameters["OutsideTexture"].SetValue(iOutsideTexture);

            Parameters["SpritePositionTopLeft"].SetValue(iPositionTopLeft);

            Parameters["SpriteDimensions"].SetValue(iSpriteDimensions);

            Parameters["ScreenDimensions"].SetValue(iScreenDimensions);

            _playerPositionParam = Parameters["PlayerPosition"];
            _proximityRadiusParam = Parameters["ProximityRadius"];
        }

        public void SetPlayerPosition(Vector2 iPlayerPosition)
        {
            _playerPositionParam.SetValue(iPlayerPosition);
        }

        public void SetProximityRadius(float iProximityRadius)
        {
            _proximityRadiusParam.SetValue(iProximityRadius);
        }
        
        private readonly EffectParameter _playerPositionParam;
        private readonly EffectParameter _proximityRadiusParam;
    }
}
