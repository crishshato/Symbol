using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode reloadKey = KeyCode.R;   // press R to restart

    void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            Reload();
        }
    }

    public void Reload()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
