using UnityEngine;

namespace Momentum
{
    
    public abstract class Entity : MonoBehaviour, ITick
    {
        public abstract void Tick();
    }
}