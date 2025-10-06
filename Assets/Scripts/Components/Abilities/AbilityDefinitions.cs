

using System;
using UnityEngine;

namespace Momentum
{


    public class AbilityRequest
    {
        public Ability ability;

        public ComboValidation combo;
        public Guid cancelExecutorID;

        public AbilityRequest(Ability ability)
        {
            this.ability = ability;
        }

        public int   Priority       => ability.priority;
        public float Expiration     => ability.queueing.expiration;
        public float InputBuffer    => ability.queueing.inputBuffer;
        public float ValidBuffer    => ability.queueing.validBuffer;
        public RequestMeta Meta     { get; } = new();
    }

    
    public class AbilityInstance
    {
        public Context context;
        public Ability ability;

        public GameObject caster;
        public GameObject target;
        public Vector2?   targetPoint;

        public AbilityInstance(Context context, Ability ability)
        {
            this.context = context;
            this.ability = ability;
        }

        public ComponentContext Component => context.component;
        public EntityContext Entity       => context.entity;
        public AbilityMeta Meta          { get; } = new();

    }


    public class AbilityEvent
    {
        public string id;
        public string label;
        public object payload;

        public Meta Meta { get; private set; } = new();

        public T GetPayload<T>() => (T)payload;
    }


    public enum AbilityCategory
    {
        Movement,
        Offense,
        Defense,
        Control,
        Utility
    }

    public enum AbilityExecution
    {
        Cast,
        Action,
        Instant,
        Channel,
        Toggle,
    }

    public enum AbilityMode
    {
        Exclusive,
        Concurrent,
    }

    public enum AbilityPhase
    {
        None,
        Activate,
        Active,
        Execute,
        Cancel,
        Interrupt,
        Complete,
        Deactivate,
        Deactivated,
    }

    public enum AbilityCastPhase
    {
        None,
        Start,
        Active,
        Cancel,
        Interrupt,
        Complete,
    }

    public enum AbilityState
    {
        Idle,
        Attack,
        Dash,
        ShieldHeld,
        ShieldBlock,
        AimShield,
        ShieldThrow,
        StatusEffect
    }


        // Rework required - implement as flags?
    // public enum AbilityTag
    // {
    //     Damage,
    //     Mitigation,
    //     Mobility,
    //     CrowdControl,
    //     Buff,
    //     Debuff,
    //     Utility
    // }

}