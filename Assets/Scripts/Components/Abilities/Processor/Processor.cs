

namespace Momentum.Abilities
{
    public interface IEvaluator
    {
        public Response Evaluate(Request ability);
    }

    public class Processor
    {
        Factory     factory;

        Resolver    resolver;
        Validator   validator;

        public Processor(Factory factory) 
        {
            this.factory = factory;

            resolver  = new Resolver();
            validator = new Validator();
        }

        public IEvaluator Resolver  => resolver;
        public IEvaluator Validator => validator;
    }

    public class Validator : IEvaluator
    {
        public Response Evaluate(Request request) 
        {
            return Response.Accepted;
        }
    }

}