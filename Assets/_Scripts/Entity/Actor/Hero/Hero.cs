using UnityEngine;

namespace Momentum
{

public class Hero : Entity, IHero
{

    [Header("Systems:")]
    [SerializeField] public CameraRig           cameraRig;
    [SerializeField] public Animator            animatorSystem;

    [Header("Components:")]
    [SerializeField] public Rigidbody2D         heroBody;
    [SerializeField] public CapsuleCollider2D   heroCollider;

    [Header("Configuration:")]
    [SerializeField] public HeroAttributes      attributes;

    public InputRouter          input               = new();
    public MovementEngine       movement            = new();
    public CommandSystem        command             = new();

    public HeroContext          context;

    public HeroStateMachine     stateMachine        = new();
    public AnimationController  animator            = new();

    public void Awake()
    {
        Registry.Register<IStateMachineController>(stateMachine);
        Registry.Register<IAnimationController>(animator);
        Registry.Register<ICommandDispatcher>(command.dispatcher);
    }

    public void Initialize()
    {
        context = new()
        {
            transform           = transform,
            body                = heroBody,
            collider            = heroCollider,
            attributes          = attributes,
        };


        cameraRig               .Initialize(context);

        input                   .Initialize(context);
        command                 .Initialize(context);
        movement                .Initialize(context);

        animator                .Initialize(this);
        stateMachine            .Initialize(this);

    }

    void OnGUI()
    {
        StateDebugDisplay.OnGUI();
    }

    void OnEnable()
    {
        input.OnEnable();
    }

    void OnDisable()
    {
        input.OnDisable();
    }

    public override void Tick()
    {
        input       .Tick();
        command     .Tick();
        stateMachine.Tick();
        movement    .Tick();
        animator    .Tick();
        cameraRig   .Tick();    
    }

    public void TickFixed()
    {
        stateMachine.TickFixed();
        movement    .TickFixed();
        animator    .TickFixed();
        cameraRig   .TickFixed();
    }

    public void TickLate()
    {
        cameraRig   .TickLate();
    }

    public HeroContext GetHeroContext()
    {
        return context;
    }
}
}
