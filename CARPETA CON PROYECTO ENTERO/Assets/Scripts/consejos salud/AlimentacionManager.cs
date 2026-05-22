using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AlimentacionManager : MonoBehaviour
{
    public Image[] selectores;
    private int indice = 0;

    public TMP_Text textoConsejo;
    public ConsejosSaludManager consejosSaludManager;

    private string[] consejos =
    {
        "Prioriza los alimentos integrales. Arroz integral, quinoa y pan integral son excelentes fuentes de energía sostenida.",
        "Incluye frutas y verduras en tus comidas diarias para obtener vitaminas y minerales.",
        "Evita el exceso de azúcar y de alimentos ultraprocesados en tu dieta.",
        "Mantén horarios regulares de comida para favorecer una buena alimentación.",
        "Bebe suficiente agua durante el día para mantenerte hidratado."
    };

    private int indiceConsejo = 0;

    void OnEnable()
    {
        indice = 0;
        indiceConsejo = 0;
        MostrarConsejo();
        Actualizar();
    }

    public void ProcesarEMG(int senal)
    {
        if (senal == 1)
        {
            Derecha();
        }
        else if (senal == 2)
        {
            Izquierda();
        }
        else if (senal == 3)
        {
            Seleccionar();
        }
    }

    public void Derecha()
    {
        indice++;
        if (indice >= selectores.Length)
            indice = 0;

        Actualizar();
    }

    public void Izquierda()
    {
        indice--;
        if (indice < 0)
            indice = selectores.Length - 1;

        Actualizar();
    }

    public void Seleccionar()
    {
        string nombre = selectores[indice].name;
        Debug.Log("Seleccionado en alimentacion: " + nombre);

        if (nombre == "ConsejoIzq(1)")
        {
            indiceConsejo--;
            if (indiceConsejo < 0)
                indiceConsejo = consejos.Length - 1;

            MostrarConsejo();
        }
        else if (nombre == "ConsejoDcha(1)")
        {
            indiceConsejo++;
            if (indiceConsejo >= consejos.Length)
                indiceConsejo = 0;

            MostrarConsejo();
        }
        else if (nombre == "BotonAtras(1)")
        {
            consejosSaludManager.VolverAlMenu();
        }
        else if (nombre == "BotonInicio(1)")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
    }

    void MostrarConsejo()
    {
        textoConsejo.text = consejos[indiceConsejo];
    }

    void Actualizar()
    {
        for (int i = 0; i < selectores.Length; i++)
        {
            Color c = selectores[i].color;
            c.a = (i == indice) ? 1f : 0f;
            selectores[i].color = c;
        }
    }
}