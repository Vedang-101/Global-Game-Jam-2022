using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawPrints : MonoBehaviour
{
    public ParticleSystem PawPrintFX;
    public Transform LeftFootTransform;
    public Transform RightFootTransform;

    Vector3 prevLocation;
    private void Start()
    {
        prevLocation= transform.position;
    }

    public void PrintPaw(int footIndex)
    {
        Vector3 direction = transform.position - prevLocation;
        direction.Normalize();

        if (footIndex == 0)
            PawPrintFX.transform.position = LeftFootTransform.position;
        else
            PawPrintFX.transform.position = RightFootTransform.position;

        ParticleSystem.MainModule mainModule = PawPrintFX.main;
        mainModule.startRotationY = Mathf.Atan2(-direction.z, -direction.x);
        PawPrintFX.Emit(1);
        prevLocation = transform.position;
    }
}
