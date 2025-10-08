

namespace Momentum.Abilities
{
    public class Inbound : PipelineBase
    {
        public Inbound(Router router) => this.router = router;

        protected override void ProcessItem(Request request) 
        {
            var decision = router.Processor.Validator.Evaluate(request);

            switch(decision)
            {
                case Response.Accepted:
                    request.Meta.MarkAccepted();
                    router.Eligible.Enqueue(request);
                break;

                case Response.Buffered:
                    request.Meta.MarkBuffered();
                    router.Buffer.Enqueue(request);
                break;

                case Response.Rejected:
                    request.Meta.MarkRejected();
                break;
           }
        }

    }

}