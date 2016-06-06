using UnityEngine;

public class BalloonAid : Balloon
{
    public GameObject[] collectibles;

    public override void Activate()
    {
        Instantiate(collectibles[Random.Range(0, collectibles.Length)], transform.position, Quaternion.identity);
        ShowParticlesAndDie();
    }
}
