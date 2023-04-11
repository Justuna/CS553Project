using Godot;
using System;
using System.Collections.Generic;

[Flags]
public enum InputFlags
{
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Punch = 1 << 4
}

public partial class InputBuffer : Node
{
    [Signal]
    public delegate void InputReceivedEventHandler();

    [Export]
    private int _frameDelay = 3;

    private Queue<int> _tickInputs = new Queue<int>();

    private int _currentInputs = 0;

    public override void _Ready()
    {
        for (int i = 0; i < _frameDelay; i++)
        {
            _tickInputs.Enqueue(0);
        }
    }

    public void ReceiveInput(int input)
    {
        _tickInputs.Enqueue(input);
        EmitSignal(SignalName.InputReceived);
    }

    public int? ConsumeInput()
    {
        int? input;
        try
        {
            input = _tickInputs.Dequeue();
        } catch (InvalidOperationException)
        {
            input = null;
        }
        return input;
    }

}
