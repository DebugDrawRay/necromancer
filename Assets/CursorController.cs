using UnityEngine;
using System.Collections;

public class CursorController : Action
{
    public InputActions.Actions controlAction;
     
    public GameObject commandCursor;
    public LayerMask cursorMask;

    private CommandCursor cursor;

    void Start()
    {
        cursor = Instantiate(commandCursor).GetComponent<CommandCursor>();
    }

    protected override void Execute(InputActions actions)
    {
        Vector3 position = Vector3.zero;
        switch(controlAction)
        {
            case InputActions.Actions.PrimaryDirection:
                position = actions.primaryDirection;
                break;
            case InputActions.Actions.SecondaryDirection:
                position = actions.secondaryDirection;
                break;
        }

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cursorMask))
        {
            Vector3 pos = hit.point;
            cursor.transform.position = pos;
        }
    }
}
