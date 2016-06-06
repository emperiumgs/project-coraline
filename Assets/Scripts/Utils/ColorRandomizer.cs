using UnityEngine;
using System.Collections;

public class ColorRandomizer : MonoBehaviour
{
    Light l;

    void Awake()
    {
        l = GetComponent<Light>();
        StartCoroutine(RandomLights());
    }

    IEnumerator RandomLights()
    {
        while (true)
        {
            l.color = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
            yield return new WaitForSeconds(.25f);
        }
    }
}
