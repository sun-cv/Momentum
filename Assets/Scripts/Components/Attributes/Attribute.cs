using UnityEngine;


namespace Momentum
{

    public abstract class Attribute : ScriptableObject
    {
        public abstract IRuntimeAttribute CreateRuntime();
    }

    public interface IRuntimeAttribute 
    {
        public StatsMediator Mediator { get; }
    }


}