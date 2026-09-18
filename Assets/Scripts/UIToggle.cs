using UnityEngine;


public class UIToggle : MonoBehaviour
{
    
    [SerializeField] private GameObject targetPanel;

   
    void Start()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

   
    public void TogglePanel()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (targetPanel != null)
        {
            bool currentState = targetPanel.activeSelf;
            targetPanel.SetActive(!currentState);
        }
    }

   
    public void ShowPanel()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    
    public void HidePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }
}