


using System;
using UnityEngine;

namespace Momentum
{


    public class EntityContext : ContextCast
    {
        public GameObject               self;
        public EntityBodyContext        body;
        public EntityMovementContext    movement;
        public EntityIntent             intent;
    }

}