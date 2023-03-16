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

    const float GROUND_ACCEL = 5f;
    const float GROUND_DECEL = 25f;

    private Animator anim;
    private Rigidbody rb;

    private bool isMoveInput
    {
        get { return !Mathf.Approximately(_moveDirection.sqrMagnitude, 0f); }
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
        if(direction.sqrMagnitude > 1f)
            direction.Normalize();

        _desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        float acceleration = isMoveInput ? GROUND_ACCEL : GROUND_DECEL;

        _fowardSpeed = Mathf.MoveTowards(_fowardSpeed, _desiredSpeed, acceleration * Time.deltaTime);
        anim.SetFloat("ForwardSpeed", _fowardSpeed);

        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);

    }

    private void Jump(float direction)
    {
        Debug.Log(direction);
        if (direction > 0)
        {
            anim.SetBool("ReadyJump", true);
            readyJump = true;
            
        } else if(readyJump)
        {
            anim.SetBool("Launch", true);
        }
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed, 0);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = false;
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
    }
}
