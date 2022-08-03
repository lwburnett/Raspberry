using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Raspberry_Lib.Components
{
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
