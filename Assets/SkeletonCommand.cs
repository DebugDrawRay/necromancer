using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RaiseSkeleton))]
public class SkeletonCommand : Action 
{
    public GameObject commandCursor;
    public LayerMask cursorMask;

    private CommandCursor cursor;
    private Transform currentWaypoint;

    private RaiseSkeleton manager;

    void Start()
    {
        cursor = Instantiate(commandCursor).GetComponent<CommandCursor>();
        manager = GetComponent<RaiseSkeleton>();
    }

    protected override void Execute(InputActions actions)
    {
        bool invokeSecondary = false;
        switch (secondaryActionToRegister)
        {
            case InputActions.Actions.PrimaryAction:
                invokeSecondary = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invokeSecondary = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                invokeSecondary = actions.tertiaryAction;
                break;
        }

        bool invokeTertiary = false;
        switch (teritiaryActionToRegister)
        {
            case InputActions.Actions.PrimaryAction:
                invokeTertiary = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invokeTertiary = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                invokeTertiary = actions.tertiaryAction;
                break;
        }

        Ray ray = Camera.main.ScreenPointToRay(actions.secondaryDirection);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cursorMask))
        {
            Vector3 pos = hit.point;
            cursor.transform.position = pos;
        }

        if (invokeSecondary)
        {
            //Need to implement a per skeleton waypoint manager???
            /*if (currentWaypoint != null)
            {
                Destroy(currentWaypoint.gameObject);
            }*/
            currentWaypoint = new GameObject().transform;
            currentWaypoint.position = cursor.transform.position;

            if (manager.followingSkeletons.Count > 0)
            {
                Skeleton skel = manager.followingSkeletons[0];
                skel.ChangeLocation(currentWaypoint);
                manager.followingSkeletons.Remove(skel);
            }
        }

        if (invokeTertiary)
        {
            if (manager.followingSkeletons.Count != manager.activeSkeletons.Count)
            {
                for (int i = 0; i < manager.activeSkeletons.Count; i++)
                {
                    manager.activeSkeletons[i].ReturnHome(transform);
                    manager.followingSkeletons.Add(manager.activeSkeletons[i]);
                }
            }
        }
    }
}
