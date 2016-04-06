using UnityEngine;
using System.Collections;

public class UIBillboard : MonoBehaviour
{
    Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void FixedUpdate()
    {
        transform.LookAt(2 * transform.position - cam.position, Vector3.up);
    }
}
