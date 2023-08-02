using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

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
                
                /*
                if(zigSimData.sensordata.accel != null) Debug.Log(zigSimData.sensordata.accel.y.ToString());
                else
                {
                    Debug.Log("accel is null");
                }*/
            }
        }
    }
}
