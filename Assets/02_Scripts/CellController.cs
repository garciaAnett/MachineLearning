using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class CellController : MonoBehaviour
{
    // Prefab de la c�lula a instanciar
    public GameObject celulaPrefab;

    // N�mero de c�lulas a generar por generaci�n
    public int numeroDeCelulas = 10;

    // Intervalo de tiempo entre generaciones
    public float intervaloTiempo = 10f;

    // Imagen de la barra de vida
    public Image lifeB;

    // Total de c�lulas capturadas
    public int totalCells = 0;

    // Lista de GameObjects de c�lulas existentes
    private List<GameObject> celulasExistentes = new List<GameObject>();

    // Clase para representar las propiedades de una c�lula
    [System.Serializable]
    public class Celula
    {
        public Color color;
        public float tama�o;
        public bool sobrevivio;
    }

    // Listas para manejar las experiencias de las c�lulas
    private List<Celula> experienciasPrevias = new List<Celula>();
    public List<Celula> experienciasActuales = new List<Celula>();
    private List<Celula> experienciasSupervivientes = new List<Celula>();

    // Tiempo transcurrido desde el inicio del juego
    private float tiempoTranscurrido = 0f;

    void Start()
    {
        // Iniciar la generaci�n de c�lulas en intervalos definidos
        InvokeRepeating(nameof(GenerarCelulas), 0f, intervaloTiempo);
    }

    void Update()
    {
        // Actualizar el tiempo transcurrido
        tiempoTranscurrido += Time.deltaTime;

        // Asegurarse de que haya pasado alg�n tiempo
        if (tiempoTranscurrido > 0f)
        {
            // Calcular el promedio de c�lulas capturadas por segundo
            float promedioCelulasPorSegundo = totalCells / tiempoTranscurrido;

            // Actualizar el fillAmount de la barra con el promedio normalizado
            lifeB.fillAmount = Mathf.Clamp(promedioCelulasPorSegundo / 10f, 0f, 1f);
        }
    }

    void GenerarCelulas()
    {
        // Eliminar c�lulas existentes
        foreach (GameObject celula in celulasExistentes)
        {
            Destroy(celula);
        }
        celulasExistentes.Clear();

        // Filtrar experiencias para obtener solo las c�lulas que sobrevivieron
        experienciasSupervivientes.Clear();
        foreach (Celula experiencia in experienciasPrevias)
        {
            if (experiencia.sobrevivio)
            {
                experienciasSupervivientes.Add(experiencia);
            }
        }

        // Determinar si se usar�n c�lulas supervivientes como "padres"
        bool usarSupervivientes = experienciasSupervivientes.Count > 0;

        // Generar nuevas c�lulas
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar la c�lula
            GameObject nuevaCelula = Instantiate(celulaPrefab);

            // Asignar posici�n aleatoria
            float x = Random.Range(-20f, 10f);
            float y = Random.Range(1f, 4f);
            nuevaCelula.transform.position = new Vector2(x, y);

            // Determinar atributos de color y tama�o
            Color colorAjustado;
            float tama�oAjustado;

            if (usarSupervivientes)
            {
                // Seleccionar aleatoriamente una c�lula superviviente como "padre"
                Celula padre = experienciasSupervivientes[Random.Range(0, experienciasSupervivientes.Count)];

                // Convertir color del padre a HSV
                Color.RGBToHSV(padre.color, out float padreHue, out float padreSat, out float padreVal);

                // Aplicar mutaci�n al tono (hue)
                float mutacionHue = Random.Range(-0.02f, 0.02f); // Puedes ajustar este valor
                float nuevoHue = (padreHue + mutacionHue + 1f) % 1f; // Asegurar que est� en el rango [0,1]

                // Limitar el hue dentro del rango deseado (opcional)
                float minHue = 0.25f; // L�mite inferior
                float maxHue = 0.6f;  // L�mite superior
                nuevoHue = Mathf.Clamp(nuevoHue, minHue, maxHue);

                // Reconstruir el color ajustado
                colorAjustado = Color.HSVToRGB(nuevoHue, padreSat, padreVal);

                // Aplicar mutaci�n al tama�o
                float mutacionTama�o = Random.Range(-0.02f, 0.02f); // Puedes ajustar este valor
                tama�oAjustado = Mathf.Clamp(padre.tama�o + mutacionTama�o, 0.5f, 1.5f); // Rango definido para el tama�o
            }
            else
            {
                // Si no hay supervivientes previos, usar valores aleatorios
                float hueAleatorio = Random.Range(0f, 1f); // Tono del c�rculo
                float satAleatorio = Random.Range(0.5f, 1f);    // Saturaci�n del color
                float valAleatorio = Random.Range(0.5f, 1f); // Luminosidad
                colorAjustado = Color.HSVToRGB(hueAleatorio, satAleatorio, valAleatorio);
                tama�oAjustado = Random.Range(0.3f, 1f);
            }

            // Asignar tama�o y color
            nuevaCelula.transform.localScale = new Vector3(tama�oAjustado, tama�oAjustado, 1f); // Definir el tama�o del c�rculo
            SpriteRenderer spriteRenderer = nuevaCelula.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = colorAjustado;
            }

            // A�adir a la lista de c�lulas existentes
            celulasExistentes.Add(nuevaCelula);

            // Registrar la experiencia inicial
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tama�o = tama�oAjustado,
                sobrevivio = true // Asumimos que sobrevivir� por ahora
            };
            experienciasActuales.Add(nuevaExperiencia);

            // Asignar el �ndice de experiencia a la c�lula y referencia al controlador
            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.indiceExperiencia = experienciasActuales.Count - 1;
                scriptCelula.cellController = this; // Asignar referencia al CellController
            }
        }

        // Preparar las experiencias para la siguiente generaci�n
        experienciasPrevias.Clear();
        experienciasPrevias.AddRange(experienciasActuales);
        experienciasActuales.Clear(); // Vaciar la lista actual
    }

    // M�todo para actualizar cuando una c�lula es capturada
    public void CellCaptured(int indiceExperiencia)
    {
        if (indiceExperiencia >= 0 && indiceExperiencia < experienciasActuales.Count)
        {
            experienciasActuales[indiceExperiencia].sobrevivio = false;
        }
        totalCells += 1;
        Debug.Log("CANTIDAD DE C�LULAS CAPTURADAS: " + totalCells);
    }
}

