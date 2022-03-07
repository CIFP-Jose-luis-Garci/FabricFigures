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
    GameObject player;
    [SerializeField] Transform[] pathPoints;
    [SerializeField] Transform aimTarget;

    //Position
    private Vector3 enemyPos;
    private Vector3 startPos;
    private Vector3 newPos;
    private float baseSpeed = 2f;

    //Idle route
    [SerializeField] Collider[] attackColliders;
    private int destPoint = 0;
    //[SerializeField] float movementArea;

    //Aggro and detection
    bool aggro = false;
    [SerializeField] float visionRange = 10f;
    [SerializeField] float visionAngle = 60f;
    private Vector3 target;
    float targetDistance;

    //Raycast
    Vector3 rayCastPos;
    RaycastHit hit;

    //Combat
    bool isAttacking;
    [SerializeField] float combatRadius;
    bool isInCombat = false;
    private float playerDistance;

    //METHODS
    #region START - UPDATE
    void Start()
    {
        //Get components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        //Dissable colliders
        DisableAtCol();

        //Positions
        target = enemyPos;
        startPos = transform.position;

        //Other
        agent.speed = baseSpeed;
        agent.autoBraking = false;
        GotoNextPoint();
    }

    void Update()
    {
        //Raycast
        enemyPos = transform.position;
        rayCastPos = aimTarget.position;

        //Target distance
        if (target != null)
        {
            Vector3 vectorToTarget = agent.destination - transform.position;
            vectorToTarget.y = 0f;
            targetDistance = vectorToTarget.magnitude;
        }
        //Player distance
        if(player != null)
        {
            Vector3 vectorToTarget = player.transform.position - transform.position;
            vectorToTarget.y = 0f;
            playerDistance = vectorToTarget.magnitude;
        }


        //Animations
        if (agent.speed < 1f)
            animator.SetBool("IsWalking", false);
        else
            animator.SetBool("IsWalking", true);

        animator.SetBool("IsAggro", aggro);

        //Aggro state
        if (aggro)
        {
            target = player.transform.position;

            if (targetDistance < 2)
                agent.speed = 0;
            else
                agent.speed = baseSpeed * 2;

            if (playerDistance < combatRadius)
            {
                agent.autoBraking = true;
                FaceTarget();
                if (!isInCombat)
                {
                    StartCoroutine(CombatMovement());
                    StartCoroutine(ClosenessCheck());
                }
            }
            else
            {
                StopCoroutine(CombatMovement());
                StopCoroutine(ClosenessCheck());
                isInCombat = false;
                agent.SetDestination(target);
            }
        }
        else
        {
            agent.speed = baseSpeed;
            agent.autoBraking = false;
            agent.destination = target;
        }

        float distanceFromSpawn = Vector3.Distance(transform.position, startPos);
        if (distanceFromSpawn > 30)
        {
            aggro = false;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !aggro)
            GotoNextPoint();

        Vision();
        //FaceTarget();
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
    //Patrol
    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (pathPoints.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        target = pathPoints[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % pathPoints.Length;
    }
    void FaceTarget()
    {
        Vector3 direction;
        if (aggro && !isAttacking)
        {
            direction = player.transform.position.normalized;
            gameObject.transform.LookAt(new Vector3(direction.x, transform.position.y, direction.z));
        }

        /*
        else
            direction = (agent.destination - transform.position).normalized;

         
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime);*/

    }
    //Player targeting
    void Vision()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 vectorToPlayer = player.transform.position - rayCastPos;

        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        float angleToPlayer = Vector3.Angle(transform.forward, vectorToPlayer);
        Ray ray = new Ray(rayCastPos, vectorToPlayer);
        Physics.Raycast(ray, out hit, layerMask);
        
        if (distanceToPlayer <= visionRange && angleToPlayer <= visionAngle || aggro)
        {
            if (distanceToPlayer < 6 || aggro)
            {
                visionAngle = 360;
            }
            else
            {
                visionAngle = 80;
            }

            //Raycast vision check
            if (hit.collider.gameObject.tag == "Player" && distanceToPlayer > 4)
            {
                Debug.DrawRay(rayCastPos, vectorToPlayer, Color.red);
                if (!aggro)
                {
                    aggro = true;
                }
            }
            else if (distanceToPlayer <= 4)
            {
                Debug.DrawRay(rayCastPos, vectorToPlayer, Color.red);
                if (!aggro)
                {
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
            if (aggro)
            {
                //Invoke("StopAggro", 6f);
            }
        }
    }

    void StopAggro()
    {
    }
    #endregion

    IEnumerator CombatMovement()
    {
        while (true)
        {
            isInCombat = true;
            float combatBehaviour = Random.Range(0, 4);
            if (combatBehaviour < 2)
            {
                float randomWaitingTime = Random.Range(1f, 2f);
                float randPosX = target.x + Random.Range(2, combatRadius - 3);
                float randPosZ = target.x + Random.Range(2, combatRadius - 3);
                Vector3 destination = new Vector3(randPosX, target.y, randPosZ);
                agent.SetDestination(destination);

                float extraTime = Vector3.Distance(destination, transform.position) / (baseSpeed * 1.5f);
                yield return new WaitForSeconds(randomWaitingTime + extraTime);
            }

            else
            {
                Attack();
                yield break;
            }
        }
    }

    IEnumerator ClosenessCheck()
    {
        while (true)
        {
            isInCombat = true;
            if (playerDistance < 2f)
            {
                Attack();
                yield break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Attack()
    {
        StopCoroutine(CombatMovement());
        StopCoroutine(ClosenessCheck());
        

        agent.SetDestination(player.transform.position);

        int attackID = Random.Range(0, 2);
        if (targetDistance < 2)
        {
            isAttacking = true;
            //Transform rotation
            Vector3 direction = player.transform.position;
            float angle = Vector3.Angle(transform.position, direction);
            gameObject.transform.rotation = Quaternion.Euler(0, angle, 0);

            //Release attack
            if (attackID == 0)
                animator.SetTrigger("At1Trigger");
            else if (attackID == 1)
                animator.SetTrigger("At2Trigger");
        }
    }

    void RestartCombatMovement()
    {
        isInCombat = false;
        isAttacking = false;
        if (!isInCombat)
        {
            StartCoroutine(CombatMovement());
            StartCoroutine(ClosenessCheck());
        }
    }
}
