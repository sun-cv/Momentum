using UnityEngine;
using character.controller;


namespace character
{


public class Movement : MonoBehaviour
{   
    public Context          Context         { get; private set; }

    public IdleControl      Idle            { get; private set; }
    public DashControl      Dash            { get; private set; }
    public SprintControl    Sprint          { get; private set; }

    public FacingControl    Facing          { get; private set; }

    public void Initialize(Context _context)
    {
        Context         = _context;

        Idle            = new IdleControl   (Context);
        Dash            = new DashControl   (Context);
        Sprint          = new SprintControl (Context);

        Facing          = new FacingControl (Context);
    }

    public void Tick()
    {
        Dash    .Tick();
        Sprint  .Tick();
        Idle    .Tick();
        Facing  .Tick();
    }

    public void TickFixed()
    {
        Dash    .TickFixed();
        Sprint  .TickFixed();
        Idle    .TickFixed();
    }


}

}