using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;

using Game.Common;



namespace Game.Content
{
    public sealed class SheetLoader
    {
        private static readonly Dictionary<string, Vector2> Directions = new()
        {
            { "North",  Vector2.up      },
            { "East",   Vector2.right   },
            { "South",  Vector2.down    },
            { "West",   Vector2.left    },
        };

        private static readonly HashSet<string>[] Shapes =
        {
            new() { "North", "East", "South", "West" },
            new() { "East", "West" },
        };

        private readonly Registry registry;

        internal SheetLoader(Registry registry)
        {
            this.registry = registry;
        }

        public AsyncOperationHandle Load(HashSet<string> labels)
        {
            var texts   = Addressables.LoadAssetsAsync<TextAsset>(labels, null, Addressables.MergeMode.Intersection);

            var sprites = Addressables.ResourceManager.CreateChainOperation<IList<AsyncOperationHandle>, IList<TextAsset>>(texts, loaded =>
                {
                    var handles = new List<AsyncOperationHandle>();

                    foreach (var text in loaded.Result)
                    {
                        handles.Add(Addressables.LoadAssetAsync<IList<Sprite>>($"{text.name}.Sprites"));
                    }

                    return Addressables.ResourceManager.CreateGenericGroupOperation(handles);
                });

            sprites.Completed += loaded => Register(texts.Result, loaded.Result);

            return sprites;
        }

        private void Register(IList<TextAsset> texts, IList<AsyncOperationHandle> sprites)
        {
            for (var i = 0; i < texts.Count; i++)
            {
                var id = texts[i].name;
                registry.Register(id, BuildSheet(id, texts[i].text, (IList<Sprite>)sprites[i].Result));
            }
        }

        private Sheet BuildSheet(string id, string json, IList<Sprite> sprites)
        {
            var export = JsonConvert.DeserializeObject<Export>(json);
            var frames = sprites.OrderBy(Index).ToList();

            if (frames.Count != export.Frames.Count)
                throw new Exception($"[SheetLoader] {id}: {export.Frames.Count} frames in JSON, {frames.Count} sprites");

            var states     = export.Meta.FrameTags.Where(tag => !Directions.ContainsKey(tag.Name)).ToList();
            var directions = export.Meta.FrameTags.Where(tag =>  Directions.ContainsKey(tag.Name)).ToList();
            var clips      = new Dictionary<(string State, Vector2 Direction), Clip>();

            RequireSeparate(id, states);

            foreach (var state in states)
            {
                if (state.Direction != "forward")
                    throw new Exception($"[SheetLoader] {id}: state {state.Name} plays {state.Direction}, only forward is supported");

                var covering = directions.Where(tag => tag.From <= state.To && tag.To >= state.From).ToList();

                if (covering.Count == 0)
                {
                    Add(id, clips, BuildClip(state, Vector2.zero, state.From, state.To, export.Frames, frames));
                    continue;
                }

                RequireShape(id, state, covering);

                var covered = 0;

                foreach (var tag in covering)
                {
                    var from = Math.Max(state.From, tag.From);
                    var to   = Math.Min(state.To, tag.To);

                    covered += to - from + 1;
                    Add(id, clips, BuildClip(state, Directions[tag.Name], from, to, export.Frames, frames));
                }

                if (covered != state.To - state.From + 1)
                    throw new Exception($"[SheetLoader] {id}: direction tags over state {state.Name} leave frames uncovered or overlap");
            }

            return new Sheet(id, clips);
        }

        private static void RequireSeparate(string id, List<ExportTag> states)
        {
            for (var i = 0; i < states.Count; i++)
            {
                for (var j = i + 1; j < states.Count; j++)
                {
                    if (states[i].From <= states[j].To && states[j].From <= states[i].To)
                        throw new Exception($"[SheetLoader] {id}: states {states[i].Name} and {states[j].Name} overlap; is one a misspelled direction?");
                }
            }
        }

        private static void RequireShape(string id, ExportTag state, List<ExportTag> covering)
        {
            var names = covering.Select(tag => tag.Name).ToHashSet();

            if (!Shapes.Any(shape => shape.SetEquals(names)))
                throw new Exception($"[SheetLoader] {id}: state {state.Name} has directions {string.Join(", ", names)}; expected North, East, South, West or East, West");
        }

        private static void Add(string id, Dictionary<(string State, Vector2 Direction), Clip> clips, Clip clip)
        {
            if (!clips.TryAdd((clip.State, clip.Direction), clip))
                throw new Exception($"[SheetLoader] {id}: state {clip.State} has more than one clip facing {clip.Direction}");
        }

        private Clip BuildClip(ExportTag state, Vector2 direction, int from, int to, List<ExportFrame> frames, List<Sprite> sprites)
        {
            var count = to - from + 1;
            var ticks = new List<int>(count);

            for (var i = from; i <= to; i++)
            {
                ticks.Add(Mathf.Max(1, Mathf.RoundToInt(frames[i].Duration * Config.Engine.Tick.Base / 1000f)));
            }

            return new Clip(state.Name, direction, sprites.GetRange(from, count), ticks, string.IsNullOrEmpty(state.Repeat));
        }

        private static int Index(Sprite sprite) => int.Parse(sprite.name[(sprite.name.LastIndexOf('_') + 1)..]);

        private sealed class Export
        {
            public List<ExportFrame> Frames                     { get; set; }
            public ExportMeta Meta                              { get; set; }
        }

        private sealed class ExportFrame
        {
            public int Duration                                 { get; set; }
        }

        private sealed class ExportMeta
        {
            public List<ExportTag> FrameTags                    { get; set; }
        }

        private sealed class ExportTag
        {
            public string Name                                  { get; set; }
            public int From                                     { get; set; }
            public int To                                       { get; set; }
            public string Direction                             { get; set; }
            public string Repeat                                { get; set; }
        }
    }
}

