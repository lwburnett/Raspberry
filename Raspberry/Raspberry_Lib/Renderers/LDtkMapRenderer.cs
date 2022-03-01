using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Raspberry_Lib.Renderers
{
    internal class LDtkMapRenderer : RenderableComponent
    {
        public LDtkMapRenderer(string iPath, ContentManager iContentManager, int iLevelIndex = 0)
        {
            _path = iPath;
            _contentManager = iContentManager;
            _world = LDtkWorld.LoadWorld(Path.Combine(_contentManager.RootDirectory, _path));
            _level = iLevelIndex;

            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(0 <= _level && _level < _world.Levels.Length);
            System.Diagnostics.Debug.Assert(_contentManager != null);
            _layersToTileMap = PreloadLevelTilesets();
        }

        public override float Width => _world.Levels[_level].PxWid;
        public override float Height => _world.Levels[_level].PxHei;
        
        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            var level = _world.Levels[0];
            System.Diagnostics.Debug.Assert(Math.Abs(Entity.Transform.Scale.X - Entity.Transform.Scale.Y) < .001);
            var scale = Entity.Transform.Scale.X;
            for (var ii = level.LayerInstances.Length - 1; ii >= 0; --ii)
            {
                var thisLayer = level.LayerInstances[ii];
                if (thisLayer._TilesetRelPath == null || thisLayer._Type == LayerType.Entities || _layersToTileMap[ii] == null)
                    continue;

                var thisTileSet = _layersToTileMap[ii];

                IEnumerable<TileInstance> tilesToRender;
                if (thisLayer._Type == LayerType.AutoLayer || thisLayer._Type == LayerType.IntGrid)
                    tilesToRender = thisLayer.AutoLayerTiles.Where(_ => thisLayer._TilesetDefUid.HasValue);
                else
                    tilesToRender = thisLayer.GridTiles.Where(_ => thisLayer._TilesetDefUid.HasValue);

                RenderTiles(thisLayer, tilesToRender, thisTileSet, iBatcher, scale);
            }
        }

        public LDtkWorld World => _world;

        private readonly string _path;
        private readonly ContentManager _contentManager;
        private readonly LDtkWorld _world;
        private readonly int _level;
        private readonly Dictionary<int, Texture2D> _layersToTileMap;

        Dictionary<int, Texture2D> PreloadLevelTilesets()
        {
            var dictionaryToReturn = new Dictionary<int, Texture2D>();

            var layers = _world.Levels[_level].LayerInstances;
            for (var ii = 0; ii < layers.Length; ii++)
            {
                var thisLayer = layers[ii];

                if (string.IsNullOrWhiteSpace(thisLayer._TilesetRelPath))
                    continue;

                var texturePath = Path.Combine(GetParentPath(_path), thisLayer._TilesetRelPath);
                var texture = this.LoadTexture(texturePath);
                dictionaryToReturn.Add(ii, texture);
            }

            return dictionaryToReturn;
        }

        private Texture2D LoadTexture(string iPath) => _contentManager.Load<Texture2D>(Path.ChangeExtension(iPath, null));

        private string GetParentPath(string iPath)
        {
            if (string.IsNullOrWhiteSpace(iPath))
                return null;

            for (var ii = iPath.Length - 1; ii >= 0; ii--)
            {
                if (iPath[ii] == '\\' || iPath[ii] == '/')
                    return iPath.Substring(0, ii + 1);
            }

            return iPath;
        }

        private void RenderTiles(LayerInstance iLayer, IEnumerable<TileInstance> iTiles, Texture2D iTileSet, Batcher iBatcher, float iScale)
        {
            foreach (var tileInstance in iTiles)
            {
                System.Diagnostics.Debug.Assert(tileInstance != null);
                var position = new Vector2((tileInstance.Px.X + iLayer._PxTotalOffsetX) * iScale, (tileInstance.Px.Y + iLayer._PxTotalOffsetY) * iScale);
                var rectangle = new Rectangle(tileInstance.Src.X, tileInstance.Src.Y, iLayer._GridSize, iLayer._GridSize);
                var f = (SpriteEffects)tileInstance.F;
                iBatcher.Draw(iTileSet, position, rectangle, Color.White, 0.0f, Vector2.Zero, iScale, f, 0.0f);
            }
        }
    }
}
