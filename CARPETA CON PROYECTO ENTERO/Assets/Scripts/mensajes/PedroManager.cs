using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PedroManager : MonoBehaviour
{
    public Image[] selectores;
    public Image selectorCambioModo;

    private Image[] selectoresCompletos;
    private Image[] selectoresFinales;
    private int indice = 0;

    public MensajesManager mensajesManager;
    public TecladoManager tecladoManager;

    public TMP_Text textoOpcion1;
    public TMP_Text textoOpcion2;
    public TMP_Text textoOpcion3;

    public GameObject recibido1;
    public GameObject respuesta1;
    public GameObject recibido2;
    public GameObject respuesta2;
    public GameObject recibido3;

    public TMP_Text texto1Recibido;
    public TMP_Text texto1Respuesta;
    public TMP_Text texto2Recibido;
    public TMP_Text texto2Respuesta;
    public TMP_Text texto3Recibido;

    public TMP_Text hora1Recibido;
    public TMP_Text hora1Respuesta;
    public TMP_Text hora2Recibido;
    public TMP_Text hora2Respuesta;
    public TMP_Text hora3Recibido;

    private int nivelActual = 1;
    private int opcionNivel1Elegida = 0;
    private int minutosExtra = 0;
    private bool conversacionIniciada = false;
    private bool esperandoRespuesta = false;
    private bool modoTeclado = false;

    private readonly float esperaPedroSegundos = 2f;

    void OnEnable()
    {
        modoTeclado = false;

        if (tecladoManager != null)
            tecladoManager.CerrarTeclado();

        if (textoOpcion1 != null && textoOpcion1.text == "")
            OcultarCambioModo();
        else
            MostrarCambioModo();

        if (selectores != null && selectores.Length >= 5)
        {
            selectoresCompletos = new Image[]
            {
                selectores[0],
                selectores[1],
                selectores[2],
                selectores[3],
                selectores[4],
                selectorCambioModo
            };

            selectoresFinales = new Image[]
            {
                selectores[3],
                selectores[4]
            };
        }

        if (!conversacionIniciada)
        {
            ReiniciarConversacion();
            conversacionIniciada = true;
        }
        else
        {
            Actualizar();
        }
    }

    public void ProcesarEMG(int senal)
    {
        if (esperandoRespuesta)
            return;

        if (modoTeclado)
        {
            if (tecladoManager == null)
                return;

            tecladoManager.ProcesarEMG(senal);

            if (tecladoManager.ConsumirSalirModoTeclado())
            {
                if (nivelActual == 1)
                {
                    if (texto1Respuesta != null) texto1Respuesta.text = "";
                    if (respuesta1 != null) respuesta1.SetActive(false);
                }
                else if (nivelActual == 2)
                {
                    if (texto2Respuesta != null) texto2Respuesta.text = "";
                    if (respuesta2 != null) respuesta2.SetActive(false);
                }

                CerrarModoTeclado();
                Actualizar();
                return;
            }

            if (tecladoManager.ConsumirEnvio())
            {
                EnviarMensajeLibre();
                return;
            }

            return;
        }

        if (senal == 1)
            Derecha();
        else if (senal == 2)
            Izquierda();
        else if (senal == 3)
            Seleccionar();
    }

    public void Derecha()
    {
        if (selectores == null || selectores.Length == 0)
            return;

        indice++;
        if (indice >= selectores.Length)
            indice = 0;

        Actualizar();
    }

    public void Izquierda()
    {
        if (selectores == null || selectores.Length == 0)
            return;

        indice--;
        if (indice < 0)
            indice = selectores.Length - 1;

        Actualizar();
    }

    public void Seleccionar()
    {
        if (selectores == null || selectores.Length == 0)
            return;

        if (indice < 0 || indice >= selectores.Length)
        {
            indice = 0;
            Actualizar();
            return;
        }

        string nombre = selectores[indice].name;

        if (nombre == "1OpcionSelector(7)")
        {
            ElegirOpcion(1);
        }
        else if (nombre == "2OpcionSelector(7)")
        {
            ElegirOpcion(2);
        }
        else if (nombre == "3OpcionSelector(7)")
        {
            ElegirOpcion(3);
        }
        else if (nombre == "BotonAtras(7)")
        {
            CerrarModoTeclado();
            mensajesManager.VolverAlMenu();
        }
        else if (nombre == "BotonInicio(7)")
        {
            CerrarModoTeclado();
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (nombre == "SelectorCambioModo")
        {
            ActivarModoTeclado();
        }
    }

    void ActivarModoTeclado()
    {
        if (tecladoManager == null)
            return;

        modoTeclado = true;

        if (nivelActual == 1)
        {
            if (respuesta1 != null) respuesta1.SetActive(true);
            if (texto1Respuesta != null) texto1Respuesta.text = "";

            tecladoManager.AsignarTextoDestino(texto1Respuesta);
        }
        else if (nivelActual == 2)
        {
            if (respuesta2 != null) respuesta2.SetActive(true);
            if (texto2Respuesta != null) texto2Respuesta.text = "";

            tecladoManager.AsignarTextoDestino(texto2Respuesta);
        }

        tecladoManager.ActivarTeclado();
    }

    void CerrarModoTeclado()
    {
        modoTeclado = false;

        if (tecladoManager != null)
            tecladoManager.CerrarTeclado();
    }

    void EnviarMensajeLibre()
    {
        string mensajeUsuario = tecladoManager.ObtenerMensaje();

        CerrarModoTeclado();

        if (string.IsNullOrWhiteSpace(mensajeUsuario))
        {
            if (nivelActual == 1)
            {
                if (respuesta1 != null) respuesta1.SetActive(false);
                if (texto1Respuesta != null) texto1Respuesta.text = "";
            }
            else if (nivelActual == 2)
            {
                if (respuesta2 != null) respuesta2.SetActive(false);
                if (texto2Respuesta != null) texto2Respuesta.text = "";
            }

            Actualizar();
            return;
        }

        int opcionDetectada = DetectarOpcionPorTexto(mensajeUsuario);

        if (opcionDetectada != 0)
        {
            ElegirOpcion(opcionDetectada);
        }
        else
        {
            StartCoroutine(MostrarRespuestaNoDisponible(mensajeUsuario));
        }
    }

    int DetectarOpcionPorTexto(string mensajeUsuario)
    {
        string mensaje = NormalizarMensajeUsuario(mensajeUsuario);

        if (nivelActual == 1)
        {
            if (mensaje == NormalizarPredefinido("¿Sigues en la notaría?")) return 1;
            if (mensaje == NormalizarPredefinido("¿Ha sido un día largo?")) return 2;
            if (mensaje == NormalizarPredefinido("¿Te queda mucho?")) return 3;
        }
        else if (nivelActual == 2)
        {
            if (opcionNivel1Elegida == 1)
            {
                if (mensaje == NormalizarPredefinido("¿Vuelves pronto?")) return 1;
                if (mensaje == NormalizarPredefinido("Vale, avísame")) return 2;
                if (mensaje == NormalizarPredefinido("No trabajes mucho")) return 3;
            }
            else if (opcionNivel1Elegida == 2)
            {
                if (mensaje == NormalizarPredefinido("¿Muchos clientes?")) return 1;
                if (mensaje == NormalizarPredefinido("Entonces descansa luego")) return 2;
                if (mensaje == NormalizarPredefinido("Ánimo")) return 3;
            }
            else if (opcionNivel1Elegida == 3)
            {
                if (mensaje == NormalizarPredefinido("Entonces bien")) return 1;
                if (mensaje == NormalizarPredefinido("Luego me escribes")) return 2;
                if (mensaje == NormalizarPredefinido("Te espero")) return 3;
            }
        }

        return 0;
    }

    string NormalizarMensajeUsuario(string texto)
    {
        texto = texto.ToLower();
        texto = texto.Replace(" ", "");
        return texto;
    }

    string NormalizarPredefinido(string texto)
    {
        texto = texto.ToLower();

        texto = texto.Replace("á", "a");
        texto = texto.Replace("é", "e");
        texto = texto.Replace("í", "i");
        texto = texto.Replace("ó", "o");
        texto = texto.Replace("ú", "u");
        texto = texto.Replace("ñ", "n");

        texto = texto.Replace(" ", "");
        texto = texto.Replace(".", "");
        texto = texto.Replace(",", "");
        texto = texto.Replace("¿", "");
        texto = texto.Replace("?", "");
        texto = texto.Replace("¡", "");
        texto = texto.Replace("!", "");

        return texto;
    }

    IEnumerator MostrarRespuestaNoDisponible(string mensajeUsuario)
    {
        esperandoRespuesta = true;

        string mensajePedro = "Respuesta no disponible";

        if (nivelActual == 1)
        {
            if (texto1Respuesta != null) texto1Respuesta.text = mensajeUsuario;
            if (respuesta1 != null) respuesta1.SetActive(true);

            minutosExtra++;
            PonerHora(hora1Respuesta, minutosExtra);

            yield return new WaitForSeconds(esperaPedroSegundos);

            if (texto2Recibido != null) texto2Recibido.text = mensajePedro;
            if (recibido2 != null) recibido2.SetActive(true);

            minutosExtra++;
            PonerHora(hora2Recibido, minutosExtra);

            mensajesManager.ActualizarChat(6, mensajePedro, hora2Recibido.text, false);

            OcultarOpcionesYDejarSoloBotones();
        }
        else if (nivelActual == 2)
        {
            if (texto2Respuesta != null) texto2Respuesta.text = mensajeUsuario;
            if (respuesta2 != null) respuesta2.SetActive(true);

            minutosExtra++;
            PonerHora(hora2Respuesta, minutosExtra);

            yield return new WaitForSeconds(esperaPedroSegundos);

            if (texto3Recibido != null) texto3Recibido.text = mensajePedro;
            if (recibido3 != null) recibido3.SetActive(true);

            minutosExtra++;
            PonerHora(hora3Recibido, minutosExtra);

            mensajesManager.ActualizarChat(6, mensajePedro, hora3Recibido.text, false);

            OcultarOpcionesYDejarSoloBotones();
        }

        indice = 0;
        Actualizar();

        esperandoRespuesta = false;
    }

    void ElegirOpcion(int opcion)
    {
        if (esperandoRespuesta)
            return;

        StartCoroutine(MostrarSecuencia(opcion));
    }

    IEnumerator MostrarSecuencia(int opcion)
    {
        esperandoRespuesta = true;

        if (nivelActual == 1)
        {
            opcionNivel1Elegida = opcion;

            string mensajeUsuario = "";
            string mensajePedro = "";

            if (opcion == 1)
            {
                mensajeUsuario = "¿Sigues en la notaría?";
                mensajePedro = "Sí, salí un momento por unos papeles.";
            }
            else if (opcion == 2)
            {
                mensajeUsuario = "¿Ha sido un día largo?";
                mensajePedro = "Sí, hoy hemos tenido bastante trabajo.";
            }
            else if (opcion == 3)
            {
                mensajeUsuario = "¿Te queda mucho?";
                mensajePedro = "No, ya me queda poco.";
            }

            if (texto1Respuesta != null) texto1Respuesta.text = mensajeUsuario;
            if (respuesta1 != null) respuesta1.SetActive(true);

            minutosExtra++;
            PonerHora(hora1Respuesta, minutosExtra);

            yield return new WaitForSeconds(esperaPedroSegundos);

            if (texto2Recibido != null) texto2Recibido.text = mensajePedro;
            if (recibido2 != null) recibido2.SetActive(true);

            minutosExtra++;
            PonerHora(hora2Recibido, minutosExtra);

            mensajesManager.ActualizarChat(6, mensajePedro, hora2Recibido.text, false);

            nivelActual = 2;
            MostrarOpcionesNivel2();
            indice = 0;
            Actualizar();
        }
        else if (nivelActual == 2)
        {
            string mensajeUsuario = "";
            string mensajePedro = "";

            if (opcionNivel1Elegida == 1)
            {
                if (opcion == 1)
                {
                    mensajeUsuario = "¿Vuelves pronto?";
                    mensajePedro = "Sí, en un rato.";
                }
                else if (opcion == 2)
                {
                    mensajeUsuario = "Vale, avísame";
                    mensajePedro = "Claro.";
                }
                else
                {
                    mensajeUsuario = "No trabajes mucho";
                    mensajePedro = "Lo intento.";
                }
            }
            else if (opcionNivel1Elegida == 2)
            {
                if (opcion == 1)
                {
                    mensajeUsuario = "¿Muchos clientes?";
                    mensajePedro = "Sí, bastantes.";
                }
                else if (opcion == 2)
                {
                    mensajeUsuario = "Entonces descansa luego";
                    mensajePedro = "Sí, eso haré.";
                }
                else
                {
                    mensajeUsuario = "Ánimo";
                    mensajePedro = "Gracias.";
                }
            }
            else if (opcionNivel1Elegida == 3)
            {
                if (opcion == 1)
                {
                    mensajeUsuario = "Entonces bien";
                    mensajePedro = "Sí.";
                }
                else if (opcion == 2)
                {
                    mensajeUsuario = "Luego me escribes";
                    mensajePedro = "Vale.";
                }
                else
                {
                    mensajeUsuario = "Te espero";
                    mensajePedro = "Perfecto.";
                }
            }

            if (texto2Respuesta != null) texto2Respuesta.text = mensajeUsuario;
            if (respuesta2 != null) respuesta2.SetActive(true);

            minutosExtra++;
            PonerHora(hora2Respuesta, minutosExtra);

            yield return new WaitForSeconds(esperaPedroSegundos);

            if (texto3Recibido != null) texto3Recibido.text = mensajePedro;
            if (recibido3 != null) recibido3.SetActive(true);

            minutosExtra++;
            PonerHora(hora3Recibido, minutosExtra);

            mensajesManager.ActualizarChat(6, mensajePedro, hora3Recibido.text, false);

            OcultarOpcionesYDejarSoloBotones();
        }

        esperandoRespuesta = false;
    }

    void ReiniciarConversacion()
    {
        indice = 0;
        nivelActual = 1;
        opcionNivel1Elegida = 0;
        minutosExtra = 0;
        esperandoRespuesta = false;
        modoTeclado = false;

        MostrarCambioModo();

        if (tecladoManager != null)
            tecladoManager.CerrarTeclado();

        if (selectoresCompletos != null && selectoresCompletos.Length > 0)
            selectores = selectoresCompletos;

        if (recibido1 != null) recibido1.SetActive(true);
        if (respuesta1 != null) respuesta1.SetActive(false);
        if (recibido2 != null) recibido2.SetActive(false);
        if (respuesta2 != null) respuesta2.SetActive(false);
        if (recibido3 != null) recibido3.SetActive(false);

        if (texto1Recibido != null) texto1Recibido.text = "Estoy fuera";
        if (hora1Recibido != null) hora1Recibido.text = "16:10";

        if (texto1Respuesta != null) texto1Respuesta.text = "";
        if (texto2Recibido != null) texto2Recibido.text = "";
        if (texto2Respuesta != null) texto2Respuesta.text = "";
        if (texto3Recibido != null) texto3Recibido.text = "";

        if (hora1Respuesta != null) hora1Respuesta.text = "";
        if (hora2Recibido != null) hora2Recibido.text = "";
        if (hora2Respuesta != null) hora2Respuesta.text = "";
        if (hora3Recibido != null) hora3Recibido.text = "";

        MostrarOpcionesNivel1();
        Actualizar();
    }

    void MostrarOpcionesNivel1()
    {
        if (textoOpcion1 != null) textoOpcion1.text = "¿Sigues en la notaría?";
        if (textoOpcion2 != null) textoOpcion2.text = "¿Ha sido un día largo?";
        if (textoOpcion3 != null) textoOpcion3.text = "¿Te queda mucho?";
    }

    void MostrarOpcionesNivel2()
    {
        if (opcionNivel1Elegida == 1)
        {
            if (textoOpcion1 != null) textoOpcion1.text = "¿Vuelves pronto?";
            if (textoOpcion2 != null) textoOpcion2.text = "Vale, avísame";
            if (textoOpcion3 != null) textoOpcion3.text = "No trabajes mucho";
        }
        else if (opcionNivel1Elegida == 2)
        {
            if (textoOpcion1 != null) textoOpcion1.text = "¿Muchos clientes?";
            if (textoOpcion2 != null) textoOpcion2.text = "Entonces descansa luego";
            if (textoOpcion3 != null) textoOpcion3.text = "Ánimo";
        }
        else if (opcionNivel1Elegida == 3)
        {
            if (textoOpcion1 != null) textoOpcion1.text = "Entonces bien";
            if (textoOpcion2 != null) textoOpcion2.text = "Luego me escribes";
            if (textoOpcion3 != null) textoOpcion3.text = "Te espero";
        }
    }

    void OcultarOpcionesYDejarSoloBotones()
    {
        CerrarModoTeclado();
        OcultarCambioModo();

        if (textoOpcion1 != null) textoOpcion1.text = "";
        if (textoOpcion2 != null) textoOpcion2.text = "";
        if (textoOpcion3 != null) textoOpcion3.text = "";

        if (selectoresCompletos != null && selectoresCompletos.Length >= 5)
        {
            for (int i = 0; i < 3; i++)
            {
                if (selectoresCompletos[i] != null)
                    selectoresCompletos[i].gameObject.SetActive(false);
            }
        }

        if (selectoresFinales != null && selectoresFinales.Length > 0)
            selectores = selectoresFinales;

        indice = 0;
        Actualizar();
    }

    void MostrarCambioModo()
    {
        if (selectorCambioModo != null)
            selectorCambioModo.gameObject.SetActive(true);

        if (mensajesManager != null && mensajesManager.selectorCambioModo != null)
            mensajesManager.selectorCambioModo.SetActive(true);
    }

    void OcultarCambioModo()
    {
        if (selectorCambioModo != null)
            selectorCambioModo.gameObject.SetActive(false);

        if (mensajesManager != null && mensajesManager.selectorCambioModo != null)
            mensajesManager.selectorCambioModo.SetActive(false);
    }

    void PonerHora(TMP_Text textoHora, int minutosSumar)
    {
        if (textoHora == null) return;

        System.DateTime baseHora = System.DateTime.Today.AddHours(16).AddMinutes(10);
        System.DateTime nuevaHora = baseHora.AddMinutes(minutosSumar);
        textoHora.text = nuevaHora.ToString("HH:mm");
    }

    void Actualizar()
    {
        if (selectores == null || selectores.Length == 0)
            return;

        if (indice < 0 || indice >= selectores.Length)
            indice = 0;

        for (int i = 0; i < selectores.Length; i++)
        {
            if (selectores[i] == null)
                continue;

            Color c = selectores[i].color;
            c.a = (i == indice) ? 1f : 0f;
            selectores[i].color = c;
        }
    }
}
