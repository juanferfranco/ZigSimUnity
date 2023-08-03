# Integración de Zig Sim con Unity

La idea de esta práctica es que aprendas a integrar la aplicación móvil Zig Sim con una aplicación en Unity. Ten presente que esta práctica es simplemente un medio para que analices:

* La comunicación entre aplicaciones utilizando WiFi.
* La verificación de aplicaciones utilizando un software de pruebas que te permita simular clientes y servidores UDP.
* El uso del protocolo UDP.
* El intercambio de información mediante JSON.
* El uso de hilos de entrada-salida para liberar al hilo principal o hilo del motor de la espera por los datos.
* El uso COLAS como mecanismo de comunicación entre hilos.

## Ejercicio 1

Dale una mirada al sitio web de la aplicación [Zig Sim](https://1-10.github.io/zigsim/). La idea de este ejercicio es que te familiarices con el sitio y el tipo de documentación que ofrece.

## Ejercicio 2

Instala la aplicación en:

* [iOS](https://apps.apple.com/jp/app/zig-sim/id1112909974)
* [Android](https://play.google.com/store/apps/details?id=com.oneten.drive.zig_sim&pcampaignid=web_share)

## Ejercicio 3

Ahora vas a probar que la aplicación se pueda comunicar con tu computador. Ten presente hacer SIEMPRE este tipo de pruebas antes de comenzar a trabajar con Unity o cualquier otro motor.

* Descarga e instala la aplicación [ScriptCommunicator](https://sourceforge.net/projects/scriptcommunicator/).
* Ve al símbol de sistema de Windows y escribe el comando:

  ```bash
  ipconfig /all. 
  ```
  Este comando te permitirá encontrar la dirección IP de tu interfaz de red inalámbrica. ¿Tienes una dirección IP? ¿A qué red estás conectado?

## Ejercicio 4

Abre Windows Defender Firewall para apagar el Firewall. NO OLVIDES encenderlo de nuevo una vez termines de hacer TODAS las pruebas:

Esto inicial:

<img width="585" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/9e0c7f61-d2b2-4f6e-924f-bc449b357cc1">

Luego de apagar el Firewall:

<img width="572" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/061587b4-fdd0-400e-8fb3-deff097c58cc">

## Ejercicio 5

* Para hacer esta prueba vas a tener que crear una red inalámbrica. Esto lo puedes hacer con tu propio celular mediante un punto de acceso móvil. Busca cómo hacerlo 
  para el modelo particular que tengas.
* Ahora conecta el computador a esta red. Mira, ES MUY IMPORTANTE, que tanto el celular que corre Zig Sim como el computador que tendrá SriptCommunicator y Unity 
  estén en la misma red inalámbrica.
* Toma nota de la dirección IP asignada al computador en la red que acabas de crear en tu celular.
* En ScriptCommunicator vas a crear un socket UDP. En este socket ScriptCommunicator escuchará los datos que transmite Zig Sim:

<img width="340" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/cf0dab10-f483-4674-8a7d-27775cc72a6b">

* Nota el campo own port. Este es muy importante porque es precisamente en este puerto donde ScriptCommunicator estará escuchando los datos enviados por Zig Sim. 
  Siempre usa valores para own port que no estén ocupados por [otras aplicaciones](https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers). Si el puerto está 
  ocupado no podrás conectarte al socket. Dale click a connect. Con esto ya estás listo para recibir datos del celular.

<img width="317" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/4a677503-49fd-4095-ae83-926f43d2419a">

La interfaz de usuario de ScriptCommunicator se verá algo así:

<img width="267" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/0e35a204-493c-4ad7-89b5-7403cb75085c">

* Observa que en la imagen está seleccionada la pestaña UTF8, de no ser así en los settings puedes habilitar esta opción en la pestaña console options.

* Ahora abre Zig Sim y en los Settings configura:

  * PROTOCOL: UDP
  * IP ADDRESS: LA TU PC (recuerda, la IP de tu interfaz de red inalámbrica conectada a la red o punto de acceso creado en el celular).
  * PORT NUMBER: el mismo que colocaste en own port.
  * MESSAGE FORMAT: JSON
  * MESSAGE RATE (PER SEC): 1

* En el menú Sensor de Zig Sim habilita solo el sensor ACCEL (verifica que solo este tenga la palomita o la marca de check).
* Por último, selecciona Start en Zig Sim. Si todo está bien configurado deberás comenzar a recir cada segundo un mensaje en formato JSON en ScriptCommunicator.

<img width="755" alt="image" src="https://github.com/juanferfranco/ZigSimUnity/assets/2473101/6c1cb48e-4b83-465f-ac64-2f8e4b80281e">

## Ejercicio 6

Cuando lo anterior funcione, estás listo para comenzar a trabajar en Unity. Si estás trabajando en Unity y no funciona, verifica los pasos anteriores y también asegurate que la aplicación móvil esté enviando datos. Si no estás seguro selecciona Sensor y vuelve a seleccionar Start. Esto hará que Zig Sim envíe datos de nuevo.

Ahora sigue estos pasos:

* Desconecta ScriptCommunicator del socket. ¿Por qué? Porque ahora conectaremos a ese socket a Unity.
* Crea un proyecto en Unity.
* Crea una escena.
* Crea un GameObject
* Crear un Script llamado ZigSimTest

Ahora te voy a mostrar el código:

```csharp

using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;

public class ZigSimTest : MonoBehaviour
{
    private Thread _receiveThread;
    private bool _isInitialized;
    private ConcurrentQueue<ZigSimData> _receiveQueue;

    private UdpClient _receiveClient;
    private readonly int _receivePort = 32000;
    
    void Start()
    {
        _receiveClient = new UdpClient(_receivePort);
        _receiveQueue = new ConcurrentQueue<ZigSimData>();
        _receiveThread = new Thread(ReceiveDataListener);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        _isInitialized = true;
    }

    private void ReceiveDataListener()
    {
        IPEndPoint receiveEndPoint = new IPEndPoint(0, 0);

        while (true)
        {
            try
            {
                byte[] data = _receiveClient.Receive(ref receiveEndPoint);
                string jsonString = Encoding.UTF8.GetString(data);
                ZigSimData zigSimData = ZigSimData.CreateFromJSON(jsonString);
                Debug.Log("Data received from " + receiveEndPoint + ": " + jsonString);
                SerializeMessage(zigSimData);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    private void SerializeMessage(ZigSimData message)
    {
        try
        {
            _receiveQueue.Enqueue(message);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void OnDestroy()
    {
        TryKillThread();
    }

    private void OnApplicationQuit()
    {
        TryKillThread();
    }

    private void TryKillThread()
    {
        if (_isInitialized)
        {
            _receiveThread.Abort();
            _receiveThread = null;
            _receiveClient.Close();
            _receiveClient = null;
            Debug.Log("Thread killed");
            _isInitialized = false;
        }
    }


    void Update()
    {
        if (_receiveQueue.Count != 0)
        {
            ZigSimData zigSimData;
            if (!_receiveQueue.TryDequeue(out zigSimData))
            {
                Debug.Log("Dequeue fails");
            }
            else
            {
                Debug.Log(zigSimData.sensordata.accel.y.ToString());    
            }
        }
    }
}

```

Para crear un objeto en memoria con la cadena JSON recibida necesitarás:


```csharp
using UnityEngine;

/*
 {
    "device":{
        "os":"Android",
        "osversion":"10",
        "name":"MI 9",
        "uuid":"aa42e875-efbe-4212-9f1d-33ffe1814a9e",
        "displaywidth":1080,
        "displayheight":2135
    },
    "timestamp":"2023\/08\/02 15:42:49.883",
    "sensordata":{
        "accel":{
            "x":-0.0024089559447020292,
            "y":0.001887790858745575,
            "z":0.004580116365104914
        }
    }
}
 
 */

[System.Serializable]
public class ZigSimData
{
    public Device device;
    public string timestamp;
    public Sensordata sensordata;

    public static ZigSimData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ZigSimData>(jsonString);   
    }
}

[System.Serializable]
public class Device
{
    public string os;
    public string osversion;
    public string name;
    public string uuid;
    public int displaywidth;
    public int displayheight;
}

[System.Serializable]
public class Sensordata
{
    public accelData accel;
}

[System.Serializable]
public class accelData
{
    public float x;
    public float y;
    public float z;
}

```

Prueba la aplicación. La aplicación funciona casi bien. Trata de hacer este experimento. Mientras está funcionando bien, presiona Sensor y luego Start de nuevo. Qué pasa. ¿Por qué? ¿Qué puedes hacer para arreglar este problema?

# Ejercicio 7

Ahora te toca a ti. Vas a recibir datos de otros sensores del celular.

# Ejercicio 8

Crea una interfaz de usuario, no uses la Consola del editor de Unity para que visialices la información recibida. 





