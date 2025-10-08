using System;
using System.Collections.Generic;
using System.Diagnostics;
using Momentum.HSM.Hero.Movement;
using Momentum.Abilities;
using UnityEngine;

namespace Momentum
{
    [Serializable]
    public struct EffectTriggerSettings
    {
        public bool enable;
        public bool enablePayload;

        public List<string> tags;
    }

    [Serializable]
    public abstract class Effect
    {
        [Header("Effect Identity")]
        public string id;

        [Header("Trigger Rules")]
        public EffectTriggerSettings trigger;

        public abstract RuntimeEffect CreateRuntime(Instance instance);
    }


    public abstract class RuntimeEffect
    {
        public readonly Instance instance;
        public readonly Effect effect;

        public List<string>                             triggerTags;
        public Dictionary<string, Action<AbilityEvent>> triggerResolver;

        public EffectPhase      phase;
        public Phase     Phase;
        public CastPhase castPhase;

        public EventBus.Signal<AbilityEvent> bus;

        public RuntimeEffect(Instance instance, Effect effect)
        {
            this.instance       = instance;
            this.effect         = effect;
            this.triggerTags    = effect.trigger.tags;
            this.phase          = EffectPhase.None;
        }

        public abstract void Tick();

        protected virtual void OnActivate()     {}     
        protected virtual void OnExecute()      {}  
        protected virtual void OnCancel()       {}   
        protected virtual void OnInterrupt()    {}    
        protected virtual void OnComplete()     {}     
        protected virtual void OnDeactivate()   {}   
        protected virtual void OnLinger()       {}   

        protected virtual void OnCastStart()    {}
        protected virtual void OnCastCancel()   {}
        protected virtual void OnCastInterrupt(){}
        protected virtual void OnCastComplete() {}


        public void OnPhase(Phase phase)
        {
            Phase = phase;

            switch(phase)
            {
                case Phase.Activating:
                    OnActivate();
                    break;
                case Phase.Executing:
                    OnExecute();
                    break;
                case Phase.Cancelled:
                    OnCancel();
                    break;
                case Phase.Interrupted:
                    OnInterrupt();
                    break;
                case Phase.Completed:
                    OnComplete();
                    break;
                case Phase.Deactivated:
                    OnDeactivate();
                    break;
            }
        }

        public void OnCastPhase(CastPhase phase)
        {
            castPhase = phase;

            switch(phase)
            {
                case CastPhase.Starting:
                    OnCastStart();
                    break;
                case CastPhase.Cancelled:
                    OnCastCancel();
                    break;
                case CastPhase.Interrupted:
                    OnCastInterrupt();
                    break;
                case CastPhase.Completed:
                    OnCastComplete();
                    break;
            }
        }

        void Trigger(AbilityEvent evt)  => triggerResolver[evt.id](evt);
        void Raise(EffectPhase phase)   => this.phase = phase;


        public void AssignBus(EventBus.Signal<AbilityEvent> bus) { this.bus = bus; triggerTags.ForEach(trigger => this.bus.Subscribe(trigger, Trigger)); }


        void DebugLog() => Logwin.Log($"Effect {effect.id}", phase, "Ability Executor");
    }
}