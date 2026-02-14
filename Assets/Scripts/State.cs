




public abstract class State : Service
{
    public Actor Owner { get; }

    public State(Actor owner)
    {
        Owner = owner;
    }
}
