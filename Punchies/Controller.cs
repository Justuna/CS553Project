using Godot;
using System;
using System.Collections.Generic;

public partial class Controller : Node2D
{
    private Player _hostPlayer;
    private Player _clientPlayer;

    [Export]
    private InputReader _inputReader;

    [Export]
    private int _frameDelay = 3;

    private Queue<int> _inputBufferHost = new Queue<int>();
    private Queue<int> _inputBufferClient = new Queue<int>();

    private bool _gameOver = false;

    public override void _Ready()
    {
        _hostPlayer = GetNode<Player>("Player1");
        _clientPlayer = GetNode<Player>("Player2");

        for (int i = 0; i < _frameDelay; i++)
        {
            _inputBufferHost.Enqueue(0);
            _inputBufferClient.Enqueue(0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hostPlayer.Health == 0 || _clientPlayer.Health == 0)
        {
            _hostPlayer.HandleInputs(0);
            _clientPlayer.HandleInputs(0);

            if (!_gameOver)
            {
                _gameOver = true;
                GameOverHandler();
            }

            return;
        }
        if (_inputBufferClient.Count > 0)
        {
            _inputBufferHost.Enqueue(_inputReader.ConsumeInput());

            int input1 = _inputBufferHost.Dequeue();
            //int input2 = _inputBufferRemote.Dequeue();

            _hostPlayer.HandleInputs(input1);
            _clientPlayer.HandleInputs((int)InputFlags.Punch);
            _hostPlayer.HitDetection();
            _clientPlayer.HitDetection();
        }
        else
        {
            GetTree().Paused = true;
        }
    }

    public void ReceiveNetworkInput(int input)
    {
        _inputBufferClient.Enqueue(input);
        if (_inputBufferClient.Count > _frameDelay)
        {
            GetTree().Paused = false;
        }
    }

    private void GameOverHandler()
    {
        Timer endDisplayTimer = GetNode<Timer>("EndDisplayTimer");
        
        endDisplayTimer.Timeout += DisplayGameOver;
        endDisplayTimer.Start();
    }

    private void DisplayGameOver()
    {
        Label endDisplayLabel = GetNode<Label>("EndDisplayLabel");

        if (_hostPlayer.Health <= 0 && _clientPlayer.Health <= 0)
        {
            endDisplayLabel.Text = "Draw!";
        }
        else if (_hostPlayer.Health <= 0)
        {
            endDisplayLabel.Text = "Player 2 Wins!";
        }
        else
        {
            endDisplayLabel.Text = "Player 1 Wins!";
        }

        endDisplayLabel.Visible = true;
    }
}
