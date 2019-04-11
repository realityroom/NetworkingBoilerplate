using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class OSCHeadBroadcaster : MonoBehaviour , OSCTransmitter
{
    // ================= PUBLIC MEMBERS AND METHODS =================== //

    // tell listeners which port to communicate over
    public ushort OSCPort;
    // identifier to tell apart your thing from everyone else who is beaconing on the LAN right now
    public string discoveryIdentifier;
    

    // send a message to all our listeners
    public bool SendMessage( string address, params object[] args )
    {
        if( mySenders.Count == 0 ) return false;

        SharpOSC.OscMessage newMessage = new SharpOSC.OscMessage( address, args );
        foreach( SharpOSC.UDPSender sender in mySenders.Values )
        {
            sender.Send( newMessage );
        }

        return true;
    }

    // provide a callback for what to do when we receive a message with address address
    public void ListenForMessage( string address, Action< List< object > >  callback ) 
    {
        myOSCResponders[ address ] = callback;
    }







    // ================== PRIVATE ========================= //

    private BeaconLib.Beacon myBeacon;

    private Dictionary< string, SharpOSC.UDPSender > mySenders;
    private SharpOSC.UDPListener myListener;
    private Dictionary< string, Action< List< object > > > myOSCResponders;
    private Queue< Tuple< string, List< object > > > myOSCIncomingMessages;


    // init data structures
    void Awake()
    {
        mySenders = new Dictionary< string, SharpOSC.UDPSender >();
        myOSCResponders = new Dictionary< string, Action< List< object > > >();
        myOSCIncomingMessages = new Queue< Tuple< string, List< object > > >();
    }

    void Start()
    {
        // start discoverability, telling people who know discoveryIdentifier about OSCPort
        myBeacon = new BeaconLib.Beacon( discoveryIdentifier, OSCPort );
        myBeacon.BeaconData = "";
        myBeacon.Start();

        // start OSC listener on port OSCPort
        // define the callback
        SharpOSC.HandleOscPacket listenerCallback = delegate( SharpOSC.OscPacket packet )
        {
            // get message
            SharpOSC.OscMessage messageReceived = (SharpOSC.OscMessage) packet;

            // send message along to be processed on the main thread in Update()
            myOSCIncomingMessages.Enqueue( Tuple.Create( messageReceived.Address, messageReceived.Arguments ) ); 
        };

        // tell the callback our hidden action
        ListenForMessage( "/___broadcastToMePlease___", RespondToNewListener );

        // set up the callback
        myListener = new SharpOSC.UDPListener( OSCPort, listenerCallback );

    }

    void Update()
    {
        // while we have messages
        while( myOSCIncomingMessages.Count > 0 )
        {
            // fetch messages
            Tuple< string, List< object > > oscMessage = myOSCIncomingMessages.Dequeue();

            // route messages
            // check if we know this address
            if( myOSCResponders.ContainsKey( oscMessage.Item1 ) )
            {
                // send the address along to the responder
                myOSCResponders[ oscMessage.Item1 ]( oscMessage.Item2 );
            }
        }
    }

    void RespondToNewListener( List< object > oscValues )
    {
        // the listener is now broadcasting to us.
        // we should send to IT too.
        // get the listener's IP address
        string listenerIP = (string) oscValues[0];
        if( listenerIP == "" )
        {
            // sadness. what to do?
            return;
        }

        // add it to the list of people we send to
        mySenders[ listenerIP ] = new SharpOSC.UDPSender( listenerIP, OSCPort );
    }

    void OnApplicationQuit()
    {
        // close OSC callback / listener
        myListener.Close();

        // close beacon responder
        myBeacon.Stop();
    }

}

public interface OSCTransmitter
{
    // send a message to whoever listens to me
    bool SendMessage( string address, params object[] args );
    
    // provide a callback for what to do when we receive a message
    void ListenForMessage( string address, Action< List< object > >  callback ) ;
}
