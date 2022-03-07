using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Variables

    //Components
    GameObject initObject;
    InitGameScript initGameScript;
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
        initObject = GameObject.Find("InitObject");
        initGameScript = initObject.GetComponent<InitGameScript>();
        animator = gameObject.GetComponent<Animator>();
        chController = gameObject.GetComponent<ChController>();
        hud = gameObject.GetComponent<HUD>();
        mRadius = 5;
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currHealth <= 0 && alive)
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

    public void Death()
    {
        alive = false;
        animator.SetBool("Death", true);
        chController.CameraLockOff();
        Invoke("Transition", 4f);
        Invoke("Restart", 6f);
    }

    void Transition()
    {
        hud.FadeToBlack(false);
    }
    void Restart()
    {
        initGameScript.LoadScene(1);
    }
}
