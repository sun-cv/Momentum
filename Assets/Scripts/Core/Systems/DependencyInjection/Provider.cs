using System;
using UnityEngine;


namespace Momentum
{


    public class Provider : MonoBehaviour, IDependencyProvider
    {       
        [Provide] public IInputRouter           provideInputRouter()            => new InputRouter();
        // [Provide] public IAbilitySystem         provideAbilitySystem()          => new AbilitySystem();
        [Provide] public ICommandSystem         provideCommandsystem()          => new CommandSystem();
        [Provide] public IAttributeSystem       provideAttributeSystem()        => new AttributeSystem();
        [Provide] public IMovementEngine        provideMovementEngine()         => new MovementEngine();

        [Provide] public IAnimationController   provideAnimationController()    => new AnimationController();

    }
}