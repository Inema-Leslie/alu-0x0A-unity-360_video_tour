using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class TourManager : MonoBehaviour
{
    [Header("Spheres")]
    public GameObject livingRoomSphere;
    public GameObject cantinaSphere;

    private VideoPlayer livingRoomVP;
    private VideoPlayer cantinaVP;

    [Header("UI Canvases")]
    public GameObject livingRoomUI;
    public GameObject cantinaUI;

    void Awake()
    {
        if (livingRoomSphere) livingRoomVP = livingRoomSphere.GetComponent<VideoPlayer>();
        if (cantinaSphere) cantinaVP = cantinaSphere.GetComponent<VideoPlayer>();
    }

    void Start()
    {
        
        ShowLivingRoom();
    }

    public void ShowLivingRoom()
    {
        SwitchEnvironment(livingRoomSphere, livingRoomVP, cantinaSphere, cantinaVP);
    }

    public void ShowCantina()
{
    
    if (livingRoomSphere != null) livingRoomSphere.SetActive(false);
    if (livingRoomUI != null) livingRoomUI.SetActive(false);
    if (livingRoomVP != null) livingRoomVP.Stop();

    
    if (cantinaSphere != null) cantinaSphere.SetActive(true);
    if (cantinaUI != null) cantinaUI.SetActive(true);
    if (cantinaVP != null) cantinaVP.Play();
}

    private void SwitchEnvironment(GameObject activateObj, VideoPlayer activateVP, GameObject deactivateObj, VideoPlayer deactivateVP)
    {
        if (deactivateObj)
        {
            if (deactivateVP) deactivateVP.Stop();
            deactivateObj.SetActive(false);
        }

        if (activateObj)
        {
            activateObj.SetActive(true);
            if (activateVP) activateVP.Play();
        }
    }
}