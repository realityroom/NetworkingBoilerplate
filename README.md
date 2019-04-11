# NetworkingBoilerplate
Unity projects for network discovery (and subsequent communication) over the local area network (LAN).




## GenericBroadcasterListener
This contains two Unity projects that use the Beacon library to do generic discovery. Start here if your use case isn't covered by something else in the repository.

The "head broadcaster" is the one being discovered. It sets the name of the service being discovered (e.g. "myApp") and sends along a port number and other arbitrary string data to the listener doing the discovering.

The "listener" is the one doing the discovering. It sends probes out to find the service being discovered (e.g. "myApp") and is told by the broadcaster the correct IP address, port number, and other data. It can then contact the broadcaster at that address and port number, using the supplemental data as necessary.



## OSCTwoWayCommunication
This contains a Unity project with two scenes, `OSCHeadBroadcaster` and `OSCOtherBroadcaster`. The scripts in these scenes set up a two-way OSC communication flow where both sides can send and receive messages to / from one another.

The architecture is a star pattern, where the HeadBroadcaster is connected to many OtherBroadcasters. The Head listens to messages from all of its Others (and cannot figure out who is sending the message unless this information is included in the message). When it sends a message, it sends the message to all of its Others.

Each OtherBroadcaster can send and receive messages from the HeadBroadcaster. It doesn't know about the existence of any other Others.

Both of these components implement the `OSCTransmitter` interface, which provides the following two methods for sending and receiving messages. (Thus, you can write code that asks for a component of type `OSCTransmitter` and your code doesn't need to know if it has a Head or Other component attached.)

### Sending Messages
`SendMessage( string address, args... )`. Send a message with an OSC address (the first argument) and any number of mixed-type other arguments. Example:

```
myTransmitter.SendMessage( "/hand", 0, 0.5f, -0.5f, "left" );
```

### Receiving Messages
`ListenForMessage( string address, Action< List< object > > callback )`. Provide a callback that is called on the main `Update()` thread when a message matching the address provided is received. Since OSC messages can have any type in any order, the arguments are provided as a `List< object >`. It is your responsibility to know what the types will be. Example:

```
myTransmitter.ListenForMessage( "/hand", RespondToHandMessage );

// ...

void RespondToHandMessage( List< object > values )
{
    Vector3 position = new Vector3( (float) values[0], (float) values[1], (float) values[2] );
    string whichHand = (string) values[3];
}
```


### Troubleshooting
This has been tested minimally. Please report any bugs to Jack, or try fixing it yourself if you want! Note, however, that if no messages are going through at all, it is probably a firewall issue. Make sure both computers are on the same WiFi network, and try setting the type of this network to Private so that your computers will be discoverable to each other.
