using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EjercicioManager : MonoBehaviour
{
    public Image[] selectores;
    private int indice = 0;

    public TMP_Text textoConsejo;
    public ConsejosSaludManager consejosSaludManager;

    private string[] consejos =
    {
        "Realiza actividad fisica de forma regular para mejorar tu salud cardiovascular.",
        "Caminar al menos 30 minutos al día puede aportar grandes beneficios.",
        "Haz estiramientos suaves antes y despues del ejercicio para evitar lesiones.",
        "Mantener una rutina de ejercicio ayuda a reducir el estrés y mejorar el estado de ánimo.",
        "Elige una actividad física que disfrutes para mantener la constancia."
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
        Debug.Log("Seleccionado en ejercicio: " + nombre);

        if (nombre == "ConsejoIzq(2)")
        {
            indiceConsejo--;
            if (indiceConsejo < 0)
                indiceConsejo = consejos.Length - 1;

            MostrarConsejo();
        }
        else if (nombre == "ConsejoDcha(2)")
        {
            indiceConsejo++;
            if (indiceConsejo >= consejos.Length)
                indiceConsejo = 0;

            MostrarConsejo();
        }
        else if (nombre == "BotonAtras(2)")
        {
            consejosSaludManager.VolverAlMenu();
        }
        else if (nombre == "BotonInicio(2)")
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