using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum.Abilities
{
    

    public abstract class Executor
    {
        public ExecutionManager manager;

        public Instance         instance;
        public RuntimeSettings  runtime;

        public Phase         phase = Phase.None;
        public CastPhase castPhase = CastPhase.None;

        public EventBus.Signal<AbilityEvent> bus          = new();
        public readonly List<RuntimeEffect> activeEffects = new();

        public Executor(ExecutionManager manager, Instance instance)
        {
            this.manager    = manager;
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
            Raise(Phase.Activating);
        }

        protected void Active()
        {
            if (phase is not Phase.Activating) return;
            meta.MarkActive();
            OnActive();
            Raise(Phase.Active);
        }

        protected void Execute()
        {
            if (phase is not Phase.Active) return;

            meta.MarkExecuting();
            OnExecute();
            Raise(Phase.Executing);
        }

        public void Cancel()
        {
            if (!CanCancel()) return;

            meta.MarkCancelled();
            OnCancel();
            Raise(Phase.Cancelled);
        }

        public void Interrupt()
        {
            if (!CanInterrupt()) return;

            Raise(Phase.Interrupted);
            OnInterrupt();
            meta.MarkInterrupted();
        }

        protected void Complete()
        {
            if (phase is not Phase.Executing) return;

            meta.MarkCompleted();
            OnComplete();
            Raise(Phase.Completed);
        }

        protected void Deactivate()
        {
            if (phase is not Phase.Completed) return;

            meta.MarkDeactivated();
            OnDeactivate();
            Raise(Phase.Deactivating);
        }

        protected void Deactivated()
        {
            meta.MarkDeactivated();
            manager.DeactivateExecutor(meta.Id);
        }

        protected void CastStart()
        {
            if (castPhase is not CastPhase.None) return;
            meta.MarkCastStarting();
            OnCastStart();
            RaiseCast(CastPhase.Starting);
        }

        protected void CancelCast()
        {
            meta.MarkCastCancelled();
            OnCastCancel();
            RaiseCast(CastPhase.Cancelled);
        }

        protected void InterruptCast()
        {
            meta.MarkCastInterrupted();
            OnCastInterrupt();
            RaiseCast(CastPhase.Interrupted);
        }

        protected void CompleteCast()
        {
            meta.MarkCastCompleted();
            OnCastComplete();
            RaiseCast(CastPhase.Completed);
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
    
        public AbilityInstanceMeta meta = new();

        public bool IsActivating    => phase == Phase.Activating;
        public bool IsActive        => phase == Phase.Active;
        public bool IsExecuting     => phase == Phase.Executing;
        public bool IsCancelled     => phase == Phase.Cancelled;
        public bool IsInterrupted   => phase == Phase.Interrupted;
        public bool IsComplete      => phase == Phase.Completed;
        public bool IsDeactivating  => phase == Phase.Deactivating;
        public bool IsDeactivated   => phase == Phase.Deactivated;

        public bool IsUnreachable   => phase == Phase.None || phase == Phase.Cancelled || phase == Phase.Interrupted || phase == Phase.Deactivated;
    }
}
