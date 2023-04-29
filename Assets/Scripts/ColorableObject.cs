using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ColorableObject : MonoBehaviour
{
    [HideInInspector] public bool IsInColor = false;
    [HideInInspector] public bool WasInColor;

    public Sprite IconImage;

    public Renderer[] LerpMaterialMeshes;

    [Header("Juice")]
    public Animator SquashAnimator;
    public ParticleSystem ParticleFX;
    public Color[] ParticleColors = { Color.gray, Color.cyan };

    List<Material> m_lerpMaterials;

    void Start()
    {
        m_lerpMaterials = new List<Material>();
        foreach(Renderer m in LerpMaterialMeshes)
        {
            m_lerpMaterials.Add(m.material);
        }
    }
    public void UpdateColors()
    {
        if(IsInColor != WasInColor)
        {
            SoundManager.instance.PlayTick(transform.position);
            StopAllCoroutines();

            float fromColor = IsInColor ? 0: 1;
            float toColor = 1 - fromColor;
            StartCoroutine(switchColor(fromColor, toColor));
        }
    }

    IEnumerator switchColor(float fromColor, float toColor)
    {
        SquashAnimator.Play("Idle", -1, 0f);
        SquashAnimator.SetTrigger("Squash");

        ParticleFX.Stop();
        ParticleSystem.MainModule main = ParticleFX.main;
        main.startColor = ParticleColors[(int)toColor];
        ParticleFX.Play();

        float color = fromColor;

        foreach (Material material in m_lerpMaterials)
            material.SetFloat("_Blend", color);

        float slider = 0.0f;
        while(slider < 0.1f)
        {
            slider += Time.deltaTime;

            color = Mathf.Lerp(fromColor, toColor, slider / 0.1f);
            foreach (Material material in m_lerpMaterials)
                material.SetFloat("_Blend", color);

            yield return null;
        }

        foreach (Material material in m_lerpMaterials)
            material.SetFloat("_Blend", toColor);

        yield return null;
    }
}