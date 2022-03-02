using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    void Awake()
    {
        Material mat = gameObject.GetComponent<Material>();
        mat.renderQueue = 4000;
    }
}
