using System;
using System.Collections.Generic;
using System.Linq;
using Momentum.Abilities;
using Momentum.HSM.Hero.Behavior;


namespace Momentum
{

    public interface IAbilityEngine
    {
        public void Cast(Ability ability);
        public void Cast(List<Ability> abilities);
    }

    public class AbilityEngine : IAbilityEngine
    {
        Arbiter     arbiter;

        Factory     factory;
        Router      router;
        Processor   processor;

        public void Cast(Ability ability)           { router.Inbound.Enqueue(factory.Request(ability)); }
        public void Cast(List<Ability> abilities)   { abilities.ForEach(ability => Cast(ability));      }

        public AbilityEngine(Context context)
        {
            arbiter   = new Arbiter(processor);
            factory   = new Factory(context);
            router    = new Router(factory, processor);
            processor = new Processor(factory);
        }

        public void Tick()
        {
            router.Process();
            arbiter.Process();
        }
    }
}


public struct ReservedEntries
{
    public HashSet<TokenEntry> stateful;
    public HashSet<TokenEntry> casting;
}

namespace Momentum.Abilities
{
    public class Arbiter
    {     
        Processor processor;
        Dictionary<Token, TokenEntry> reservations = new();

        public Arbiter(Processor processor)
        {
            this.processor = processor;
        }

        public void Process()
        {
            var reserved = ProcessReservedEntries();
            ProcessCasting(reserved.casting);
        }

        public ReservedEntries ProcessReservedEntries()
        {
            HashSet<TokenEntry> stateful = new();
            HashSet<TokenEntry> casting  = new();

            reservations.Values.ToList().ForEach(entry =>
            {
                if (entry.state != TokenState.Reserved)
                    return;
                
                if (entry.request.ability.casting.requiresState)
                    stateful.Add(entry);
                else
                    casting.Add(entry);
            });

            return new ReservedEntries { stateful = stateful, casting = casting };
        }



        public bool TryReserve(Request request)
        {
            List<Token> accepted = new();

            foreach (var token in request.ability.arbitration.tokens)
            {
                if (Reserve(token, request))
                    accepted.Add(token);
                else
                {
                    accepted.ForEach(token => Release(token));
                    return false;
                }
            }
            return true;
        }
        
        private bool Reserve(Token token, Request request)
        {
            if (!reservations.ContainsKey(token))
            {   
                reservations[token] = new TokenEntry(request);
                return true;
            }

            if (!request.ability.arbitration.preemptReserved)
                return false;

            var entry = reservations[token];

            if (request.preemptRequestID == entry.requestID )
            {
                reservations[token] = new TokenEntry(request);
                return true;
            }

            return false;
        }

        public void Release(Token token)
        {
            if (reservations.ContainsKey(token))
                reservations.Remove(token);
        }

        public void Release(Guid instanceID)
        {
            var keys = reservations.Keys.ToList();

            foreach (var key in keys)
            {
                if (reservations[key].instanceID == instanceID)
                    reservations.Remove(key);
            }
        }   

        public IEnumerable<Token> ReservedTokens => reservations.Keys.ToList();

    }

}