using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum.Abilities
{
    

    public abstract class AbilityExecutor
    {
        public Instance instance;
        public RuntimeSettings runtime;

        public Phase         phase = Phase.None;
        public CastPhase castPhase = CastPhase.None;

        public EventBus.Signal<AbilityEvent> bus = new();
        // public AbilityExecutionManager manager;

        public readonly List<RuntimeEffect> activeEffects = new();

        public AbilityExecutor(Instance instance)
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
            if (phase is not Phase.None) return;

            meta.MarkActivating();
            OnActivate();
            Raise(Phase.Activate);
        }

        protected void Active()
        {
            if (phase is not Phase.Activate) return;
            meta.MarkActivated();
            OnActive();
            Raise(Phase.Active);
        }

        protected void Execute()
        {
            if (phase is not Phase.Active) return;

            meta.MarkExecuted();
            OnExecute();
            Raise(Phase.Execute);
        }

        public void Cancel()
        {
            if (!CanCancel()) return;

            meta.MarkCancelled();
            OnCancel();
            Raise(Phase.Cancel);
        }

        public void Interrupt()
        {
            if (!CanInterrupt()) return;

            Raise(Phase.Interrupt);
            OnInterrupt();
            meta.MarkInterrupted();
        }

        protected void Complete()
        {
            if (phase is not Phase.Execute) return;

            meta.MarkCompleted();
            OnComplete();
            Raise(Phase.Complete);
        }

        protected void Deactivate()
        {
            if (phase is not Phase.Complete) return;

            meta.MarkDeactivating();
            OnDeactivate();
            Raise(Phase.Deactivate);
        }

        protected void Deactivated()
        {
            meta.MarkDeactivated();
        }

        protected void CastStart()
        {
            if (castPhase is not CastPhase.None) return;
            meta.MarkCastActivated();
            OnCastStart();
            RaiseCast(CastPhase.Start);
        }

        protected void CancelCast()
        {
            meta.MarkCastCancelled();
            OnCastCancel();
            RaiseCast(CastPhase.Cancel);
        }

        protected void InterruptCast()
        {
            meta.MarkCastInterrupted();
            OnCastInterrupt();
            RaiseCast(CastPhase.Interrupt);
        }

        protected void CompleteCast()
        {
            meta.MarkCastCompleted();
            OnCastComplete();
            RaiseCast(CastPhase.Complete);
        }

        void Raise(Phase phase) 
        { 
            this.phase = phase;
            foreach (var effect in activeEffects) effect.OnPhase(phase);
            DebugLogPhase();
        }
        
        void RaiseCast(CastPhase phase)
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

        public bool IsActivating    => phase == Phase.Activate;
        public bool IsActive        => phase == Phase.Active;
        public bool IsExecuting     => phase == Phase.Execute;
        public bool IsCancelled     => phase == Phase.Cancel;
        public bool IsInterrupted   => phase == Phase.Interrupt;
        public bool IsComplete      => phase == Phase.Complete;
        public bool IsDeactivating  => phase == Phase.Deactivate;
        public bool IsDeactivated   => phase == Phase.Deactivated;

        public bool IsUnreachable   => phase == Phase.None || phase == Phase.Cancel || phase == Phase.Interrupt || phase == Phase.Deactivated;
    }
}
