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
        if (targetPanel != null)
        {
            
            bool currentState = targetPanel.activeSelf;
            targetPanel.SetActive(!currentState);
        }
    }
}