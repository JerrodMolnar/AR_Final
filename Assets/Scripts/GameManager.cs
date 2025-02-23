using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    private static ARPlaneManager _planeManager;

    private void Start()
    {
        try
        {
            _planeManager = FindFirstObjectByType<ARPlaneManager>();
        }
        catch
        {
            Debug.LogError("*** Plane manager on GameManager is null");
        }
    }

    public static void ViewPlanes(bool isShowing)
    {
        _planeManager.SetTrackablesActive(isShowing);      
        Debug.Log("*** View Planes Running");
    }


    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }


}
