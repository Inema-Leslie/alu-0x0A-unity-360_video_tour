using UnityEngine;
using UnityEngine.Video;

public class TourManager : MonoBehaviour
{
    [Header("Spheres")]
    public GameObject livingRoomSphere;
    public GameObject cantinaSphere;

    [Header("UI Canvases")]
    public GameObject livingRoomUI;
    public GameObject cantinaUI;

    private VideoPlayer livingRoomVP;
    private VideoPlayer cantinaVP;

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
        
        if (livingRoomSphere != null) livingRoomSphere.SetActive(true);
        if (livingRoomUI != null) livingRoomUI.SetActive(true);
        if (livingRoomVP != null) livingRoomVP.Play();

        
        if (cantinaSphere != null) cantinaSphere.SetActive(false);
        if (cantinaUI != null) cantinaUI.SetActive(false);
        if (cantinaVP != null) cantinaVP.Stop();
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
}