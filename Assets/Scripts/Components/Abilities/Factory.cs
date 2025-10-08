

namespace Momentum.Abilities
{

    public class Factory
    {
        Context context;

        public Factory(Context context) 
        {
            this.context = context;
        }

        public Request Request(Ability ability)
        {
            return new Request(ability);
        }

        public Instance Instance(Ability ability)
        {
            return new Instance(context, ability);
        }

        public Executor Executor(ExecutionManager manager, Request request)
        {
            switch(request.ability.execution)
            {
                case Execution.Cast:    return new CastExecutor   (manager, Instance(request.ability));
                case Execution.Action:  return new ActionExecutor (manager, Instance(request.ability));
                case Execution.Instant: return new InstantExecutor(manager, Instance(request.ability));
                case Execution.Channel: return new ChannelExecutor(manager, Instance(request.ability));
                case Execution.Toggle:  return new ToggleExecutor (manager, Instance(request.ability));
                default: throw new System.Exception("Unknown execution type: " + request.ability.execution);
            }
        }


        public Context Context => context;
    }
}