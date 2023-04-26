using Godot;
using System;

public partial class SceneController : Node
{
    [Export]
    private PackedScene _mainMenu;
    [Export]
    private PackedScene _game;
    [Export]
    private PNMType _networkManagerType;

    private PunchiesNetworkManager _pnm;

    private Node _currentScene;

    public override void _Ready()
    {
        Node nm = null;
        switch (_networkManagerType)
        {
            case PNMType.ENet:
                nm = new PNM_ENet();
                break;
            case PNMType.LiteNetLib:
                nm = new PNM_LiteNetLib();
                break;
        }

        try
        {
            _pnm = (PunchiesNetworkManager)nm;
            _pnm.Initialize(this);
            nm.Name = "Network Manager";
            nm.ProcessMode = ProcessModeEnum.Always;
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, nm);
        } 
        catch (InvalidCastException) 
        {
            GD.Print("Error: Network Manager must be of a type implementing the PunchiesNetworkManager interface");
        }
        catch (NullReferenceException)
        {
            GD.Print("Error: No Network Manager was able to be loaded. Did you forget to implement it?");
        }

        MainMenu mainMenu = (MainMenu)SwitchScene(_mainMenu);
        mainMenu.Initialize(_pnm);
    }

    public GameController StartGameAsHost()
    {
        GameController game = (GameController)SwitchScene(_game);
        game.Initialize(_pnm, true);

        return game;
    }

    public GameController StartGameAsClient() 
    {
        GameController game = (GameController)SwitchScene(_game);
        game.Initialize(_pnm, false);

        return game;
    }

    private Node SwitchScene(PackedScene scene)
    {
        if (_currentScene != null) RemoveChild(_currentScene);

        Node s = scene.Instantiate();
        _currentScene = s;

        AddChild(s);

        return s;
    }
}
