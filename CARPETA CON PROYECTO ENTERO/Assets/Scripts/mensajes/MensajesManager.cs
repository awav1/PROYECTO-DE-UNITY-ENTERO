using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;

public class MensajesManager : MonoBehaviour
{
    public Image[] selectoresMenu1;
    public Image[] selectoresMenu2;

    private Image[] selectoresActuales;
    private int indice = 0;

    public GameObject menuMensajes1;
    public GameObject menuMensajes2;

    public GameObject pantallaPapa;
    public GameObject pantallaMama;
    public GameObject pantallaTiaLaura;
    public GameObject pantallaSergio;
    public GameObject pantallaTrabajo;
    public GameObject pantallaFiesta;
    public GameObject pantallaPedro;
    public GameObject pantallaLidia;
    public GameObject pantallaClub;

    public GameObject pantallaTeclado;
    public GameObject selectorCambioModo;

    public PapaManager papaManager;
    public MamaManager mamaManager;
    public TiaLauraManager tiaLauraManager;
    public SergioManager sergioManager;
    public TrabajoManager trabajoManager;
    public FiestaManager fiestaManager;
    public PedroManager pedroManager;
    public LidiaManager lidiaManager;
    public ClubManager clubManager;

    public TMP_Text[] textosUltimoMensaje;
    public TMP_Text[] textosHora;
    public GameObject[] puntosAzules;

    private string[] ultimosMensajes;
    private string[] horas;
    private bool[] tieneNuevo;

    private int ultimoMenu = 1;
    private bool enMenu = true;

    UdpClient client;
    IPEndPoint remoteEndPoint;

    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        menuMensajes1.SetActive(true);
        menuMensajes2.SetActive(false);

