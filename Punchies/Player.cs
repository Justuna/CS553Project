using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBody2D
{
    [Export]
    private float _speed;
    [Export]
    private float _gravity;
    [Export]
    private float _jumpSpeed;
    [Export]
    private Color _color;

    private Vector2 LEFT_SCALE = new Vector2(-1, 1);
    private Vector2 RIGHT_SCALE = new Vector2(1, 1);

    private Vector2 _direction = Vector2.Right;

    public override void _Ready()
    {
        GetNode<ColorRect>("Model/Body").Color = _color;
        GetNode<ColorRect>("Model/Nose").Color = _color;
    }

    public void HandleInputs(int input)
    {
        if (IsOnFloor())
        {
            if ((input & (int)InputFlags.Left) == (int)InputFlags.Left)
            {
                _direction = Vector2.Left;
                Velocity = new Vector2(-_speed, Velocity.Y);
            }
            else if ((input & (int)InputFlags.Right) == (int)InputFlags.Right)
            {
                _direction = Vector2.Right;
                Velocity = new Vector2(_speed, Velocity.Y);
            }
            else
            {
                Velocity = new Vector2(0, Velocity.Y);
            }

            if ((input & (int)InputFlags.Jump) == (int)InputFlags.Jump)
            {
                Velocity = new Vector2(Velocity.X, -_jumpSpeed);
            }
        }
        else
        {
            Velocity += new Vector2(0, _gravity);
        }
        MoveAndSlide();
    }
}
