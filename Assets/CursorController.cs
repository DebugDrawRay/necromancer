using UnityEngine;
using System.Collections;

public class CursorController : Action
{
    public InputActions.Actions controlAction;
     
    public GameObject commandCursor;
    public LayerMask cursorMask;

    public string snapTo;
    public float snapToDistance;
    public Vector3 snapOffset;

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

        Vector3 cursorPos = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cursorMask))
        {
            cursorPos = hit.point;
        }

        GameObject[] snaps = GameObject.FindGameObjectsWithTag(snapTo);
        GameObject snapTarget = null;
        float closestRange = snapToDistance + 5;

        for (int i = 0; i < snaps.Length; i++)
        {
            float dist = Vector3.Distance(cursorPos, snaps[i].transform.position);
            if (dist <= snapToDistance && dist <= closestRange)
            {
                snapTarget = snaps[i];
                closestRange = dist;
            }
        }

        if(snapTarget == null)
        {
            cursor.transform.position = cursorPos;
        }
        else
        {
            cursor.transform.position = snapTarget.transform.position + snapOffset;

            if (Vector3.Distance(cursorPos, snapTarget.transform.position) > snapToDistance)
            {
                snapTarget = null;
            }
        }
    }
}
