using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DescansoManager : MonoBehaviour
{
    public Image[] selectores;
    private int indice = 0;

    public TMP_Text textoConsejo;
    public ConsejosSaludManager consejosSaludManager;

    private string[] consejos =
    {
        "Dormir entre 7 y 8 horas favorece la recuperacion física y mental.",
        "Mantener horarios regulares de sueño ayuda a mejorar la calidad del descanso.",
        "Evita el uso de pantallas justo antes de dormir para conciliar mejor el sueño.",
        "Crear un ambiente tranquilo y oscuro en la habitación favorece el descanso.",
        "Hacer pausas durante el día también ayuda a reducir la fatiga acumulada."
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
        Debug.Log("Seleccionado en descanso: " + nombre);

        if (nombre == "ConsejoIzq(3)")
        {
            indiceConsejo--;
            if (indiceConsejo < 0)
                indiceConsejo = consejos.Length - 1;

            MostrarConsejo();
        }
        else if (nombre == "ConsejoDcha(3)")
        {
            indiceConsejo++;
            if (indiceConsejo >= consejos.Length)
                indiceConsejo = 0;

            MostrarConsejo();
        }
        else if (nombre == "BotonAtras(3)")
        {
            consejosSaludManager.VolverAlMenu();
        }
        else if (nombre == "BotonInicio(3)")
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