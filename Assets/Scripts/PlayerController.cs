using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxForwardSpeed = 8f;
    public float turnSpeed = 100f;
    public bool isDead = false;

    private float _jumpDirection;
    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private Vector2 _lastLookDirection;
    private float _desiredSpeed;
    private float _fowardSpeed;
    private bool readyJump = false;
    private float jumpSpeed = 20000f;
    private bool onGround = true;
    private float groundRayDistance = 1f;
    private bool _escapePressed = false;
    private bool _cursorIsLocked = true;
    private int _health = 100;
    private bool _firing = false;
    private float jumpEffort = 0;

    [SerializeField] private float _xSensitivity = 0.5f;
    [SerializeField] private float _ySensitivity = 0.5f;
    [SerializeField] Transform spine;
    [SerializeField] private Transform weapon;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform hip;
    [SerializeField] private LineRenderer _laser;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _crossLight;

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
        UpdateCursorLock();

        if (isDead) return;

        Move(_moveDirection);
        Jump(_jumpDirection);

        Aiming();

        CheckOnGround();
    }

    private void LateUpdate()
    {
        if (isDead) return;
        if (anim.GetBool("Armed"))
        {
            _lastLookDirection += new Vector2(-_lookDirection.y * _ySensitivity, _lookDirection.x * _xSensitivity);
            _lastLookDirection.x = Mathf.Clamp(_lastLookDirection.x, -40f, 40f);
            _lastLookDirection.y = Mathf.Clamp(_lastLookDirection.y, -30f, 60f);

            spine.localEulerAngles = _lastLookDirection;
        }
    }

    public void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.tag == "Bullet")
        {
            _health -= 10;
            anim.SetTrigger("Hit");

            if (_health <= 0)
            {
                isDead = true;
                anim.SetLayerWeight(1, 0);
                anim.SetBool("Dead", true);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpDirection = context.ReadValue<float>();
    }

    private void Aiming()
    {
        if (anim.GetBool("Armed"))
        {
            _laser.gameObject.SetActive(true);
            //_crosshair.gameObject.SetActive(true);
            _crossLight.gameObject.SetActive(true);

            RaycastHit laserHit;
            Ray laserRay = new Ray(_laser.transform.position, _laser.transform.forward);
            if (Physics.Raycast(laserRay, out laserHit))
            {
                _laser.SetPosition(1, _laser.transform.InverseTransformPoint(laserHit.point));
                Vector3 crosshairLocation = Camera.main.WorldToScreenPoint(laserHit.point);
                //_crosshair.transform.position = crosshairLocation;
                _crossLight.transform.localPosition = new Vector3(0, 0, _laser.GetPosition(1).z * 0.9f);

                if (_firing && laserHit.collider.gameObject.tag == "Orb")
                {
                    laserHit.collider.gameObject.GetComponent<AIController>().BlowUp();
                }
            }
            else
            {
                //_crosshair.gameObject.SetActive(false);
                _crossLight.gameObject.SetActive(true);
            }
        }
        else
        {
            _laser.gameObject.SetActive(false);
            //_crosshair.gameObject.SetActive(false);
            _crossLight.gameObject.SetActive(true);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        _firing = false;
        if ((int)context.ReadValue<float>() == 1 && anim.GetBool("Armed"))
        {
            anim.SetTrigger("Fire");
            _firing = true;
        }
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if ((int)context.ReadValue<float>() == 1)
            _escapePressed = true;
        else
            _escapePressed = false;
    }

    public void OnArmed(InputAction.CallbackContext context)
    {
        anim.SetBool("Armed", !anim.GetBool("Armed"));
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
            jumpEffort += Time.deltaTime;
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
                anim.SetFloat("LandingVelocity", rb.velocity.magnitude);
                anim.SetBool("Land", true);
                anim.SetBool("Falling", false);
            }
        }
        else
        {
            onGround = false;
            anim.SetBool("Falling", true);
            anim.applyRootMotion = false;
        }

        Debug.DrawRay(transform.position + Vector3.up * groundRayDistance * 0.5f, -Vector3.up * groundRayDistance, Color.red);
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed * Mathf.Clamp(jumpEffort, 1, 3), 0);
        rb.AddForce(transform.forward * _fowardSpeed * 2000f);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = false;
        onGround = false;
    }

    public void Land()
    {
        anim.SetBool("Land", false);
        anim.applyRootMotion = true;
        anim.SetBool("Launch", false);
        jumpEffort = 0;
    }

    public void OnPickupGun()
    {
        weapon.SetParent(hand);
        weapon.localPosition = new Vector3(-0.043f, 0.097f, 0.021f);
        weapon.localRotation = Quaternion.Euler(-77.70f, -287.7f, 31.702f);
        weapon.localScale = Vector3.one;
    }

    public void PutGunDown()
    {
        weapon.SetParent(hip);
        weapon.localPosition = new Vector3(-0.151f, -0.097f, -0.08f);
        weapon.localRotation = Quaternion.Euler(-105.9f, -156.8f, -69.15f);
        weapon.localScale = Vector3.one;
    }

    public void UpdateCursorLock()
    {
        if (_escapePressed)
            _cursorIsLocked = false;

        if (_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
