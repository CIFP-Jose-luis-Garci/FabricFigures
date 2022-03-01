using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //VARIABLES
    GameObject player;
    ChController chController;

    [SerializeField] float _Dmg;
    [SerializeField] float _pDmg;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        chController = player.GetComponent<ChController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            //Get enemy script
            EnemyManager eMan;
            eMan = other.gameObject.GetComponent<EnemyManager>();

            //Deal damage
            eMan.currHP -= _Dmg;
            eMan.currPoise -= _pDmg;
            Debug.Log(other.gameObject.name + " health: " + eMan.currHP);

            //HP check
            if (eMan.currHP <= 0)
                eMan.Death();
        }
    }
}
