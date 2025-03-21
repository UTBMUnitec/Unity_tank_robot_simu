using UnityEngine;


public class PauseButton : MonoBehaviour
{

    bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
        }

    }

}