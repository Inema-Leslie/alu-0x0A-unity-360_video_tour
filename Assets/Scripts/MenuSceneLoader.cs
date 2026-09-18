using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuSceneLoader : MonoBehaviour
{
    
    public string sceneName;

    
    public void LoadTargetScene()
    {
        LoadSceneByName(sceneName);
    }

    
    public void LoadSceneByName(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[MenuSceneLoader] Target scene name is empty!");
            return;
        }

        if (VRScreenFader.Instance != null)
        {
            VRScreenFader.Instance.FadeOutAndLoad(targetScene, 0.4f);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}