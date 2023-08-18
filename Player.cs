using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public float sprintMultiplier = 1.5f;
    public float mouseSensitivity = 100.0f;
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 5.0f;
    public float jumpHeight = 10.0f;
    public float groundDistance = 0.5f;
    public float groundRayOffset = -0.75f;
    public Transform gunHoldPoint;
    public float pickUpRange = 2f;
    
    private GameObject heldGun;
    private RaycastHit hit;
    private float mouseX, mouseY;
    private float horizontalInput, verticalInput;
    public float crouchHeight = 0.5f;
    private float originalColliderHeight;
    private CapsuleCollider playerCollider;
    private float currentSpeed;
    private bool isGrounded;
    private float originalY;
    private float bobbingTimer = 0;
    private float xAxisClamp = 0.0f;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool shouldJump = false;
    private float originalBobbingSpeed;
    private bool shouldSprint = false;
    private Transform cameraTransform;
    private Rigidbody rb;

    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction pickUpAction;
    private InputAction dropAction;
    private InputAction shootAction;




    private float originalHeight;
    private Vector3 originalCenter;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    
    void OnDrawGizmos()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        // Existing code
        Vector3 rayStartPoint = transform.position + new Vector3(0, groundDistance * -0.75f, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayStartPoint, Vector3.down * groundDistance);

        // New code to draw the green line for the gun pickup raycast
        if (cameraTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * pickUpRange);
        }
    }

    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        playerCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        originalColliderHeight = playerCollider.height;
        Cursor.lockState = CursorLockMode.Locked;
        originalY = cameraTransform.localPosition.y;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        originalBobbingSpeed = bobbingSpeed;

        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;
    }

    void Awake()
    {
        jumpAction = new InputAction(binding: "<Keyboard>/space");
        sprintAction = new InputAction(binding: "<Keyboard>/leftShift");
        crouchAction = new InputAction(binding: "<Keyboard>/leftCtrl"); 

        pickUpAction = new InputAction(binding: "<Keyboard>/e");
        dropAction = new InputAction(binding: "<Keyboard>/q");

        shootAction = new InputAction(binding: "<Mouse>/leftButton");
        
        

        pickUpAction.performed += _ => TryPickUpGun();
        dropAction.performed += _ => { if (heldGun != null) DropGun(); };
        shootAction.performed += _ => { if (heldGun != null) ShootGun(); };

        crouchAction.started += _ => Crouch(); 
        crouchAction.canceled += _ => Uncrouch(); 
        jumpAction.performed += _ => Jump();
        sprintAction.started += _ => shouldSprint = true;
        sprintAction.canceled += _ => shouldSprint = false;

        crouchAction.Enable(); 
        jumpAction.Enable();
        sprintAction.Enable();
        pickUpAction.Enable();
        dropAction.Enable();
        shootAction.Enable();
    }


    void Jump()
    {
        if (isGrounded) shouldJump = true;
    }

    void Crouch()
    {
        playerCollider.height = crouchHeight;
        isCrouching = true;
    }

    void Uncrouch()
    {
        playerCollider.height = originalColliderHeight;
        isCrouching = false;
    }

    void Update()
    {
        // Mouse look
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Movement
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        currentSpeed = speed;

        Vector3 rayStartPoint = transform.position + new Vector3(0, groundRayOffset, 0); // Start slightly above base

        if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, groundDistance))
        {
            // Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.green);
            // Debug.Log("Hit " + hit.collider.name);
            isGrounded = true;
            isJumping = false;

        }
        else
        {
            // Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.red);
            isGrounded = false;
            // if (hit.collider != null) // Check for null before accessing
            // {
            //     Debug.Log("Missed " + hit.collider.name);
            // }
            // else
            // {
            //     Debug.Log("Missed");
            // }
        }

        xAxisClamp += mouseY;

        if (xAxisClamp > 90.0f) {
            mouseY -= xAxisClamp - 90.0f;
            xAxisClamp = 90.0f;
        } else if (xAxisClamp < -90.0f) {
            mouseY -= xAxisClamp + 90.0f;
            xAxisClamp = -90.0f;
        }

        cameraTransform.Rotate(Vector3.left * mouseY);
        transform.Rotate(Vector3.up * mouseX);

        if (shouldSprint && (verticalInput > 0 || horizontalInput != 0) && verticalInput >= 0)
        {
            currentSpeed *= sprintMultiplier;
            bobbingSpeed = originalBobbingSpeed * (sprintMultiplier * 2); // Increase bobbing speed
        }
        else
        {
            bobbingSpeed = originalBobbingSpeed; // Reset to original bobbing speed
        }

        if (isCrouching)
        {
            currentSpeed *= 0.5f; // reduce speed while crouching (optional)
        }

        Vector3 moveDirection = cameraTransform.right * horizontalInput + cameraTransform.forward * verticalInput;
        moveDirection.y = 0; // This ensures that the movement doesn't affect the vertical axis
        moveDirection.Normalize(); // Normalize to ensure consistent speed

        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);

        if ((horizontalInput != 0 || verticalInput != 0) && !isJumping && isGrounded)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float newY = originalY + Mathf.Sin(bobbingTimer) * bobbingAmount;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, newY, cameraTransform.localPosition.z);

        }
        else if (!shouldJump && isGrounded)
        {
            bobbingTimer = 0;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, originalY, cameraTransform.localPosition.z);
        }

    }

    void FixedUpdate()
    {
        // Check for jump
        if (isGrounded && shouldJump)
        {
            shouldJump = false;

            Debug.Log("Jumping");
            Debug.Log("Rigidbody Constraints: " + rb.constraints);
            rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse); // Jump
            Debug.Log("Rigidbody Velocity After Jump: " + rb.velocity);
        }
    }

    void TryPickUpGun()
    {
        // Cast a ray from the camera's position, in the direction it's looking, to check for a gun in pickup range
        RaycastHit gunHit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out gunHit, pickUpRange))
        {
            Debug.Log("Hit an object: " + gunHit.transform.name); // Log the name of hit object
            // Check if the hit object has a Gun script (or another identifying component)
            Gun gun = gunHit.transform.GetComponent<Gun>();
            if (gun != null)
            {
                Debug.Log("Picking up the gun"); // Log gun pickup
                // Pick up the gun
                PickUpGun(gunHit.transform.gameObject);
            }
            else
            {
                Debug.Log("Hit object doesn't have a Gun component"); // Log lack of Gun component
            }
        }
        else
        {
            Debug.Log("No object hit within pick up range"); // Log if nothing is hit
        }
    }



    void PickUpGun(GameObject gun)
    {
        // Set the gun's parent to the gun hold point, and position it there
        gun.transform.SetParent(gunHoldPoint);
        gun.transform.localPosition = new Vector3(0.5f, -0.3f, 0.8f); // Adjust these values as needed
        gunHoldPoint.localRotation = Quaternion.identity;
        gun.transform.localRotation = Quaternion.Euler(0, 0, 0); // Adjust these values as needed

        // Enable the Gun script so the player can shoot
        gun.GetComponent<Gun>().enabled = true;

        // Save a reference to the held gun
        heldGun = gun;
    }

    void DropGun()
    {
        // Disable the Gun script so the player can't shoot
        heldGun.GetComponent<Gun>().enabled = false;

        // Unparent the gun and reset its position and rotation
        heldGun.transform.SetParent(null);
        heldGun.transform.localPosition = transform.position + transform.forward * 1f; // Drop in front of player
        heldGun.transform.localRotation = Quaternion.identity;

        // Clear the reference to the held gun
        heldGun = null;
    }

    // This method calls the Shoot method on the Gun script
    void ShootGun()
    {
        Gun gunScript = heldGun.GetComponent<Gun>();
        if (gunScript != null)
        {
            gunScript.Shoot();
        }
    }

    // Also, remember to disable the shootAction in OnDisable method
    void OnDisable()
    {
        shootAction.Disable();
    }

}

