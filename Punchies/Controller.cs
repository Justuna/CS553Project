using Godot;
using System;

public partial class Controller : Node2D
{
	[Export]
	private Player player1;

    private bool _hasInput = true;

    public override void _PhysicsProcess(double delta)
    {
        GD.Print("Running");
        if (!_hasInput)
        {
            GetTree().Paused = true;
            return;
        }

        player1.HandleInputs();
    }

    public void ToggleInput()
    {
        _hasInput = !_hasInput;
        if (_hasInput)
        {
            GetTree().Paused = false;
        }
    }
}
