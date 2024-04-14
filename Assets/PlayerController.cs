using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    public LayerMask interactableLayer;
    public LayerMask ladderLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isClimbing;
    private Vector3 movement;

    private enum State { Idle, Walking, Jumping, Interacting, Climbing }
    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentState = State.Idle;
    }

    void Update()
    {
        CheckGroundStatus();
        ProcessInputs();

        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Walking:
                HandleWalkingState();
                break;
            case State.Jumping:
                HandleJumpingState();
                break;
            case State.Interacting:
                HandleInteractingState();
                break;
            case State.Climbing:
                HandleClimbingState();
                break;
        }
    }

    private void ProcessInputs()
    {
        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (movement.magnitude > 0.1f)
        {
            if (currentState != State.Climbing)
                currentState = State.Walking;
            movement.Normalize();
        }
        else if (currentState != State.Climbing)
        {
            currentState = State.Idle;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && currentState != State.Climbing)
        {
            currentState = State.Jumping;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == State.Climbing)
            {
                currentState = State.Idle; // Allow dismounting the ladder with the same interact key
            }
            else
            {
                currentState = State.Interacting;
            }
        }

        if (currentState == State.Climbing && Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            transform.Translate(Vector3.up * Input.GetAxis("Vertical") * climbSpeed * Time.deltaTime);
        }
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleIdleState() { }
    private void HandleWalkingState()
    {
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
    private void HandleJumpingState()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            currentState = State.Idle;
        }
    }
    private void HandleInteractingState()
    {
        // Detect and interact with ladder
        Collider[] ladders = Physics.OverlapSphere(transform.position, 1.0f, ladderLayer);
        if (ladders.Length > 0)
        {
            currentState = State.Climbing; // Transition to climbing state
            rb.useGravity = false; // Disable gravity while climbing
        }
        else
        {
            currentState = State.Idle; // No ladder found, return to idle
        }
    }
    private void HandleClimbingState()
    {
        // Add logic to climb up or down the ladder
        rb.velocity = new Vector3(0, Input.GetAxis("Vertical") * climbSpeed, 0);
        Debug.Log("climbing");
        // Optional: Check if at the top or bottom of the ladder to exit climbing state
        // This could involve raycasting or checking transform.position against the ladder's top/bottom position
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
