using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InicioPartida : MonoBehaviour
{
    ControladorPersonaje cPJ;

    // Start is called before the first frame update
    void Start()
    {
        cPJ = GameObject.Find("PJ(Clone)").GetComponent<ControladorPersonaje>();
    }

    public void Saltar()
    {
        cPJ.Salto();
    }

    public void Caer()
    {
        cPJ.Caida();
    }
}
