using Godot;
using System;
using System.Diagnostics;

public enum PlayerState { MOBILE, PUNCHING, STUNNED }

public partial class Player : CharacterBody2D
{
    [Export]
    private float _speed;
    [Export]
    private float _gravity;
    [Export]
    private float _jumpSpeed;
    [Export]
    private float _punchSpeed;
    [Export]
    private float _punchActiveStart;
    [Export]
    private float _punchActiveEnd;
    [Export]
    private Color _color;
    [Export]
    private Color _fistColor;
    [Export]
    private Color _fistColorActive;
    [Export]
    private Player _opponent;

    public bool PunchActive { get; private set; } = false;

    private const int MODEL_SCALE = 6;
    private Vector2 LEFT_SCALE = new Vector2(-1, 1);
    private Vector2 RIGHT_SCALE = new Vector2(1, 1);

    private Vector2 _direction = Vector2.Right;
    
    private PlayerState _state = PlayerState.MOBILE;

    private const int MAX_HEALTH = 100;
    private int _health = MAX_HEALTH;

    public override void _Ready()
    {
        GetNode<ColorRect>("Model/Body").Color = _color;
        GetNode<ColorRect>("Model/Nose").Color = _color;
        GetNode<ColorRect>("FistPath/Fist/Model").Color = _fistColor;
    }

    public void HandleInputs(int input)
    {
        GD.Print(_state);
        if (_state == PlayerState.MOBILE && (input & (int)InputFlags.Punch) == (int)InputFlags.Punch)
        {
            _state = PlayerState.PUNCHING;
            if (IsOnFloor())
            {
                Velocity = Vector2.Zero;
            }
        }
        if (_state == PlayerState.PUNCHING)
        {
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio += _punchSpeed * 0.1f;
            if (Fist.ProgressRatio < _punchActiveEnd && Fist.ProgressRatio >= _punchActiveStart && !PunchActive)
            {
                PunchActive = true;
                Fist.GetNode<ColorRect>("Model").Color = _fistColorActive;
            }
            else if (Fist.ProgressRatio >= _punchActiveEnd && PunchActive)
            {
                PunchActive = false;
                Fist.GetNode<ColorRect>("Model").Color = _fistColor;
            }

            if (Fist.ProgressRatio >= 1)
            {
                Fist.ProgressRatio = 0;
                _state = PlayerState.MOBILE;
            }
        }
        else if (_state == PlayerState.MOBILE)
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
        }

        if (!IsOnFloor())
        {
            Velocity += new Vector2(0, _gravity);
        }

        MoveAndSlide();
        
        if (_state == PlayerState.MOBILE && IsOnFloor()) 
        {
            Node2D Model = GetNode<Node2D>("Model");
            Path2D FistPath = GetNode<Path2D>("FistPath");
            if (_opponent.Position.X > Position.X)
            {
                Model.Scale = RIGHT_SCALE * MODEL_SCALE;
                FistPath.Scale = RIGHT_SCALE;
                _direction = Vector2.Right;
            }
            else if (_opponent.Position.X < Position.X)
            {
                Model.Scale = LEFT_SCALE * MODEL_SCALE;
                FistPath.Scale = LEFT_SCALE;
                _direction = Vector2.Left;
            }
        }
    }
}
