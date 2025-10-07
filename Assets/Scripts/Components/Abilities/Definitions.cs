using System;
using UnityEngine;

namespace Momentum.Abilities
{


    public class Request
    {
        public Ability ability;

        // public ComboValidation combo;
        public Guid cancelExecutorID;

        public Request(Ability ability)
        {
            this.ability = ability;
        }

        public int   Priority       => ability.priority;
        public float Expiration     => ability.queueing.expiration;
        public float InputBuffer    => ability.queueing.input;
        public float EligibleBuffer => ability.queueing.eligible;
        public RequestMeta Meta     { get; } = new();
    }

    
    public class Instance
    {
        public Context context;
        public Ability ability;

        public GameObject caster;
        public GameObject target;
        public Vector2?   targetPoint;

        public Instance(Context context, Ability ability)
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


    public enum Category
    {
        Movement,
        Offense,
        Defense,
        Control,
        Utility
    }

    public enum Execution
    {
        Cast,
        Action,
        Instant,
        Channel,
        Toggle,
    }

    public enum Mode
    {
        Exclusive,
        Concurrent,
    }

    public enum Phase
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

    public enum CastPhase
    {
        None,
        Start,
        Active,
        Cancel,
        Interrupt,
        Complete,
    }

    public enum State
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