using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    const int OFFSET_SPEED = 6,
        ANGULAR_SPEED = 4,
        X_ROTATION = 10,
        PI_RAD = 180,
        MAX_ROTATION = 30,
        MIN_ROTATION = -50;
    const float CAST_RADIUS = 0.2f,
        ZOOM_TIME = 0.3f,
        SHAKE_INTENSITY = 1f,
        BACK_COOLDOWN = 0.3f;

    public Vector3 targetOffset;
    public LayerMask mask;

    Vector3 origPos,
        aimPos;

    Transform target, 
        pivot;
    Vector3 offsetVector, 
        dir;
    RaycastHit hit;
    float curDist,
        x, 
        y, 
        vRot;
    bool locked,
        getBack = true;

    Coroutine zooming,
        backCooldown;

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
        {
            transform.localPosition = transform.localPosition * (hit.distance / curDist);
            if (backCooldown != null)
                StopCoroutine(backCooldown);
            backCooldown = StartCoroutine(GetBackCooldown());
        }
        else if (getBack)
            transform.localPosition = Vector3.Lerp(transform.localPosition, offsetVector, Time.deltaTime * OFFSET_SPEED);

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
        Vector3 prevRot = transform.localEulerAngles,
            movement = new Vector3();
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

    IEnumerator GetBackCooldown()
    {
        getBack = false;
        yield return new WaitForSeconds(BACK_COOLDOWN);
        getBack = true;
    }

    Vector3 gPoint,
        gDir,
        prevGDir;
    void OnDrawGizmos()
    {
        if (target == null)
            return;
        
        gPoint = target.position;
        prevGDir = gDir;
        gDir = (transform.position - gPoint).normalized;
        if (prevGDir == gDir)
            return;

        Gizmos.color = Color.cyan;
        float dist = Vector3.Distance(transform.position, gPoint);
        while (Vector3.Distance(gPoint, target.position) < dist)
        {
            Gizmos.DrawWireSphere(gPoint, CAST_RADIUS);
            gPoint += gDir * 2 * CAST_RADIUS;
        }
    }
}