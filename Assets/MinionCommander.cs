using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MinionManager))]
public class MinionCommander : Action 
{
    public InputActions.Actions moveToLocationAction;
    public InputActions.Actions returnHomeAction;

    public GameObject waypoint;
    private Transform currentWaypoint;

    private MinionManager manager;
    private CommandCursor cursor;

    void Start()
    {
        manager = GetComponent<MinionManager>();
        cursor = CommandCursor.instance;
    }

    protected override void Execute(InputActions actions)
    {
        bool moveToLocation = false;
        switch (moveToLocationAction)
        {
            case InputActions.Actions.PrimaryAction:
                moveToLocation = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                moveToLocation = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                moveToLocation = actions.tertiaryAction;
                break;
        }

        bool returnHome = false;
        switch (returnHomeAction)
        {
            case InputActions.Actions.PrimaryAction:
                returnHome = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                returnHome = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                returnHome = actions.tertiaryAction;
                break;
        }

        if (moveToLocation)
        {
            //Need to implement a per skeleton waypoint manager???
            /*if (currentWaypoint != null)
            {
                Destroy(currentWaypoint.gameObject);
            }*/
            currentWaypoint = Instantiate(waypoint).transform;
            currentWaypoint.position = cursor.transform.position;

            if (manager.followingMinions.Count > 0)
            {
                Minion skel = manager.followingMinions[0];
                skel.ChangeLocation(currentWaypoint);
                manager.followingMinions.Remove(skel);
            }
        }

        if (returnHome)
        {
            if (manager.followingMinions.Count != manager.activeMinions.Count)
            {
                for (int i = 0; i < manager.activeMinions.Count; i++)
                {
                    manager.activeMinions[i].ReturnHome(transform);
                    manager.followingMinions.Add(manager.activeMinions[i]);
                }
            }
        }
    }
}


public class WaypointQueue
{
    private List<Transform> waypoints = new List<Transform>();
    private List<Minion> referenceList;

    public Transform this[int index]
    {
        get
        {
            return waypoints[index];
        }
    }

    public WaypointQueue(List<Minion> list)
    {
        referenceList = list;
    }

    public void Add(Transform transform)
    {
        waypoints.Add(transform);

        if (waypoints.Count > referenceList.Count)
        {
            waypoints.RemoveAt(0);
            Object.Destroy(waypoints[0].gameObject);
        }
    }

    public void Remove(Transform transform)
    {
        waypoints.Remove(transform);
    }
}
