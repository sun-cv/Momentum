using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;



namespace Momentum
{

    public interface IAbilitySystem
    {
        void Initialize(Context context);
        void Tick();

        void Cast();
        void Cast(AbilityState state);

        void Request(Ability ability);
        void Request(List<Ability> ability);
        
        bool HasEngaged(AbilityState state);
        public bool CastRequested { get; } 
    }

    public class AbilityFactory
    {
        private Context context;

        public AbilityFactory(Context context) => this.context = context;

        public AbilityRequest  CreateRequest(Ability ability)           => new AbilityRequest(ability);
        public AbilityInstance CreateInstance(AbilityRequest request)   => new AbilityInstance(context, request.ability);

        public AbilityExecutor CreateExecutor(AbilityRequest request)
        {
            var instance = CreateInstance(request);

            if (instance.ability.execution == AbilityExecution.Cast)    return new CastExecutor(instance);
            if (instance.ability.execution == AbilityExecution.Action)  return new ActionExecutor(instance);
            if (instance.ability.execution == AbilityExecution.Instant) return new InstantExecutor(instance);
            if (instance.ability.execution == AbilityExecution.Channel) return new ChannelExecutor(instance);
            if (instance.ability.execution == AbilityExecution.Toggle)  return new ToggleExecutor(instance);

            return null;            
        }

        public Context Context => context;
    }


    public class AbilitySystem : IAbilitySystem
    {
        AbilityFactory          factory;
        AbilityValidator        validator;
        AbilityCooldowns        cooldown;
        Abilitypipeline         pipeline;
        AbilityResolver         resolver;
        AbilityCaster           caster;
        AbilityExecutionManager execution;
        AbilityComboManager     combos;


        bool castRequested;

        public void Initialize(Context context)
        {
            combos      = new();
            cooldown    = new();
            resolver    = new();
            pipeline    = new(validator);
            factory     = new(context);
            validator   = new(context, cooldown);
            execution   = new(factory, cooldown, combos);
            caster      = new(pipeline, execution);
        }

        public void Cast() => caster.Cast();
        public void Cast(AbilityState state) => caster.Cast(state);

        public void Request(List<Ability> abilities)    => abilities.ForEach(ability => Request(ability));
        public void Request(Ability ability)            { combos .CheckForCombo(factory.CreateRequest(ability), out var request); pipeline.Inbound.Enqueue(request);}

        public bool Stateful(AbilityState state)        => caster.HasStateful(state);
        public bool HasEngaged(AbilityState state)      => caster.HasStateful(state) || execution.HasActiveStateExecutor(state);

        public void Tick()
        {
            cooldown.ProcessCooldowns();

            combos.Tick();

            pipeline.Inbound.Process();
            pipeline.Buffer .Process();
            pipeline.Valid  .Process();

            resolver.Resolve(pipeline.Valid, pipeline.Resolved, execution);

            pipeline.Resolved.Process();

            execution.Tick();
            
            DebugExecution();
        }

        public void DebugExecution()
        {
            Logwin.Log(" Exclusive:", execution.ExclusiveExecutors.FirstOrDefault()?.instance.ability.name, "Ability System");
            Logwin.Log("Concurrent:", execution.ConcurrentExecutors.FirstOrDefault()?.instance.ability.name, "Ability System");

        }

        public bool CastRequested => caster.CastRequest;
    }


    [Serializable]
    public class ComboSequence
    {
        public List<ComboChain> chains = new();
    }

    [Serializable]
    public struct ComboChain 
    {
        public Ability replace;
        public Ability ability;
    }

    public class ComboRequest
    {
        public bool isCombo;
        public bool isReplaced;

        public int step;
        public ComboChain    chain;
        public ComboSequence sequence;
    
        public ComboRequest(ComboSequence sequence, ComboChain chain, int step)
        {

        }
    }

    public class AbilityComboManager
    {
        readonly Dictionary<List<ComboSequence>, int>   possibleSequences    = new();
        readonly Dictionary<ComboSequence, int>         activeSequences      = new();

        public void Tick()
        {
        }



        public void CheckForCombo(AbilityRequest request, out AbilityRequest outbound)
        {
            outbound = request;

            if (!IsActiveCombo(request.ability) && !IsEligible(request))
                return;

            if (IsActiveCombo(request.ability))
            {
                foreach ( var (sequence, step) in activeSequences)
                {   
                    var chain = sequence.chains[step];
                    if (chain.replace == request.ability)
                    {
                        request.ability =  chain.ability;
                        request.combo = new(sequence, chain, step);
                    }
                }
            }


        }

        public void Handle(AbilityRequest request)
        {
        }
        

        public bool IsEligible(AbilityRequest request)   => request.ability.enableCombo && request.ability.combos.Count > 0;
        public bool IsCombo(AbilityRequest request)      => IsEligible(request)|| request.combo.isCombo || request.combo.isReplaced;      
        public bool IsActiveCombo(Ability ability)       => activeSequences.Keys.SelectMany(sequence => sequence.chains).Any( chain => chain.ability == ability);

    }
    


}