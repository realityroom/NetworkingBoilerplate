using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class OSCOtherBroadcaster : MonoBehaviour, OSCTransmitter
{
    // identifier to tell apart your thing from everyone else who is beaconing on the LAN right now
    public string discoveryIdentifier;

    // send a message to our listener
    public bool SendMessage( string address, params object[] args )
    {
        if( mySender == null ) return false;

        SharpOSC.OscMessage newMessage = new SharpOSC.OscMessage( address, args );
        mySender.Send( newMessage );

        return true;
    }

    // provide a callback for what to do when we receive a message with address address
    public void ListenForMessage( string address, Action<List<object>> callback )
    {
        myOSCResponders[address] = callback;
    }




    // this class is written assuming there will only be one such 
    // possible connection on the network

    private BeaconLib.Probe myProbe;

    private System.Net.IPEndPoint newAddress = null;
    private System.Net.IPEndPoint myAddress = null;


    private SharpOSC.UDPSender mySender = null;
    private SharpOSC.UDPListener myListener = null;

    private Dictionary<string, Action<List<object>>> myOSCResponders;
    private Queue<Tuple<string, List<object>>> myOSCIncomingMessages;


    // init data structures
    void Awake()
    {
        myOSCResponders = new Dictionary<string, Action<List<object>>>();
        myOSCIncomingMessages = new Queue<Tuple<string, List<object>>>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // launch the discovery probe
        myProbe = new BeaconLib.Probe( discoveryIdentifier );
        myProbe.BeaconsUpdated += ProcessBeaconList;
        myProbe.Start();
    }

    void Update()
    {
        // check if we have an address to initialize
        if( newAddress != null )
        {
            // get new address
            myAddress = newAddress;
            newAddress = null;

            // initialize!
            InitiateConnection();
        }

        // while we have messages
        while( myOSCIncomingMessages.Count > 0 )
        {
            // fetch messages
            Tuple<string, List<object>> oscMessage = myOSCIncomingMessages.Dequeue();

            // route messages
            // check if we know this address
            if( myOSCResponders.ContainsKey( oscMessage.Item1 ) )
            {
                // send the address along to the responder
                myOSCResponders[oscMessage.Item1]( oscMessage.Item2 );
            }
        }
    }

    void InitiateConnection()
    {
        // initialize the sender: easy!
        mySender = new SharpOSC.UDPSender( myAddress.Address.ToString(), myAddress.Port );

        // initialize the listener: create a callback to route messages
        SharpOSC.HandleOscPacket listenerCallback = delegate ( SharpOSC.OscPacket packet )
        {
            // get message
            SharpOSC.OscMessage messageReceived = (SharpOSC.OscMessage)packet;

            // send message along to be processed on the main thread in Update()
            myOSCIncomingMessages.Enqueue( Tuple.Create( messageReceived.Address, messageReceived.Arguments ) );
        };

        // initialize the listener
        myListener = new SharpOSC.UDPListener( myAddress.Port, listenerCallback );


        // tell the other side we exist
        SendMessage( "/___broadcastToMePlease___", GetMyIP() );
    }


    // process incoming beacon responses
    void ProcessBeaconList( IEnumerable<BeaconLib.BeaconLocation> beacons )
    {
        // This function will NOT be called on the main thread...
        foreach( BeaconLib.BeaconLocation beacon in beacons )
        {
            if( myAddress == null )
            {
                // ... hence we store it for processing on the main thread
                newAddress = beacon.Address;
            }
        }
    }

    string GetMyIP()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
        foreach( IPAddress addr in localIPs )
        {
            if( addr.AddressFamily == AddressFamily.InterNetwork )
            {
                return addr.ToString();
            }
        }

        return "";
    }

    void OnApplicationQuit()
    {
        if( myListener != null )
        {
            // close UDP listener
            myListener.Close();
        }

        // stop probing for beacons
        myProbe.Stop();
    }
}
