using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropsGen : MonoBehaviour
{
    int currDrop;
    [SerializeField] Transform lTrans;
    EnemyManager eMan;
    

    //Loot
    [SerializeField] GameObject[] Loot;
    
    //Currency
    [SerializeField] GameObject cB; //Big
    [SerializeField] GameObject cM; //Medium
    [SerializeField] GameObject cS; //Small

    void Start()
    {
        eMan = gameObject.GetComponent<EnemyManager>();
        currDrop = eMan.currDrop;
    }

    public void Drops()
    {
        CurrDrop();
        LootDrop();
    }

    public void CurrDrop()
    {
        //Big coins
        int n1 = currDrop / 50;
        int r1 = Random.Range(0, n1 + 1);
        int currDrop2 = currDrop - (50 * r1);

        //Medium-Small coins
        int n2 = currDrop2 / 5;
        int r2 = Random.Range(0, n2 + 1);
        int n3 = currDrop2 - (5 * r2);

        print(r1 +", " + r2 + ", " + n3);


        //Instances
        if(r1 != 0)
            for (int n = 0; n < r1; n++)
            {
                Instantiate(cB, lTrans.position, lTrans.rotation);
            }

        if(r2 != 0)
            for (int n = 0; n < r2; n++)
            {
                Instantiate(cM, lTrans.position, lTrans.rotation);
            }
        
        for (int n = 0; n < n3; n++)
        {
            Instantiate(cS, lTrans.position, lTrans.rotation);
        }
    }

    void LootDrop()
    {
    }
}
