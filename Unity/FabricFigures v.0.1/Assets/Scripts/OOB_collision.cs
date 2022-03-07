using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OOB_collision : MonoBehaviour
{
    GameObject player;
    PlayerManager pMan;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pMan = player.GetComponent<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            print("You have fallen!");
            pMan.currHealth = 0;
            pMan.Death();
        }
    }
}
