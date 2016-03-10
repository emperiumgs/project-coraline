using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerPhysics : MonoBehaviour
{
    const int SPEED = 6;
    const int JUMP_FORCE = 15;
    const int MASS = 6;

    CharacterController control;
    CollisionFlags flags;
    Camera cam;
    Vector3 move;
    float moveDir;
    float h;
    float v;
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

        // Face the direction to walk to
        transform.LookAt(transform.position + h * cam.transform.right + v * cam.transform.forward);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y);

        if (control.isGrounded)
        {
            flags = control.collisionFlags;

            // Turns all values to positive            
            if (h < 0) h = Mathf.Abs(h);
            if (v < 0) v = Mathf.Abs(v);

            // The higher input names the speed
            if (h != 0 && v != 0)
                moveDir = v > h ? v : h;
            else
                moveDir = v + h;

            move = moveDir * Vector3.forward * SPEED;
            // Converts to local direction
            move = transform.TransformDirection(move);

            if (jump)
            {
                jump = false;
                move.y = JUMP_FORCE;

                // Ignores horizontal movement if colliding on the sides (to avoid climbing mountains)
                if ((flags & CollisionFlags.Sides) == CollisionFlags.Sides)
                    move.x = move.z = 0;
            }            
        }

        move.y += gravity * MASS * Time.deltaTime;

        control.Move(move * Time.deltaTime);
    }
}