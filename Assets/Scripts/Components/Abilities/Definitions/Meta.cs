using UnityEngine;


namespace Momentum.Abilities
{


    public class AbilityRequestMeta : RequestMeta
    {
        public float Buffered               { get; private set; }
        public float Validated              { get; private set; }
        public float Resolved               { get; private set; }

        public void MarkBuffered()          => Buffered         = Time.time;
        public void MarkValidated()         => Validated        = Time.time;
        public void MarkResolved()          => Resolved         = Time.time;
    }

    public class AbilityInstanceMeta : Meta
    {

        public float Activating             { get; private set; }
        public float Active                 { get; private set; }
        public float Executing              { get; private set; }
        public float Completing             { get; private set; }
        public float Completed              { get; private set; }
        public float Cancelled              { get; private set; }
        public float Interrupted            { get; private set; }
        public float Deactivating           { get; private set; }
        public float Deactivated            { get; private set; }

        public void MarkActivating()        => Activating       = Time.time;
        public void MarkActive()            => Active           = Time.time;
        public void MarkExecuting()         => Executing        = Time.time;
        public void MarkCompleting()        => Completing       = Time.time;
        public void MarkCompleted()         => Completed        = Time.time;
        public void MarkCancelled()         => Cancelled        = Time.time;
        public void MarkInterrupted()       => Interrupted      = Time.time;
        public void MarkDeactivating()      => Deactivating      = Time.time;
        public void MarkDeactivated()       => Deactivated      = Time.time;

        public float CastStarting           { get; private set; }
        public float CastCasting            { get; private set; }
        public float CastCompleting         { get; private set; }
        public float CastCompleted          { get; private set; }
        public float CastCancelled          { get; private set; }
        public float CastInterrupted        { get; private set; }

        public void MarkCastStarting()      => CastStarting     = Time.time;
        public void MarkCastCasting()       => CastCasting      = Time.time;
        public void MarkCastCompleting()    => CastCompleting   = Time.time;
        public void MarkCastCompleted()     => CastCompleted    = Time.time;
        public void MarkCastCancelled()     => CastCancelled    = Time.time;
        public void MarkCastInterrupted()   => CastInterrupted  = Time.time;
    }

}