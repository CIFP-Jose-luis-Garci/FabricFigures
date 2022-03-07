using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCollDetection : MonoBehaviour
{
    //VARIABLES

    //Components
    GameObject player;
    PlayerManager playerManager;
    [SerializeField] int damage;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerManager = player.GetComponent<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerManager.Hit(damage);
        }
    }
}
