using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering.PostProcessing;

public class OscuridadPost : MonoBehaviour
{
    public PostProcessVolume volume;

    Vignette vignette;

    public float speed;

    private GameObject pj;

    public float k = 0.022f;

    private void Awake()
    {
        //vignette = ScriptableObject.CreateInstance<Vignette>();

        //vignette.enabled.Override(true);

        //volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 0f, vignette);

        volume.profile.TryGetSettings(out vignette);

        //pj = GameObject.Find("PJ(Clone)");
    }

    private void Start()
    {
        pj = GameObject.Find("PJ(Clone)");
    }

    private void FixedUpdate()
    {
        if (vignette.intensity.value < 1f )
        {
            vignette.intensity.value += 1f * speed * Time.fixedDeltaTime;
        }
        
        if (vignette.intensity.value > 1)
        {
            vignette.intensity.value = 1f;
        }
       
    }

    private void Update()
    {
        vignette.center.value.y = pj.transform.position.y * k + 0.381f;
    }
}
