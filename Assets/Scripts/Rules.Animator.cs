using System;
using System.Collections.Generic;
using UnityEngine;



public class AnimatorParameter
{

    public struct Entry
    {
        public ServiceRate Rate;
        public int Parameter;
        public Action<Animator, Actor> Handler;
    }

    public struct Override
    {
        public enum ParamType { Float, Bool, Int }

        public int Parameter;
        public float Float;
        public bool Bool;
        public int Int;
        public ParamType Type;
    }

    public static Override Float(int param, float value) => new() { Parameter = param, Float = value, Type = Override.ParamType.Float };
    public static Override Bool (int param, bool value)  => new() { Parameter = param, Bool  = value, Type = Override.ParamType.Bool  };
    public static Override Int  (int param, int value)   => new() { Parameter = param, Int   = value, Type = Override.ParamType.Int   };

    public static int ResolvedAimX          = Animator.StringToHash(nameof(ResolvedAimX));
    public static int ResolvedAimY          = Animator.StringToHash(nameof(ResolvedAimY));
    public static int ResolvedCardinalAimX  = Animator.StringToHash(nameof(ResolvedCardinalAimX));
    public static int ResolvedCardinalAimY  = Animator.StringToHash(nameof(ResolvedCardinalAimY));
    public static int ResolvedFacingX       = Animator.StringToHash(nameof(ResolvedFacingX));
    public static int ResolvedFacingY       = Animator.StringToHash(nameof(ResolvedFacingY));
    public static int Alive                 = Animator.StringToHash(nameof(Alive));
    public static int Dead                  = Animator.StringToHash(nameof(Dead));
    public static int Inactive              = Animator.StringToHash(nameof(Inactive));
    public static int Disabled              = Animator.StringToHash(nameof(Disabled));
    public static int IsMoving              = Animator.StringToHash(nameof(IsMoving));
    public static int Idle                  = Animator.StringToHash(nameof(Idle));
    public static int IdleTime              = Animator.StringToHash(nameof(IdleTime));
    public static int CorpseFresh           = Animator.StringToHash(nameof(CorpseFresh));
    public static int CorpseDecaying        = Animator.StringToHash(nameof(CorpseDecaying));
    public static int CorpseConsumed        = Animator.StringToHash(nameof(CorpseConsumed));
    public static int CorpseRemains         = Animator.StringToHash(nameof(CorpseRemains));

        public enum State { Fresh, Decaying, Consumed, Remains, Disposal }

    public static Dictionary<string, int> Library = new()
    {
        { nameof(ResolvedAimX),             ResolvedAimX            },
        { nameof(ResolvedAimY),             ResolvedAimY            },
        { nameof(ResolvedCardinalAimX),     ResolvedCardinalAimX    },
        { nameof(ResolvedCardinalAimY),     ResolvedCardinalAimY    },
        { nameof(ResolvedFacingX),          ResolvedFacingX         },
        { nameof(ResolvedFacingY),          ResolvedFacingY         },
        { nameof(Alive),                    Alive                   },
        { nameof(Dead),                     Dead                    },
        { nameof(Inactive),                 Inactive                },
        { nameof(Disabled),                 Disabled                },
        { nameof(IsMoving),                 IsMoving                },
        { nameof(Idle),                     Idle                    },
        { nameof(IdleTime),                 IdleTime                },
        { nameof(CorpseFresh),              CorpseFresh             },
        { nameof(CorpseDecaying),           CorpseDecaying          },
        { nameof(CorpseConsumed),           CorpseConsumed          },
        { nameof(CorpseRemains),            CorpseRemains           },

    };

