using UnityEngine;


namespace Momentum
{


    public interface IMovementEngine : ITickFixed
    {
        public void Initialize();

        public void EnterChannel(MovementChannel selection, MovementChannelParams param);
        public void RequestExit();
        public void RequestCancel();

        public void BindConfig(MovementEngineConfig config);
        public void BindIntent(MovementIntent intent);
        public void BindContext(Context context);
        public void BindBody(Rigidbody2D body);
    }


    public class MovementChannelParams {}

}

