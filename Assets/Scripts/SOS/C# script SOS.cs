using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class sosManager : MonoBehaviour
{
    [Header("Pantallas")]
    public GameObject llamar;
    public GameObject cancelar;
    public GameObject colgar;

    [Header("Selectores UI")]
    public Image[] selectores;
    private int indice = 0;

    UdpClient client;
    IPEndPoint remoteEndPoint;
    public enum EstadoPantalla
    {
        Llamar,
        Cancelar,
        Colgar
    }
    private EstadoPantalla estadoActual;
    float timer = 0f;

    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        DesactivarTodo();
        llamar.SetActive(true);

        estadoActual = EstadoPantalla.Llamar;

        Actualizar();
    }

    void Update()
    {
        if (client != null && client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            ProcesarEMG(data[0]);  
        }
        if (estadoActual == EstadoPantalla.Cancelar)
        {
            timer += Time.deltaTime;
            if (timer >= 7f)
            {
                colgar.SetActive(true);
                cancelar.SetActive(false);

                estadoActual = EstadoPantalla.Colgar;
                timer = 0f;
            }
        }
        
    }

    void ProcesarEMG(int señal)
    {
        if (señal == 1) Derecha();
        else if (señal == 2) Izquierda();
        else if (señal == 3) Seleccionar();
    }

    void Derecha()
    {
        indice = (indice + 1) % selectores.Length;
        Actualizar();
    }

    void Izquierda()
    {
        indice--;
        if (indice < 0) indice = selectores.Length - 1;
        Actualizar();
    }

    
    void Seleccionar()
    {
        DesactivarTodo();

        if (indice == selectores.Length - 1 || indice == selectores.Length - 2)
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else
        {
            SiguientePantalla();
        }
    }

    void SiguientePantalla()
    {
        DesactivarTodo();
        if (estadoActual == EstadoPantalla.Llamar)
        {
            cancelar.SetActive(true);
            estadoActual = EstadoPantalla.Cancelar;
        }
        else if (estadoActual == EstadoPantalla.Cancelar)
        {
            llamar.SetActive(true);
            estadoActual = EstadoPantalla.Llamar;
        }
        else if (estadoActual == EstadoPantalla.Colgar)
        {
            llamar.SetActive(true);
            estadoActual = EstadoPantalla.Llamar;
        }
    }

        void DesactivarTodo()
    {
        llamar.SetActive(false);
        cancelar.SetActive(false);
        colgar.SetActive(false);
    }

    void Actualizar()
    {
        for (int i = 0; i < selectores.Length; i++)
        {
            selectores[i].color = (i == indice) ? Color.white : new Color(1, 1, 1, 0);
        }
    }

    void OnDestroy()
    {
        client?.Close();
    }
}
