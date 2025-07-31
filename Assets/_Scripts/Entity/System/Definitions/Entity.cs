

using Momentum.Actor.Hero;
using Momentum.Interface;
using UnityEngine;

namespace Momentum.Definition
{
    
    public abstract class Entity : MonoBehaviour
    {
        // REWORK REQUIRED  - Mandate stats? 
    }

    public interface IEntity        : ITick {}
    public interface IEntityEnemy   : IEntity {}
    public interface IEntityHero    : IEntity, IInitialize, ITickAll { public HeroContext GetHeroContext();}
}