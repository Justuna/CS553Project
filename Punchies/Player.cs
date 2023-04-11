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

    private Vector2 LEFT_SCALE = new Vector2(-1, 1);
    private Vector2 RIGHT_SCALE = new Vector2(1, 1);

    private Vector2 _direction = Vector2.Right;

    public override void _Ready()
    {
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
