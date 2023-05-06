using Godot;
using System;
using System.Collections.Generic;

public partial class GameController : Node
{
    private Player _homePlayer;
    private Player _awayPlayer;

    [Export]
    private Player _player1;
    [Export] 
    private Player _player2;
    [Export]
    private InputReader _inputReader;
    [Export]
    private Timer _matchTimer;
    [Export]
    private Timer _endDisplayTimer;
    [Export]
    private Label _matchTimeLabel;
    [Export]
    private Label _endDisplayLabel;

    [Export]
    private int _frameDelay = 3;

    private Queue<int> _inputBufferHome = new Queue<int>();
    private Queue<int> _inputBufferAway = new Queue<int>();

    private bool _gameOver = false;

    private bool _isHost;

    private PunchiesNetworkManager _pnm;

    public ulong TotalTime {get; private set;}
    public int TotalFrames {get; private set;}
    private ulong _lastActiveFrameTime = 0;
    public ulong LargestSpike {get; private set;}

    public void Initialize(PunchiesNetworkManager pnm, bool isHost)
    {
        _pnm = pnm;
        _inputReader.Initialize(_pnm);
        _isHost = isHost;
        _matchTimer.Timeout += GameOverHandler;
        _lastActiveFrameTime = Time.GetTicksMsec();

        // Get and fill references to players
        // Host is always player 1 to maintain consistency between clients
        if (_isHost)
        {
            _homePlayer = _player1;
            _awayPlayer = _player2;
        }
        else
        {
            _homePlayer = _player2;
            _awayPlayer = _player1;
        }

        // We're using delay-based netcode
        // Essentially, the game delays your input by a couple of frames in order to give the opponent time for their inputs to reach your computer
        // If the network takes longer than the delay to deliver the frames, the game has to freeze and wait for enough input to arrive to maintain the delay
        // Kind of like how video streaming works if you took Internet Tech
        // This just fills the buffer with "no input" messages
        for (int i = 0; i < _frameDelay; i++)
        {
            _inputBufferHome.Enqueue(0);
            _inputBufferAway.Enqueue(0);
        }
    }

    // Default physics update function that Godot provides
    // Happens 60 times per second
    // Will be paused if the game pauses
    public override void _PhysicsProcess(double delta)
    {
        _matchTimeLabel.Text = "" + Mathf.Ceil(_matchTimer.TimeLeft);
        // Detect if the game should be over
        if (_homePlayer.Health == 0 || _awayPlayer.Health == 0 || _gameOver)
        {
            // If the game is over, still simulate physics, but the players should no longer be able to input
            // So just call the "handle input" function with "no input" messages
            _homePlayer.HandleInputs(0);
            _awayPlayer.HandleInputs(0);

            // Also call whatever cleanup functions we need
            if (!_gameOver)
            {
                GameOverHandler();
            }

            // Don't need the actual game loop stuff anymore
            return;
        }

        // The actual game loop stuff
        // Check if the buffer for the opponent is filled enough to maintain delay
        if (_inputBufferAway.Count > 0)
        {
            ulong time = Time.GetTicksMsec();
            ulong betterDelta = time - _lastActiveFrameTime;

            TotalTime += betterDelta;
            LargestSpike = (LargestSpike > betterDelta) ? LargestSpike : betterDelta;

            _lastActiveFrameTime = time;

            TotalFrames += 1;

            // Get the input from the local input reader and queue it
            _inputBufferHome.Enqueue(_inputReader.ConsumeInput());

            // Get the current frame's inputs for each player
            int input1 = _inputBufferHome.Dequeue();
            int input2 = _inputBufferAway.Dequeue();

            // Handle the inputs for each player
            // Then handle any attacks that landed
            // Host always runs first to maintain consistency between clients
            // Otherwise the game might diverge
            if (_isHost)
            {
                _homePlayer.HandleInputs(input1);
                _awayPlayer.HandleInputs(input2);
                _homePlayer.HitResolution();
                _awayPlayer.HitResolution();
            }
            else
            {
                _awayPlayer.HandleInputs(input2);
                _homePlayer.HandleInputs(input1);
                _awayPlayer.HitResolution();
                _homePlayer.HitResolution();
            }
        }

        // If the buffer for the "Away" player was not filled, pause the game until the buffer is filled enough
        else
        {
            GetTree().Paused = true;
        }
    }

    // Fills the "Away" player's buffer
    // A function to be called by whatever script reads from the network
    // When you make your netcode script, make sure the Node it's attached to is set to never pause so that it can alert the game to unpause
    public void QueueNetworkInput(int input)
    {
        _inputBufferAway.Enqueue(input);
        //GD.Print(_inputBufferAway.Count);
        // If the game was paused, unpause only if there are enough frames of input to maintain the delay
        if (_inputBufferAway.Count >= _frameDelay)
        {
            GetTree().Paused = false;
        }
    }

    // Called once when game ends
    private void GameOverHandler()
    {
        _gameOver = true;
        _matchTimer.Paused = true;
        GD.Print("Average FPS: " + ((float)TotalFrames / ((float)TotalTime / 1000)) + " frames per second");
        GD.Print("Largest Latency Spike: " + LargestSpike + " ms");

        _endDisplayTimer.Timeout += DisplayGameOver;
        _endDisplayTimer.Start();
    }

    // Display to screen message telling you who won
    private void DisplayGameOver()
    {
        if ((_homePlayer.Health > _awayPlayer.Health && _isHost) || (_homePlayer.Health < _awayPlayer.Health && !_isHost))
        {
            _endDisplayLabel.Text = "Player 1 Wins!";
        }
        else if ((_homePlayer.Health > _awayPlayer.Health && !_isHost) || (_homePlayer.Health < _awayPlayer.Health && _isHost))
        {
            _endDisplayLabel.Text = "Player 2 Wins!";
        }
        else
        {
            _endDisplayLabel.Text = "Draw!";
        }

        _endDisplayLabel.Visible = true;
    }
}
