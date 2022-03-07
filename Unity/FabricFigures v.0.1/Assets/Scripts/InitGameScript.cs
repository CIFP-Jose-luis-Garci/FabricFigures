using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitGameScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Physics.IgnoreLayerCollision(8, 8, true);
        Physics.IgnoreLayerCollision(6, 9, true);
        Physics.IgnoreLayerCollision(7, 9, true);
        Physics.IgnoreLayerCollision(7, 7, true);
        Physics.IgnoreLayerCollision(6, 6, true);
        Physics.IgnoreLayerCollision(6, 9, true);
    }

    public void LoadScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }
}
