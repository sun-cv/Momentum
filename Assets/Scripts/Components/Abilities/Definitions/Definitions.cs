using System;
using UnityEditor;
using UnityEngine;

namespace Momentum.Abilities
{


    public class Request
    {
        public Ability ability;
        public Guid preemptRequestID;

        public Request(Ability ability)
        {
            this.ability = ability;
        }
        public AbilityRequestMeta Meta { get; } = new();
    }

    
    public class Instance
    {
        public Context context;
        public Ability ability;

        // public GameObject caster;
        // public GameObject target;
        // public Vector2?   targetPoint;

        public Instance(Context context, Ability ability)
        {
            this.context = context;
            this.ability = ability;
        }

        public EntityContext Entity       => context.entity;
        public ComponentContext Component => context.component;
        public AbilityInstanceMeta Meta   { get; } = new();

    }


    public class AbilityEvent
    {
        public string id;
        public string label;
        public object payload;

        public Meta Meta { get; private set; } = new();

        public T GetPayload<T>() => (T)payload;
    }

    public class TokenEntry
    {
        public TokenState   state;
        public Request      request;

        public Guid         requestID;
        public Guid         instanceID;

        public TokenEntry(Request request)
        {
            this.request    = request;
            this.requestID  = request.Meta.Id;
            state           = TokenState.Reserved;
        }
    }


}