using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables

    //Components
    Animator animator;
    ChController chController;
    InputActions inputActions;

    public int charCurrency = 0;
    public float mRadius = 5;
    public float _dmg;

    public int maxHealth = 5;
    public int currHealth;
    public bool inv;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        chController = gameObject.GetComponent<ChController>();
        mRadius = 5;
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit(int damage)
    {
        if (!inv)
        {
            animator.SetTrigger("Stagger");
            chController.isStaggered = true;
            currHealth -= damage;
            print(currHealth);
            inv = true;
            Invoke("RevokeInv", 1f);
            if (currHealth <= 0)
                Death();
        }

    }

    void RevokeInv()
    {
        inv = false;
    }

    void Death()
    {
        animator.SetTrigger("Death");
        inputActions.Disable();
    }
}
