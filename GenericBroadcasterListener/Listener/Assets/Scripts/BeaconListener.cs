using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconListener : MonoBehaviour
{
    // identifier to tell apart your thing from everyone else who is beaconing on the LAN right now
    public string discoveryIdentifier;

    private BeaconLib.Probe myProbe;

    private Queue<System.Net.IPEndPoint> newAddresses;
    private HashSet<System.Net.IPEndPoint> initializedAddresses;


    // Start is called before the first frame update
    void Start()
    {
        // init
        newAddresses = new Queue<System.Net.IPEndPoint>();
        initializedAddresses = new HashSet<System.Net.IPEndPoint>();

        // launch
        myProbe = new BeaconLib.Probe( discoveryIdentifier );
        myProbe.BeaconsUpdated += ProcessBeaconList;
        myProbe.Start();
    }

    void Update()
    {
        while( newAddresses.Count > 0 )
        {
            // get new address
            System.Net.IPEndPoint source = newAddresses.Dequeue();

            // do something with it
            Debug.Log( source.Address + ": " + source.Port );

            // store it for later
            initializedAddresses.Add( source );
        }
    }

    void ProcessBeaconList( IEnumerable<BeaconLib.BeaconLocation> beacons )
    {
        // This function will NOT be called on the main thread...
        foreach( BeaconLib.BeaconLocation beacon in beacons )
        {
            if( ! initializedAddresses.Contains( beacon.Address ) )
            {
                // ... hence we store it for processing on the main thread
                // NOTE: we could do something with beacon.Data here too.
                newAddresses.Enqueue( beacon.Address );
            }
        }
    }

    void OnApplicationQuit()
    {
        myProbe.Stop();
    }
}
