using UnityEngine;

public class Character : MonoBehaviour
{
    ThirdPersonCharacter characterController;
    [SerializeField] Transform cam;
    Vector3 camForward;  // The current forward direction of the camera
    Vector3 move;
    bool jump;
    void Start ()
    {
        Camera c = GetComponent<Camera>();
        cam = (cam == null && c != null) ? c.transform : cam;
        characterController = GetComponent<ThirdPersonCharacter>();
    }

    void Update()
    {
        if (!jump)
        {
            jump = Input.GetKeyDown(KeyCode.Space);
        }
    }

    void FixedUpdate()
    {
        // read inputs
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        //bool crouch = Input.GetKey(KeyCode.C);

        // calculate move direction to pass to character
        if (cam != null)
        {
            // calculate camera relative direction to move:
            camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            move = v * camForward + h * cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            move = v * Vector3.forward + h * Vector3.right;
        }
        // walk speed multiplier
        //if (Input.GetKey(KeyCode.LeftShift))
        //    move *= 0.5f;

        // pass all parameters to the character control script
        characterController.Move(move, false);
        jump = false;
    }
}
