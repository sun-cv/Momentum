

namespace Momentum
{
    

    public enum ConflictAction
    {
        None,
        Queue,
        Ignore,
        Cancel,
        Interrupt,
        Replace,
        Restart,
    }

}


// Queue:          Wait until the current one finishes.
// Ignore:         Leave the existing instance as-is. Do not process the new one.
// Cancel:         Remove the existing one and accept the new one.
// Interrupt:      Forcefully stop the existing one and start the new one.
// Replace:        Swap the existing one with the new one.
// Restart:        Restart the existing one from scratch.

