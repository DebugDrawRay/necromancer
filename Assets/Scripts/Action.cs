using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InputBus))]
public class Action : MonoBehaviour 
{
    public InputActions.Actions primaryActionToRegister;
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

}
