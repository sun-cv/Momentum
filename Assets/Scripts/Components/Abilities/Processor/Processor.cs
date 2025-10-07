

namespace Momentum.Abilities
{
    public enum Decision { Accept, Reject, Buffer, Resolve, Execute, Pending }

    public interface IEvaluator
    {
        public Decision Decide(Request ability);
    }

    public class Processor
    {
        Resolver    resolver;
        Validator   validator;

        public Decision Resolve(Request request)  => resolver.Decide(request);
        public Decision Validate(Request request) => validator.Decide(request);
    }


    public class Resolver : IEvaluator
    {

        public Decision Decide(Request request) 
        { 
            return Decision.Accept;
        }

    }

    public class Validator : IEvaluator
    {
        public Decision Decide(Request request) 
        {
            return Decision.Accept;
        }

    }

}