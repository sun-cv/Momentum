using System;
using System.Collections.Generic;
using System.Diagnostics;
using Momentum.HSM.Hero.Movement;
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
    public abstract class AbilityEffect
    {
        [Header("Effect Identity")]
        public string id;

        [Header("Trigger Rules")]
        public EffectTriggerSettings trigger;

        public abstract RuntimeEffect CreateRuntime(AbilityInstance instance);
    }


    public abstract class RuntimeEffect
    {
        public readonly AbilityInstance instance;
        public readonly AbilityEffect effect;

        public List<string>                             triggerTags;
        public Dictionary<string, Action<AbilityEvent>> triggerResolver;

        public EffectPhase      phase;
        public AbilityPhase     abilityPhase;
        public AbilityCastPhase castPhase;

        public EventBus.Signal<AbilityEvent> bus;

        public RuntimeEffect(AbilityInstance instance, AbilityEffect effect)
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


        public void OnPhase(AbilityPhase phase)
        {
            abilityPhase = phase;

            switch(phase)
            {
                case AbilityPhase.Activate:
                    OnActivate();
                    break;
                case AbilityPhase.Execute:
                    OnExecute();
                    break;
                case AbilityPhase.Cancel:
                    OnCancel();
                    break;
                case AbilityPhase.Interrupt:
                    OnInterrupt();
                    break;
                case AbilityPhase.Complete:
                    OnComplete();
                    break;
                case AbilityPhase.Deactivate:
                    OnDeactivate();
                    break;
            }
        }

        public void OnCastPhase(AbilityCastPhase phase)
        {
            castPhase = phase;

            switch(phase)
            {
                case AbilityCastPhase.Start:
                    OnCastStart();
                    break;
                case AbilityCastPhase.Cancel:
                    OnCastCancel();
                    break;
                case AbilityCastPhase.Interrupt:
                    OnCastInterrupt();
                    break;
                case AbilityCastPhase.Complete:
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