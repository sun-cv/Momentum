using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Momentum
{

    public class CommandDispatcher : ICommandDispatcher
    {
        private ICommandQueue queue;

        public void Register(ICommandQueue queue)
        {        
            this.queue = queue;
        }

        public void Enqueue(Command command)
        {
            queue.Enqueue(command);
        }
    }


    public class CommandSystem
    {
        private HeroContext                 context;
        private IStateMachineController     stateMachine;

        public  CommandDispatcher dispatcher    = new();

        private CommandQueue centralQueue       = new();
        private CommandQueue bufferQueue        = new();
        private CommandQueue commandQueue       = new();

        private Command command;
        private Command pendingCommand;

        private bool ActiveCommand => command != null;
        private bool PendingCommand => pendingCommand != null;

        private float MaxBuffer = 1;
        private float MaxExpire = 1;

        private bool debug = false;

        public void Initialize(HeroContext context)
        {
            this.context    = context;

            stateMachine    = Registry.Get<IStateMachineController>();

            dispatcher.Register(centralQueue);
        }

        public void Tick()
        {
            RouteCentral();
            ValidateBuffer();
            ProcessNextCommand();
        }

        private void RouteCentral()
        {
            while (!centralQueue.IsEmpty)
            {
                var request = centralQueue.Peek();

                Register(request);

                if (IsExpired(request))
                {
                    centralQueue.Dequeue();
                    continue;
                }

                if (!ActiveCommand && !PendingCommand && IsValid(request))
                {
                    commandQueue.Enqueue(request);
                    centralQueue.Dequeue();
                    return;
                }

                if (IsBufferable(request))
                {
                    bufferQueue.Enqueue(request);
                    centralQueue.Dequeue();
                    return;
                }

                centralQueue.Dequeue();
                return;
            }
        }


        private void ValidateBuffer()
        {

            float bufferCount = bufferQueue.Count;

            for (int i = 0; i < bufferCount; i++)
            {
                var request = bufferQueue.Peek();

                if (IsExpired(request))
                {
                    bufferQueue.Dequeue();
                    continue;
                }

                if (IsPriority(request) && IsValid(request))
                {
                    bufferQueue.Dequeue();
                    commandQueue.Enqueue(request);
                    return;
                }

                if (!ActiveCommand && !PendingCommand && IsValid(request))
                {
                    bufferQueue.Dequeue();
                    commandQueue.Enqueue(request);
                    return;             
                }

                if (IsBuffering(request))
                {
                    bufferQueue.Dequeue();
                    bufferQueue.Enqueue(request);
                    continue;
                }

                bufferQueue.Dequeue();
            }
        }

        private void ProcessNextCommand()
        {
            while (!commandQueue.IsEmpty)
            {
                var request = commandQueue.Peek();

                if (IsExpired(request))
                {
                    commandQueue.Dequeue();
                    continue;
                }
 
                if (!ActiveCommand && !PendingCommand)
                {
                    command = request;
                    command.Execute();
                    commandQueue.Dequeue();
                    return;
                }

                if (command is ICancellable)
                {
                    Debug.Log("Cancel request");
                    if (command.CanCancel())
                    {
                        Debug.Log("Cancel request accepted");
                        pendingCommand = request;
                        commandQueue.Dequeue(); 
                        command.RequestCancel();
                    }
                    return;
                }
                return;
            }
        }

        private void ClearCommand(Result result)
        {
            command = null;

            switch (result)
            {
                case Result.Success:
                    command = null;
                    break;

                case Result.Interrupted:
                    command = null;
                    bufferQueue.Clear();
                    commandQueue.Clear();
                    break;
                    
                case Result.Failed:
                    break;

                case Result.Cancelled:
                    if (pendingCommand != null)
                    {
                        command = pendingCommand;
                        pendingCommand  = null;
                        command.Execute();
                        return;
                    }
                    break;
            }
        }

        bool IsExpired(Command request)
        {
            bool expired = Time.time - request.TimeRequested > request.ExpirePeriod && request.ExpirePeriod < MaxExpire;
            if (expired && debug) Debug.Log($"Expired command: {request.GetType()}");
            return expired;
        }

        bool IsBuffering(Command request)
        {
            bool buffering = Time.time - request.TimeRequested < request.BufferPeriod && request.BufferPeriod < MaxBuffer;
            if (buffering && debug) Debug.Log($"Buffering command: {request.GetType()}");
            return buffering;
        }

        bool IsValid(Command request)
        {
            var validators = CommandValidatorRegistry.Get(request.GetType());
            bool valid = validators.Validate();
            if (!valid && debug) Debug.Log($"Invalid command: {request.GetType()}");
            return valid;
        }

        bool IsBufferable(Command request)
        {
            if (request is IBufferable)
            {
                request.Buffer();
                return true;
            }
            if (debug) Debug.Log($"Command is not bufferable: {request.GetType()}");
            return false;
        }

        bool IsPriority(Command request)
        {
            var requestPriority = request.Priority;
            var currentPriority = command?.Priority ?? Priority.None;
            var queuePriority   = commandQueue.HighestPriority();
            var pendingPriority = pendingCommand?.Priority ?? Priority.None;

            bool priority = requestPriority > currentPriority &&
                            requestPriority > queuePriority &&
                            requestPriority > pendingPriority;

            if (!priority && debug) 
            {
                Debug.Log($"Low Priority command: {request.GetType()}");
                Debug.Log($"currentPriority: {requestPriority > currentPriority}");
                Debug.Log($"queuePriority: {requestPriority > queuePriority}");
                Debug.Log($"pendingPriority: {requestPriority > pendingPriority}");
            }
            return priority;
        }

        void Register(Command request)
        {
            request.Initialize(stateMachine, result => ClearCommand(result));
        }
    }


    public class CommandQueue : ICommandQueue
    {
        protected readonly Queue<Command> queue  = new();
        protected Priority priority;

        public void Enqueue(Command command)
        {
            if (queue.Count(cmd => cmd.GetType() == command.GetType()) > 3)
            {
                return;
            }

            queue.Enqueue(command);
        }

        public Command Peek()
        {
            return queue.Count == 0 ? null : queue.Peek();
        }

        public Command Dequeue()
        {
            return queue.Count == 0 ? null : queue.Dequeue();
        }

        public void Clear()
        {
            queue.Clear();
        }

        public Priority HighestPriority()
        {
            return IsEmpty ? Priority.None : queue.Max((command) => command.Priority);
        }

        public bool IsEmpty => queue.Count == 0;
        public float Count => queue.Count;
    }

    
}
