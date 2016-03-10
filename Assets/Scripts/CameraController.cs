using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    const int OFFSET_SPEED = 6;
    const int ANGULAR_SPEED = 4;
    const int X_ROTATION = 10;
    const int PI_RAD = 180;
    const float MAX_ROTATION = 15;
    const float MIN_ROTATION = -30;
    const float CAST_RADIUS = 0.2f;

    public Transform target;

    Transform pivot;
    Vector3 offsetVector;
    Vector3 dir;
    RaycastHit hit;
    float curDist;
    float x;
    float y;
    float vRot;
    int mask;

    void Awake()
    {
        offsetVector = transform.localPosition;
        pivot = transform.parent;
        mask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    void FixedUpdate()
    {
        pivot.position = Vector3.Lerp(pivot.position, target.position, Time.deltaTime * OFFSET_SPEED);
        curDist = transform.localPosition.magnitude;

        dir = (transform.position - target.position).normalized;        

        if (Physics.SphereCast(target.position, CAST_RADIUS, dir, out hit, offsetVector.magnitude, mask) && !hit.collider.isTrigger)
        {
            transform.localPosition = transform.localPosition * (hit.distance / curDist);
        }
        else
            transform.localPosition = offsetVector;

        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");

        vRot = pivot.localEulerAngles.x + y * -ANGULAR_SPEED;
        if (vRot > PI_RAD)
            vRot -= PI_RAD * 2;
        vRot = Mathf.Clamp(vRot, MIN_ROTATION, MAX_ROTATION);
        pivot.localEulerAngles = new Vector3(vRot, pivot.localEulerAngles.y + x * ANGULAR_SPEED);

        transform.rotation = Quaternion.LookRotation(pivot.position - transform.position);
        transform.localEulerAngles = new Vector3(X_ROTATION, transform.localEulerAngles.y);
    }
}
