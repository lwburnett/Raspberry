using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        public int PhysicsLayer = 1 << 0;
        public override float Width => _world.Levels[_level].PxWid;
        public override float Height => _world.Levels[_level].PxHei;
        
        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            var level = _world.Levels[_level];
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

        public override void OnAddedToEntity() => AddColliders();

        public override void OnRemovedFromEntity() => RemoveColliders();

        public override void OnEntityTransformChanged(Transform.Component comp)
        {
            // This was directly copied from TiledMapRenderer.cs. No idea if LDtk maps can be scaled
            // we only deal with positional changes here. TiledMaps cant be scaled.
            if (comp == Transform.Component.Position)
            {
                RemoveColliders();
                AddColliders();
            }
        }

        public LDtkWorld World => _world;

        private enum CollisionEnum
        {
            //Nothing,
            //Overlappable,
            Collidable
        }

        private readonly string _path;
        private readonly ContentManager _contentManager;
        private readonly LDtkWorld _world;
        private readonly int _level;
        private readonly Dictionary<int, Texture2D> _layersToTileMap;
        private List<Collider> _colliders;

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

        private void AddColliders()
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(Math.Abs(Entity.Transform.Scale.X - Entity.Transform.Scale.Y) < .001);

            _colliders = new List<Collider>();
            var level = _world.Levels[_level];
            var scale = Entity.Transform.Scale.X;
            
            for (var ii = level.LayerInstances.Length - 1; ii >= 0; --ii)
            {
                var thisLayer = level.LayerInstances[ii];
                if (!thisLayer._TilesetDefUid.HasValue)
                    continue;

                var tileSet = _world.Defs.Tilesets.FirstOrDefault(iT => iT.Uid == thisLayer._TilesetDefUid);
                System.Diagnostics.Debug.Assert(tileSet != null);
                var collidableTag = tileSet.EnumTags.FirstOrDefault(iEt => ((JsonElement)iEt["enumValueId"]).GetString() == CollisionEnum.Collidable.ToString());
                System.Diagnostics.Debug.Assert(collidableTag != null);

                var collidableIdArray = new List<int>(); 
                var enumerator = ((JsonElement)collidableTag["tileIds"]).EnumerateArray();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current.ToString();
                    var success = int.TryParse(current, out var result);
                    System.Diagnostics.Debug.Assert(success);
                    collidableIdArray.Add(result);
                }

                List<TileInstance> tilesToCheck = new List<TileInstance>();
                tilesToCheck.AddRange(thisLayer.AutoLayerTiles);
                tilesToCheck.AddRange(thisLayer.GridTiles);
                foreach (var thisTile in tilesToCheck)
                {
                    if (collidableIdArray.Contains(thisTile.T))
                    {
                        var rectangle = new Rectangle((int)(thisTile.Src.X * scale), (int)(thisTile.Src.Y * scale), (int)(thisLayer._GridSize * scale), (int)(thisLayer._GridSize * scale));
                        var collider = new BoxCollider(rectangle)
                        {
                            PhysicsLayer = PhysicsLayer,
                            Entity = Entity
                        };
                        _colliders.Add(collider);
                        Physics.AddCollider(collider);
                    }
                }
            }
        }

        private void RemoveColliders()
        {
            if (_colliders == null)
                return;

            foreach (var collider in _colliders)
                Physics.RemoveCollider(collider);
            _colliders.Clear();
        }
    }
}
