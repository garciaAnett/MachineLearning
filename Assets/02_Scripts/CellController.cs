using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CellController : MonoBehaviour
{
    // Prefab de la célula a instanciar
    public GameObject celulaPrefab;

    // Número de células a generar por generación
    public int numeroDeCelulas = 10;

    // Intervalo de tiempo entre generaciones (duración de cada ronda)
    public float intervaloTiempo = 10f;

    // Variables para la interfaz de usuario
    public Text timeCounterText;    // Texto para mostrar los segundos restantes
    public Text scoreText;          // Texto para mostrar la cantidad de células destruidas
    public Text roundText;          // Texto para mostrar la ronda actual

    // Variables privadas
    private List<GameObject> celulasExistentes = new List<GameObject>();
    private int currentRound = 0;
    private float timeLeftInRound;
    private int totalCellsDestroyed = 0;

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
    private List<Celula> experienciasSupervivientes = new List<Celula>();

    void Start()
    {
        // Inicializar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Iniciar la generación de células en intervalos definidos
        InvokeRepeating(nameof(GenerarCelulas), 0f, intervaloTiempo);
    }

    void Update()
    {
        // Actualizar el tiempo restante de la ronda
        timeLeftInRound -= Time.deltaTime;
        timeLeftInRound = Mathf.Max(0f, timeLeftInRound);

        // Actualizar la interfaz de usuario
        if (timeCounterText != null)
        {
            timeCounterText.text = "Tiempo: " + Mathf.CeilToInt(timeLeftInRound).ToString() + " s";
        }

        if (scoreText != null)
        {
            scoreText.text = "Células destruidas: " + totalCellsDestroyed.ToString();
        }

        if (roundText != null)
        {
            roundText.text = "Ronda: " + currentRound.ToString();
        }
    }

    void GenerarCelulas()
    {
        // Incrementar el número de ronda
        currentRound++;

        // Reiniciar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Filtrar las experiencias de las células que sobrevivieron en la ronda anterior
        experienciasSupervivientes.Clear();
        foreach (Celula experiencia in experienciasPrevias)
        {
            if (experiencia.sobrevivio)
            {
                experienciasSupervivientes.Add(experiencia);
            }
        }

        // Destruir las células existentes de la ronda anterior
        foreach (GameObject celula in celulasExistentes)
        {
            if (celula != null)
            {
                Destroy(celula);
            }
        }
        celulasExistentes.Clear();

        // Limpiar experiencias previas para olvidar herencias anteriores
        experienciasPrevias.Clear();

        // Generar nuevas células
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar y posicionar la célula
            GameObject nuevaCelula = Instantiate(celulaPrefab);
            PosicionarCelulaDentroDeCamara(nuevaCelula);

            // Determinar atributos de color y tamaño
            Color colorAjustado;
            float tamañoAjustado;

            if (experienciasSupervivientes.Count > 0)
            {
                // Heredar atributos de una célula superviviente
                Celula padre = experienciasSupervivientes[Random.Range(0, experienciasSupervivientes.Count)];

                colorAjustado = MutarColor(padre.color);
                tamañoAjustado = MutarTamaño(padre.tamaño);
            }
            else
            {
                // Generar atributos aleatorios si no hay supervivientes
                colorAjustado = GenerarColorAleatorio();
                tamañoAjustado = Random.Range(0.3f, 1f);
            }

            // Asignar atributos a la célula
            AsignarAtributosACelula(nuevaCelula, colorAjustado, tamañoAjustado);

            // Crear y asociar la experiencia con la célula
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tamaño = tamañoAjustado,
                sobrevivio = true
            };
            experienciasPrevias.Add(nuevaExperiencia);

            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.cellController = this;
                scriptCelula.experiencia = nuevaExperiencia;
            }

            // Añadir a la lista de células existentes
            celulasExistentes.Add(nuevaCelula);
        }

        // Limpiar la lista de supervivientes para la próxima ronda
        experienciasSupervivientes.Clear();
    }

    void PosicionarCelulaDentroDeCamara(GameObject celula)
    {
        Camera camara = Camera.main;
        if (camara == null)
        {
            Debug.LogError("No se encontró la cámara principal.");
            return;
        }

        Vector2 minLimites = camara.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxLimites = camara.ViewportToWorldPoint(new Vector2(1, 1));

        float x = Random.Range(minLimites.x, maxLimites.x);
        float y = Random.Range(minLimites.y, maxLimites.y);
        celula.transform.position = new Vector2(x, y);
    }

    void AsignarAtributosACelula(GameObject celula, Color color, float tamaño)
    {
        celula.transform.localScale = new Vector3(tamaño, tamaño, 1f);
        SpriteRenderer spriteRenderer = celula.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    float MutarTamaño(float tamañoPadre)
    {
        float mutacionTamaño = Random.Range(-0.1f, 0.1f); // Variación entre -10% y +10%
        return Mathf.Clamp(tamañoPadre + mutacionTamaño, 0.3f, 1.5f);
    }

    Color MutarColor(Color colorPadre)
    {
        Color.RGBToHSV(colorPadre, out float hue, out float sat, out float val);
        float nuevaVal = Mathf.Clamp(val + Random.Range(-0.1f, 0.1f), 0f, 1f); // Variación en brillo
        return Color.HSVToRGB(hue, sat, nuevaVal);
    }

    Color GenerarColorAleatorio()
    {
        return Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
    }

    // Método para capturar una célula
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
