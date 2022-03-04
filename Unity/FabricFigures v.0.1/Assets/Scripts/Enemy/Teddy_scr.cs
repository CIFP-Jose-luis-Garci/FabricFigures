using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teddy_scr : MonoBehaviour
{
    //VARIABLES

    //Components
    Animator animator;
    NavMeshAgent agent;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Collider[] attackColliders;

    //Objects
    [SerializeField] GameObject player;
    [SerializeField] Transform enemy;

    //Position
    Vector3 enemyPos;
    Vector3 startPos;
    Vector3 newPos;
    [SerializeField] float movementArea;

    //Aggro and detection
    bool aggro = false;
    bool isIdle = true;
    [SerializeField] float visionRange = 10f;
    [SerializeField] float visionAngle = 60f;
    Vector3 target;
    float targetDistance;

    //Raycast
    Vector3 rayCastPos;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        //Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = gameObject.transform;
        //Dissable colliders
        DisableAtCol();

        //Idle round
        StartCoroutine("Idle");

        //Positions
        enemyPos = enemy.position;
        target = enemyPos;
        startPos = transform.position;
    }

    void Update()
    {
        //Debug.Log(aggro);

        //Raycast
        enemyPos = enemy.position;
        rayCastPos = new Vector3(enemyPos.x, enemyPos.y + 1.4f, enemyPos.z);

        Vision();

        //Animations & aggro state
        if (target != null)
        {

            targetDistance = Vector3.Distance(transform.position, target);
            if (targetDistance > 0.3f)
                FaceTarget();

            if (targetDistance > 0.1f && !aggro)
            {
                animator.SetBool("IsWalking", true);
            }
            else if (targetDistance > agent.stoppingDistance && aggro)
            {
                DisableAtCol();
                //animator.SetBool("InRange", false);
            }
            else
            {
                if (aggro)
                {
                    //animator.SetBool("InRange", true);
                    EnableAtCol();
                }
                else
                {
                    DisableAtCol();
                    //animator.SetBool("InRange", false);
                    animator.SetBool("IsWalking", false);
                }
            }            
        }

        //Aggro follow
        if (aggro)
        {
            if (targetDistance > 0.7f)
            {
                agent.speed = 3f;
            }
            else
            {
                agent.speed = 0f;
            }
            target = player.transform.position;
            agent.SetDestination(target);
        }

        float distanceFromSpawn = Vector3.Distance(transform.position, startPos);
        if (distanceFromSpawn > 30)
        {
            aggro = false;
            StartCoroutine("Idle");
        }
    }

    //METHODS

    void FaceTarget()
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    IEnumerator Idle()
    {
        isIdle = true;
        agent.speed = 0f;
        target = enemyPos;
        //animator.SetBool("Walk", false);
        //animator.SetBool("Run", false);
        //animator.SetBool("InRange", false);

        while (!aggro)
        {
            //New destination
            agent.speed = 1f;

            RandomPos();
            target = newPos;
            agent.SetDestination(target);

            //Waiting time
            float r = Random.Range(0f, 5f);
            float t = Vector3.Distance(enemyPos, target) / agent.speed;
            yield return new WaitForSeconds(5f + t + r);
        }
    }

    //Enable/disable attack colliders
    void EnableAtCol()
    {
        foreach (Collider atCol in attackColliders)
        {
            atCol.enabled = true;
        }
    }
    void DisableAtCol()
    {
        foreach (Collider atCol in attackColliders)
        {
            atCol.enabled = false;
        }
    }
    void Vision()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 vectorToPlayer = playerPosition - transform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        float angleToPlayer = Vector3.Angle(transform.forward, vectorToPlayer);
        Ray ray = new Ray(rayCastPos, vectorToPlayer);
        Physics.Raycast(ray, out hit);
        
        if (distanceToPlayer <= visionRange && angleToPlayer <= visionAngle)
        {
            if (distanceToPlayer < 4)
            {
                visionAngle = 360;
            }
            else
            {
                visionAngle = 60;
            }

            //Raycast vision check
            if (hit.collider.tag == "Player" && hit.collider.tag != null && distanceToPlayer > 4)
            {
                Debug.DrawRay(rayCastPos, vectorToPlayer, Color.red);
                if (isIdle)
                {
                    StopCoroutine("Idle");
                    isIdle = false;
                    aggro = true;
                }
            }
            else if (distanceToPlayer <= 4)
            {
                Debug.DrawRay(rayCastPos, vectorToPlayer, Color.red);
                if (isIdle)
                {
                    StopCoroutine("Idle");
                    isIdle = false;
                    aggro = true;
                }
            }
            else
            {
                Debug.DrawRay(rayCastPos, vectorToPlayer * 1000, Color.white);
            }
        }
        else
        {
            if (!isIdle)
            {
                Invoke("StartIdle", 4f);
            }
        }
    }

    void StartIdle()
    {
        aggro = false;
        StartCoroutine("Idle");
    }

    void RandomPos()
    {
        float newX = startPos.x + Random.Range(-movementArea, movementArea);
        float newZ = startPos.z + Random.Range(-movementArea, movementArea);

        newPos = new Vector3(newX, enemyPos.y, newZ);
    }
}
