using InControl;

public class PlayerActions : PlayerActionSet 
{
    public PlayerTwoAxisAction Move;
    public PlayerAction MoveUp;
    public PlayerAction MoveDown;
    public PlayerAction MoveLeft;
    public PlayerAction MoveRight;

    public PlayerTwoAxisAction Cursor;
    public PlayerAction CursorUp;
    public PlayerAction CursorDown;
    public PlayerAction CursorLeft;
    public PlayerAction CursorRight;

    public PlayerAction RaiseSkeleton;

    public PlayerAction Command;

    public PlayerAction CallSkeletons;

    public PlayerActions()
    {
        MoveUp = CreatePlayerAction("Move Up");
        MoveDown = CreatePlayerAction("Move Down");
        MoveLeft = CreatePlayerAction("Move Left");
        MoveRight = CreatePlayerAction("Move Right");

        Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveDown, MoveUp);

        CursorUp = CreatePlayerAction("Cursor Up");
        CursorDown = CreatePlayerAction("Cursor Down");
        CursorLeft = CreatePlayerAction("Cursor Left");
        CursorRight = CreatePlayerAction("Cursor Right");

        Cursor = CreateTwoAxisPlayerAction(CursorLeft, CursorRight, CursorDown, CursorUp);

        RaiseSkeleton = CreatePlayerAction("Raise Skeleton");

        Command = CreatePlayerAction("Command Skeleton");

        CallSkeletons = CreatePlayerAction("Call Skeletons");
    }

    public static PlayerActions BindAll()
    {
        PlayerActions action = new PlayerActions();

        action.MoveUp.AddDefaultBinding(InputControlType.LeftStickUp);
        action.MoveDown.AddDefaultBinding(InputControlType.LeftStickDown);
        action.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        action.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);

        action.CursorUp.AddDefaultBinding(InputControlType.RightStickUp);
        action.CursorDown.AddDefaultBinding(InputControlType.RightStickDown);
        action.CursorLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        action.CursorRight.AddDefaultBinding(InputControlType.RightStickRight);

        action.RaiseSkeleton.AddDefaultBinding(InputControlType.Action1);

        action.Command.AddDefaultBinding(InputControlType.Action2);

        action.CallSkeletons.AddDefaultBinding(InputControlType.Action3);

        action.MoveUp.AddDefaultBinding(Key.W);
        action.MoveDown.AddDefaultBinding(Key.S);
        action.MoveLeft.AddDefaultBinding(Key.A);
        action.MoveRight.AddDefaultBinding(Key.D);

        action.CursorUp.AddDefaultBinding(Mouse.PositiveY);
        action.CursorDown.AddDefaultBinding(Mouse.NegativeY);
        action.CursorLeft.AddDefaultBinding(Mouse.NegativeX);
        action.CursorRight.AddDefaultBinding(Mouse.PositiveX);

        action.RaiseSkeleton.AddDefaultBinding(Key.Space);

        action.Command.AddDefaultBinding(Mouse.LeftButton);

        action.CallSkeletons.AddDefaultBinding(Mouse.RightButton);

        return action;
    }

    public static PlayerActions BindKeyboardMouse()
    {
        PlayerActions action = new PlayerActions();

        action.MoveUp.AddDefaultBinding(Key.W);
        action.MoveDown.AddDefaultBinding(Key.S);
        action.MoveLeft.AddDefaultBinding(Key.A);
        action.MoveRight.AddDefaultBinding(Key.D);

        action.CursorUp.AddDefaultBinding(Mouse.PositiveY);
        action.CursorDown.AddDefaultBinding(Mouse.NegativeY);
        action.CursorLeft.AddDefaultBinding(Mouse.NegativeX);
        action.CursorRight.AddDefaultBinding(Mouse.PositiveX);

        action.RaiseSkeleton.AddDefaultBinding(Key.Space);

        action.Command.AddDefaultBinding(Mouse.LeftButton);

        return action;
    }

}
