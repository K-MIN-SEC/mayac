using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(1);
        }

        if(Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SceneManager.LoadScene(1);
        }
    }
}
