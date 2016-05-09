using UnityEngine;

public class BalloonAid : Balloon
{
    public GameObject[] collectibles;

    protected override void Activate()
    {
        Instantiate(collectibles[Random.Range(0, collectibles.Length)], transform.position, Random.rotation);
        ShowParticlesAndDie();
    }
}
