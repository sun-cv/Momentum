

namespace Momentum.Abilities
{
    public interface IPipeline
    {
        public void Enqueue(Request ability);
        public void Process();
        public GenericQueue<Request> Queue { get; }
    }

    public abstract class PipelineBase : IPipeline
    {
        protected Router router;
        protected GenericQueue<Request> queue;

        public void Enqueue(Request request)
        {
            queue.Enqueue(request);
        }

        public void Process()
        {
            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                var request = queue.Dequeue();
                ProcessItem(request);
            }
        }

        protected abstract void ProcessItem(Request request);

        public GenericQueue<Request> Queue => queue;
    }
}