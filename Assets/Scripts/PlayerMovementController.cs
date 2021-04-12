using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;
using PlayFab;
using System;


public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private CharacterController controller = null;
    [SerializeField] private Animator animator = null;
    public Transform camera;

    [SerializeField] public float movementSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public Slider gravitySlider;
    public Slider walkSpeedSlider;
    public Slider jumpSpeedSlider;

    public Toggle sittingToggle;
    public Toggle dancingToggle;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    [SerializeField] public float jumpSpeed = 5.0f;
    [SerializeField] private float gravity = Physics.gravity.y;
    [SerializeField] private float yVelocity = 0;

    private bool isDancing = false;
    private bool isSitting = false;

    private Cinemachine.CinemachineFreeLook cam;
    private bool controlsEnabled = true;

    [Client]

    public override void OnStartLocalPlayer()
    {
        cam = FindObjectOfType<Cinemachine.CinemachineFreeLook>();
        cam.Follow = transform;
        cam.LookAt = transform;
        camera = Camera.main.transform;

        MovementMenuManager movementMenu = GameObject.Find("UI").GetComponent<MovementMenuManager>();
        jumpSpeedSlider = movementMenu.jumpSpeedSlider;
        gravitySlider = movementMenu.gravitySlider;
        walkSpeedSlider = movementMenu.walkSpeedSlider;

        dancingToggle = movementMenu.dancingToggle;
        sittingToggle = movementMenu.sittingToggle;

        gameObject.name = "Local";
    }


    [Client]

    void Update()
    {
        GetComponentInParent<Player>().floatingInfo.transform.LookAt(Camera.main.transform);

        if (!isLocalPlayer) {
            return;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (controller.isGrounded && controlsEnabled)
        {
            yVelocity = gravitySlider.value * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                yVelocity = jumpSpeedSlider.value;
            }
        }
        else
        {
            yVelocity += gravitySlider.value * Time.deltaTime;
        }

        if (dancingToggle.isOn)
        {
            if (!isDancing) macarena();

            return;
        }
        else if (isDancing) macarena();

        if (sittingToggle.isOn)
        {
            if (!isSitting) sit();

            return;
        }
        else if (isSitting) sit();


        if (direction.magnitude >= 0.1f && controlsEnabled)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            Vector3 movement = moveDir.normalized * walkSpeedSlider.value;

            movement.y = yVelocity;

            controller.Move(movement * Time.deltaTime);

        } else
        {
            Vector3 jumpVector = new Vector3(0, yVelocity, 0);
            controller.Move(jumpVector * Time.deltaTime);
        }

        if (controlsEnabled) animator.SetFloat("move", direction.magnitude);
    }

    public void macarena()
    {
        if (isSitting) return;

        sittingToggle.interactable = isDancing;

        animator.SetBool("mac", !isDancing);
        isDancing = !isDancing;
    }

    public void sit()
    {
        if (isDancing) return;

        dancingToggle.interactable = isSitting;

        animator.SetBool("sitting", !isSitting);
        isSitting = !isSitting;
    }


    public void EnableControls()
    {
        this.controlsEnabled = true;
    }

    public void DisableControls()
    {
        this.controlsEnabled = false;
    }
}
