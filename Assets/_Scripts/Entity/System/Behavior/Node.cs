using System.Collections.Generic;


namespace Momentum.Actor.Behavior
{


    public class BehaviorTree : Node
    {
        public BehaviorTree(string name) : base(name) {}

        public override Status Process()
        {
            while (currentChild < children.Count)
            {
                var status = children[currentChild].Process();
                if (status != Status.Success)
                {
                    return status;
                }
                currentChild++;
            }
            return Status.Success;
        }

    }


    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy) : base(name)
        {
            this.strategy = strategy;
        }

        public override Status Process()    => strategy.Process();
        public override void Reset()        => strategy.Reset();
    }


    public class Node 
    {
        public enum Status 
        { 
            Success,
            Failure,
            Running 
        };

        public readonly string      name;
        public readonly List<Node>  children = new();
        protected int               currentChild;

        public Node(string name = "Node")
        {
            this.name = name;
        }

        public void AddChild(Node child)    => children.Add(child);
        public virtual Status Process()     => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }

    }







}