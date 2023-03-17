using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxForwardSpeed = 8f;
    public float turnSpeed = 100f;

    private float _jumpDirection;
    private Vector2 _moveDirection;
    private float _desiredSpeed;
    private float _fowardSpeed;
    private bool readyJump = false;
    private float jumpSpeed = 20000f;
    private bool onGround = true;
    private float groundRayDistance = 1f;

    const float GROUND_ACCEL = 5f;
    const float GROUND_DECEL = 25f;

    private Animator anim;
    private Rigidbody rb;

    private bool isMoveInput
    {
        get { return !Mathf.Approximately(_moveDirection.sqrMagnitude, 0f); }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(_moveDirection);
        Jump(_jumpDirection);

        CheckOnGround();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpDirection = context.ReadValue<float>();
    }

    private void Move(Vector2 direction)
    {
        float turnAmount = direction.x;
        float fDirection = direction.y;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        _desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        float acceleration = isMoveInput ? GROUND_ACCEL : GROUND_DECEL;

        _fowardSpeed = Mathf.MoveTowards(_fowardSpeed, _desiredSpeed, acceleration * Time.deltaTime);
        anim.SetFloat("ForwardSpeed", _fowardSpeed);

        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);

    }

    private void Jump(float direction)
    {
        if (direction > 0 && onGround)
        {
            anim.SetBool("ReadyJump", true);
            readyJump = true;

        }
        else if (readyJump)
        {
            anim.SetBool("Launch", true);
            readyJump = false;
            anim.SetBool("ReadyJump", false);
        }
    }

    private void CheckOnGround()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * groundRayDistance * 0.5f, -Vector3.up);
        if (Physics.Raycast(ray, out hit, groundRayDistance))
        {
            if (!onGround)
            {
                onGround = true;
                anim.SetBool("Land", true);
            }
        }
        else
        {
            onGround = false;
        }

        Debug.DrawRay(transform.position + Vector3.up * groundRayDistance * 0.5f, -Vector3.up * groundRayDistance, Color.red);
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed, 0);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = false;
    }

    public void Land()
    {
        anim.SetBool("Land", false);
        anim.applyRootMotion = true;
        anim.SetBool("Launch", false);
    }
}
