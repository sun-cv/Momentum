

namespace Momentum
{

    public class RequestFlag : Flag
    {
        private bool requested = false;

        public void Set()
        {
            requested = true;
        }
    
        public bool Consume()
        {
            if (requested)
            {
                requested = false;
                return true;
            }
            return false;
        }
    
        public new bool Value   => requested;
        public bool IsRequested => requested;
    }
}
