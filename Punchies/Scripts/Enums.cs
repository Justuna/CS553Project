using Godot;
using System;

public enum ConnectionType { NOT_CONNECTED, CLIENT, HOST}

// Store each input as a bit in a bit string
// (0 for no input, 1 for input detected)
// Just to keep the messages small when they're travelling down the wire
[Flags]
public enum InputFlags
{
    Left = 1 << 0,
    Right = 1 << 1,
    Jump = 1 << 2,
    Down = 1 << 3,
    Punch = 1 << 4
}