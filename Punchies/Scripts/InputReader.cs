using Godot;
using System;

public partial class InputReader : Node
{
	private int currentFlags = 0;
	private PunchiesNetworkManager _pnm;

	public void Initialize(PunchiesNetworkManager pnm) 
	{ 
		_pnm = pnm;
	}

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

		_pnm.SendInput(oldInput);

        return oldInput;
    }
}