        OcultarPantallasChat();

        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);

        if (selectorCambioModo != null)
            selectorCambioModo.SetActive(false);

        selectoresActuales = selectoresMenu1;
        indice = 0;
        ultimoMenu = 1;
        enMenu = true;

        InicializarChats();
        RefrescarTodosLosChats();
        Actualizar();
    }

    void Update()
    {
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            int senal = data[0];
            ProcesarEMG(senal);
        }
    }

    void InicializarChats()
    {
        ultimosMensajes = new string[]
        {
            "Ya estoy aquí,bajas?",
            "He ido a hacer la compra",
            "Hola, ¿cómo estás?",
            "¿Vas a venir a casa de los abuelos?",
            "Antonio: Mañana hay que llevar la presentación.",
            "Claudia: Esta noche quedamos en casa de Sara.",
            "Estoy fuera",
            "Ya he terminado los ejercicicios...",
            "Me está gustando el libro"
        };

        horas = new string[]
        {
            "19:40",
            "19:10",
            "18:55",
            "18:20",
            "17:30",
            "16:45",
            "16:10",
            "15:25",
            "14:50"
        };

        tieneNuevo = new bool[]
        {
            true,
            true,
            false,
            true,
            true,
            false,
            true,
            false,
            false
        };
    }

    public void ProcesarEMG(int senal)
    {
        if (enMenu)
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
        else
        {
            if (pantallaPapa.activeSelf && papaManager != null)
            {
                papaManager.ProcesarEMG(senal);
            }
            else if (pantallaMama.activeSelf && mamaManager != null)
            {
                mamaManager.ProcesarEMG(senal);
            }
            else if (pantallaTiaLaura.activeSelf && tiaLauraManager != null)
            {
                tiaLauraManager.ProcesarEMG(senal);
            }
            else if (pantallaSergio.activeSelf && sergioManager != null)
            {
                sergioManager.ProcesarEMG(senal);
            }
            else if (pantallaTrabajo.activeSelf && trabajoManager != null)
            {
                trabajoManager.ProcesarEMG(senal);
            }
            else if (pantallaFiesta.activeSelf && fiestaManager != null)
            {
                fiestaManager.ProcesarEMG(senal);
            }
            else if (pantallaPedro.activeSelf && pedroManager != null)
            {
                pedroManager.ProcesarEMG(senal);
            }
            else if (pantallaLidia.activeSelf && lidiaManager != null)
            {
                lidiaManager.ProcesarEMG(senal);
            }
            else if (pantallaClub.activeSelf && clubManager != null)
            {
                clubManager.ProcesarEMG(senal);
            }
        }
    }

    public void Derecha()
    {
        indice++;

        if (indice >= selectoresActuales.Length)
            indice = 0;

        Actualizar();
    }

    public void Izquierda()
    {
        indice--;

        if (indice < 0)
            indice = selectoresActuales.Length - 1;

        Actualizar();
    }

    public void Seleccionar()
    {
        string nombre = selectoresActuales[indice].name;
        Debug.Log("Seleccionado en mensajes: " + nombre);

        if (nombre == "SelectorPapa")
        {
            AbrirChat(pantallaPapa, 1);
            MarcarLeido(0);
        }
        else if (nombre == "SelectorMama")
        {
            AbrirChat(pantallaMama, 1);
            MarcarLeido(1);
        }
        else if (nombre == "SelectorTiaLaura")
        {
            AbrirChat(pantallaTiaLaura, 1);
            MarcarLeido(2);
        }
        else if (nombre == "SelectorSergio")
        {
            AbrirChat(pantallaSergio, 1);
            MarcarLeido(3);
        }
        else if (nombre == "SelectorTrabajo")
        {
            AbrirChat(pantallaTrabajo, 1);
            MarcarLeido(4);
        }
        else if (nombre == "SelectorFiesta")
        {
            AbrirChat(pantallaFiesta, 1);
            MarcarLeido(5);
        }
        else if (nombre == "SelectorPedro")
        {
            AbrirChat(pantallaPedro, 1);
            MarcarLeido(6);
        }
        else if (nombre == "BotonAbajo")
        {
            CambiarAMenu2();
        }
        else if (nombre == "BotonAtrasM1")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (nombre == "BotonInicioM1")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (nombre == "SelectorLidia")
        {
            AbrirChat(pantallaLidia, 2);
            MarcarLeido(7);
        }
        else if (nombre == "SelectorClub")
        {
            AbrirChat(pantallaClub, 2);
            MarcarLeido(8);
        }
        else if (nombre == "BotonArriba")
        {
            CambiarAMenu1();
        }
        else if (nombre == "BotonAtrasM2")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (nombre == "BotonInicioM2")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
    }

    void CambiarAMenu1()
    {
        menuMensajes1.SetActive(true);
        menuMensajes2.SetActive(false);

        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);

        if (selectorCambioModo != null)
            selectorCambioModo.SetActive(false);

        selectoresActuales = selectoresMenu1;
        indice = 0;
        ultimoMenu = 1;
        enMenu = true;

        Actualizar();
    }

    void CambiarAMenu2()
    {
        menuMensajes1.SetActive(false);
        menuMensajes2.SetActive(true);

        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);

        if (selectorCambioModo != null)
            selectorCambioModo.SetActive(false);

        selectoresActuales = selectoresMenu2;
        indice = 0;
        ultimoMenu = 2;
        enMenu = true;

        Actualizar();
    }

    void AbrirChat(GameObject pantallaChat, int menuOrigen)
    {
        ultimoMenu = menuOrigen;
        enMenu = false;

        menuMensajes1.SetActive(false);
        menuMensajes2.SetActive(false);

        OcultarPantallasChat();

        if (pantallaChat != null)
            pantallaChat.SetActive(true);

        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);
    }

    public void VolverAlMenu()
    {
        OcultarPantallasChat();

        if (pantallaTeclado != null)
            pantallaTeclado.SetActive(false);

        if (selectorCambioModo != null)
            selectorCambioModo.SetActive(false);

        if (ultimoMenu == 1)
        {
            menuMensajes1.SetActive(true);
            menuMensajes2.SetActive(false);
            selectoresActuales = selectoresMenu1;
        }
        else
        {
            menuMensajes1.SetActive(false);
            menuMensajes2.SetActive(true);
            selectoresActuales = selectoresMenu2;
        }

        indice = 0;
        enMenu = true;
        Actualizar();
    }

    void OcultarPantallasChat()
    {
        if (pantallaPapa != null) pantallaPapa.SetActive(false);
        if (pantallaMama != null) pantallaMama.SetActive(false);
        if (pantallaTiaLaura != null) pantallaTiaLaura.SetActive(false);
        if (pantallaSergio != null) pantallaSergio.SetActive(false);
        if (pantallaTrabajo != null) pantallaTrabajo.SetActive(false);
        if (pantallaFiesta != null) pantallaFiesta.SetActive(false);
        if (pantallaPedro != null) pantallaPedro.SetActive(false);
        if (pantallaLidia != null) pantallaLidia.SetActive(false);
        if (pantallaClub != null) pantallaClub.SetActive(false);
    }

    void Actualizar()
    {
        for (int i = 0; i < selectoresMenu1.Length; i++)
        {
            Color c = selectoresMenu1[i].color;
            c.a = 0f;
            selectoresMenu1[i].color = c;
        }

        for (int i = 0; i < selectoresMenu2.Length; i++)
        {
            Color c = selectoresMenu2[i].color;
            c.a = 0f;
            selectoresMenu2[i].color = c;
        }

        Color seleccionado = selectoresActuales[indice].color;
        seleccionado.a = 1f;
        selectoresActuales[indice].color = seleccionado;
    }

    public void ActualizarChat(int indiceChat, string nuevoMensaje, string nuevaHora, bool esNuevo)
    {
        if (indiceChat < 0 || indiceChat >= ultimosMensajes.Length)
            return;

        ultimosMensajes[indiceChat] = nuevoMensaje;
        horas[indiceChat] = nuevaHora;
        tieneNuevo[indiceChat] = esNuevo;

        RefrescarChat(indiceChat);
    }

    public void MarcarLeido(int indiceChat)
    {
        if (indiceChat < 0 || indiceChat >= tieneNuevo.Length)
            return;

        tieneNuevo[indiceChat] = false;
        RefrescarChat(indiceChat);
    }

    void RefrescarTodosLosChats()
    {
        for (int i = 0; i < ultimosMensajes.Length; i++)
        {
            RefrescarChat(i);
        }
    }

    void RefrescarChat(int indiceChat)
    {
        if (indiceChat < textosUltimoMensaje.Length && textosUltimoMensaje[indiceChat] != null)
            textosUltimoMensaje[indiceChat].text = ultimosMensajes[indiceChat];

        if (indiceChat < textosHora.Length && textosHora[indiceChat] != null)
            textosHora[indiceChat].text = horas[indiceChat];

        if (indiceChat < puntosAzules.Length && puntosAzules[indiceChat] != null)
            puntosAzules[indiceChat].SetActive(tieneNuevo[indiceChat]);
    }

    void OnDestroy()
    {
        if (client != null)
            client.Close();
    }
}