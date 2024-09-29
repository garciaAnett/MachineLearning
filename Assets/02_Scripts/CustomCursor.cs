using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture; // La imagen del cursor que deseas usar
    public Vector2 cursorHotspot = Vector2.zero; // Posición del "hotspot" (donde el clic ocurre)

    void Start()
    {
        // Cambiamos el cursor con la textura seleccionada
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    // Puedes tener una función para cambiar el cursor cuando quieras
    public void ChangeCursor(Texture2D newCursorTexture, Vector2 newHotspot)
    {
        Cursor.SetCursor(newCursorTexture, newHotspot, CursorMode.Auto);
    }

    // Para restaurar el cursor a su valor por defecto
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

}
