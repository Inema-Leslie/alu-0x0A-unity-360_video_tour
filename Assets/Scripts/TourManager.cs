using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;


public class TourManager : MonoBehaviour
{
  
    [System.Serializable]
    public class CampusRoom
    {
        
        [Tooltip("Exact name to reference from buttons (e.g. 'LivingRoom', 'Cantina', 'Cube', 'Mezzanine', 'Room1', 'Room2', 'Room3')")]
        public string roomID;

       
        public string displayName;

        
        [Header("Room Objects")]
        public GameObject roomSphere;

        
        public VideoPlayer videoPlayer;

        
        [Tooltip("Optional: 360 Hotspots root container/canvas for this room")]
        public GameObject roomHotspotsCanvas;

       
        [Tooltip("Optional filename in StreamingAssets (e.g. 'LivingRoom.mp4' or 'room1.mp4')")]
        public string videoFileName;

        
        [Tooltip("Optional static 360 picture texture (e.g. Picture1, Picture2, Picture3) for photo-based tour rooms")]
        public Texture roomPicture;
    }

   
    [System.Serializable]
    public class RoomNode
    {
        
        public string roomName;

       
        public GameObject sphereObject;

        
        public GameObject uiCanvas;

       
        public string videoFileName;

        public VideoPlayer videoPlayer;
    }

    
    [Header("Intranet Tour Serialized Rooms (Scene 2)")]
    public List<RoomNode> rooms = new List<RoomNode>();

    
    public int startingRoomIndex = 0;

   
    public string mainMenuSceneName = "MainMenuScene";

    
    [Header("Generic / Campus Rooms Setup")]
    [SerializeField] private List<CampusRoom> campusRooms = new List<CampusRoom>();
    [SerializeField] private string defaultStartingRoom = "LivingRoom";

   
    [Header("All Floating Info Canvases")]
    public GameObject[] allInfoCanvases;

    
    [Header("Camera & Visuals")]
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private bool resetLookAngleOnTransition = false;

   
    [Header("Transition Visual Polish")]
    [SerializeField] private bool useFadeTransition = false;
    [SerializeField] private float fadeDuration = 0.3f;

    
    private int currentRoomIndex = -1;
    
    private bool isTransitioning = false;

    
    void Awake()
    {
        isTransitioning = false;

        if (mainCameraTransform == null)
        {
            Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam != null) mainCameraTransform = cam.transform;
        }

