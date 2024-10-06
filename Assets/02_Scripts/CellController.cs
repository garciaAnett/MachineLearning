using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CellController : MonoBehaviour
{
    // Prefab de la c�lula a instanciar
    public GameObject celulaPrefab;

    // N�mero de c�lulas a generar por generaci�n
    public int numeroDeCelulas = 10;

    // Intervalo de tiempo entre generaciones (duraci�n de cada ronda)
    public float intervaloTiempo = 10f;

    // Variables para la interfaz de usuario
    public Text timeCounterText;    // Texto para mostrar los segundos restantes
    public Text scoreText;          // Texto para mostrar la cantidad de c�lulas destruidas
    public Text roundText;          // Texto para mostrar la ronda actual

    // Variables privadas
    private List<GameObject> celulasExistentes = new List<GameObject>();
    private int currentRound = 0;
    private float timeLeftInRound;
    private int totalCellsDestroyed = 0;

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
    private List<Celula> experienciasSupervivientes = new List<Celula>();

    void Start()
    {
        // Inicializar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Iniciar la generaci�n de c�lulas en intervalos definidos
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
            scoreText.text = "C�lulas destruidas: " + totalCellsDestroyed.ToString();
        }

        if (roundText != null)
        {
            roundText.text = "Ronda: " + currentRound.ToString();
        }
    }

    void GenerarCelulas()
    {
        // Incrementar el n�mero de ronda
        currentRound++;

        // Reiniciar el tiempo restante de la ronda
        timeLeftInRound = intervaloTiempo;

        // Filtrar las experiencias de las c�lulas que sobrevivieron en la ronda anterior
        experienciasSupervivientes.Clear();
        foreach (Celula experiencia in experienciasPrevias)
        {
            if (experiencia.sobrevivio)
            {
                experienciasSupervivientes.Add(experiencia);
            }
        }

        // Destruir las c�lulas existentes de la ronda anterior
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

        // Generar nuevas c�lulas
        for (int i = 0; i < numeroDeCelulas; i++)
        {
            // Instanciar y posicionar la c�lula
            GameObject nuevaCelula = Instantiate(celulaPrefab);
            PosicionarCelulaDentroDeCamara(nuevaCelula);

            // Determinar atributos de color y tama�o
            Color colorAjustado;
            float tama�oAjustado;

            if (experienciasSupervivientes.Count > 0)
            {
                // Heredar atributos de una c�lula superviviente
                Celula padre = experienciasSupervivientes[Random.Range(0, experienciasSupervivientes.Count)];

                colorAjustado = MutarColor(padre.color);
                tama�oAjustado = MutarTama�o(padre.tama�o);
            }
            else
            {
                // Generar atributos aleatorios si no hay supervivientes
                colorAjustado = GenerarColorAleatorio();
                tama�oAjustado = Random.Range(0.3f, 1f);
            }

            // Asignar atributos a la c�lula
            AsignarAtributosACelula(nuevaCelula, colorAjustado, tama�oAjustado);

            // Crear y asociar la experiencia con la c�lula
            Celula nuevaExperiencia = new Celula
            {
                color = colorAjustado,
                tama�o = tama�oAjustado,
                sobrevivio = true
            };
            experienciasPrevias.Add(nuevaExperiencia);

            Cell scriptCelula = nuevaCelula.GetComponent<Cell>();
            if (scriptCelula != null)
            {
                scriptCelula.cellController = this;
                scriptCelula.experiencia = nuevaExperiencia;
            }

            // A�adir a la lista de c�lulas existentes
            celulasExistentes.Add(nuevaCelula);
        }

        // Limpiar la lista de supervivientes para la pr�xima ronda
        experienciasSupervivientes.Clear();
    }

    void PosicionarCelulaDentroDeCamara(GameObject celula)
    {
        Camera camara = Camera.main;
        if (camara == null)
        {
            Debug.LogError("No se encontr� la c�mara principal.");
            return;
        }

        Vector2 minLimites = camara.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxLimites = camara.ViewportToWorldPoint(new Vector2(1, 1));

        float x = Random.Range(minLimites.x, maxLimites.x);
        float y = Random.Range(minLimites.y, maxLimites.y);
        celula.transform.position = new Vector2(x, y);
    }

    void AsignarAtributosACelula(GameObject celula, Color color, float tama�o)
    {
        celula.transform.localScale = new Vector3(tama�o, tama�o, 1f);
        SpriteRenderer spriteRenderer = celula.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    float MutarTama�o(float tama�oPadre)
    {
        float mutacionTama�o = Random.Range(-0.1f, 0.1f); // Variaci�n entre -10% y +10%
        return Mathf.Clamp(tama�oPadre + mutacionTama�o, 0.3f, 1.5f);
    }

    Color MutarColor(Color colorPadre)
    {
        Color.RGBToHSV(colorPadre, out float hue, out float sat, out float val);
        float nuevaVal = Mathf.Clamp(val + Random.Range(-0.1f, 0.1f), 0f, 1f); // Variaci�n en brillo
        return Color.HSVToRGB(hue, sat, nuevaVal);
    }

    Color GenerarColorAleatorio()
    {
        return Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
    }

    // M�todo para capturar una c�lula
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
