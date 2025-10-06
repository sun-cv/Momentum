using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;



namespace Momentum
{

    public enum CooldownPhase
    {
        Idle, Tracking, Blocking, Expired   
    }

    public interface IRuntimeCooldown
    {
        public void Enable();
        public void Trigger();

        public CooldownPhase GetPhase();
    }


    [Serializable]
    public abstract class CooldownRule
    {

        [Header("Config")]
        public string label;
        
        [Header("Allow Rule to Rule")]
        public bool   enableSignal;
        public bool   enablePublish;
        public bool   enableSubscribe;
        public bool   enableTrigger;
        public bool   enablePayload;

        [Header("Subscription")]
        public string SubscribedTo;
        public string PublishingTo;

        [Header("Lifecycle")]
        public bool   automaticStartup;
        public bool   enableSystemTrigger;

        public abstract IRuntimeCooldown CreateRuntime(CooldownContext context);
        
    }


    public abstract class BaseCooldownRuntime<TRule> : IRuntimeCooldown where TRule : CooldownRule
    {
        protected TRule             rule;
        protected CooldownContext   context;
        protected CooldownPhase     phase = CooldownPhase.Idle;
    
        protected BaseCooldownRuntime(TRule rule, CooldownContext context)
        {
            this.rule    = rule;
            this.context = context;
            Subscribe();
        }
    
        public abstract void Enable();
        public abstract void Trigger();

        public virtual void TriggerOnSignal()               => Trigger();
        public virtual void TriggerOnSignal(object payload) => Trigger();
    

        protected void Publish()                    => context.EventBus.Publish(rule.PublishingTo);
        protected void Publish(object payload)      => context.EventBus.Publish(rule.PublishingTo, payload);

        protected void Subscribe()
        {
            if (rule.enableSignal && rule.enableSubscribe && !string.IsNullOrEmpty(rule.SubscribedTo))
            {
                if (rule.enablePayload) context.EventBus.Subscribe(rule.SubscribedTo, (payload) => TriggerOnSignal(payload));
                else                    context.EventBus.Subscribe(rule.SubscribedTo, ()        => TriggerOnSignal());
            }
        }
    
        protected void Idle()           => phase = CooldownPhase.Idle;
        protected void Tracking()       => phase = CooldownPhase.Tracking;
        protected void Blocking()       => phase = CooldownPhase.Blocking;
        protected void Expired()        => phase = CooldownPhase.Expired;
        public CooldownPhase GetPhase() => phase;
    }




}