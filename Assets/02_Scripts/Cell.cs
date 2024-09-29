using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cell : MonoBehaviour
{
    // Índice de experiencia en la lista de experiencias actuales
    public int indiceExperiencia;

    // Referencia al CellController
    [HideInInspector]
    public CellController cellController;

    void Start()
    {
        // Destruir la célula después de 10 segundos
        Destroy(gameObject, 10f);
    }

    void OnMouseDown()
    {
        // Actualizar experiencia como eliminada en el controlador
        if (cellController != null)
        {
            cellController.CellCaptured(indiceExperiencia);
        }

        // Destruir la célula
        Destroy(gameObject);
    }
   
}
