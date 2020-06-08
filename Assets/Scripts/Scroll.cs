using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{

    public float velocidad = 0f;

    private Renderer rend;

    public bool enMovimiento = false;

    private float tiempoInicio = 0f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        NotificationCenter.DefaultCenter().AddObserver(this, "PersonajeEmpiezaACorrer");
        NotificationCenter.DefaultCenter().AddObserver(this, "PersonajeHaMuerto");
    }

    // Update is called once per frame
    void Update()
    {
        if (enMovimiento)
        {
            rend.material.mainTextureOffset = new Vector2((Time.time - tiempoInicio) * velocidad, 0f);
        }
    }

    void PersonajeEmpiezaACorrer()
    {
        enMovimiento = true;
        tiempoInicio = Time.time;
    }

    void PersonajeHaMuerto()
    {
        enMovimiento = false;
    }
}
