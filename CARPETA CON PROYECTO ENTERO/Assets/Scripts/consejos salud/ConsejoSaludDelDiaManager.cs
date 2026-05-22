using UnityEngine;
using TMPro;

public class ConsejoDinamico : MonoBehaviour
{
    public TMP_Text textoConsejo;

    private string[] consejos =
    {
        "Mantente hidratado y bebe agua a lo largo del día.",
        "Intenta dormir entre 7 y 8 horas cada noche.",
        "Realiza algo de ejercicio físico todos los días, aunque sea caminar.",
        "Incluye frutas y verduras en tu alimentación diaria.",
        "Haz pausas cortas si pasas mucho tiempo estudiando o sentado.",
        "Dedica unos minutos al día a relajarte y cuidar tu salud mental.",
        "Evita el exceso de azúcar y alimentos ultraprocesados.",
        "Mantén una postura correcta al sentarte para prevenir molestias.",
        "Lávate las manos con frecuencia para prevenir infecciones.",
        "Organiza tus horarios para equilibrar estudio, descanso y ocio."
    };

    private int indice = 0;
    private float temporizador = 0f;
    public float tiempoCambio = 5f;

    void Start()
    {
        if (consejos.Length > 0)
        {
            indice = Random.Range(0, consejos.Length);
            textoConsejo.text = consejos[indice];
        }
    }

    void Update()
    {
        temporizador += Time.deltaTime;

        if (temporizador >= tiempoCambio)
        {
            temporizador = 0f;
            CambiarConsejo();
        }
    }

    void CambiarConsejo()
    {
        int nuevoIndice = Random.Range(0, consejos.Length);

        while (nuevoIndice == indice && consejos.Length > 1)
        {
            nuevoIndice = Random.Range(0, consejos.Length);
        }

        indice = nuevoIndice;
        textoConsejo.text = consejos[indice];
    }
}