using System;
using UnityEngine;

namespace Momentum
{

    //     // REWORK REQUIRED GLOBAL DEFAULTS?
    //     public static class GameDefault
    //     {
    //         public static float LastReleasedBufferForExecution = 0.2f;
    //     }


    // public class AttackPressedThisFramePredicate        : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.attack.Condition    == InputButtonCondition.PressedThisFrame;   }
    // public class AttackPressedPredicate                 : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.attack.Condition    == InputButtonCondition.Pressed;            }
    // public class AttackHeldPredicate                    : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.attack.Condition    == InputButtonCondition.Held;               }
    // public class AttackReleasedThisFramePredicate       : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.attack.Condition    == InputButtonCondition.ReleasedThisFrame;  }
    // public class AttackReleasedPredicate                : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.attack.lastReleased - Time.time <= GameDefault.LastReleasedBufferForExecution; }
    // public class AttackFrameOrPressPredicate            : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<AttackPressedThisFramePredicate>(), Predicate.Get<AttackPressedPredicate>())).Evaluate();}
    // public class AttackFrameOrReleasePredicate          : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<AttackReleasedPredicate>(), Predicate.Get<AttackReleasedPredicate>())).Evaluate();}


    // public class BlockPressedThisFramePredicate         : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.block.Condition     == InputButtonCondition.PressedThisFrame;   }
    // public class BlockPressedPredicate                  : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.block.Condition     == InputButtonCondition.Pressed;            }
    // public class BlockHeldPredicate                     : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.block.Condition     == InputButtonCondition.Held;               }
    // public class BlockReleasedThisFramePredicate        : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.block.Condition     == InputButtonCondition.ReleasedThisFrame;  }
    // public class BlockReleasedPredicate                 : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.block.lastReleased - Time.time <= GameDefault.LastReleasedBufferForExecution; }
    // public class BlockFrameOrPressPredicate             : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<BlockPressedThisFramePredicate>(), Predicate.Get<BlockPressedPredicate>())).Evaluate();}
    // public class BlockFrameOrReleasePredicate           : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<BlockReleasedPredicate>(), Predicate.Get<BlockReleasedPredicate>())).Evaluate();}

    // public class DashPressedThisFramePredicate          : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.dash.Condition      == InputButtonCondition.PressedThisFrame;   }
    // public class DashPressedPredicate                   : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.dash.Condition      == InputButtonCondition.Pressed;            }
    // public class DashHeldPredicate                      : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.dash.Condition      == InputButtonCondition.Held;               }
    // public class DashReleasedThisFramePredicate         : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.dash.Condition      == InputButtonCondition.ReleasedThisFrame;  }
    // public class DashReleasedPredicate                  : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().input.dash.lastReleased - Time.time <= GameDefault.LastReleasedBufferForExecution; }
    // public class DashFrameOrPressPredicate              : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<DashPressedThisFramePredicate>(), Predicate.Get<DashPressedPredicate>())).Evaluate();}
    // public class DashFrameOrReleasePredicate            : CachedPredicate   { protected override bool EvaluateInternal() => new And( new Or(Predicate.Get<DashReleasedPredicate>(), Predicate.Get<DashReleasedPredicate>())).Evaluate();}

    // public class IdleIntentPredicate                    : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().movement.idle;           }
    // public class MovementIntentPredicate                : CachedPredicate   { protected override bool EvaluateInternal() => Access<Context>().movement.locomotion;     }

    // public class DisabledPredicate                      : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().condition.disabled;      }
    // public class StunnedPredicate                       : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().condition.stunned;       }
    // public class KnockedBackPredicate                   : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().condition.knockedBack;   }
    // public class SlowedPredicate                        : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().condition.slowed;        }

    // public class AttackCommandPredicate                 : BasePredicate     { protected override bool EvaluateInternal() => Access<ICommandSystem>().ActiveRequest()?.Mechanic is Attack;                                  }
    // public class AttackOnCooldownPredicate              : BasePredicate     { protected override bool EvaluateInternal() => Access<ICooldownSystem>().IsActive<AttackCooldown>();                                   }
    // public class AttackComboOnCooldownPredicate         : BasePredicate     { protected override bool EvaluateInternal() => Access<ICooldownSystem>().IsActive<AttackComboCooldown>();                              }
    // public class AttackAvailablePredicate               : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().attack.count < Config.Resolve<AttackDefinition>().attacks.Length;   }

    // public class DashCommandPredicate                   : BasePredicate     { protected override bool EvaluateInternal() => Access<ICommandSystem>().ActiveRequest()?.Mechanic is Dash;  }
    // public class DashOnCooldownPredicate                : BasePredicate     { protected override bool EvaluateInternal() => Access<ICooldownSystem>().IsActive<DashCooldown>();                                   }
    // public class DashComboOnCooldownPredicate           : BasePredicate     { protected override bool EvaluateInternal() => Access<ICooldownSystem>().IsActive<DashComboCooldown>();                              }
    // public class DashAvailablePredicate                 : BasePredicate     { protected override bool EvaluateInternal() => Access<Context>().dash.count < Config.Resolve<DashDefinition>().dashes.Length;   }

    // public class ShieldAvailablePredicate               : BasePredicate     { protected override bool EvaluateInternal() => false; };

    // public class BlockCommandPredicate                  : BasePredicate     { protected override bool EvaluateInternal() => Access<ICommandSystem>().ActiveRequest()?.Mechanic is Block;        }
    // public class BlockOnCooldownPredicate               : BasePredicate     { protected override bool EvaluateInternal() => Access<ICooldownSystem>().IsActive<BlockCooldown>();}
    // public class BlockAvailablePredicate                : BasePredicate     { protected override bool EvaluateInternal() => true; }



}
