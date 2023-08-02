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

