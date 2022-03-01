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
    [SerializeField] Transform cam;
    [SerializeField] GameObject camObj;
    [SerializeField] CinemachineFreeLook freeLookCamera;
    CinemachineInputProvider inputProvider;
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
    //[SerializeField] float visionRadius = 6f;
    private Vector3 rayOrigin;
    LayerMask layerMask;

    //Camera target
    [SerializeField] Transform aimTarget;
    private int currEnemyTarget;
    public bool isAim = false;
    [SerializeField] Transform selfFocus;
    InputActionReference inputActionRef;

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
        inputActions.Player.Jump.started += ctx => ctx.ReadValueAsButton();

        //Run
        inputActions.Player.Run.started += ctx =>
        {
            isRunning = ctx.ReadValueAsButton();
            isAim = false;
            aimTarget = null;
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsStrafing", false);
            baseSpeed *= 1.6f;
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
            if (!controller.isGrounded && !isAirHold)
                Plunge();
            else if (controller.isGrounded)
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
            animator.SetBool("IsStrafing", isAim);
            if (isAim)
                CameraLockOn();
            else
                CameraLockOff();
        };


        //Cinemachine Camera
        Camera.main.gameObject.TryGetComponent<CinemachineBrain>(out var brain);
        if (brain == null)
            brain = Camera.main.gameObject.AddComponent<CinemachineBrain>();

        freeLookCamera = camObj.gameObject.GetComponent<CinemachineFreeLook>();
        inputProvider = camObj.gameObject.GetComponent<CinemachineInputProvider>();
    }

    //------------------------------------------|| START AND UPDATE ||-----------------------------------------
    void Start()
    {
        inputActionRef = inputProvider.XYAxis;
        print(inputActionRef);

        OnEnable();
        //Assign components
        referenceToAnimation = GetComponent<Animation>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        cam = mainCam.transform;

        camObj = GameObject.Find("TP Camera");
    }


    void Update()
    {
        freeLookCamera.LookAt = aimTarget;

        if (aimTarget == null)
            CameraLockOff();

        if ((isCharging || isAttacking) && controller.isGrounded)
        {
            speed = 0f;
        }
        else
        {
            speed = baseSpeed;
        }

        //if (aimTarget = null)
            //isAim = false;

        if (!isAirHold)
        {
            DashCD();
            Jump();
            Move();
        }
        PlungeHit();

        TargetArea();
    }


    //------------------------------------------|| METHODS ||---------------------------------------------------

    #region MOVEMENT
    //Jump
    void Jump()
    {
        if (controller.isGrounded &&(!isCharging || !isAirHold || !isAttacking))
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
        if (isAim && aimTarget != null)
        {
            if (isMoving)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                Vector3 targetDirection = aimTarget.position - transform.position;
                targetDirection.y = 0;
                var rotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
            }
            else if (!isMoving)
            {
                Vector3 targetDirection = aimTarget.position - transform.position;
                targetDirection.y = 0;
                var rotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
                moveDir = Vector3.zero;
            }
            animator.SetFloat("MoveDirY", dirInputs.y);
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
        StopCoroutine("Dash");
        isDashing = false;
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
        if (controller.isGrounded && isPlunging)
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
        while (!controller.isGrounded)
        {
            //plungeSpeed /= Time.deltaTime;
            controller.Move(Vector3.down * plungeSpeed * Time.deltaTime);
            yield return null;
        }
    }
    IEnumerator AirHold()
    {
        while(!controller.isGrounded)
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
        rayOrigin = cam.position;

        /*if (Physics.SphereCast(rayOrigin, visionRadius, cam.transform.forward, out hit, 10) && hit.transform.tag == "enemyTarget")
        {
            Debug.Log("TargetInRange");
            aimTarget = hit.transform;
        }*/
    }

    void CameraLockOn()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("AimTarget");
        aimTarget = enemy.transform;
        freeLookCamera.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        inputProvider.XYAxis = null;
    }

    void CameraLockOff()
    {
        isAim = false;
        aimTarget = selfFocus;
        freeLookCamera.m_BindingMode = CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp;
        inputProvider.XYAxis = inputActionRef;
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
