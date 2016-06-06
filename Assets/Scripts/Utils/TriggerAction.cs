using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerAction : MonoBehaviour
{
    public LayerMask targetedMask;
    public Vector3 offsetPosition,
        halfExtents;
    public UnityEvent onTrigger;

    void FixedUpdate()
    {
        if (Physics.CheckBox(transform.position + offsetPosition, halfExtents, transform.rotation, targetedMask))
        {
            if (onTrigger != null)
            {
                onTrigger.Invoke();
                Destroy(gameObject);
            }
        }
    }
}