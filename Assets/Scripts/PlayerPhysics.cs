using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerPhysics : MonoBehaviour
{
    [HideInInspector]
    public bool camOrient = false;

    const int SPEED = 6;
    const int JUMP_FORCE = 15;
    const int MASS = 6;

    CharacterController control;
    Vector3 move;
    Camera cam;    
    float moveDir;
    float h;
    float v;
    float prevY;
    float gravity = Physics.gravity.y;
    bool jump;

    void Awake()
    {
        control = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jump = true;
        else if (Input.GetButtonUp("Jump"))
            jump = false;
    }

    void FixedUpdate()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");        

        prevY = move.y;
        move = Vector3.zero;

        // Force orientation to follow camera
        if (camOrient)
        {
            transform.localEulerAngles = new Vector3(0, cam.transform.eulerAngles.y);

            // Move to the direction pressed, according to the facing direction
            move = v * Vector3.forward + h * Vector3.right;
        }
        else
        {
            // Face the direction to walk to
            transform.LookAt(transform.position + h * cam.transform.right + v * cam.transform.forward);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y);
            
            move = Vector3.forward;
        }

        // Turns all values to positive            
        if (h < 0) h = Mathf.Abs(h);
        if (v < 0) v = Mathf.Abs(v);

        // The higher input names the speed
        if (h != 0 && v != 0)
            moveDir = v > h ? v : h;
        else
            moveDir = v + h;

        move *= SPEED * moveDir;
        // Converts to local direction
        move = transform.TransformDirection(move);
        move.y = prevY;

        if (control.isGrounded)
        {
            if (jump)
            {
                jump = false;
                move.y = JUMP_FORCE;
            }
        }            

        move.y += gravity * MASS * Time.deltaTime;

        control.Move(move * Time.deltaTime);
    }
}