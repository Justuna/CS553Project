using Godot;
using System;

public partial class AltPlayer : Area2D
{
    [Export]
    private float _speed;
    [Export]
    private float _gravity;
    [Export]
    private float _jumpSpeed;
    [Export]
    private Node2D _leftBound;
    [Export]
    private Node2D _rightBound;
    [Export]
    private Node2D _floorBound;

    private Vector2 LEFT_SCALE = new Vector2(-1, 1);
    private Vector2 RIGHT_SCALE = new Vector2(1, 1);

    private Vector2 _direction = Vector2.Right;
    private Vector2 _velocity = Vector2.Zero;

    private bool _grounded = true;

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleInputs();
    }

    public void HandleInputs()
    {
        if (_grounded)
        {
            GD.Print("Grounded");
            if (Input.IsActionPressed("Left"))
            {
                _direction = Vector2.Left;
                _velocity = new Vector2(-_speed, _velocity.Y);
            }
            else if (Input.IsActionPressed("Right"))
            {
                _direction = Vector2.Right;
                _velocity = new Vector2(_speed, _velocity.Y);
            }
            else
            {
                _velocity = new Vector2(0, _velocity.Y);
            }

            if (Input.IsActionPressed("Jump"))
            {
                _velocity = new Vector2(_velocity.X, -_jumpSpeed);
            }
        }
        else
        {
            _velocity += new Vector2(0, _gravity);
        }

        Position += _velocity;

        if (Position.X < _leftBound.Position.X)
        {
            Position = new Vector2(_leftBound.Position.X, Position.Y);
        }
        else if (Position.X > _rightBound.Position.X)
        {
            Position = new Vector2(_rightBound.Position.X, Position.Y);
        }

        if (Position.Y >= _floorBound.Position.Y)
        {
            _grounded = true;
            Position = new Vector2(Position.X, _floorBound.Position.Y);
        }
        else
        {
            _grounded = false;
        }
    }

}
