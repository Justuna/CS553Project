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

    // Just a class to hold a bunch of fields at once
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

    private HitInfo _outstandingHit = null;

    private int _stunFrames = 0;
    private int _endlagFrames = 0;
    private int _blockDelay = 0;

    // Set the color of the player programatically
    public override void _Ready()
    {
        GetNode<ColorRect>("Model/Body").Color = _color;
        GetNode<ColorRect>("Model/Nose").Color = _color;
        GetNode<ColorRect>("FistPath/Fist/Model").Color = _fistColor;
    }

    // The big boy function
    // Called every physics update by the controller script
    // Takes the input and does whatever it needs to
    // Basic idea: (clunky) state machine
    public void HandleInputs(int input)
    {
        // First thing's first, check if the player wants to punch and if they can punch
        if (State == PlayerState.MOBILE && (input & (int)InputFlags.Punch) == (int)InputFlags.Punch)
        {
            State = PlayerState.PUNCHING;
            // If they're on the floor, they should also stop moving
            // If they're in the air, they can keep going
            if (IsOnFloor())
            {
                Velocity = Vector2.Zero;
            }
        }

        // A temp variable to hold the amount of frames that the player has been holding block for (before it actually starts)
        int newBlockDelay = 0;

        // Next, if the player's punching, animate their arm and register a hit if it lands
        if (State == PlayerState.PUNCHING)
        {
            // The fist is a type of Node that can follow a predetermined path
            // We just increment it through the path until it's done
            // Around the middle of the path, the punch is active
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

                // Check if there's an overlap and that the enemy isn't immune
                if (hitbox.OverlapsArea(hurtbox) && (_opponent.State != PlayerState.STUNNED && _opponent.State != PlayerState.BLOCKING))
                {
                    _opponent.RegisterHit(10, 500, 1000, 30, _direction);
                }
            }

            // Fist has completed animation, go into endlag state
            if (Fist.ProgressRatio >= 1)
            {
                Fist.ProgressRatio = 0;
                State = PlayerState.ENDLAG;
                _endlagFrames = 10;
            }
        }
        // OR, if the player started blocking, stick their hand out until it's as far as it goes
        // To save on the amount of nodes we need, reuses the attack fist path but less of it
        else if (State == PlayerState.STARTBLOCK)
        {
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio += _blockSpeed * BASE_FIST_SPEED;

            // Once the fist travels far enough, switch to actual blocking state
            if (Fist.ProgressRatio >= BLOCK_FIST_DIST)
            {
                Fist.ProgressRatio = BLOCK_FIST_DIST;
                State = PlayerState.BLOCKING;
                Fist.GetNode<ColorRect>("Model").Color = _fistColorBlock;
            }
        }
        // OR, if the player is currently blocking, check if they stopped holding block
        else if (State == PlayerState.BLOCKING)
        {
            // To continue blocking, the player needs to hold the block input
            // If not, go into end block state
            if (!((input & (int)InputFlags.Left) == (int)InputFlags.Left && _direction == Vector2.Right || 
                (input & (int)InputFlags.Right) == (int)InputFlags.Right && _direction == Vector2.Left))
            {
                State = PlayerState.ENDBLOCK;
                GetNode<ColorRect>("FistPath/Fist/Model").Color = _fistColor;
            }
        }
        // OR, if the player is ending the block, pull their hand back in
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
        // OR, if the player is allowed to move, check if they moved/jumped and set their velocity accordingly
        else if (State == PlayerState.MOBILE)
        {
            // If grounded, they can jump/move/block
            if (IsOnFloor())
            {
                bool jumped = false;

                // Set their velocity to be upward
                // Upward velocity is negative because Godot is weird
                if ((input & (int)InputFlags.Jump) == (int)InputFlags.Jump)
                {
                    Velocity = new Vector2(0, -_jumpSpeed);
                    jumped = true;
                }

                // If going left/right, set their velocity to be in that direction
                // If combined with a jump, jumps in that direction because the velocities combine
                // If the player holds back (away from enemy) and is within a certain range, they block instead
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
                            // Technically, the block takes a couple extra frames to begin, so that it's easier to do a backwards jump
                            // Backwards jump is the only backward mobility option within a certain range don't judge me
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
                        Velocity = new Vector2(_speed, Velocity.Y);
                    }
                }
                else
                {
                    Velocity = new Vector2(0, Velocity.Y);
                }
            }
        }
        // OR, if the player is stunned/finishing their attack, count down on a timer until they can move again
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

        // Next, apply gravity to the velocity if not grounded
        if (!IsOnFloor())
        {
            Velocity += new Vector2(0, _gravity);
        }
        
        // Next, move the player according to their velocity using a built-in function
        MoveAndSlide();

        // If the player hits the wall, they bounce off
        KinematicCollision2D collision = GetLastSlideCollision();
        if (collision != null && collision.GetNormal() != UpDirection && State == PlayerState.STUNNED) {
            if (collision.GetNormal().X < 0) Velocity = new Vector2(-500, 0);
            else Velocity = new Vector2(500, 0);
        }

        // Either increase progress toward block or reset it
        if (newBlockDelay <= _blockDelay)
        {
            _blockDelay = 0;
        }
        else
        {
            _blockDelay = newBlockDelay;
        }
        
        // Change the direction of the player to face the opponent
        // Only when they're not in the middle of something (jumping, attacking, etc.)
        // Typical fighting game stuff
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

    // Function that can be used to mark a hit on a player for the attack resolution phase
    public void RegisterHit(int damage, float upwardKnockback, float forwardKnockback, int stunFrames, Vector2 direction)
    {
        _outstandingHit = new HitInfo(damage, upwardKnockback, forwardKnockback, stunFrames, direction);
    }

    // Function for resolving attacks
    // Separate from the movement function to allow for trading blows
    public void HitResolution()
    {
        // If there is a hit, do stuff
        if (_outstandingHit != null)
        {
            // Player gets stunned
            State = PlayerState.STUNNED;
            GetNode<ColorRect>("Model/Body").Color = _stunColor;
            GetNode<ColorRect>("Model/Nose").Color = _stunColor;

            // Reset fist if they were attacking/blocking
            _punchActive = false;
            PathFollow2D Fist = GetNode<PathFollow2D>("FistPath/Fist");
            Fist.ProgressRatio = 0;
            Fist.GetNode<ColorRect>("Model").Color = _fistColor;

            // Set the player's velocity to be flying backward
            Velocity = new Vector2(_outstandingHit.forwardKnockback * _outstandingHit.direction.X, -_outstandingHit.upwardKnockback);

            // Set their stun frames
            _stunFrames = _outstandingHit.stunFrames;

            // Set their health
            Health = Mathf.Max(0, Health - _outstandingHit.damage);

            // Update health bar
            _healthbar.SetHealthbar(Health / (float)MAX_HEALTH);

            // Reset the hit variable so they dont keep getting hit
            _outstandingHit = null;

            // Move the player according to their velocity again
            MoveAndSlide();
        }
    }
}
