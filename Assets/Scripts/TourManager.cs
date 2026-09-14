using UnityEngine;
using UnityEngine.Video;

public class TourManager : MonoBehaviour
{
    [System.Serializable]
    public struct RoomNode
    {
        public string roomName;
        public GameObject sphereObject;
        public GameObject uiCanvas;
    }

    [Header("Room Nodes")]
    public RoomNode livingRoom;
    public RoomNode cantina;
    public RoomNode cube;
    public RoomNode mezzanine;

    [Header("All Floating Info Canvases (Auto-Dismissed on Room Change)")]
    public GameObject[] allInfoCanvases;

    private RoomNode[] allRooms;

    void Awake()
    {
        allRooms = new RoomNode[] { livingRoom, cantina, cube, mezzanine };
    }

    void Start()
    {
        ShowLivingRoom();
    }

    public void ShowLivingRoom()  => SwitchToRoom(livingRoom);
    public void ShowCantina()     => SwitchToRoom(cantina);
    public void ShowCube()        => SwitchToRoom(cube);
    public void ShowMezzanine()   => SwitchToRoom(mezzanine);

    private void SwitchToRoom(RoomNode targetRoom)
    {
        
        if (allRooms == null || allRooms.Length == 0)
        {
            allRooms = new RoomNode[] { livingRoom, cantina, cube, mezzanine };
        }

        
        if (allInfoCanvases != null)
        {
            foreach (var infoCanvas in allInfoCanvases)
            {
                if (infoCanvas != null)
                {
                    infoCanvas.SetActive(false);
                }
            }
        }

        
        foreach (var room in allRooms)
        {
            bool isTarget = (room.sphereObject == targetRoom.sphereObject);

            if (room.sphereObject != null)
            {
                var vp = room.sphereObject.GetComponent<VideoPlayer>();

                if (isTarget)
                {
                    room.sphereObject.SetActive(true);
                    if (vp != null)
                    {
                        vp.Play();
                    }
                }
                else
                {
                   
                    if (vp != null && vp.isPlaying)
                    {
                        vp.Pause();
                    }
                    room.sphereObject.SetActive(false);
                }
            }

            if (room.uiCanvas != null)
            {
                room.uiCanvas.SetActive(isTarget);
            }
        }
    }
}