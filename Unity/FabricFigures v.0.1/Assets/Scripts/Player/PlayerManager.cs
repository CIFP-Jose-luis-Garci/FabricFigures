using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables

    //Components
    Animator animator;
    ChController chController;
    HUD hud;
    InputActions inputActions;

    public int charCurrency = 0;
    public float mRadius = 5;
    public float _dmg;

    public bool alive = true;
    public int maxHealth = 5;
    public int currHealth;
    public bool inv;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        chController = gameObject.GetComponent<ChController>();
        hud = gameObject.GetComponent<HUD>();
        mRadius = 5;
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth <= 0)
            Death();
    }

    public void Hit(int damage)
    {
        if (!inv && currHealth > 0)
        {
            animator.SetTrigger("Staggered");
            chController.isStaggered = true;
            currHealth -= damage;
            print(currHealth);
            inv = true;
            Invoke("RevokeInv", 1f);
            hud.HealthBarUpdate();
           
        }

    }

    void RevokeInv()
    {
        inv = false;
    }

    void Death()
    {
        alive = false;
        animator.SetTrigger("Death");
        inputActions.Disable();
        hud.FadeToBlack(true);
    }
}
