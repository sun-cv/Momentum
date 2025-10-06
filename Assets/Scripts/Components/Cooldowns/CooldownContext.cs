using System;
using System.Collections.Generic;
using UnityEngine;



namespace Momentum
{
    public class CooldownContext
    {
        public Ability ability;
        public float LastTriggered                       = Time.time;

        public Container<object>       Data     { get; } = new();
        public EventBus.Signal<object> EventBus { get; } = new();

        public CooldownContext(Ability ability) => this.ability = ability;
    }

}