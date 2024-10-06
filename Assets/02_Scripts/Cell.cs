using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector]
    public CellController cellController;

    public CellController.Celula experiencia;

    void Start()
    {
        // Destruir la célula después de 10 segundos (opcional)
        Destroy(gameObject, 10f);
    }

    void OnMouseDown()
    {
        if (cellController != null)
        {
            cellController.CellCaptured(this);
        }

        Destroy(gameObject);
    }
}




