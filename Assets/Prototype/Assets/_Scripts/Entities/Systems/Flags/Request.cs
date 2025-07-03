public class FlagRequest
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

    public bool IsRequested => requested;
}
