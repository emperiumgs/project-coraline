using UnityEngine;
using System.Collections;

public class CheckpointZone : MonoBehaviour
{
    public GameObject[] respawnableObjects;
    public Vector3 offsetPos;
    public Vector3 halfExtents;

    CameraController cam;
    HudController hud;
    PlayerCombat player;
    GameObject playerClone;
    GameObject[] respawnedObjects;
    LayerMask mask;
    bool done;

    void Awake()
    {
        mask = LayerMask.NameToLayer("Player");
        hud = FindObjectOfType<HudController>();
        cam = Camera.main.GetComponent<CameraController>();
        StartCoroutine(CheckContact());
    }

    void SetupClones()
    {
        respawnedObjects = new GameObject[respawnableObjects.Length];
        GameObject r,
            o;
        for (int i = 0; i < respawnableObjects.Length; i++)
        {
            r = respawnableObjects[i];
            o = Instantiate(r, r.transform.position, r.transform.rotation) as GameObject;
            o.SetActive(false);
            respawnedObjects[i] = o;
        }
        player = FindObjectOfType<PlayerCombat>();
        playerClone = Instantiate(player.gameObject, transform.position, transform.rotation) as GameObject;
        playerClone.SetActive(false);
    }

    public void Respawn()
    {
        GameObject r;
        for (int i = 0; i < respawnableObjects.Length; i++)
        {
            r = respawnableObjects[i];
            if (r != null)
                Destroy(r.gameObject);
            respawnedObjects[i].SetActive(true);
            respawnableObjects[i] = respawnedObjects[i];
        }
        Destroy(player.gameObject);
        playerClone.SetActive(true);
        cam.Reorient();
        player = playerClone.GetComponent<PlayerCombat>();
        player.GetComponent<PlayerPhysics>().enabled = false;
        player.enabled = false;
        player.UpdateCheckpoint(this);
        hud.Awake();
        SetupClones();
    }

    public void EnablePlayer()
    {
        player.EnablePlayer();
    }

    public void Clear()
    {
        foreach (GameObject o in respawnedObjects)
            Destroy(o);
        Destroy(playerClone);
        Destroy(gameObject);
    }

    IEnumerator CheckContact()
    {
        Collider[] cols;
        while (!done)
        {
            cols = Physics.OverlapBox(transform.position + offsetPos, halfExtents, transform.rotation, 1 << mask);
            if (cols.Length != 0)
            {
                done = true;
                cols[0].GetComponent<PlayerCombat>().UpdateCheckpoint(this);
                SetupClones();
            }
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawWireCube(offsetPos, halfExtents * 2);
    }
}
