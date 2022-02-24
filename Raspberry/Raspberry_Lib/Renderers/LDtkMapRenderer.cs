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
        public LDtkMapRenderer(string iPath, ContentManager iContentManager)
        {
            _path = iPath;
            _contentManager = iContentManager;
            _world = LDtkWorld.LoadWorld(Path.Combine(_contentManager.RootDirectory, _path));
        }

        public override float Width => _world.Levels[0].PxWid;
        public override float Height => _world.Levels[0].PxHei;

        // ReSharper disable once IdentifierTypo
        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            var level = _world.Levels[0];
            
            for (var index = level.LayerInstances.Length - 1; index >= 0; --index)
            {
                var layer = level.LayerInstances[index];
                if (layer._TilesetRelPath != null && layer._Type != LayerType.Entities)
                {
                    var texturePath = Path.Combine(GetParentPath(_path), layer._TilesetRelPath);
                    var texture = this.LoadTexture(texturePath);
                    switch (layer._Type)
                    {
                        case LayerType.IntGrid:
                        case LayerType.AutoLayer:
                            if (layer.AutoLayerTiles.Length != 0)
                            {
                                using (var enumerator = layer.AutoLayerTiles.Where(_ => layer._TilesetDefUid.HasValue).GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var current = enumerator.Current;
                                        System.Diagnostics.Debug.Assert(current != null);
                                        var position = new Vector2(current.Px.X + layer._PxTotalOffsetX, current.Px.Y + layer._PxTotalOffsetY);
                                        var rectangle = new Rectangle(current.Src.X, current.Src.Y, layer._GridSize, layer._GridSize);
                                        var f = (SpriteEffects)current.F;
                                        iBatcher.Draw(texture, position, rectangle, Color.White, 0.0f, Vector2.Zero, 1f, f, 0.0f);
                                    }
                                    continue;
                                }
                            }
                            else
                                continue;
                        case LayerType.Tiles:
                            using (var enumerator = layer.GridTiles.Where(_ => layer._TilesetDefUid.HasValue).GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var current = enumerator.Current;
                                    System.Diagnostics.Debug.Assert(current != null);
                                    var position = new Vector2(current.Px.X + layer._PxTotalOffsetX, current.Px.Y + layer._PxTotalOffsetY);
                                    var rectangle = new Rectangle(current.Src.X, current.Src.Y, layer._GridSize, layer._GridSize);
                                    var f = (SpriteEffects)current.F;
                                    iBatcher.Draw(texture, position, rectangle, Color.White, 0.0f, Vector2.Zero, 1f, f, 0.0f);
                                }
                                continue;
                            }
                        default:
                            continue;
                    }
                }
            }
        }

        public LDtkWorld World => _world;

        private readonly string _path;
        private readonly ContentManager _contentManager;
        private readonly LDtkWorld _world;

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
    }
}
