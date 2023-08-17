using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public float sprintMultiplier = 1.5f;
    public float slideMultiplier = 2.0f; // Boost for sliding
    public float mouseSensitivity = 100.0f;
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 5.0f;
    public float jumpHeight = 10.0f; // Ascend height for jump
    public float slideDuration = 0.5f; // Duration of sliding
    public float groundDistance = 0.5f; // Distance to check for ground
    
    private bool isGrounded;
    private float originalY;
    private float bobbingTimer = 0;
    private float xAxisClamp = 0.0f;
    private float slideTimer = 0.0f; // Timer for sliding duration
    public float groundRayOffset = -0.1f; // Slightly below the base of the player
    private bool isSliding = false; // Track sliding state
    private bool isJumping = false;
    private float sphereRadius = 0.5f;
    private Transform cameraTransform;
    private Rigidbody rb;

    void OnDrawGizmos()
    {
        Vector3 rayStartPoint = transform.position + new Vector3(0, groundRayOffset, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayStartPoint, Vector3.down * groundDistance);
    }



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        originalY = transform.localPosition.y;
        cameraTransform = GetComponentInChildren<Camera>().transform;
        rb = GetComponent<Rigidbody>();
    }

    [Header("Ground Check")]
    public LayerMask groundLayer; // Assign ground layer in Inspector
    

    void Update()
    {
        RaycastHit hit;
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        float currentSpeed = speed;

       Vector3 rayStartPoint = transform.position + new Vector3(0, groundRayOffset, 0); // Start slightly above base

        if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, groundDistance, groundLayer))
        {
            Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.green);
            Debug.Log("Hit " + hit.collider.name);
            isGrounded = true;
        }
        else
        {
            Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.red);
            isGrounded = false;
        }


        // // Compute the starting point for the SphereCast
        // Vector3 rayStartPoint = transform.position + new Vector3(0, groundRayOffset, 0);
        // Vector3 sphereCastStartPoint = rayStartPoint + Vector3.down * groundDistance;

        // // Perform the SphereCast from the computed starting point
        // if (Physics.SphereCast(sphereCastStartPoint, sphereRadius, Vector3.down, out hit, 0.1f, groundLayer))
        // {
        //     Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.green);
        //     Debug.Log("Hit " + hit.collider.name);
        //     isGrounded = true;
        // }
        // else
        // {
        //     Debug.DrawRay(rayStartPoint, Vector3.down * groundDistance, Color.red);
        //     isGrounded = false;
        // }

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

        if (Input.GetKey(KeyCode.LeftShift) && (verticalInput > 0 || horizontalInput != 0) && verticalInput >= 0)
        {
            currentSpeed *= sprintMultiplier;
        }

        if (isSliding)
        {
            slideTimer += Time.deltaTime;
            currentSpeed *= slideMultiplier;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, 0.1f, cameraTransform.localPosition.z); // Adjust camera position
            if (slideTimer >= slideDuration)
            {
                slideTimer = 0;
                isSliding = false;
                transform.localScale = new Vector3(1, 1, 1); // Reset scale
                cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, 1, cameraTransform.localPosition.z); // Reset camera position
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSliding)
        {
            isSliding = true;
            transform.localScale = new Vector3(1, 0.1f, 1); // Reduce height
    }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("Jumping");
            rb.AddForce(new Vector3(0, jumpHeight, 0), ForceMode.Impulse); // Jump
            isJumping = true;
        }

        else if(Input.GetKeyDown(KeyCode.Space)) {
            Input.GetKeyDown(KeyCode.Space);
            Debug.Log("Player cannot jump is not Grounded");
        }

        Vector3 moveDirection = cameraTransform.right * horizontalInput + cameraTransform.forward * verticalInput;
        moveDirection.y = 0; // This ensures that the movement doesn't affect the vertical axis
        moveDirection.Normalize(); // Normalize to ensure consistent speed

        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);

        if ((horizontalInput != 0 || verticalInput != 0) && !isJumping)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, originalY + Mathf.Sin(bobbingTimer) * bobbingAmount, transform.localPosition.z);
        }
        else
        {
            bobbingTimer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, originalY, transform.localPosition.z);
        }

    }
}


        // Vector3 rayStartPoint = transform.position + new Vector3(0, groundRayOffset, 0); // Start slightly above base

        // RaycastHit hit;
        // if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, groundDistance, groundLayer))
        // {
        //     Debug.DrawLine(rayStartPoint, rayStartPoint + Vector3.down * groundDistance, Color.green);
        //     Debug.Log("Hit " + hit.collider.name);
        //     isGrounded = true;
        // }
        // else
        // {
        //     Debug.DrawLine(rayStartPoint, rayStartPoint + Vector3.down * groundDistance, Color.red);
        //     isGrounded = false;
        // }
