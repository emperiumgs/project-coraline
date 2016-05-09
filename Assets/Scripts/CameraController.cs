using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    const int OFFSET_SPEED = 6;
    const int ANGULAR_SPEED = 4;
    const int X_ROTATION = 10;
    const int PI_RAD = 180;
    const float MAX_ROTATION = 30;
    const float MIN_ROTATION = -50;
    const float CAST_RADIUS = 0.2f;
    const float ZOOM_TIME = 0.3f;
    const float SHAKE_INTENSITY = 2f;

    public Vector3 targetOffset;
    public LayerMask mask;

    Vector3 origPos;
    Vector3 aimPos;

    Transform target;
    Transform pivot;
    Vector3 offsetVector;
    Vector3 dir;
    RaycastHit hit;
    float curDist;
    float x;
    float y;
    float vRot;
    bool locked;

    Coroutine zooming;

    void Awake()
    {
        target = FindObjectOfType<PlayerPhysics>().transform;
        origPos = transform.localPosition;
        aimPos = origPos - new Vector3(1, 0, -1.5f);
        offsetVector = origPos;
        pivot = transform.parent;
    }

    void LateUpdate()
    {
        if (locked)
            return;

        pivot.position = Vector3.Lerp(pivot.position, target.position + targetOffset, Time.deltaTime * OFFSET_SPEED);
        curDist = transform.localPosition.magnitude;

        dir = (transform.position - target.position).normalized;        

        if (Physics.SphereCast(target.position, CAST_RADIUS, dir, out hit, offsetVector.magnitude, mask) && !hit.collider.isTrigger)
            transform.localPosition = transform.localPosition * (hit.distance / curDist);
        else
            transform.localPosition = offsetVector;

        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");

        vRot = pivot.localEulerAngles.x + y * -ANGULAR_SPEED;
        if (vRot > PI_RAD)
            vRot -= PI_RAD * 2;
        vRot = Mathf.Clamp(vRot, MIN_ROTATION, MAX_ROTATION);
        pivot.localEulerAngles = new Vector3(vRot, pivot.localEulerAngles.y + x * ANGULAR_SPEED);
        
        transform.localEulerAngles = new Vector3(X_ROTATION, transform.localEulerAngles.y);
    }

    public void Zoom(bool zoomIn)
    {
        Vector3 targetPos = zoomIn ? aimPos : origPos;
        if (zooming != null)
            StopCoroutine(zooming);
        zooming = StartCoroutine(ZoomProcess(targetPos));
    }

    public void Shake(float time)
    {
        StartCoroutine(ShakeProcess(time));
    }

    IEnumerator ZoomProcess(Vector3 targetPos)
    {
        float time = 0;
        while (time < ZOOM_TIME)
        {
            time += Time.deltaTime;
            offsetVector = Vector3.Slerp(offsetVector, targetPos, time / ZOOM_TIME);
            yield return null;
        }
        offsetVector = targetPos;
    }

    IEnumerator ShakeProcess(float shakeTime)
    {
        locked = true;
        Vector3 prevRot = transform.localEulerAngles;
        Vector3 movement = new Vector3();
        float time = 0;
        while (time < shakeTime)
        {
            time += Time.deltaTime;
            movement.z = Random.Range(-SHAKE_INTENSITY, SHAKE_INTENSITY);
            transform.localEulerAngles = prevRot + movement;
            yield return null;
        }
        locked = false;
    }
}