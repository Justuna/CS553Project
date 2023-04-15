using Godot;
using System;
using System.Diagnostics;

public enum PlayerState { MOBILE, PUNCHING, STUNNED, ENDLAG, STARTBLOCK, BLOCKING, ENDBLOCK }

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
    private float _blockSpeed;
    [Export]
    private float _punchActiveStart;
    [Export]
    private float _punchActiveEnd;
    [Export]
    private Color _color;
    [Export]
    private Color _stunColor;
    [Export]
    private Color _fistColor;
    [Export]
    private Color _fistColorActive;
    [Export]
    private Color _fistColorBlock;
    [Export]
    private Player _opponent;
    [Export]
    private Healthbar _healthbar;

    private class HitInfo
    {
        public HitInfo(int damage, float upwardKnockback, float forwardKnockback, int stunFrames, Vector2 direction)
        {
            this.damage = damage;
            this.upwardKnockback = upwardKnockback;
            this.forwardKnockback = forwardKnockback;
            this.stunFrames = stunFrames;
            this.direction = direction;
        }

        public int damage;
        public float upwardKnockback;
        public float forwardKnockback;
        public int stunFrames;
        public Vector2 direction;
    }
    
    public PlayerState State { get; private set; } = PlayerState.MOBILE;

    private const int MODEL_SCALE = 6;
    private Vector2 LEFT_SCALE = new Vector2(-1, 1);
    private Vector2 RIGHT_SCALE = new Vector2(1, 1);
    private const float BASE_FIST_SPEED = 0.1f;
    private const float BLOCK_FIST_DIST = 0.25f;
    private const float AUTO_BLOCK_RANGE = 500;
    private const float FRAMES_TO_BLOCK = 5;

    private Vector2 _direction = Vector2.Right;

    private bool _punchActive = false;

    private const int MAX_HEALTH = 100;
    public int Health { get; private set; } = MAX_HEALTH;

    private HitInfo? _outstandingHit = null;

    private int _stunFrames = 0;
    private int _endlagFrames = 0;
    private int _blockDelay = 0;

    public override void _Ready()
    {
        GetNode<ColorRect>("Model/Body").Color = _color;
        GetNode<ColorRect>("Model/Nose").Color = _color;
        GetNode<ColorRect>("FistPath/Fist/Model").Color = _fistColor;
    }

    public void HandleInputs(int input)
    {
        GD.Print(Name + " " + State);
        if (State == PlayerState.MOBILE && (input & (int)InputFlags.Punch) == (int)InputFlags.Punch)
        {
            State = PlayerState.PUNCHING;
            if (IsOnFloor())
            {
                Velocity = Vector2.Zero;
            }
        }

        int newBlockDelay = 0;

        if (State == PlayerState.PUNCHING)
        {
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio += _punchSpeed * BASE_FIST_SPEED;
            if (Fist.ProgressRatio < _punchActiveEnd && Fist.ProgressRatio >= _punchActiveStart && !_punchActive)
            {
                _punchActive = true;
                Fist.GetNode<ColorRect>("Model").Color = _fistColorActive;
            }
            else if (Fist.ProgressRatio >= _punchActiveEnd && _punchActive)
            {
                _punchActive = false;
                Fist.GetNode<ColorRect>("Model").Color = _fistColor;
            }

            if (_punchActive)
            {
                Area2D hitbox = GetNode<Area2D>("FistPath/Fist/Hitbox");
                Area2D hurtbox = _opponent.GetNode<Area2D>("Hurtbox");

                if (hitbox.OverlapsArea(hurtbox) && (_opponent.State != PlayerState.STUNNED && _opponent.State != PlayerState.BLOCKING))
                {
                    _opponent.RegisterHit(10, 500, 1000, 30, _direction);
                }
            }

            if (Fist.ProgressRatio >= 1)
            {
                Fist.ProgressRatio = 0;
                State = PlayerState.ENDLAG;
                _endlagFrames = 10;
            }
        }
        else if (State == PlayerState.STARTBLOCK)
        {
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio += _blockSpeed * BASE_FIST_SPEED;

            if (Fist.ProgressRatio >= BLOCK_FIST_DIST)
            {
                Fist.ProgressRatio = BLOCK_FIST_DIST;
                State = PlayerState.BLOCKING;
                Fist.GetNode<ColorRect>("Model").Color = _fistColorBlock;
            }
        }
        else if (State == PlayerState.BLOCKING)
        {
            if (!((input & (int)InputFlags.Left) == (int)InputFlags.Left && _direction == Vector2.Right))
            {
                State = PlayerState.ENDBLOCK;
                GetNode<ColorRect>("FistPath/Fist/Model").Color = _fistColor;
            }
        }
        else if (State == PlayerState.ENDBLOCK)
        {
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio -= _blockSpeed * BASE_FIST_SPEED;

            if (Fist.ProgressRatio <= 0)
            {
                Fist.ProgressRatio = 0;
                State = PlayerState.MOBILE;
            }
        }

        else if (State == PlayerState.MOBILE)
        {
            if (IsOnFloor())
            {
                bool jumped = false;
                if ((input & (int)InputFlags.Jump) == (int)InputFlags.Jump)
                {
                    Velocity = new Vector2(0, -_jumpSpeed);
                    jumped = true;
                }

                if ((input & (int)InputFlags.Left) == (int)InputFlags.Left)
                {
                    if (_direction == Vector2.Right && !jumped && Mathf.Abs(_opponent.Position.X - Position.X) < AUTO_BLOCK_RANGE)
                    {
                        if (_blockDelay >= FRAMES_TO_BLOCK)
                        {
                            State = PlayerState.STARTBLOCK;
                            Velocity = Vector2.Zero;
                            _blockDelay = 0;
                        }
                        else 
                        {
                            newBlockDelay = _blockDelay + 1;
                        }
                    }
                    else
                    {
                        Velocity = new Vector2(-_speed, Velocity.Y);
                    }
                }
                else if ((input & (int)InputFlags.Right) == (int)InputFlags.Right)
                {
                    if (_direction == Vector2.Left && !jumped && Mathf.Abs(_opponent.Position.X - Position.X) < AUTO_BLOCK_RANGE)
                    {
                        State = PlayerState.STARTBLOCK;
                        Velocity = Vector2.Zero;
                    }
                    else
                    {
                        Velocity = new Vector2(_speed, Velocity.Y);
                    }
                }
                else
                {
                    Velocity = new Vector2(0, Velocity.Y);
                }
            }
        } 

        else if (State == PlayerState.STUNNED)
        {
            if (IsOnFloor())
            {
                Velocity = Vector2.Zero;
            }
            _stunFrames -= 1;
            if (_stunFrames == 0)
            {
                GetNode<ColorRect>("Model/Body").Color = _color;
                GetNode<ColorRect>("Model/Nose").Color = _color;

                State = PlayerState.MOBILE;
            }
        }

        else if (State == PlayerState.ENDLAG) {
            if (IsOnFloor())
            {
                Velocity = Vector2.Zero;
            }
            _endlagFrames -= 1;
            if (_endlagFrames == 0)
            {
                State = PlayerState.MOBILE;
            }
        }

        if (!IsOnFloor())
        {
            Velocity += new Vector2(0, _gravity);
        }

        MoveAndSlide();
        KinematicCollision2D collision = GetLastSlideCollision();
        if (collision != null && collision.GetNormal() != UpDirection && State == PlayerState.STUNNED) {
            if (collision.GetNormal().X < 0) Velocity = new Vector2(-500, 0);
            else Velocity = new Vector2(500, 0);
        }

        if (newBlockDelay <= _blockDelay)
        {
            _blockDelay = 0;
        }
        else
        {
            _blockDelay = newBlockDelay;
        }
        
        if (State == PlayerState.MOBILE && IsOnFloor()) 
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

    public void RegisterHit(int damage, float upwardKnockback, float forwardKnockback, int stunFrames, Vector2 direction)
    {
        GD.Print(Name + " took " + damage + " damage!");
        _outstandingHit = new HitInfo(damage, upwardKnockback, forwardKnockback, stunFrames, direction);
    }

    public void HitDetection()
    {
        if (_outstandingHit != null)
        {
            State = PlayerState.STUNNED;
            GetNode<ColorRect>("Model/Body").Color = _stunColor;
            GetNode<ColorRect>("Model/Nose").Color = _stunColor;

            _punchActive = false;
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio = 0;
            Fist.GetNode<ColorRect>("Model").Color = _fistColor;

            Velocity = new Vector2(_outstandingHit.forwardKnockback * _outstandingHit.direction.X, -_outstandingHit.upwardKnockback);
            _stunFrames = _outstandingHit.stunFrames;
            Health = Mathf.Max(0, Health - _outstandingHit.damage);
            _healthbar.SetHealthbar(Health / (float)MAX_HEALTH);
            _outstandingHit = null;

            MoveAndSlide();
        }
    }
}
