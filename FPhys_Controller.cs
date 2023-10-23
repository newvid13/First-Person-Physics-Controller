using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FPhys_Controller : MonoBehaviour
{
    private Rigidbody rig;
    private CapsuleCollider col;
    public bool isActive;

    [SerializeField] private Transform orientation, camPos, carryPos;
    private Vector3 moveDirection;
    private float inputX, inputZ, mainMSpeed;
    [SerializeField] private float moveSpeed, accelerateSpeed, airMod, crouchSpeed, runSpeed;

    private float rayLength;
    public LayerMask groundMask;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundDrag, airDrag;

    [SerializeField] private bool isCrouched, crouchProgress;
    [SerializeField] private float crouchedHeight, fullHeight;

    [SerializeField] private bool isJumping;
    [SerializeField] private float jumpHeight, dmgHeight;
    private float yVelocity;

    private RaycastHit Hit;
    private FPS_Audio scrAudio;
    public float footTime, footCooldown;

    [SerializeField] private Transform lowerLegRay, upperLegRay, foot;
    [SerializeField] private float maxStepHeight, stepSmooth;
    [SerializeField] private float angleMin, angleMax;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rig.freezeRotation = true;
        rig.collisionDetectionMode = CollisionDetectionMode.Continuous;

        scrAudio = GetComponent<FPS_Audio>();
        SetSpeed(moveSpeed);
        CalcHeight();

        MainManager.Input.OnCrouch += CrouchPress;
        MainManager.Input.OnJump += JumpPress;
        MainManager.Input.OnRunDown += RunDown;
        MainManager.Input.OnRunUp += RunUp;
    }

    private void Update()
    {
        if (!isActive)
            return;

        CheckGround();
        GetInput();
        LimitSpeed();
    }

    private void FixedUpdate()
    {
        if (!isActive)
            return;

        Move();
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(foot.position, 0.25f, groundMask, QueryTriggerInteraction.Ignore);
        bool rayHit = Physics.Raycast(transform.position, Vector3.down, out Hit, rayLength + 0.4f, groundMask);

        if (isGrounded)
        {
            if (rayHit)
            {
                if (isJumping)
                {
                    Landing();
                }

                footTime -= (Time.deltaTime * rig.velocity.magnitude);

                if (rig.velocity.magnitude > 0.2f && footTime < 0f)
                {
                    footTime = footCooldown;
                    scrAudio.Footsteps(Hit.transform);
                }
            }
        }
        else
        {
            isJumping = true;
            yVelocity = rig.velocity.y;
        }
    }

    private void Landing()
    {
        isJumping = false;

        if (yVelocity < -3)
        {
            scrAudio.FootLanding(Hit.transform);

            if (yVelocity < dmgHeight)
            {
                Player_Destructable scrDsr = GetComponent<Player_Destructable>();
                int dmg = Mathf.RoundToInt(yVelocity) * -2;
                scrDsr.Damage(dmg);
            }
        }
    }

    private void GetInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
    }

    private void Move()
    {
        float angle = Vector3.Angle(Vector3.up, Hit.normal);

        if (angle > angleMin && angle < angleMax && isGrounded)
        {
            rig.useGravity = false;
        }
        else
        {
            rig.useGravity = true;
        }

        moveDirection = orientation.forward * inputZ + orientation.right * inputX;
        moveDirection.Normalize();

        Vector3 finalMove = moveDirection;

        if (isGrounded)
        {
            if (angle < angleMin)
            {
                StepClimb();
            }

            rig.drag = groundDrag;
            rig.AddForce(finalMove * accelerateSpeed * mainMSpeed, ForceMode.Force);
        }
        else
        {
            rig.drag = airDrag;
            rig.AddForce(finalMove * accelerateSpeed * mainMSpeed * airMod, ForceMode.Force);
        }

    }

    private void CalcHeight()
    {
        rayLength = col.height / 2f;

        foot.localPosition = new Vector3(0, -rayLength + 0.1f, 0);
        lowerLegRay.localPosition = new Vector3(0, -rayLength + 0.03f, 0);
        upperLegRay.localPosition = new Vector3(0, -rayLength + maxStepHeight, 0);
    }

    private void SetSpeed(float spd)
    {
        mainMSpeed = spd;
    }

    private void LimitSpeed()
    {
        Vector3 flatVel = new Vector3(rig.velocity.x, 0, rig.velocity.z);

        if(flatVel.magnitude > mainMSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * mainMSpeed;
            rig.velocity = new Vector3(limitedVel.x, rig.velocity.y, limitedVel.z);
        }
    }

    private void JumpPress()
    {
        if (!isGrounded || !isActive)
            return;

        rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
        rig.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    private void RunDown()
    {
        if (!isActive || isCrouched)
            return;

        SetSpeed(runSpeed);
    }

    private void RunUp()
    {
        if (!isActive || isCrouched)
            return;

        SetSpeed(moveSpeed);
    }

    private void CrouchPress()
    {
        if (!isGrounded || crouchProgress || !isActive)
            return;

        if (isCrouched)
        {
            if (!Physics.Raycast(transform.position, Vector3.up, fullHeight - rayLength, groundMask, QueryTriggerInteraction.Ignore))
            {
                crouchProgress = true;

                rig.AddForce(Vector3.up * 30, ForceMode.Impulse);
                camPos.DOLocalMoveY(camPos.transform.localPosition.y + crouchedHeight / 2, 0.3f);
                carryPos.DOLocalMoveY(carryPos.transform.localPosition.y + crouchedHeight / 2, 0.3f);
                DOTween.To(() => col.height, x => col.height = x, fullHeight, 0.3f).OnComplete(CrouchedUp);
            }
        }
        else
        {
            crouchProgress = true;

            camPos.DOLocalMoveY(camPos.transform.localPosition.y - crouchedHeight / 2, 0.3f);
            carryPos.DOLocalMoveY(carryPos.transform.localPosition.y - crouchedHeight / 2, 0.3f);
            col.height = crouchedHeight;
            isCrouched = true;

            SetSpeed(crouchSpeed);
            CalcHeight();
            crouchProgress = false;
        }
    }

    private void CrouchedUp()
    {
        isCrouched = false;
        crouchProgress = false;

        SetSpeed(moveSpeed);
        CalcHeight();
    }

    private void StepClimb()
    {
        if (moveDirection == Vector3.zero)
            return;

        if (Physics.Raycast(lowerLegRay.position, moveDirection, 0.35f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (!Physics.Raycast(upperLegRay.position, moveDirection, 0.45f, groundMask, QueryTriggerInteraction.Ignore))
            {
                rig.AddForce(Vector3.up * stepSmooth, ForceMode.Force);
            }
        }
    }
}