        ConsolidateRooms();
    }

    
    void Start()
    {
        isTransitioning = false;

        if (campusRooms.Count == 0)
        {
            AutoDiscoverRooms();
        }

        if (campusRooms.Count == 0)
        {
            Debug.LogWarning("[TourManager] No rooms found or configured in scene!");
            return;
        }

        int startIndex = -1;

        if (startingRoomIndex >= 0 && startingRoomIndex < campusRooms.Count)
        {
            startIndex = startingRoomIndex;
        }

        if (startIndex < 0 && !string.IsNullOrEmpty(defaultStartingRoom))
        {
            startIndex = campusRooms.FindIndex(r => r.roomID.Replace(" ", "").Equals(defaultStartingRoom.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        }

        if (startIndex < 0) startIndex = 0;

        ExecuteRoomSwitch(startIndex);
    }

    
    private void ConsolidateRooms()
    {
        if (rooms != null && rooms.Count > 0)
        {
            foreach (var r in rooms)
            {
                if (r != null && r.sphereObject != null)
                {
                    string id = !string.IsNullOrEmpty(r.roomName) ? r.roomName.Replace(" ", "") : r.sphereObject.name.Replace(" ", "");
                    if (!campusRooms.Exists(c => c.roomID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    {
                        VideoPlayer vp = r.videoPlayer != null ? r.videoPlayer : r.sphereObject.GetComponent<VideoPlayer>();
                        campusRooms.Add(new CampusRoom
                        {
                            roomID = id,
                            displayName = !string.IsNullOrEmpty(r.roomName) ? r.roomName : id,
                            roomSphere = r.sphereObject,
                            roomHotspotsCanvas = r.uiCanvas,
                            videoPlayer = vp,
                            videoFileName = !string.IsNullOrEmpty(r.videoFileName) ? r.videoFileName : id + ".mp4"
                        });
                    }
                }
            }
        }
    }

    
    public void AutoDiscoverRooms()
    {
        var rootObjs = gameObject.scene.GetRootGameObjects();
        var allTransforms = new List<Transform>();
        foreach (var root in rootObjs)
        {
            allTransforms.AddRange(root.GetComponentsInChildren<Transform>(true));
        }

        string[] candidateNames = { "LivingRoom", "Cantina", "Cube", "Mezzanine", "Room1", "Room2", "Room3" };
        foreach (var cName in candidateNames)
        {
            string cleanTarget = cName.Replace(" ", "").Trim().ToLowerInvariant();

            if (campusRooms.Exists(r => r.roomID.Replace(" ", "").Trim().ToLowerInvariant() == cleanTarget))
                continue;

            Transform sphereT = allTransforms.Find(t => t.name.Replace(" ", "").Trim().ToLowerInvariant() == cleanTarget);
            if (sphereT != null)
            {
                var sphere = sphereT.gameObject;
                var vp = sphere.GetComponent<VideoPlayer>();

                Transform uiT = allTransforms.Find(t => t.name.Replace(" ", "").Trim().ToLowerInvariant() == cleanTarget + "ui");
                if (uiT == null)
                {
                    uiT = allTransforms.Find(t => t.name.Replace(" ", "").Trim().ToLowerInvariant() == "hotspot_to" + cleanTarget);
                }

                campusRooms.Add(new CampusRoom
                {
                    roomID = cName,
                    displayName = cName,
                    roomSphere = sphere,
                    videoPlayer = vp,
                    roomHotspotsCanvas = uiT != null ? uiT.gameObject : null,
                    videoFileName = vp != null ? cName + ".mp4" : ""
                });
            }
        }

        if (allInfoCanvases == null || allInfoCanvases.Length == 0)
        {
            var foundCanvases = new List<GameObject>();
            foreach (var t in allTransforms)
            {
                string n = t.name.Trim().ToLowerInvariant();
                if (n.EndsWith("infocanvas") || n.EndsWith("boothcanvas") || n.EndsWith("deskcanvas") || n.EndsWith("librarycanvas"))
                {
                    foundCanvases.Add(t.gameObject);
                }
            }
            if (foundCanvases.Count > 0)
            {
                allInfoCanvases = foundCanvases.ToArray();
            }
        }
    }

        public void ShowLivingRoom() => GoToRoomByName("LivingRoom");

   
    public void ShowCantina()    => GoToRoomByName("Cantina");

    
    public void ShowCube()       => GoToRoomByName("Cube");

    
    public void ShowMezzanine()  => GoToRoomByName("Mezzanine");

    
    public void ShowRoom1()      => GoToRoomByName("Room1");

   
    public void ShowRoom2()      => GoToRoomByName("Room2");

    
    public void ShowRoom3()      => GoToRoomByName("Room3");

       public void GoToRoomByName(string roomID)
    {
        if (campusRooms.Count == 0) ConsolidateRooms();
        if (campusRooms.Count == 0) AutoDiscoverRooms();

        string cleanID = roomID.Replace(" ", "").Trim().ToLowerInvariant();
        int index = campusRooms.FindIndex(r => r.roomID.Replace(" ", "").Trim().ToLowerInvariant() == cleanID);

        if (index == -1)
        {
            index = campusRooms.FindIndex(r => r.displayName.Replace(" ", "").Trim().ToLowerInvariant() == cleanID);
        }

        if (index == -1)
        {
            AutoDiscoverRooms();
            index = campusRooms.FindIndex(r => r.roomID.Replace(" ", "").Trim().ToLowerInvariant() == cleanID);
        }

        if (index == -1)
        {
            Debug.LogError($"[TourManager] Room with ID '{roomID}' was not found in the list! Total rooms registered: {campusRooms.Count}");
            return;
        }

        SwitchToRoom(index);
    }

    
    public void SwitchToRoom(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= campusRooms.Count)
        {
            Debug.LogError($"[TourManager] Invalid room index: {targetIndex}");
            return;
        }

        if (!Application.isPlaying)
        {
            isTransitioning = false;
        }

        if (targetIndex == currentRoomIndex || isTransitioning) return;

        if (useFadeTransition && Application.isPlaying && VRScreenFader.Instance != null)
        {
            isTransitioning = true;
            VRScreenFader.Instance.FadeTransition(() =>
            {
                ExecuteRoomSwitch(targetIndex);
                isTransitioning = false;
            }, fadeDuration, () =>
            {
                isTransitioning = false;
            });
        }
        else
        {
            ExecuteRoomSwitch(targetIndex);
            isTransitioning = false;
        }
    }

    
    private void ExecuteRoomSwitch(int targetIndex)
    {
        currentRoomIndex = targetIndex;
        var activeRoom = campusRooms[currentRoomIndex];

        for (int i = 0; i < campusRooms.Count; i++)
        {
            var room = campusRooms[i];
            bool isCurrent = (i == currentRoomIndex);

            if (room.roomSphere != null)
                room.roomSphere.SetActive(isCurrent);

            if (room.roomHotspotsCanvas != null)
                room.roomHotspotsCanvas.SetActive(isCurrent);

            if (room.videoPlayer != null)
            {
                if (!isCurrent && room.videoPlayer.isPlaying)
                {
                    room.videoPlayer.Pause();
                }
            }
        }

        if (allInfoCanvases != null)
        {
            foreach (var infoCanvas in allInfoCanvases)
            {
                if (infoCanvas == null) continue;
                bool belongs = CanvasBelongsToRoom(infoCanvas.name, activeRoom.roomID);
                infoCanvas.SetActive(belongs);
                if (belongs)
                {
                    var toggle = infoCanvas.GetComponent<UIToggle>();
                    if (toggle != null) toggle.HidePanel();
                }
            }
        }

        if (activeRoom.roomPicture != null && activeRoom.roomSphere != null)
        {
            var mr = activeRoom.roomSphere.GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null)
            {
                mr.sharedMaterial.mainTexture = activeRoom.roomPicture;
                if (mr.sharedMaterial.HasProperty("_BaseMap"))
                {
                    mr.sharedMaterial.SetTexture("_BaseMap", activeRoom.roomPicture);
                }
            }
        }

        if (activeRoom.videoPlayer != null)
        {
            SetupAndPlayVideo(activeRoom);
        }

        if (resetLookAngleOnTransition && mainCameraTransform != null)
        {
            mainCameraTransform.rotation = Quaternion.identity;
        }

        Debug.Log($"[TourManager] Switched to room: {activeRoom.displayName} ({activeRoom.roomID})");
    }

   
    private bool CanvasBelongsToRoom(string canvasName, string roomID)
    {
        string cName = canvasName.Trim().ToLowerInvariant().Replace(" ", "");
        string rID = roomID.Trim().ToLowerInvariant().Replace(" ", "");
        if (rID == "livingroom")
        {
            return cName.Contains("livingroom") || cName.Contains("library");
        }
        return cName.Contains(rID);
    }

    
    private void SetupAndPlayVideo(CampusRoom room)
    {
        VideoPlayer vp = room.videoPlayer;
        if (vp == null) return;

        if (vp.source == VideoSource.VideoClip && vp.clip != null)
        {
            vp.isLooping = true;
            vp.Play();
            return;
        }

        string cleanID = room.roomID.Trim().ToLowerInvariant().Replace(" ", "");
        string targetFile = "";

        if (cleanID == "room1")
        {
            targetFile = "Clip1.mp4";
        }
        else if (cleanID == "room2")
        {
            targetFile = "Clip2.mp4";
        }
        else if (cleanID == "room3")
        {
            targetFile = "Clip3.mp4";
        }
        else if (!string.IsNullOrEmpty(room.videoFileName))
        {
            targetFile = room.videoFileName;
        }
        else if (!string.IsNullOrEmpty(vp.url))
        {
            targetFile = Path.GetFileName(vp.url);
        }
        else
        {
            targetFile = room.roomID + ".mp4";
        }

        string cleanFile = targetFile.Replace("file://", "").TrimStart('/', '\\');
        cleanFile = Path.GetFileName(cleanFile);

        string fullPath;
        if (Application.platform == RuntimePlatform.Android)
        {
            fullPath = Application.streamingAssetsPath + "/" + cleanFile;
        }
        else
        {
            fullPath = Path.Combine(Application.streamingAssetsPath, cleanFile);
            if (!File.Exists(fullPath))
            {
                if (cleanID == "room1") fullPath = Path.Combine(Application.streamingAssetsPath, "room1.mp4");
                else if (cleanID == "room2") fullPath = Path.Combine(Application.streamingAssetsPath, "room2.mp4");
                else if (cleanID == "room3") fullPath = Path.Combine(Application.streamingAssetsPath, "room3.mp4");
            }
        }

        vp.source = VideoSource.Url;
        vp.url = fullPath;
        vp.isLooping = true;
        vp.Play();
    }

        public string GetCurrentRoomName()
    {
        if (currentRoomIndex >= 0 && currentRoomIndex < campusRooms.Count)
            return campusRooms[currentRoomIndex].displayName;
        return "";
    }

   
    public List<CampusRoom> GetRooms() => campusRooms;
}