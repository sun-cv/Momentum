using System;
using UnityEngine;


namespace Momentum
{


    public interface IEvent {};


    public readonly struct EventMeta
    {
        public Guid  Id                 { get; }
        public float Timestamp          { get; }

        public EventMeta(Guid? guid = null)
        {
            Id          = guid ?? Guid.NewGuid();
            Timestamp   = Time.time;
        }
    }
}
