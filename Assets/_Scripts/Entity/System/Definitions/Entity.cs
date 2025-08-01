using Momentum.Actor.Hero;
using Momentum.Definition;
using Momentum.Events;
using Momentum.Interface;
using UnityEngine;

namespace Momentum.Definition
{
    
    public abstract class Entity : MonoBehaviour, ITick
    {
        public abstract void Tick();
    }

    public interface IEnemy {};
    public interface IHero  : ITickAll, IInitialize {}
}
