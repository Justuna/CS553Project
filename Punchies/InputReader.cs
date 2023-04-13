using Godot;
using System;
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

	public int ConsumeInput()
    {
		int oldInput = currentFlags;
		currentFlags = 0;

		// Send oldInput down the wire to other client

        return oldInput;
    }
}
