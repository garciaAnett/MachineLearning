using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class CellController : MonoBehaviour
{
    // Prefab de la célula a instanciar
    public GameObject celulaPrefab;

    // Número de células a generar por generación
    public int numeroDeCelulas = 10;

    // Intervalo de tiempo entre generaciones
    public float intervaloTiempo = 10f;

    // Imagen de la barra de vida
    public Image lifeB;

    // Total de células capturadas
    public int totalCells = 0;

    // Lista de GameObjects de células existentes
    private List<GameObject> celulasExistentes = new List<GameObject>();

    // Clase para representar las propiedades de una célula
    [System.Serializable]
    public class Celula
    {
        public Color color;
        public float tamaño;
        public bool sobrevivio;
    }

    // Listas para manejar las experiencias de las células
    private List<Celula> experienciasPrevias = new List<Celula>();
    public List<Celula> experienciasActuales = new List<Celula>();
    private List<Celula> experienciasSupervivientes = new List<Celula>();

    // Tiempo transcurrido desde el inicio del juego
    private float tiempoTranscurrido = 0f;

    void Start()
    {
        // Iniciar la generación de células en intervalos definidos
        InvokeRepeating(nameof(GenerarCelulas), 0f, intervaloTiempo);
    }

    void Update()
    {
        // Actualizar el tiempo transcurrido
        tiempoTranscurrido += Time.deltaTime;

        // Asegurarse de que haya pasado algún tiempo
        if (tiempoTranscurrido > 0f)
        {
            // Calcular el promedio de células capturadas por segundo
            float promedioCelulasPorSegundo = totalCells / tiempoTranscurrido;

            // Actualizar el fillAmount de la barra con el promedio normalizado
            lifeB.fillAmount = Mathf.Clamp(promedioCelulasPorSegundo / 10f, 0f, 1f);
        }
    }

    void GenerarCelulas()
    {
        // Eliminar células existentes
        foreach (GameObject celula in celulasExistentes)
        {
            Destroy(celula);
        }
        celulasExistentes.Clear();

        // Filtrar experiencias para obtener solo las células que sobrevivieron
        experienciasSupervivientes.Clear();
        foreach (Celula experiencia in experienciasPrevias)
        {
            if (experiencia.sobrevivio)
            {
                experienciasSupervivientes.Add(experiencia);
            }
        }

        // Determinar si se usarán células supervivientes como "padres"
        bool usarSupervivientes = experienciasSupervivientes.Count > 0;

        // Generar nuevas células
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar la célula
            GameObject nuevaCelula = Instantiate(celulaPrefab);

            // Asignar posición aleatoria
            float x = Random.Range(-20f, 10f);
            float y = Random.Range(1f, 4f);
            nuevaCelula.transform.position = new Vector2(x, y);

            // Determinar atributos de color y tamaño
            Color colorAjustado;
            float tamañoAjustado;

            if (usarSupervivientes)
            {
                // Seleccionar aleatoriamente una célula superviviente como "padre"
                Celula padre = experienciasSupervivientes[Random.Range(0, experienciasSupervivientes.Count)];

                // Convertir color del padre a HSV
                Color.RGBToHSV(padre.color, out float padreHue, out float padreSat, out float padreVal);

                // Aplicar mutación al tono (hue)
                float mutacionHue = Random.Range(-0.02f, 0.02f); // Puedes ajustar este valor
                float nuevoHue = (padreHue + mutacionHue + 1f) % 1f; // Asegurar que está en el rango [0,1]

                // Limitar el hue dentro del rango deseado (opcional)
                float minHue = 0.25f; // Límite inferior
                float maxHue = 0.6f;  // Límite superior
                nuevoHue = Mathf.Clamp(nuevoHue, minHue, maxHue);

                // Reconstruir el color ajustado
                colorAjustado = Color.HSVToRGB(nuevoHue, padreSat, padreVal);

                // Aplicar mutación al tamaño
                float mutacionTamaño = Random.Range(-0.02f, 0.02f); // Puedes ajustar este valor
                tamañoAjustado = Mathf.Clamp(padre.tamaño + mutacionTamaño, 0.5f, 1.5f); // Rango definido para el tamaño
            }
            else
            {
                // Si no hay supervivientes previos, usar valores aleatorios
                float hueAleatorio = Random.Range(0f, 1f); // Tono del círculo
                float satAleatorio = Random.Range(0.5f, 1f);    // Saturación del color
                float valAleatorio = Random.Range(0.5f, 1f); // Luminosidad
                colorAjustado = Color.HSVToRGB(hueAleatorio, satAleatorio, valAleatorio);
                tamañoAjustado = Random.Range(0.3f, 1f);
            }

            // Asignar tamaño y color
            nuevaCelula.transform.localScale = new Vector3(tamañoAjustado, tamañoAjustado, 1f); // Definir el tamaño del círculo
            SpriteRenderer spriteRenderer = nuevaCelula.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = colorAjustado;
            }

            // Añadir a la lista de células existentes
            celulasExistentes.Add(nuevaCelula);

            // Registrar la experiencia inicial
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tamaño = tamañoAjustado,
                sobrevivio = true // Asumimos que sobrevivirá por ahora
            };
            experienciasActuales.Add(nuevaExperiencia);

            // Asignar el índice de experiencia a la célula y referencia al controlador
            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.indiceExperiencia = experienciasActuales.Count - 1;
                scriptCelula.cellController = this; // Asignar referencia al CellController
            }
        }

        // Preparar las experiencias para la siguiente generación
        experienciasPrevias.Clear();
        experienciasPrevias.AddRange(experienciasActuales);
        experienciasActuales.Clear(); // Vaciar la lista actual
    }

    // Método para actualizar cuando una célula es capturada
    public void CellCaptured(int indiceExperiencia)
    {
        if (indiceExperiencia >= 0 && indiceExperiencia < experienciasActuales.Count)
        {
            experienciasActuales[indiceExperiencia].sobrevivio = false;
        }
        totalCells += 1;
        Debug.Log("CANTIDAD DE CÉLULAS CAPTURADAS: " + totalCells);
    }
}

