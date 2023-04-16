using Godot;
using System;


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

public partial class InputReader : Node
{
	private int currentFlags = 0;

    // Default update function that Godot provides, runs every frame
    // The Node that this script is attached to does not (or should not) pause with the rest of the game when the game has to wait
    // Detect an input and then log it
    public override void _Process(double delta)
	{
		if (Input.IsActionPressed("Left"))
        {
			currentFlags |= (int)InputFlags.Left;
        }
		if (Input.IsActionPressed("Right"))
        {
			currentFlags |= (int)InputFlags.Right;
        }
		if (Input.IsActionPressed("Jump"))
		{
			currentFlags |= (int)InputFlags.Jump;
		}
		if (Input.IsActionPressed("Punch"))
        {
			currentFlags |= (int)InputFlags.Punch;
		}
	}

	// When the local player decides they need it, get all the inputs that have been stored within the last frame and pass them along
	public int ConsumeInput()
    {
		int oldInput = currentFlags;
		currentFlags = 0;

		// Send oldInput down the wire to other client when we implement netcode

        return oldInput;
    }
}
