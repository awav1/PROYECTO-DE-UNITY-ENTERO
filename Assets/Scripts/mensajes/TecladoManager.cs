using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TecladoManager : MonoBehaviour
{
    public GameObject pantallaTeclado;
    public GameObject tecladoPrincipal;
    public GameObject tecladoLetras;

    public Image[] selectoresPrincipal;
    public Image[] selectoresLetras;

    public Image selectorCambioModo;

    public TMP_Text textoLetra1;
    public TMP_Text textoLetra2;
    public TMP_Text textoLetra3;

    private TMP_Text textoDestino;
    private string mensajeActual = "";

    private Image[] selectoresActuales;
    private int indice = 0;

    private string letra1 = "A";
    private string letra2 = "B";
    private string letra3 = "C";

    private bool enTecladoPrincipal = false;
    private bool primeraLetra = true;
    private bool mensajeEnviado = false;
    private bool salirModoTeclado = false;

    void Start()
    {
        CerrarTeclado();
    }

    public void AsignarTextoDestino(TMP_Text nuevoTexto)
    {
        textoDestino = nuevoTexto;
        mensajeActual = "";
        primeraLetra = true;

        if (textoDestino != null)
            textoDestino.text = "";
    }

    public void ActivarTeclado()
    {
        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(true);

        MostrarTecladoPrincipal();
    }

    public void CerrarTeclado()
    {
        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);

        if (tecladoPrincipal != null)
            tecladoPrincipal.SetActive(false);

        if (tecladoLetras != null)
            tecladoLetras.SetActive(false);

        enTecladoPrincipal = false;
    }

    public void ProcesarEMG(int senal)
    {
        if ((tecladoPrincipal == null || !tecladoPrincipal.activeSelf) &&
            (tecladoLetras == null || !tecladoLetras.activeSelf))
            return;

        if (senal == 1) Derecha();
        else if (senal == 2) Izquierda();
        else if (senal == 3) Seleccionar();
    }

    void MostrarTecladoPrincipal()
    {
        enTecladoPrincipal = true;

        tecladoPrincipal.SetActive(true);
        tecladoLetras.SetActive(false);

        selectoresActuales = selectoresPrincipal;
        indice = 0;

        Actualizar();
    }

    void MostrarTecladoLetras()
    {
        enTecladoPrincipal = false;

        tecladoPrincipal.SetActive(false);
        tecladoLetras.SetActive(true);

        selectoresActuales = selectoresLetras;
        indice = 0;

        Actualizar();
    }

    int TotalSelectoresActuales()
    {
        int total = 0;

        if (selectoresActuales != null)
            total += selectoresActuales.Length;

        if (selectorCambioModo != null)
            total += 1;

        return total;
    }

    void Derecha()
    {
        indice++;

        if (indice >= TotalSelectoresActuales())
            indice = 0;

        Actualizar();
    }

    void Izquierda()
    {
        indice--;

        if (indice < 0)
            indice = TotalSelectoresActuales() - 1;

        Actualizar();
    }

    void Seleccionar()
    {
        int cantidadBase = selectoresActuales.Length;

        if (selectorCambioModo != null && indice == cantidadBase)
        {
            SalirModoTeclado();
            return;
        }

        if (enTecladoPrincipal)
            SeleccionarPrincipal();
        else
            SeleccionarLetras();
    }

    void SeleccionarPrincipal()
    {
        if (indice == 0) MostrarGrupo("A", "B", "C");
        else if (indice == 1) MostrarGrupo("D", "E", "F");
        else if (indice == 2) MostrarGrupo("G", "H", "I");
        else if (indice == 3) MostrarGrupo("J", "K", "L");
        else if (indice == 4) MostrarGrupo("M", "N", "O");
        else if (indice == 5) MostrarGrupo("P", "Q", "R");
        else if (indice == 6) MostrarGrupo("S", "T", "U");
        else if (indice == 7) MostrarGrupo("V", "W", "X");
        else if (indice == 8) MostrarGrupo("Y", "Z", "Ñ");
        else if (indice == 9) Borrar();
        else if (indice == 10) Escribir(" ");
        else if (indice == 11) Enviar();
    }

    void SeleccionarLetras()
    {
        if (indice == 0) MostrarTecladoPrincipal();
        else if (indice == 1) Escribir(letra1);
        else if (indice == 2) Escribir(letra2);
        else if (indice == 3) Escribir(letra3);
        else if (indice == 4) Borrar();
        else if (indice == 5) Escribir(" ");
        else if (indice == 6) Enviar();
    }

    void MostrarGrupo(string l1, string l2, string l3)
    {
        letra1 = l1;
        letra2 = l2;
        letra3 = l3;

        if (textoLetra1 != null) textoLetra1.text = l1;
        if (textoLetra2 != null) textoLetra2.text = l2;
        if (textoLetra3 != null) textoLetra3.text = l3;

        MostrarTecladoLetras();
    }

    void Escribir(string texto)
    {
        if (texto == " ")
        {
            mensajeActual += texto;
        }
        else if (primeraLetra)
        {
            mensajeActual += texto.ToUpper();
            primeraLetra = false;
        }
        else
        {
            mensajeActual += texto.ToLower();
        }

        if (textoDestino != null)
            textoDestino.text = mensajeActual;
    }

    void Borrar()
    {
        if (mensajeActual.Length > 0)
        {
            mensajeActual = mensajeActual.Substring(0, mensajeActual.Length - 1);

            if (textoDestino != null)
                textoDestino.text = mensajeActual;

            if (mensajeActual.Length == 0)
                primeraLetra = true;
        }
    }

    void Enviar()
    {
        mensajeEnviado = true;
        primeraLetra = true;
        CerrarTeclado();
    }

    void SalirModoTeclado()
    {
        salirModoTeclado = true;
        CerrarTeclado();
    }

    public string ObtenerMensaje()
    {
        return mensajeActual;
    }

    public void LimpiarMensaje()
    {
        mensajeActual = "";
        primeraLetra = true;

        if (textoDestino != null)
            textoDestino.text = "";
    }

    public bool ConsumirEnvio()
    {
        if (mensajeEnviado)
        {
            mensajeEnviado = false;
            return true;
        }

        return false;
    }

    public bool ConsumirSalirModoTeclado()
    {
        if (salirModoTeclado)
        {
            salirModoTeclado = false;
            return true;
        }

        return false;
    }

    void Actualizar()
    {
        foreach (var s in selectoresPrincipal)
        {
            if (s == null) continue;

            Color c = s.color;
            c.a = 0f;
            s.color = c;
        }

        foreach (var s in selectoresLetras)
        {
            if (s == null) continue;

            Color c = s.color;
            c.a = 0f;
            s.color = c;
        }

        if (selectorCambioModo != null)
        {
            Color c = selectorCambioModo.color;
            c.a = 0f;
            selectorCambioModo.color = c;
        }

        if (selectoresActuales != null && indice < selectoresActuales.Length)
        {
            Color seleccionado = selectoresActuales[indice].color;
            seleccionado.a = 1f;
            selectoresActuales[indice].color = seleccionado;
        }
        else if (selectorCambioModo != null && indice == selectoresActuales.Length)
        {
            Color seleccionado = selectorCambioModo.color;
            seleccionado.a = 1f;
            selectorCambioModo.color = seleccionado;
        }
    }
}