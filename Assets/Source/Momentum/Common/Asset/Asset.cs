using UnityEngine;



namespace Game.Common
{
    public interface IAsset {}

    public readonly struct Blueprint
    {
        public Definition Definition    { get; init; }
        public GameObject Prefab        { get; init; }
        public Vector3 Position         { get; init; }
    }
}
