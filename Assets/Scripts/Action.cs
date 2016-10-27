using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InputBus))]
public class Action : MonoBehaviour 
{
    public float cost;

    private InputBus bus;
    
    void Awake()
    {
        bus = GetComponent<InputBus>();
        bus.Action += Execute;
        InitializeOnAwake();
    }
    protected virtual void InitializeOnAwake() { }
    protected virtual void Execute(InputActions actions) { }

    protected bool GetActionInvoke(InputActions.Actions action, InputActions input)
    {
        switch(action)
        {
            case InputActions.Actions.PrimaryAction:
                return input.primaryAction;
            case InputActions.Actions.SecondaryAction:
                return input.secondaryAction;
            case InputActions.Actions.TertiaryAction:
                return input.tertiaryAction;
            default:
                return false;
        }
    }

    protected Vector3 GetDirectionInvoke(InputActions.Actions action, InputActions input)
    {
        switch(action)
        {
            case InputActions.Actions.PrimaryDirection:
                return input.primaryDirection;
            case InputActions.Actions.SecondaryDirection:
                return input.secondaryDirection;
            default:
                return Vector3.zero;
        }
    }
}
