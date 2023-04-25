using Godot;
using System;

public interface PunchiesNetworkManager
{
    public const int SERVER_PORT = 42069;
    public const int MAX_CLIENTS = 2;

    public void Initialize(SceneController sc);

    public string GetManagerType();

    public void HostGame();

    public void JoinGame(string ip);

    public void CancelGame();

    public void SendInput(int input);
}
