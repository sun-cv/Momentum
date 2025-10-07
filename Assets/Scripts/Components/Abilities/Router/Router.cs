

namespace Momentum.Abilities
{


    public class Router
    {
        Factory factory;
        Processor processor;

        IPipeline inbound;
        IPipeline buffer;
        IPipeline eligible;
        IPipeline resolved;
        IPipeline pending;

        public Router(Factory factory, Processor processor)
        {
            this.factory   = factory;
            this.processor = processor;

            inbound  = new Inbound   (this);
            buffer   = new Buffer    (this);
            eligible = new Eligible  (this);
            resolved = new Resolved  (this);
            pending  = new Pending   (this);
        }

        public void Process()
        {
            inbound .Process();
            buffer  .Process();
            eligible.Process();
            resolved.Process();
            pending .Process();
        }

        public IPipeline Inbound   => inbound;
        public IPipeline Buffer    => buffer;
        public IPipeline Eligible  => eligible;
        public IPipeline Resolved  => resolved;
        public IPipeline Pending   => pending;

        public Factory Factory     => factory;
        public Processor Processor => processor;
    }




    public class Inbound : PipelineBase
    {
        public Inbound(Router router) => this.router = router;
        protected override void ProcessItem(Request request) 
        {
            var decision = router.Processor.Validate(request);

            switch(decision)
            {
                case Decision.Buffer:
                    request.Meta.MarkBuffered();
                    router.Buffer.Enqueue(request);
                break;

                case Decision.Reject:
                    request.Meta.MarkResolved
                break;

                case Decision.Accept:
                break;
            }
        }

    }

    public class Buffer : PipelineBase
    {
        public Buffer(Router router) => this.router = router; 
        protected override void ProcessItem(Request request) {}

    }

    public class Eligible : PipelineBase
    {
        public Eligible(Router router) => this.router = router;
        protected override void ProcessItem(Request request) {}
 
    }

    public class Resolved : PipelineBase
    {
        public Resolved(Router router) => this.router = router; 
        protected override void ProcessItem(Request request) {}

    }

    public class Pending : PipelineBase
    {
        public Pending(Router router) => this.router = router; 
        protected override void ProcessItem(Request request) {}

    }

}