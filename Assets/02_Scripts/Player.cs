using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
      //  float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        // Movimiento en el eje X (izquierda/derecha) y Z (adelante/atrás)
        transform.Translate(new Vector3(moveX, 0,0 ));
    }
}
