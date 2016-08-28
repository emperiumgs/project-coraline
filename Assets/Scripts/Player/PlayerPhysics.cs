using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerPhysics : MonoBehaviour, IKnockable, IStunnable
{
    [HideInInspector]
    public bool camOrient,
        stunned;

    const int SPEED = 6,
        JUMP_FORCE = 15;
    const float MASS = 6f;

    CharacterController control;
    CameraController camCtrl;
    Coroutine stunRoutine;
    Animator anim;
    Vector3 move;
    Camera cam;
    float moveDir,
        h,
        v,
        prevY,
        gravity = Physics.gravity.y;
    bool jump,
        blockJump,
        knocked;
    int groundAngle;

    Vector3 centerPos
    {
        get { return transform.position + Vector3.up; }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        control = GetComponent<CharacterController>();
        cam = Camera.main;
        camCtrl = cam.GetComponent<CameraController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (control.isGrounded)
        {
            groundAngle = (int)Vector3.Angle(transform.up, hit.normal);
            if (groundAngle >= 44 && groundAngle < 90)
                blockJump = true;
            else
                blockJump = false;
        }
    }

    void Update()
    {
        if (!stunned)
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Jump"))
                jump = true;
            else if (Input.GetButtonUp("Jump"))
                jump = false;
        }
        else
            h = v = 0;

        if (!knocked)
        {
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

            if (blockJump && !control.isGrounded)
                moveDir = 0;

            anim.SetFloat("movSpeed", moveDir);
            move *= SPEED * moveDir;
            // Converts to local direction
            move = transform.TransformDirection(move);
            move.y = prevY;

            if (control.isGrounded)
            {
                anim.SetBool("jump", jump);
                if (jump)
                {
                    anim.SetTrigger("jumpTrigger");
                    jump = false;
                    move.y = JUMP_FORCE;
                    if (blockJump)
                        move.x = move.z = 0;
                }
            }
        }
        
        move.y += gravity * MASS * Time.deltaTime;
        control.Move(move * Time.deltaTime);

        if (knocked && control.isGrounded)
        {
            knocked = false;
            if (stunRoutine == null)
                stunned = false;
        }
    }

    public void Knockup(Vector3 dir, float strength)
    {
        move = dir * SPEED;
        move.y = strength;
        stunned = true;
        knocked = true;
    }

    public void Stun(float time)
    {
        anim.SetBool("stun", true);
        anim.SetTrigger("stunTrigger");
        stunned = true;
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(StunDuration(time));
        camCtrl.Shake(time);
    }

    IEnumerator StunDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        stunned = false;
        stunRoutine = null;
        anim.SetBool("stun", stunned);
    }
}