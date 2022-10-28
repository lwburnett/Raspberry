﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class ProximityMaterial : Material<ProximityEffect>
    {
        public ProximityMaterial(
            Texture2D iInsideTexture, 
            Vector2 iInsideUBounds, Vector2 iInsideVBounds, 
            Texture2D iOutsideTexture,
            Vector2 iOutsideUBounds, Vector2 iOutsideVBounds,
            float iRotationRadians,
            Vector2 iSpriteDimensions, 
            Vector2 iScreenDimension)
        {
            Effect = new ProximityEffect(
                iInsideTexture, 
                iInsideUBounds, iInsideVBounds, 
                iOutsideTexture,
                iOutsideUBounds, iOutsideVBounds,
                iRotationRadians,
                iSpriteDimensions, iScreenDimension);
        }
    }

    internal class ProximityEffect : Effect
    {
        public ProximityEffect(
            Texture2D iInsideTexture, 
            Vector2 iInsideUBounds, Vector2 iInsideVBounds, 
            Texture2D iOutsideTexture,
            Vector2 iOutsideUBounds, Vector2 iOutsideVBounds,
            float iRotationRadians,
            Vector2 iSpriteDimensions, 
            Vector2 iScreenDimensions) :
            base(Core.GraphicsDevice, EffectResource.GetFileResourceBytes(Content.ContentData.AssetPaths.ProximityShader))
        {
            Parameters["InsideTexture"].SetValue(iInsideTexture);

            Parameters["OutsideTexture"].SetValue(iOutsideTexture);

            Parameters["InsideUBounds"].SetValue(iInsideUBounds);
            Parameters["InsideVBounds"].SetValue(iInsideVBounds);
            Parameters["OutsideUBounds"].SetValue(iOutsideUBounds);
            Parameters["OutsideVBounds"].SetValue(iOutsideVBounds);
            Parameters["RotationRadians"].SetValue(iRotationRadians);

            Parameters["SpriteDimensions"].SetValue(iSpriteDimensions);

            Parameters["ScreenDimensions"].SetValue(iScreenDimensions);

            _positionCenter = Parameters["SpritePositionCenter"];
            _playerPositionParam = Parameters["PlayerPosition"];
            _proximityRadiusParam = Parameters["ProximityRadius"];
        }

        public void SetSpritePosition(Vector2 iSpritePosition)
        {
            _positionCenter.SetValue(iSpritePosition);
        }

        public void SetPlayerPosition(Vector2 iPlayerPosition)
        {
            _playerPositionParam.SetValue(iPlayerPosition);
        }

        public void SetProximityRadius(float iProximityRadius)
        {
            _proximityRadiusParam.SetValue(iProximityRadius);
        }

        private readonly EffectParameter _positionCenter;
        private readonly EffectParameter _playerPositionParam;
        private readonly EffectParameter _proximityRadiusParam;
    }
}
