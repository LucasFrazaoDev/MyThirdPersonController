using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxForwardSpeed = 8f;
    public float turnSpeed = 100f;

    private Vector2 _moveDiretion;
    private float _desiredSpeed;
    private float _fowardSpeed;

    const float GROUND_ACCEL = 5f;
    const float GROUND_DECEL = 25f;

    private Animator anim;

    private bool isMoveInput
    {
        get { return !Mathf.Approximately(_moveDiretion.sqrMagnitude, 0f); }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDiretion = context.ReadValue<Vector2>();
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

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(_moveDiretion);
    }
}
