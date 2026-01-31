using UnityEngine;
using UnityEngine.SceneManagement;

public class PressAnyKeyToLoadScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "GameScene"; 

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
