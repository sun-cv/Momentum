using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Momentum 
{
    public class SequentialPhase : ISequence 
    {
        readonly List<PhaseStep> steps;
        readonly CancellationToken cancellationToken;
        int index = -1;
        Task current;
        public bool IsDone { get; private set; }

        public SequentialPhase(List<PhaseStep> steps, CancellationToken cancellationToken) 
        {
            this.steps = steps;
            this.cancellationToken = cancellationToken;
        }
        
        public void Start() => Next();

        public bool Update() 
        {
            if (IsDone) return true;
            if (current == null || current.IsCompleted) Next();
            return IsDone;
        }

        void Next() 
        {
            index++;
            if (index >= steps.Count)
            {
                IsDone = true;
                return;
            }
            current = steps[index](cancellationToken);
        }
    }
}