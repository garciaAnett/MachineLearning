using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CellController : MonoBehaviour
{
    // Prefab de la c�lula a instanciar
    public GameObject celulaPrefab;

    // N�mero de c�lulas a generar por generaci�n
    public int numeroDeCelulas = 5;

    // Intervalo de tiempo entre generaciones (duraci�n de cada ronda)
    public float intervaloTiempo = 20f;

    // Variables para la interfaz de usuario (UI)
    public Text timeCounterText;    // Texto para mostrar los segundos restantes
    public Text scoreText;          // Texto para mostrar la cantidad de c�lulas destruidas
    public Text roundText;          // Texto para mostrar la ronda actual

    // Variables privadas para controlar el estado del juego
    private List<GameObject> celulasExistentes = new List<GameObject>(); // Almacena las c�lulas en la ronda actual
    private int currentRound = 0;      // N�mero de la ronda actual
    private float timeLeftInRound;     // Tiempo restante en la ronda
    private int totalCellsDestroyed = 0;  // Cantidad de c�lulas destruidas en total

    // Clase para representar las propiedades de una c�lula (color, tama�o, si sobrevivi�)
    [System.Serializable]
    public class Celula
    {
        public Color color;      // Color de la c�lula
        public float tama�o;     // Tama�o de la c�lula
        public bool sobrevivio;  // Indica si la c�lula sobrevivi� a la ronda anterior
    }

    // Listas para manejar las experiencias de las c�lulas
    private List<Celula> experienciasPrevias = new List<Celula>();  // Lista de c�lulas de la ronda anterior
    private List<Celula> experienciasSupervivientes = new List<Celula>(); // C�lulas que sobrevivieron a la ronda
    private List<Celula> experienciasSupervivientesPermanentes = new List<Celula>(); // Lista permanente de supervivientes

    // Inicializa las variables y comienza la generaci�n de c�lulas
    void Start()
    {
        // Inicializar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Iniciar la generaci�n de c�lulas cada intervalo de tiempo
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

        // Actualizar el contador de c�lulas destruidas en la UI
        if (scoreText != null)
        {
            scoreText.text = "C�lulas destruidas: " + totalCellsDestroyed.ToString();
        }

        // Actualizar la ronda actual en la UI
        if (roundText != null)
        {
            roundText.text = "Ronda: " + currentRound.ToString();
        }

        // Verificar si el tiempo de la ronda ha terminado
        if (timeLeftInRound <= 0f)
        {
            // Generar nuevas c�lulas al finalizar el tiempo
            GenerarCelulas();
        }
    }

    // M�todo para marcar las c�lulas restantes como sobrevivientes y almacenarlas permanentemente
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
                    // A�adir a la lista permanente si a�n no est� presente
                    if (!experienciasSupervivientesPermanentes.Contains(scriptCelula.experiencia))
                    {
                        experienciasSupervivientesPermanentes.Add(scriptCelula.experiencia);
                    }
                }
            }
        }
    }

    // Genera nuevas c�lulas en cada ronda
    void GenerarCelulas()
    {
        // Finalizar la ronda anterior marcando las c�lulas restantes como sobrevivientes
        FinalizarRonda();

        // Incrementar el n�mero de ronda
        currentRound++;

        // Reiniciar el tiempo de la ronda
        timeLeftInRound = intervaloTiempo;

        // Filtrar c�lulas supervivientes de la ronda anterior
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

        // Destruir las c�lulas existentes de la ronda anterior
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

        // Generar nuevas c�lulas
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar y posicionar la nueva c�lula
            GameObject nuevaCelula = Instantiate(celulaPrefab);
            PosicionarCelulaDentroDeCamara(nuevaCelula);

            // Determinar color y tama�o de la c�lula
            Color colorAjustado;
            float tama�oAjustado;

            if (todasLasExperienciasSupervivientes.Count > 0)
            {
                // Heredar atributos de una c�lula superviviente de la lista permanente
                Celula padre = todasLasExperienciasSupervivientes[Random.Range(0, todasLasExperienciasSupervivientes.Count)];
                colorAjustado = MutarColor(padre.color);  // Mutar ligeramente el color del padre
                tama�oAjustado = MutarTama�o(padre.tama�o);  // Mutar ligeramente el tama�o del padre
            }
            else
            {
                // Si no hay supervivientes, generar color y tama�o aleatorio
                colorAjustado = GenerarColorAleatorio();
                tama�oAjustado = Random.Range(0.9f, 2.25f);  // Tama�o en el rango deseado
            }

            // Asignar atributos a la c�lula (color y tama�o)
            AsignarAtributosACelula(nuevaCelula, colorAjustado, tama�oAjustado);

            // Crear una nueva experiencia (propiedades) para la c�lula
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tama�o = tama�oAjustado,
                sobrevivio = true
            };
            experienciasPrevias.Add(nuevaExperiencia);  // A�adir la nueva experiencia a la lista

            // Asignar la experiencia al script de la c�lula
            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.cellController = this;
                scriptCelula.experiencia = nuevaExperiencia;
            }

            // A�adir la nueva c�lula a la lista de c�lulas existentes
            celulasExistentes.Add(nuevaCelula);
        }

        // Limpiar la lista de supervivientes para la pr�xima ronda
        experienciasSupervivientes.Clear();
    }

    // Posiciona la c�lula dentro de los l�mites de la c�mara
    void PosicionarCelulaDentroDeCamara(GameObject celula)
    {
        Camera camara = Camera.main;
        if (camara == null)
        {
            Debug.LogError("No se encontr� la c�mara principal.");
            return;
        }

        // Calcular los l�mites visibles de la c�mara
        Vector2 minLimites = camara.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxLimites = camara.ViewportToWorldPoint(new Vector2(1, 1));

        // Posicionar la c�lula en un lugar aleatorio dentro de los l�mites
        float x = Random.Range(minLimites.x, maxLimites.x);
        float y = Random.Range(minLimites.y, maxLimites.y);
        celula.transform.position = new Vector2(x, y);
    }

    // Asignar atributos de tama�o y color a una c�lula
    void AsignarAtributosACelula(GameObject celula, Color color, float tama�o)
    {
        celula.transform.localScale = new Vector3(tama�o, tama�o, 1f);  // Asignar el tama�o
        SpriteRenderer spriteRenderer = celula.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;  // Asignar el color
        }
    }

    // Mutar el tama�o de la c�lula basada en el tama�o del padre (con un peque�o cambio)
    float MutarTama�o(float tama�oPadre)
    {
        float mutacionTama�o = Random.Range(-0.1f, 0.1f);  // Mutaci�n peque�a
        return Mathf.Clamp(tama�oPadre + mutacionTama�o, 0.9f, 2.5f);  // Asegurarse que el tama�o est� en el rango 0.9-2.5
    }

    // Mutar el color de la c�lula ligeramente para mantenerlo cercano al color del padre
    Color MutarColor(Color colorPadre)
    {
        // Convertir el color RGB a HSV para mutar sus componentes
        Color.RGBToHSV(colorPadre, out float hue, out float sat, out float val);

        // Mutar ligeramente el tono, la saturaci�n y el brillo
        float nuevoHue = Mathf.Clamp01(hue + Random.Range(-0.05f, 0.05f));  // Variaci�n peque�a en el tono
        float nuevaSat = Mathf.Clamp01(sat + Random.Range(-0.05f, 0.05f));  // Variaci�n peque�a en la saturaci�n
        float nuevaVal = Mathf.Clamp01(val + Random.Range(-0.05f, 0.05f));  // Variaci�n peque�a en el brillo

        // Devolver el nuevo color en formato RGB
        return Color.HSVToRGB(nuevoHue, nuevaSat, nuevaVal);
    }

    // Generar un color completamente aleatorio
    Color GenerarColorAleatorio()
    {
        return Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));  // Tono, saturaci�n y brillo aleatorios
    }

    // M�todo para capturar una c�lula y marcarla como destruida
    public void CellCaptured(Cell celula)
    {
        // Marcar la experiencia de la c�lula como no sobreviviente
        if (celula.experiencia != null)
        {
            celula.experiencia.sobrevivio = false;
        }

        // Eliminar la c�lula de la lista de c�lulas existentes
        celulasExistentes.Remove(celula.gameObject);

        // Incrementar el contador de c�lulas destruidas
        totalCellsDestroyed += 1;
    }
}
