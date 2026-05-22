using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class SelectorManager : MonoBehaviour
{
    public Image[] selectores;

    private int indice = 0;

    UdpClient client;

    IPEndPoint remoteEndPoint;

    // Tiempo del último dato recibido
    float ultimoDatoTiempo = -1f;

    // Último valor recibido
    int ultimoValorRecibido = -1;

    // Control para no llenar consola
    float ultimoMensajeDebug = 0f;

    // Cada cuánto imprimir estado
    float intervaloDebug = 2f;

    void Start()
    {
        Application.runInBackground = true;

        Debug.Log("Inicio SelectorManager");

        // Inicializar UDP

        client = new UdpClient(25000);

        remoteEndPoint =
            new IPEndPoint(IPAddress.Any, 0);

        Debug.Log("UDP inicializado");

        Debug.Log("Escuchando puerto 25000");

        Actualizar();
    }

    void Update()
    {
        // Escuchar datos de MATLAB

        if (client.Available > 0)
        {
            byte[] data =
                client.Receive(ref remoteEndPoint);

            int señal = data[0];

            // Guardar último dato recibido

            ultimoDatoTiempo = Time.time;

            ultimoValorRecibido = señal;

            // Debug recepción

            if (señal == 1)
            {
                Debug.Log("Recibido -> 1 (Izquierda)");
            }
            else if (señal == 2)
            {
                Debug.Log("Recibido -> 2 (Derecha)");
            }
            else if (señal == 3)
            {
                Debug.Log("Recibido -> 3 (Frente)");
            }
            else
            {
                Debug.LogWarning(
                    "Dato UDP desconocido -> " + señal);
            }

            ProcesarEMG(señal);
        }

        // Debug periódico

        if (Time.time - ultimoMensajeDebug >= intervaloDebug)
        {
            ultimoMensajeDebug = Time.time;

            // Nunca llegó ningún dato

            if (ultimoDatoTiempo < 0f)
            {
                Debug.LogWarning(
                    "Nunca he recibido datos UDP");
            }
            else
            {
                float tiempoSinDatos =
                    Time.time - ultimoDatoTiempo;

                Debug.Log(
                    "Ultimo dato recibido -> " +
                    ultimoValorRecibido);

                Debug.Log(
                    "Recibido hace " +
                    tiempoSinDatos.ToString("F2") +
                    " segundos");

                // Aviso si lleva mucho sin recibir

                if (tiempoSinDatos >= 2f)
                {
                    Debug.LogWarning(
                        "No me llegan datos UDP");
                }
                else
                {
                    Debug.Log("UDP activo");
                }
            }
        }
    }

    public void ProcesarEMG(int señal)
    {
        if (señal == 1)
        {
            Derecha();
        }
        else if (señal == 2)
        {
            Izquierda();
        }
        else if (señal == 3)
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

        Debug.Log("Seleccionado: " + nombre);

        if (nombre == "Selector SOS")
        {
            SceneManager.LoadScene("SOS");
        }
        else if (nombre == "Selector Mensajes")
        {
            SceneManager.LoadScene("Mensajes");
        }
        else if (nombre == "Selector Fotos")
        {
            SceneManager.LoadScene("Fotos");
        }
        else if (nombre == "Selector Musica")
        {
            SceneManager.LoadScene("Musica");
        }
        else if (nombre == "Selector Calculadora")
        {
            SceneManager.LoadScene("Calculadora");
        }
        else if (nombre == "Selector Consejos Salud")
        {
            SceneManager.LoadScene("ConsejosSalud");
        }
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

    void OnDestroy()
    {
        Debug.Log("Cerrando UDP");

        if (client != null)
            client.Close();
    }
}