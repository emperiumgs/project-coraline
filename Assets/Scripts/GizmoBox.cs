using UnityEngine;
using System.Collections;

public class GizmoBox : MonoBehaviour
{
    public Color gizmoColor;
    public Transform referenceTransform;
    public Vector3 offset;
    public Vector3 size;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        if (referenceTransform != null)
            Gizmos.matrix = Matrix4x4.TRS(referenceTransform.position, referenceTransform.rotation, Vector3.one);
        Gizmos.DrawCube(offset, size);
    }
}
