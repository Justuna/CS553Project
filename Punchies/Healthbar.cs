using Godot;
using System;

public partial class Healthbar : ColorRect
{
	[Export]
	private ColorRect _fill;

	public void SetHealthbar(float ratio)
    {
		_fill.Size = new Vector2(ratio * Size.X, Size.Y);
    }
}
