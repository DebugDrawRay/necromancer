using UnityEngine;
using System.Collections;

public class SkeletonCommand : Action 
{
    public InputActions.Actions commandAction;
    public GameObject commandCursor;
    public LayerMask cursorMask;
    private CommandCursor cursor;

    void Start()
    {
        cursor = Instantiate(commandCursor).GetComponent<CommandCursor>();
    }

    protected override void Execute(InputActions actions)
    {
        Ray ray = Camera.main.ScreenPointToRay(actions.secondaryDirection);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cursorMask))
        {
            Vector3 pos = hit.point;
            cursor.transform.position = pos;
        }

        bool invoke = false;
        switch (primaryActionToRegister)
        {
            case InputActions.Actions.PrimaryAction:
                invoke = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invoke = actions.secondaryAction;
                break;
        }

        if(invoke)
        {
            SkeletonEvent.Broadcast(cursor.transform.position);
        }
    }
}
