using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CellController : MonoBehaviour
{
    // Prefab de la célula a instanciar
    public GameObject celulaPrefab;

    // Número de células a generar por generación
    public int numeroDeCelulas = 5;

    // Intervalo de tiempo entre generaciones (duración de cada ronda)
    public float intervaloTiempo = 20f;

    // Variables para la interfaz de usuario (UI)
    public Text timeCounterText;    // Texto para mostrar los segundos restantes
    public Text scoreText;          // Texto para mostrar la cantidad de células destruidas
    public Text roundText;          // Texto para mostrar la ronda actual

    // Variables privadas para controlar el estado del juego
    private List<GameObject> celulasExistentes = new List<GameObject>(); // Almacena las células en la ronda actual
    private int currentRound = 0;      // Número de la ronda actual
    private float timeLeftInRound;     // Tiempo restante en la ronda
    private int totalCellsDestroyed = 0;  // Cantidad de células destruidas en total

    // Clase para representar las propiedades de una célula (color, tamaño, si sobrevivió)
    [System.Serializable]
    public class Celula
    {
        public Color color;      // Color de la célula
        public float tamaño;     // Tamaño de la célula
        public bool sobrevivio;  // Indica si la célula sobrevivió a la ronda anterior
    }

    // Listas para manejar las experiencias de las células
    private List<Celula> experienciasPrevias = new List<Celula>();  // Lista de células de la ronda anterior
    private List<Celula> experienciasSupervivientes = new List<Celula>(); // Células que sobrevivieron a la ronda
    private List<Celula> experienciasSupervivientesPermanentes = new List<Celula>(); // Lista permanente de supervivientes

    // Inicializa las variables y comienza la generación de células
    void Start()
    {
        // Inicializar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Iniciar la generación de células cada intervalo de tiempo
       InvokeRepeating(nameof(GenerarCelulas), 0f, intervaloTiempo);
    }

    // Actualiza la interfaz de usuario y el tiempo en cada frame
    void Update()
    {
        // Reducir el tiempo restante de la ronda
        timeLeftInRound -= Time.deltaTime;
        timeLeftInRound = Mathf.Max(0f, timeLeftInRound);

        // Actualizar el contador de tiempo en la UI
        if (timeCounterText != null)
        {
            timeCounterText.text = "Tiempo: " + Mathf.CeilToInt(timeLeftInRound).ToString() + " s";
        }

        // Actualizar el contador de células destruidas en la UI
        if (scoreText != null)
        {
            scoreText.text = "Células destruidas: " + totalCellsDestroyed.ToString();
        }

        // Actualizar la ronda actual en la UI
        if (roundText != null)
        {
            roundText.text = "Ronda: " + currentRound.ToString();
        }

        // Verificar si el tiempo de la ronda ha terminado
        if (timeLeftInRound <= 0f)
        {
            // Generar nuevas células al finalizar el tiempo
            GenerarCelulas();
        }
    }

    // Método para marcar las células restantes como sobrevivientes y almacenarlas permanentemente
    void FinalizarRonda()
    {
        foreach (GameObject celula in celulasExistentes)
        {
            if (celula != null)
            {
                Cell scriptCelula = celula.GetComponent<Cell>();
                if (scriptCelula != null && scriptCelula.experiencia != null)
                {
                    scriptCelula.experiencia.sobrevivio = true;
                    // Añadir a la lista permanente si aún no está presente
                    if (!experienciasSupervivientesPermanentes.Contains(scriptCelula.experiencia))
                    {
                        experienciasSupervivientesPermanentes.Add(scriptCelula.experiencia);
                    }
                }
            }
        }
    }

    // Genera nuevas células en cada ronda
    void GenerarCelulas()
    {
        // Finalizar la ronda anterior marcando las células restantes como sobrevivientes
        FinalizarRonda();

        // Incrementar el número de ronda
        currentRound++;

        // Reiniciar el tiempo de la ronda
        timeLeftInRound = intervaloTiempo;

        // Filtrar células supervivientes de la ronda anterior
        experienciasSupervivientes.Clear();
        foreach (Celula experiencia in experienciasPrevias)
        {
            if (experiencia.sobrevivio)
            {
                experienciasSupervivientes.Add(experiencia);
            }
        }

        // Combinar experienciasSupervivientes con experienciasSupervivientesPermanentes
        List<Celula> todasLasExperienciasSupervivientes = new List<Celula>(experienciasSupervivientesPermanentes);

        // Destruir las células existentes de la ronda anterior
        foreach (GameObject celula in celulasExistentes)
        {
            if (celula != null)
            {
                Destroy(celula);
            }
        }
        celulasExistentes.Clear();

        // Limpiar experiencias previas
        experienciasPrevias.Clear();

        // Generar nuevas células
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar y posicionar la nueva célula
            GameObject nuevaCelula = Instantiate(celulaPrefab);
            PosicionarCelulaDentroDeCamara(nuevaCelula);

            // Determinar color y tamaño de la célula
            Color colorAjustado;
            float tamañoAjustado;

            if (todasLasExperienciasSupervivientes.Count > 0)
            {
                // Heredar atributos de una célula superviviente de la lista permanente
                Celula padre = todasLasExperienciasSupervivientes[Random.Range(0, todasLasExperienciasSupervivientes.Count)];
                colorAjustado = MutarColor(padre.color);  // Mutar ligeramente el color del padre
                tamañoAjustado = MutarTamaño(padre.tamaño);  // Mutar ligeramente el tamaño del padre
            }
            else
            {
                // Si no hay supervivientes, generar color y tamaño aleatorio
                colorAjustado = GenerarColorAleatorio();
                tamañoAjustado = Random.Range(0.9f, 2.25f);  // Tamaño en el rango deseado
            }

            // Asignar atributos a la célula (color y tamaño)
            AsignarAtributosACelula(nuevaCelula, colorAjustado, tamañoAjustado);

            // Crear una nueva experiencia (propiedades) para la célula
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tamaño = tamañoAjustado,
                sobrevivio = true
            };
            experienciasPrevias.Add(nuevaExperiencia);  // Añadir la nueva experiencia a la lista

            // Asignar la experiencia al script de la célula
            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.cellController = this;
                scriptCelula.experiencia = nuevaExperiencia;
            }

            // Añadir la nueva célula a la lista de células existentes
            celulasExistentes.Add(nuevaCelula);
        }

        // Limpiar la lista de supervivientes para la próxima ronda
        experienciasSupervivientes.Clear();
    }

    // Posiciona la célula dentro de los límites de la cámara
    void PosicionarCelulaDentroDeCamara(GameObject celula)
    {
        Camera camara = Camera.main;
        if (camara == null)
        {
            Debug.LogError("No se encontró la cámara principal.");
            return;
        }

        // Calcular los límites visibles de la cámara
        Vector2 minLimites = camara.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxLimites = camara.ViewportToWorldPoint(new Vector2(1, 1));

        // Posicionar la célula en un lugar aleatorio dentro de los límites
        float x = Random.Range(minLimites.x, maxLimites.x);
        float y = Random.Range(minLimites.y, maxLimites.y);
        celula.transform.position = new Vector2(x, y);
    }

    // Asignar atributos de tamaño y color a una célula
    void AsignarAtributosACelula(GameObject celula, Color color, float tamaño)
    {
        celula.transform.localScale = new Vector3(tamaño, tamaño, 1f);  // Asignar el tamaño
        SpriteRenderer spriteRenderer = celula.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;  // Asignar el color
        }
    }

    // Mutar el tamaño de la célula basada en el tamaño del padre (con un pequeño cambio)
    float MutarTamaño(float tamañoPadre)
    {
        float mutacionTamaño = Random.Range(-0.1f, 0.1f);  // Mutación pequeña
        return Mathf.Clamp(tamañoPadre + mutacionTamaño, 0.9f, 2.5f);  // Asegurarse que el tamaño esté en el rango 0.9-2.5
    }

    // Mutar el color de la célula ligeramente para mantenerlo cercano al color del padre
    Color MutarColor(Color colorPadre)
    {
        // Convertir el color RGB a HSV para mutar sus componentes
        Color.RGBToHSV(colorPadre, out float hue, out float sat, out float val);

        // Mutar ligeramente el tono, la saturación y el brillo
        float nuevoHue = Mathf.Clamp01(hue + Random.Range(-0.05f, 0.05f));  // Variación pequeña en el tono
        float nuevaSat = Mathf.Clamp01(sat + Random.Range(-0.05f, 0.05f));  // Variación pequeña en la saturación
        float nuevaVal = Mathf.Clamp01(val + Random.Range(-0.05f, 0.05f));  // Variación pequeña en el brillo

        // Devolver el nuevo color en formato RGB
        return Color.HSVToRGB(nuevoHue, nuevaSat, nuevaVal);
    }

    // Generar un color completamente aleatorio
    Color GenerarColorAleatorio()
    {
        return Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));  // Tono, saturación y brillo aleatorios
    }

    // Método para capturar una célula y marcarla como destruida
    public void CellCaptured(Cell celula)
    {
        // Marcar la experiencia de la célula como no sobreviviente
        if (celula.experiencia != null)
        {
            celula.experiencia.sobrevivio = false;
        }

        // Eliminar la célula de la lista de células existentes
        celulasExistentes.Remove(celula.gameObject);

        // Incrementar el contador de células destruidas
        totalCellsDestroyed += 1;
    }
}
