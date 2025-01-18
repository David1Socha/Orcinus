using Orcinus.Scripts.Core;

public class InGameUI : SafeAreaControl
{
    public void OnPause()
    {
        Visible = false;
    }

    public void OnUnpause()
    {
        Visible = true;
    }

    public void OnGameOver()
    {
        Visible = false;
    }
}
