using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Referencia al Transform del Player
    public float speed = 5f; // Velocidad de la cámara al seguir al Player
    public float clampLeft; // Límite izquierdo para la cámara
    public float clampRight; // Límite derecho para la cámara

    private Vector3 offset; // Para mantener la cámara a cierta distancia del Player

    // Use this for initialization
    void Start()
    {
        // Calculamos la diferencia inicial entre la cámara y el Player
        offset = transform.position - player.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Calcula la nueva posición de la cámara basándose en la posición del Player
        Vector3 targetPosition = player.position + offset;

        // Aseguramos que la cámara no se mueva más allá de los límites definidos
        targetPosition.x = Mathf.Clamp(targetPosition.x, clampLeft, clampRight);

        // Movemos la cámara hacia la nueva posición
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
