public class AltPathPortal
{
    public AltPathResult destination;
    public bool isLocked;

    public AltPathPortal()
    {
    }

    public void UpdateInfo(AltPathResult inDestination, bool inLocked)
    {
        destination = inDestination;
        isLocked = inLocked;
    }
}