    public static readonly Dictionary<Type, Entry[]> Handlers = new()
    {
        { typeof(IAimable), new Entry[]
            {
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedAimX,          Handler = (animator, owner) => animator.SetFloat(ResolvedAimX,          ((IAimable)owner).ResolvedAim.X)                    },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedAimY,          Handler = (animator, owner) => animator.SetFloat(ResolvedAimY,          ((IAimable)owner).ResolvedAim.Y)                    },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedCardinalAimX,  Handler = (animator, owner) => animator.SetFloat(ResolvedCardinalAimX,  ((IAimable)owner).ResolvedAim.Cardinal.x)           },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedCardinalAimY,  Handler = (animator, owner) => animator.SetFloat(ResolvedCardinalAimY,  ((IAimable)owner).ResolvedAim.Cardinal.y)           },
            }
        },
        { typeof(IMovableActor), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = ResolvedFacingX,       Handler = (animator, owner) => animator.SetFloat(ResolvedFacingX,       ((IMovableActor)owner).ResolvedFacing.X)            },
                new() { Rate = ServiceRate.Loop, Parameter = ResolvedFacingY,       Handler = (animator, owner) => animator.SetFloat(ResolvedFacingY,       ((IMovableActor)owner).ResolvedFacing.Y)            },
                new() { Rate = ServiceRate.Loop, Parameter = Inactive,              Handler = (animator, owner) => animator.SetBool(Inactive,               ((IMovableActor)owner).Inactive)                    },
                new() { Rate = ServiceRate.Loop, Parameter = Disabled,              Handler = (animator, owner) => animator.SetBool(Disabled,               ((IMovableActor)owner).Disabled)                    },
                new() { Rate = ServiceRate.Loop, Parameter = IsMoving,              Handler = (animator, owner) => animator.SetBool(IsMoving,               ((IMovableActor)owner).IsMoving)                    },
            }
        },
        { typeof(IMortal), new Entry[]
            {
                new() { Rate = ServiceRate.Tick, Parameter = Alive,                 Handler = (animator, owner) => animator.SetBool(Alive,                  ((IMortal)owner).Alive)                             },
                new() { Rate = ServiceRate.Tick, Parameter = Dead,                  Handler = (animator, owner) => animator.SetBool(Dead,                   ((IMortal)owner).Dead)                              },
            }
        },
        { typeof(IIdle), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = Idle,                  Handler = (animator, owner) => animator.SetBool(Idle,                   ((IIdle)owner).IsIdle)                              },
                new() { Rate = ServiceRate.Loop, Parameter = IdleTime,              Handler = (animator, owner) => animator.SetFloat(IdleTime,              ((IIdle)owner).IsIdle.Duration)                     },
            }
        },
        { typeof(ICorpse), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = CorpseFresh,           Handler = (animator, owner) => { animator.SetBool(CorpseFresh,            ((ICorpse)owner).Condition == Decomposition.State.Fresh); Debug.Log("called");   }},
                new() { Rate = ServiceRate.Loop, Parameter = CorpseDecaying,        Handler = (animator, owner) => animator.SetBool(CorpseDecaying,         ((ICorpse)owner).Condition == Decomposition.State.Decaying)},
                new() { Rate = ServiceRate.Loop, Parameter = CorpseConsumed,        Handler = (animator, owner) => animator.SetBool(CorpseConsumed,         ((ICorpse)owner).Condition == Decomposition.State.Consumed)},
                new() { Rate = ServiceRate.Loop, Parameter = CorpseRemains,         Handler = (animator, owner) => animator.SetBool(CorpseRemains,          ((ICorpse)owner).Condition == Decomposition.State.Remains) },
            }
        },
    };

    public static readonly Dictionary<Type, Func<InputIntentSnapshot, Override[]>> InputIntentSnapshot = new()
    {
        { typeof(IAimable), snapshot => new[]
            {
                Float(ResolvedAimX,         snapshot.Aim.X),
                Float(ResolvedAimY,         snapshot.Aim.Y),
                Float(ResolvedCardinalAimX, snapshot.Aim.Cardinal.x),
                Float(ResolvedCardinalAimY, snapshot.Aim.Cardinal.y),
            }
        },
        { typeof(IMovableActor), snapshot => new[]
            {
                Float(ResolvedFacingX, snapshot.Facing.X),
                Float(ResolvedFacingY, snapshot.Facing.Y),
            }
        },
    };
}

