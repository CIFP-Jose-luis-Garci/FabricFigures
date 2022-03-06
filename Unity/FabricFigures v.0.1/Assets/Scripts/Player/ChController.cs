using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class ChController : MonoBehaviour
{
    #region VARIABLES

    //Components
    public CharacterController controller;
    InputActions inputActions;
    Animator animator;
    [SerializeField] Collider atColl;

    //Cameras
    [SerializeField] Transform cam;
    [SerializeField] GameObject tpCam;
    [SerializeField] GameObject aimCam;
    [SerializeField] CinemachineFreeLook aimCamFL;
    //CinemachineInputProvider inputProvider;

    //------------------------------------------


    //Movement

    //Directions
    bool isMoving;
    Vector3 moveDir;
    Vector2 dirInputs;
    Vector3 direction;
    [SerializeField] float baseSpeed = 3f;
    float speed;
    bool isRunning = false;
    float targetAngle;
    float angle;

    //Jump
    Vector3 velocity;
    [SerializeField] float gravity = 38f;
    [SerializeField] float jumpHeight = 2f;
    bool canJump = true;
    bool doubleJump = true;

    //Dash
    bool isDashing = false;
    bool canDash = true;
    float startTime;
    float dashTime = 0.25f;
    float dashCD = 0f;
    float dashSpeed = 20f;

    //Rotation
    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    //Slide
    Vector3 hitNormal;
    bool isGrounded;
    float slopeLimit;
    float slideFriction = 0.3f;



    //------------------------------------------


    //Attacks
    public float _Dmg = 10f;
    public float _pDmg = 30f;

    //Charge
    [SerializeField] float maxCharge = 2f;
    public float currentCharge;
    bool isCharging = false;
    bool isAttacking = false;

    //Air hold
    bool isAirHold = false;
    bool isPlunging = false;
    float plungeSpeed = 40f;

    //Combo animations
    bool canAttack = true;
    public AnimationClip[] attackAnimationClip;
    public Animation referenceToAnimation;
    int comboLevel = 0;

    //------------------------------------------

    //Health

    public bool inv = false;
    public int maxHP = 30;
    public float currHP;

    //------------------------------------------

    //Camera

    //Raycast
    private RaycastHit hit;
    [SerializeField] float visionRange = 6f;
    private Vector3 castOrigin;
    LayerMask layerMask;

    //Camera target
    [SerializeField] Transform currAim;
    [SerializeField] List<Transform> aimTargets = new List<Transform>();
    private int currEnemyTarget;
    public bool isAim = false;
    [SerializeField] LayerMask targetsLM;
    [SerializeField] GameObject focusParticles;

    //------------------------------------------

    //Animations


    #endregion

    //------------------------------------------|| INPUT ACTIONS ||--------------------------------------------
    private void Awake()
    {

        //Movement Inputs
        inputActions = new InputActions();

        //Movement direction (L joystick)
        inputActions.Player.Move.performed += ctx =>
        {
            dirInputs = ctx.ReadValue<Vector2>();
            isMoving = dirInputs.x != 0 || dirInputs.y != 0;
        };
        inputActions.Player.Move.canceled += ctx =>
        {
            dirInputs = Vector2.zero;
            isMoving = false;
        };

        //Jump
        inputActions.Player.Jump.started += ctx =>
        {
            ctx.ReadValueAsButton();
            if (canJump)
                animator.SetTrigger("Jump");
        };
        //Run
        inputActions.Player.Run.started += ctx =>
        {
            isRunning = ctx.ReadValueAsButton();
            isAim = false;
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsStrafing", false);
            baseSpeed *= 1.6f;
            if (!isAim)
                CameraLockOff();
        };

        inputActions.Player.Run.canceled += ctx =>
        {
            isRunning = ctx.ReadValueAsButton();
            animator.SetBool("IsRunning", false);
            baseSpeed /= 1.6f;
        };

        //Dash
        inputActions.Player.Dash.started += ctx =>
        {
            ctx.ReadValueAsButton();

            if (canDash)
            {
                animator.SetBool("IsDashing", true);
                direction = new Vector3(dirInputs.x, 0f, dirInputs.y).normalized;
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                if(!isAim)
                    transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
                StartCoroutine("Dash");
                Invoke("StopDash", dashTime + 0.05f);
            }   
            canDash = false;
        };

        //Combat Inputs
        inputActions.Player.AtL.started += ctx =>
        {
            ctx.ReadValueAsButton();
            if (comboLevel < attackAnimationClip.Length)
            {
                //print("Attack!");
                LightAttackCombo();
            }
        };

        inputActions.Player.AtH.started += ctx =>
        {
            ctx.ReadValueAsButton();
            if (!isGrounded && !isAirHold)
                Plunge();
            else if (isGrounded)
                StartCoroutine("ChargedAttack");
        };
        inputActions.Player.AtH.canceled += ctx =>
        {
            ctx.ReadValueAsButton();
            ChargedAttackRelease();
        };

        //Focus
        inputActions.Player.Focus.started += ctx =>
        {
            ctx.ReadValueAsButton();
            isAim = !isAim;
            if (isAim)
                CameraLockOn();
            else
                CameraLockOff();
        };


        //Cinemachine Camera
        Camera.main.gameObject.TryGetComponent<CinemachineBrain>(out var brain);
        if (brain == null)
            brain = Camera.main.gameObject.AddComponent<CinemachineBrain>();

        aimCamFL = aimCam.gameObject.GetComponent<CinemachineFreeLook>();
        //inputProvider = camObj.gameObject.GetComponent<CinemachineInputProvider>();
    }

    //------------------------------------------|| START AND UPDATE ||-----------------------------------------
    void Start()
    {
        OnEnable();

        slopeLimit = controller.slopeLimit;

        //Assign components
        referenceToAnimation = GetComponent<Animation>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        cam = mainCam.transform;

        tpCam = GameObject.Find("TP cam");
        aimCam = GameObject.Find("LockOn cam");
    }


    void Update()
    {
        if ((Vector3.Angle(Vector3.up, hitNormal) <= slopeLimit && controller.isGrounded))
            isGrounded = true;
        else
            isGrounded = false;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsStrafing", isAim);
        animator.SetBool("IsAttacking", isAttacking);

        if ((isCharging || isAttacking) && isGrounded)
        {
            speed = 0f;
        }
        else
        {
            speed = baseSpeed;
        }

        if (!isAirHold)
        {
            DashCD();
            Jump();
            Move();
        }

        PlungeHit();
        CameraFocusCheck();
        TargetArea();
        SurfaceSlide();
        if(isAim && currAim != null)
            FocusParticles();
    }


    //------------------------------------------|| METHODS ||---------------------------------------------------

    #region MOVEMENT
    //Jump
    void Jump()
    {
        if (isGrounded &&(!isCharging || !isAirHold || !isAttacking))
        {
            
            canJump = true;
            doubleJump = true;
            velocity.y = -2f;
        }
        else
        {
            Invoke("NotGrounded", 0.15f);
        }
        if (inputActions.Player.Jump.triggered && canJump)
        {
            //print("Jump!");
            velocity.y = 0f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * -gravity);
            
        }
        if (inputActions.Player.Jump.triggered && !canJump && doubleJump)
        {
            //print("Double jump!");
            velocity.y = 0f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * -gravity);
            doubleJump = false;
        }
    }
    private void NotGrounded()
    {
        canJump = false;
    }


    //Movement
    private void Move()
    {
        if (isDashing)
            return;

        direction = new Vector3(dirInputs.x, 0f, dirInputs.y).normalized;
        float moveInput = Mathf.Abs(dirInputs.x) + Mathf.Abs(dirInputs.y);
        float moveMult = Mathf.Clamp(moveInput, 0f, 1f);

        //print(moveMult);
        if (isAim && currAim != null)
        {
            if (isMoving)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                Vector3 targetDirection = currAim.position - transform.position;
                targetDirection.y = 0;
                var rotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
            }
            else if (!isMoving)
            {
                Vector3 targetDirection = currAim.position - transform.position;
                targetDirection.y = 0;
                var rotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
                moveDir = Vector3.zero;
            }
            animator.SetFloat("MoveDirY", dirInputs.y);
            animator.SetFloat("MoveDirX", dirInputs.x);
        }
        else
        {
            if (isMoving)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else if (!isMoving)
            {
                moveDir = Vector3.zero;
            }
            animator.SetFloat("MoveSpeed", moveMult);
        }
        moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        velocity.y -= gravity * Time.deltaTime;

        controller.Move(new Vector3(moveDir.x * speed * moveMult, velocity.y, moveDir.z * speed * moveMult) * Time.deltaTime);
        animator.SetFloat("Y vel", velocity.y);
    }


    //Dash
    private void DashCD()
    {
        if (dashCD > 0f)
        {
            canDash = false;
            dashCD -= 0.5f * Time.deltaTime;
        }
        else if (dashCD <= 0f && (!isCharging || !isAirHold || !isAttacking))
        {
            canDash = true;
        }
    }

    IEnumerator Dash()
    {
        startTime = Time.time;
            //print("Dash!");
        while (Time.time < startTime + dashTime)
        {
            inv = true;
            isDashing = true;
            dashCD = 1f;
            velocity = Vector3.zero;
            controller.Move(Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * dashSpeed * Time.deltaTime);
            yield return null;
        }    
    }

    void StopDash()
    {
        inv = false;
        animator.SetBool("IsDashing", false);
        StopCoroutine("Dash");
        isDashing = false;
        AttackAnimEnd();
    }

    void ResetJumpTrigger()
    {
        animator.ResetTrigger("Jump");
    }

    //Wall slide
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == 8)
            hitNormal = hit.normal;
    }

    void SurfaceSlide()
    {
        if (velocity.y < -3 && hitNormal != null)
        {
            controller.Move(new Vector3(((1f - hitNormal.y) * hitNormal.x), velocity.y * Time.deltaTime * 30, ((1f - hitNormal.y) * hitNormal.z)) * Time.deltaTime * slideFriction);
        }
    }

    #endregion

    #region COMBAT

    //Light attack
    public void LightAttackCombo()
    {
        if(!canAttack && !isDashing)
        {
            return;
        }
        canAttack = false;
        isAttacking = true;

        turnSmoothTime = Mathf.Infinity;

        if (comboLevel == attackAnimationClip.Length)
            comboLevel = 0;

        if (comboLevel == 0)
            animator.Play("LightAttack1");

        if (comboLevel == 1)
            animator.Play("LightAttack2");

        if (comboLevel == 2)
            animator.Play("LightAttack3");

        comboLevel++;

        Debug.Log("Light attack " + comboLevel);
    }

    void CanAttack()
    {
        canAttack = true;
    }

    //Charged attack
    void ChargedAttackRelease()
    {
        if(isCharging)
        {
            float t = 0.3f;
            turnSmoothTime = 0.6f;
            if (currentCharge >= maxCharge)
            {
                animator.SetTrigger("HeavyC");
                StopCoroutine("ChargedAttack");
                currentCharge = 0f;
                t = 0.8f;
            }
            else if (currentCharge < maxCharge)
            {
                animator.SetTrigger("HeavyNC");
                StopCoroutine("ChargedAttack");
                currentCharge = 0f;
                t = 0.3f;
            }

            Invoke("AttackAnimEnd", t);
        }
    }

    IEnumerator ChargedAttack()
    {
        while(true)
        {
            turnSmoothTime = Mathf.Infinity;
            isCharging = true;
            currentCharge += 0.2f;

            if (currentCharge >= maxCharge)
                ChargedAttackRelease();

            yield return new WaitForSeconds(0.1f);
        }   
    }

    void AttackAnimEnd()
    {
        comboLevel = 0;
        isCharging = false;
        isAttacking = false;
        canAttack = true;
        turnSmoothTime = 0.1f;
        atColl.enabled = false;
    }

    //Plunge Attack
    void Plunge()
    {
        isPlunging = true;
        canAttack = false;
        canDash = false;
        animator.SetTrigger("PlungeAttack");
        StartCoroutine("AirHold");
        Invoke("StopAirHold", 0.6f);
    }
    void PlungeHit()
    {
        if (isGrounded && isPlunging)
        {
            StopCoroutine("PlungeFall");
            animator.SetTrigger("PlungeHit");
            Invoke("PlungeAnimEnd", 0.5f);
        }
    }
    void PlungeAnimEnd()
    {
        isPlunging = false;
        isAirHold = false;
        canDash = true;
        canAttack = true;
    }
    IEnumerator PlungeFall()
    {
        while (!isGrounded)
        {
            //plungeSpeed /= Time.deltaTime;
            controller.Move(new Vector3(((1f - hitNormal.y) * hitNormal.x), -60, ((1f - hitNormal.y) * hitNormal.z)) * Time.deltaTime * slideFriction);
            yield return null;
        }
    }
    IEnumerator AirHold()
    {
        while(!isGrounded)
        {
            isAirHold = true;
            velocity.y = 0f;
            yield return null;
        }
    }
    void StopAirHold()
    {
        StopCoroutine("AirHold");
        StartCoroutine("PlungeFall");
    }
    void ColliderOn()
    {
        atColl.enabled = true;
    }    
    void ColliderOff()
    {
        atColl.enabled = true;
    }

    #endregion

    #region CAMERA
    //CameraFocus
    void TargetArea()
    {
        aimCamFL.LookAt = currAim;

        if (currAim == null)
            CameraLockOff();

        castOrigin = cam.position;
    }

    void CameraFocusCheck()
    {
        if (isAim)
            if (Physics.Raycast(aimCam.transform.position, currAim.position - aimCam.transform.position, out hit))
            {
                Debug.DrawRay(castOrigin, (currAim.position - aimCam.transform.position).normalized * hit.distance, Color.red);
                if (hit.collider.gameObject.tag != "Enemy" || hit.distance > 30f)
                {
                    print("LockOff, " + hit.collider.gameObject.tag);
                    CameraLockOff();
                }
            }
    }

    void FocusParticles()
    {
        focusParticles.transform.position = currAim.position;
        focusParticles.transform.rotation = Quaternion.LookRotation(focusParticles.transform.position - aimCam.transform.position);
    }
    void CameraLockOn()
    {
        FindClosestEnemy();

        if (currAim == null)
        {
            isAim = false;
            return;
        }

        tpCam.SetActive(false);
        focusParticles.SetActive(true);
    }

    public Transform FindClosestEnemy()
    {
        GameObject[] enemies;
        enemies = GameObject.FindGameObjectsWithTag("AimTarget");
        currAim = null;
        float distance = Mathf.Infinity;
        Vector3 position = cam.transform.position;
        foreach (GameObject enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && curDistance < visionRange)
            {
                currAim = enemy.transform;
                distance = curDistance;
            }
            else
            {
                print(curDistance);
                currAim = null;
            }
        }

        return currAim;
    }

    void CameraLockOff()
    {
        isAim = false;

        tpCam.transform.position = aimCam.transform.position;
        if (currAim != null)
            tpCam.transform.rotation = Quaternion.LookRotation(tpCam.transform.position - currAim.transform.position);
        tpCam.SetActive(true);

        focusParticles.SetActive(false);
        currAim = null;
    }
    #endregion

    //Input actions enable
    private void OnEnable()
    {
        inputActions.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
    }
}
