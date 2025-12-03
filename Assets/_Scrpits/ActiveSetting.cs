using UnityEngine;

public class ActiveSetting : MonoBehaviour
{
    public GameObject panel;        
    public GameObject closeArea;    

    private bool isOpen = false;

    public void TogglePanel()
    {
        if (isOpen)
            ClosePanel();
        else
            OpenPanel();
    }

    public void OpenPanel()
    {
        isOpen = true;
        panel.SetActive(true);
        closeArea.SetActive(true);
    }

    public void ClosePanel()
    {
        isOpen = false;
        panel.SetActive(false);
        closeArea.SetActive(false);
    }
}
