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
            _planeManager = GameObject.FindFirstObjectByType<ARPlaneManager>();
        }
        catch
        {
            Debug.LogError("*** Plane manager on GameManager is null");
        }
    }

    public static void ViewPlanes(bool isShowing)
    {
        foreach (var plane in _planeManager.trackables)
        {
            plane.GetComponent<MeshRenderer>().enabled = isShowing;
        }
        Debug.Log("*** View Planes Running");
    }


    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }


}
