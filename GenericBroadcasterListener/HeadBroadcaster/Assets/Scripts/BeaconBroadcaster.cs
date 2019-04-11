using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconBroadcaster : MonoBehaviour
{
    // tell listeners which port to communicate over
    public ushort communicationPort;
    // identifier to tell apart your thing from everyone else who is beaconing on the LAN right now
    public string discoveryIdentifier;
    // other data to send along, can be blank
    public string otherData;

    private BeaconLib.Beacon myBeacon;

    void Start()
    {
        myBeacon = new BeaconLib.Beacon( discoveryIdentifier, communicationPort );
        myBeacon.BeaconData = otherData;
        myBeacon.Start();
    }

    void OnApplicationQuit()
    {
        myBeacon.Stop();
    }
}
