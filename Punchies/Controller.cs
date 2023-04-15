using Godot;
using System;
using System.Collections.Generic;

public partial class Controller : Node2D
{
    private Player _hostPlayer;
    private Player _networkPlayer;

    [Export]
    private InputReader _inputReader;

    [Export]
    private int _frameDelay = 3;

    private Queue<int> _inputBufferHost = new Queue<int>();
    private Queue<int> _inputBufferRemote = new Queue<int>();

    private bool _gameOver = false;

    public override void _Ready()
    {
        _hostPlayer = GetNode<Player>("Player1");
        _networkPlayer = GetNode<Player>("Player2");

        for (int i = 0; i < _frameDelay; i++)
        {
            _inputBufferHost.Enqueue(0);
            _inputBufferRemote.Enqueue(0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hostPlayer.Health == 0 || _networkPlayer.Health == 0)
        {
            _hostPlayer.HandleInputs(0);
            _networkPlayer.HandleInputs(0);

            if (!_gameOver)
            {
                _gameOver = true;
                GameOverHandler();
            }

            return;
        }
        if (_inputBufferRemote.Count > 0)
        {
            _inputBufferHost.Enqueue(_inputReader.ConsumeInput());

            int input1 = _inputBufferHost.Dequeue();
            //int input2 = _inputBufferRemote.Dequeue();

            _hostPlayer.HandleInputs(input1);
            _networkPlayer.HandleInputs((int)InputFlags.Punch);
            _hostPlayer.HitDetection();
            _networkPlayer.HitDetection();
        }
        else
        {
            GetTree().Paused = true;
        }
    }

    public void ReceiveNetworkInput(int input)
    {
        _inputBufferRemote.Enqueue(input);
        if (_inputBufferRemote.Count > _frameDelay)
        {
            GetTree().Paused = false;
        }
    }

    private void GameOverHandler()
    {

    }
}
