public class RequestFlag
{
    private bool requested = false;

    public void Request()
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
