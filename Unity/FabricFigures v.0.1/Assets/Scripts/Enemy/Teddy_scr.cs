using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teddy_scr : MonoBehaviour
{
    //VARIABLES

    //Components
    private Animator animator;
    private NavMeshAgent agent;
    [SerializeField] LayerMask layerMask;

    //Objects
    [SerializeField] GameObject player;
    [SerializeField] Transform enemy;
    [SerializeField] Transform[] pathPoints;
    [SerializeField] Transform aimTarget;

    //Position
    private Vector3 enemyPos;
    private Vector3 startPos;
    private Vector3 newPos;

    //Idle route
    [SerializeField] Collider[] attackColliders;
    private int destPoint = 0;
    [SerializeField] float movementArea;

    //Aggro and detection
    bool aggro = false;
    bool isIdle = true;
    [SerializeField] float visionRange = 10f;
    [SerializeField] float visionAngle = 60f;
    private Vector3 target;
    float targetDistance;

    //Raycast
    Vector3 rayCastPos;
    RaycastHit hit;

    //METHODS
    #region START - UPDATE
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

        //Other
        agent.autoBraking = false;
        GotoNextPoint();
    }

    void Update()
    {
        //Debug.Log(destPoint);

        //Raycast
        enemyPos = enemy.position;
        rayCastPos = aimTarget.position;

        //Vision();

        //Animations & aggro state
        if (target != null)
        {
            targetDistance = Vector3.Distance(transform.position, target);
            if (targetDistance > 0.3f)
                FaceTarget();

            if (targetDistance > 0.1f && !aggro)
            {
                //animator.SetBool("IsWalking", true);
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

        if (!agent.pathPending && agent.remainingDistance < 0.5f && pathPoints[destPoint].gameObject.tag != "WaitPathPoint")
            GotoNextPoint();
    }
    #endregion

    #region Attacks
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
    #endregion

    #region Patrol & vision
    IEnumerator Idle()
    {
        isIdle = true;
        //agent.speed = 0f;
        target = enemyPos;

        while (!aggro)
        {
            //New destination
            agent.speed = 2f;
            yield return null;
        }
    }
    void FaceTarget()
    {
        Vector3 direction = (agent.destination - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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
    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (pathPoints.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = pathPoints[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % pathPoints.Length;
    }
    void StartIdle()
    {
        aggro = false;
        StartCoroutine("Idle");
    }
    #endregion

}
