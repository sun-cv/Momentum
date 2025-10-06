using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Momentum
{
    

    public abstract class AbilityExecutor
    {
        public AbilityInstance instance;
        public AbilityRuntimeSettings runtime;

        public AbilityPhase         phase = AbilityPhase.None;
        public AbilityCastPhase castPhase = AbilityCastPhase.None;

        public EventBus.Signal<AbilityEvent> bus = new();
        public AbilityExecutionManager manager;

        public readonly List<RuntimeEffect> activeEffects = new();

        public AbilityExecutor(AbilityInstance instance)
        {
            this.instance   = instance;
            this.runtime    = instance.ability.runtime;

            foreach (var effect in instance.ability.effects) activeEffects.Add(effect.CreateRuntime(instance));
            foreach (var effect in activeEffects) effect.AssignBus(bus);
        }

        public void Tick()
        {
            OnTick();

            foreach(var effect in activeEffects)
                effect.Tick();
        }

        public void Activate()
        {
            if (phase is not AbilityPhase.None) return;

            meta.MarkActivating();
            OnActivate();
            Raise(AbilityPhase.Activate);
        }

        protected void Active()
        {
            if (phase is not AbilityPhase.Activate) return;
            meta.MarkActivated();
            OnActive();
            Raise(AbilityPhase.Active);
        }

        protected void Execute()
        {
            if (phase is not AbilityPhase.Active) return;

            meta.MarkExecuted();
            OnExecute();
            Raise(AbilityPhase.Execute);
        }

        public void Cancel()
        {
            if (!CanCancel()) return;

            meta.MarkCancelled();
            OnCancel();
            Raise(AbilityPhase.Cancel);
        }

        public void Interrupt()
        {
            if (!CanInterrupt()) return;

            Raise(AbilityPhase.Interrupt);
            OnInterrupt();
            meta.MarkInterrupted();
        }

        protected void Complete()
        {
            if (phase is not AbilityPhase.Execute) return;

            meta.MarkCompleted();
            OnComplete();
            Raise(AbilityPhase.Complete);
        }

        protected void Deactivate()
        {
            if (phase is not AbilityPhase.Complete) return;

            meta.MarkDeactivating();
            OnDeactivate();
            Raise(AbilityPhase.Deactivate);
        }

        protected void Deactivated()
        {
            meta.MarkDeactivated();
        }

        protected void CastStart()
        {
            if (castPhase is not AbilityCastPhase.None) return;
            meta.MarkCastActivated();
            OnCastStart();
            RaiseCast(AbilityCastPhase.Start);
        }

        protected void CancelCast()
        {
            meta.MarkCastCancelled();
            OnCastCancel();
            RaiseCast(AbilityCastPhase.Cancel);
        }

        protected void InterruptCast()
        {
            meta.MarkCastInterrupted();
            OnCastInterrupt();
            RaiseCast(AbilityCastPhase.Interrupt);
        }

        protected void CompleteCast()
        {
            meta.MarkCastCompleted();
            OnCastComplete();
            RaiseCast(AbilityCastPhase.Complete);
        }

        void Raise(AbilityPhase phase) 
        { 
            this.phase = phase;
            foreach (var effect in activeEffects) effect.OnPhase(phase);
            DebugLogPhase();
        }
        
        void RaiseCast(AbilityCastPhase phase)
        {
            this.castPhase = phase;
            foreach (var effect in activeEffects) effect.OnCastPhase(phase);
            DebugLogCastPhase();
        }

        protected virtual void OnTick()         {}
        protected virtual void OnActivate()     {}
        protected virtual void OnActive()       {}
        protected virtual void OnExecute()      {}
        protected virtual void OnCancel()       {}
        protected virtual void OnInterrupt()    {}
        protected virtual void OnComplete()     {}
        protected virtual void OnDeactivate()   {}
        protected virtual void OnLinger()       {}

        protected virtual bool OnCanCancel()    { return false; }
        protected virtual bool OnCanInterrupt() { return false; }

        protected virtual void OnCastStart()    {}
        protected virtual void OnCastCancel()   {}
        protected virtual void OnCastInterrupt(){}
        protected virtual void OnCastComplete() {}

        public virtual bool ShouldBecomeConcurrent() => false;

        public bool CanCancel()      => OnCanCancel();
        public bool CanInterrupt()   => OnCanInterrupt();
        

        public bool AllEffectPhase(EffectPhase phase) => activeEffects.All((effect) => effect.phase == phase);
        public bool AnyEffectPhase(EffectPhase phase) => activeEffects.Any((effect) => effect.phase == phase);

        void DebugLogPhase()        => Logwin.Log($"Phase {instance.ability.id}",      phase, "Ability Executor" );
        void DebugLogCastPhase()    => Logwin.Log($"Cast Phase {instance.ability.id}", phase, "Ability Executor" );
    
        public AbilityMeta meta = new();

        public bool IsActivating    => phase == AbilityPhase.Activate;
        public bool IsActive        => phase == AbilityPhase.Active;
        public bool IsExecuting     => phase == AbilityPhase.Execute;
        public bool IsCancelled     => phase == AbilityPhase.Cancel;
        public bool IsInterrupted   => phase == AbilityPhase.Interrupt;
        public bool IsComplete      => phase == AbilityPhase.Complete;
        public bool IsDeactivating  => phase == AbilityPhase.Deactivate;
        public bool IsDeactivated   => phase == AbilityPhase.Deactivated;

        public bool IsUnreachable   => phase == AbilityPhase.None || phase == AbilityPhase.Cancel || phase == AbilityPhase.Interrupt || phase == AbilityPhase.Deactivated;
    }
}
