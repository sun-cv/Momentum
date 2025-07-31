using System.Collections.Generic;
using System.Linq;
using Momentum.Interface;
using Momentum.Markers;
using Momentum.State;
using UnityEngine;



namespace Momentum.Actor.Hero
{


    public enum CommandType
    {
        Dash,
    }


    public class CommandDispatcher : ICommandDispatcher
    {
        private ICommandQueue queue;

        public void Register(ICommandQueue queue)
        {
            this.queue = queue;
        }

        public void Enqueue(ICommand command)
        {
            queue.Enqueue(command);
        }
    }


    public class CommandSystem
    {
        private HeroContext             context;
        private IStateMachineController stateMachine;

        public  CommandDispatcher dispatcher    = new();

        private CommandQueue central            = new();
        private CommandQueue buffer             = new();
        private CommandQueue command            = new();

        private float gracePeriod               = .5f;
        private float bufferPeriod              = .2f;

        private StatusFlag executing            = new();

        public void Initialize(HeroContext context)
        {
            this.context = context;
            stateMachine = Registry.Get<IStateMachineController>();

            dispatcher.Register(central);
        }


        public void Tick()
        {
            RouteCentral();
            ValidateBuffer();
            ProcessNextCommand();
        }


        private void RouteCentral()
        {
            while (!central.IsEmpty)
            {
                var request = central.Peek();

                if (IsExpired(request))
                {
                    central.Dequeue();
                    continue;
                }

                if (IsValid(request))
                {
                    command.Enqueue(request);
                    central.Dequeue();
                    return;
                }

                if (IsBufferable(request))
                {
                    buffer.Enqueue(request);
                    central.Dequeue();
                    return;
                }
                central.Dequeue();
                return;
            }
        }


        private void ValidateBuffer()
        {

            var bufferCount = buffer.Count;

            for (int i = 0; i < bufferCount; i++)
            {
                var request = buffer.Peek();

                if (IsExpired(request))
                {
                    buffer.Dequeue();
                    continue;
                }

                if (IsValid(request))
                {
                    command.Enqueue(request);
                    buffer.Dequeue();
                    return;
                }

                if (IsBuffering(request))
                {
                    buffer.Dequeue();
                    buffer.Enqueue(request);
                    continue;
                }

                buffer.Dequeue();
            }
        }

        private void ProcessNextCommand()
        {
            while (!command.IsEmpty)
            {
                var request = command.Peek();

                if (IsExpired(request))
                {
                    command.Dequeue();
                    continue;
                }

                if (executing)
                {
                    return;
                }

                executing.Set();

                request.Execute(stateMachine, () => { executing.Clear(); });

                command.Dequeue();
                buffer.Clear();

                return;
            }

        }


        public bool IsExpired(ICommand command)
        {
            bool expired = Time.time - command.RequestedTime > gracePeriod;
            if (expired) Debug.Log($"Expired command: {command.GetType()}");
            return expired;
        }


        public bool IsBuffering(ICommand command)
        {
            bool buffering = Time.time - command.RequestedTime < bufferPeriod;
            if (buffering) Debug.Log($"Buffering command: {command.GetType()}");
            return buffering;
        }


        public bool IsValid(ICommand command)
        {
            var validators = CommandValidatorRegistry.Get(command.GetType());
            bool valid = validators.Validate(context);
            if (!valid) Debug.Log($"Invalid command: {command.GetType()}");
            return valid;
        }

        public bool IsBufferable(ICommand command)
        {
            bool bufferable = command is ICommandBufferable;
            if (!bufferable) Debug.Log($"Command is not bufferable: {command.GetType()}");
            return bufferable;
        }


    }








    public class CommandQueue : ICommandQueue
    {
        protected readonly Queue<ICommand> queue  = new();

        public void Enqueue(ICommand command)
        {
            if (queue.Any(queuedCommand => queuedCommand.Type == command.Type))
            {
                return;
            }

            queue.Enqueue(command);
        }

        public ICommand Peek()
        {
            return queue.Count == 0 ? null : queue.Peek();
        }

        public ICommand Dequeue()
        {
            return queue.Count == 0 ? null : queue.Dequeue();
        }

        public void Clear()
        {
            queue.Clear();
        }

        public bool IsEmpty => queue.Count == 0;
        public float Count => queue.Count;
    }

}
