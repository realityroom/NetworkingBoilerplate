using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCTransmitTest : MonoBehaviour
{
    OSCTransmitter myTransmitter;

    void Start()
    {
        // get reference to the transmitter, either a Head or Other OSC component
        myTransmitter = (OSCTransmitter) GetComponent(typeof(OSCTransmitter));

        // tell it to do this when we hear this message
        myTransmitter.ListenForMessage( "/keypress", RespondToKey );
    }

    void Update()
    {
        // if any key is pressed
        if( Input.anyKeyDown )
        {
            // send it to the listener
            myTransmitter.SendMessage( "/keypress", Input.inputString );
        }
    }

    void RespondToKey( List< object > keyValue )
    {
        // get the value from the OSC message
        Debug.Log( "The other computer pressed key: " + (string) keyValue[0]);
    }
}
