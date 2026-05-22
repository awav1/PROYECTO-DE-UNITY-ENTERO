using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaludMentalManager : MonoBehaviour
{
    public Image[] selectores;
    private int indice = 0;

    public TMP_Text textoConsejo;
    public ConsejosSaludManager consejosSaludManager;

    private string[] consejos =
    {
        "Dedica unos minutos al día a respirar profundamente y relajarte.",
        "Hablar con alguien de confianza puede ayudarte a gestionar mejor el estrés.",
        "Mantener una rutina diaria puede aportar sensacion de orden y tranquilidad.",
        "Reservar tiempo para actividades que disfrutas favorece tu bienestar emocional.",
        "Descansar y pedir ayuda cuando la necesitas también forma parte del autocuidado."
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
        Debug.Log("Seleccionado en salud mental: " + nombre);

        if (nombre == "ConsejoIzq(4)")
        {
            indiceConsejo--;
            if (indiceConsejo < 0)
                indiceConsejo = consejos.Length - 1;

            MostrarConsejo();
        }
        else if (nombre == "ConsejoDcha(4)")
        {
            indiceConsejo++;
            if (indiceConsejo >= consejos.Length)
                indiceConsejo = 0;

            MostrarConsejo();
        }
        else if (nombre == "BotonAtras(4)")
        {
            consejosSaludManager.VolverAlMenu();
        }
        else if (nombre == "BotonInicio(4)")
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
