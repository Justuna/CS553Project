using Godot;
using System;

public partial class MainMenu : Control
{
    [Export]
    private Button _hostButton;
    [Export]
    private Button _connectButton;
    [Export]
    private TextEdit _ipField;
    [Export]
    private Control _waitingScreen;
    [Export]
    private Button _cancelButton;

    private PunchiesNetworkManager _pnm;

    public void Initialize(PunchiesNetworkManager pnm)
    {
        _pnm = pnm;

        _hostButton.Pressed += HostGame;
        _connectButton.Pressed += JoinGame;
        _cancelButton.Pressed += CancelGame;

        GD.Print("Using " + pnm.GetManagerType());
    }

    public void HostGame()
    {
        if (_ipField.Text.Trim().Length > 0)
        {
            try
            {
                _pnm.HostGame(_ipField.Text.Trim());

                _hostButton.Disabled = true;
                _connectButton.Disabled = true;
                _ipField.Editable = false;
                _waitingScreen.Visible = true;
                _cancelButton.Disabled = false;
            }
            catch (Exception e)
            {
                GD.Print(e.Message);
            }
        }
    }

    public void JoinGame()
    {
        if (_ipField.Text.Trim().Length > 0)
        {
            try
            {
                _pnm.JoinGame(_ipField.Text.Trim());

                _hostButton.Disabled = true;
                _connectButton.Disabled = true;
                _ipField.Editable = false;
                _waitingScreen.Visible = true;
                _cancelButton.Disabled = false;
            }
            catch (Exception e)
            {
                GD.Print(e.Message);
            }
        }
    }

    public void CancelGame()
    {
        _pnm.CancelGame();

        _waitingScreen.Visible = false;
        _cancelButton.Disabled = true;
        _hostButton.Disabled = false;
        _connectButton.Disabled = false;
        _ipField.Editable = true;
    }
}
