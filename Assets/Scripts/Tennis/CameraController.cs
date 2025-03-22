using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [SerializeField] private List<Camera> cameras = new List<Camera>();
    [SerializeField] private KeyCode previousCameraKey = KeyCode.A;
    [SerializeField] private KeyCode nextCameraKey = KeyCode.E;
    [SerializeField] private KeyCode reloadSceneKey = KeyCode.R;
    
    private int currentCameraIndex = 0;
    
    private void Start()
    {
        if (cameras.Count > 0)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i] != null)
                {
                    cameras[i].gameObject.SetActive(i == currentCameraIndex);
                }
            }
        }
        else
        {
            Debug.LogWarning("No cameras assigned to CameraController!");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(previousCameraKey))
        {
            SwitchToPreviousCamera();
        }
        else if (Input.GetKeyDown(nextCameraKey))
        {
            SwitchToNextCamera();
        }
        
        if (Input.GetKeyDown(reloadSceneKey))
        {
            ReloadCurrentScene();
        }
    }
    
    private void SwitchToPreviousCamera()
    {
        if (cameras.Count == 0) return;
        
        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex--;
        
        if (currentCameraIndex < 0)
        {
            currentCameraIndex = cameras.Count - 1;
        }
        
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }
    
    private void SwitchToNextCamera()
    {
        if (cameras.Count == 0) return;
        
        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }
    
    private void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
