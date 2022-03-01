using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyMagnet : MonoBehaviour
{
    //VARIABLES

    //Components
    GameObject player;
    PlayerManager pMan;
    Rigidbody rb;

    //Magnet activation
    private float distToPlayer;
    float mRadius;
    float mRadInc;
    bool isMagnet = false;
    float mStrenght = 1f;


    //START-UPDATE
    void Start()
    {
        //Find components
        player = GameObject.FindGameObjectWithTag("Player");
        pMan = player.GetComponent<PlayerManager>();
        mRadius = pMan.mRadius;
        mRadInc = mRadius * 2;
        rb = gameObject.GetComponent<Rigidbody>();
        RandomInst();
    }

    void Update()
    {
        distToPlayer = Vector3.Distance(player.transform.position, transform.position);
        Invoke("MagnetActive", 2f);
        //MagnetActive();
    }

    //METHODS
    

    void RandomInst()
    {
        float rStrenght = Random.Range(1f, 3f);
        float rX = Random.Range(-1f, 1f);
        float rZ = Random.Range(-1f, 1f);
        float rY = Random.Range(1f, 1.5f);
        rb.AddForce(new Vector3(rX, rY, rZ) * rStrenght, ForceMode.Impulse);
    }

    void MagnetActive()
    {
        //Is in range
        if (distToPlayer < mRadius)
        {
            if (!isMagnet)
            {
                StartCoroutine(Magnet());
            }

            mRadius = mRadInc;
            rb.useGravity = false;
        }
        else
        {
            isMagnet = false;
            StopCoroutine(Magnet());
            mRadius = pMan.mRadius;
            rb.useGravity = true;
        }
    }

    IEnumerator Magnet()
    {
        while (true)
        {
            isMagnet = true;
            mStrenght *= 1.01f;
            Vector3 moveDir = ((player.transform.position - transform.position).normalized) * mStrenght;
            rb.AddForce(moveDir);
            yield return null;
        }
    }
}
