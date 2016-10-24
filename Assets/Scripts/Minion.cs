using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(PathManager))]
public class Minion : Actor 
{
    [HideInInspector]
    public MinionManager commander;

    [Header("Control Variation")]
    public float speedVariation;

    [Header("Position Radiuses")]
    public float atNodeRange;

    public float atPositionRadius;
    public float followCommanderRadius;

    public float checkTargetRadius;
    public float attackTargetRadius;

    public bool withinCommanderRange
    {
        get
        {
            return Vector3.Distance(transform.position, Necromancer.instance.transform.position) <= followCommanderRadius;
        }
    }

    private bool withinGroupRange
    {
        get
        {
            for (int i = 0; i < commander.activeMinions.Count; i++)
            {
                Minion skel = commander.activeMinions[i];
                if (Vector3.Distance(transform.position, skel.transform.position) <= followCommanderRadius && skel.withinCommanderRange)
                {
                    return true;
                }
            }
            return false;
        }
    }

    //Pathing
    private PathManager pathing;
    private int currentPathIndex;

    public List<GraphNode> pathNodes;
    private GraphNode currentNode;

    [Header("Combat")]
    public string validTargets;
    public LayerMask targetLayers;

    public float attackInterval;
    private float currentInterval;
    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, currentTarget.transform.position) <= attackTargetRadius;
        }
    }

    private Transform currentTarget;
    private Actor enemyTarget
    {
        get
        {
            if(currentTarget.GetComponent<Actor>())
            {
                return currentTarget.GetComponent<Actor>();
            }
            return null;
        }
    }
    private Transform previousTarget;

    private float currentHealth;

    //State Control
    public enum States
    {
        Idle,
        MoveToLocation,
        Attack
    }
    public States currentState;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        pathing = GetComponent<PathManager>();
        currentInterval = attackInterval;
    }

    void Start()
    {
        currentStats.speed += Random.Range(-speedVariation, speedVariation);
        UpdateBaseStats();
    }

    void OnEnable()
    {
        currentTarget = commander.transform;
        previousTarget = commander.transform;
        pathing.SetTarget(currentTarget);
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    void FollowPath()
    {
        if(pathing.currentPath.newPath)
        {
            pathNodes = new List<GraphNode>();
            foreach(GraphNode node in pathing.currentPath.path.path)
            {
                pathNodes.Add(node);
            }
            pathing.currentPath.newPath = false;
        }
        if (pathNodes.Count > 0)
        {
            currentNode = pathNodes[0];

            Vector3 pos = (Vector3)currentNode.position;
            Vector3 direction = pos - transform.position;
            direction.y = direction.z;

            direction = direction.normalized;
            direction.x = Mathf.Round(direction.x);
            direction.y = Mathf.Round(direction.y);
            direction.z = Mathf.Round(direction.z);

            actions.primaryDirection = direction;

            if (Vector3.Distance(transform.position, pos) < atNodeRange)
            {
                pathNodes.Remove(currentNode);
                if (pathNodes.Count > 0)
                {
                    currentNode = pathNodes[0];
                }
            }
        }
    }

    public override void RecieveDamage(Actor source)
    {
        base.RecieveDamage(source);
        if (currentState == States.Idle)
        {
            currentTarget = source.transform;
            previousTarget = currentTarget;
            currentState = States.MoveToLocation;
        }
    }

    void Attack(Actor target)
    {
        if (currentInterval > 0)
        {
            currentInterval -= Time.deltaTime;
        }
        else
        {
            target.RecieveDamage(this);
            currentInterval = attackInterval;
        }
    }

    public void ChangeLocation(Transform trans)
    {
        List<Actor> enemies = CheckForEnemies(trans, checkTargetRadius, targetLayers, validTargets);
        if (enemies.Count > 0)
        {
            currentTarget = FindClosestTarget(enemies, trans, checkTargetRadius).transform;
            if (previousTarget != currentTarget)
            {
                previousTarget = currentTarget;
                pathing.SetTarget(currentTarget);
                currentState = States.MoveToLocation;
            }
        }
        else
        {
            currentTarget = trans;
            previousTarget = trans;
            pathing.SetTarget(currentTarget);
            currentState = States.MoveToLocation;
        }
    }

    public void ReturnHome(Transform home)
    {
        currentTarget = home;
        previousTarget = home;
        pathing.SetTarget(currentTarget);
        currentState = States.MoveToLocation;
    }

    List<Actor> CheckForEnemies(Transform origin, float radius, LayerMask layers, string tagToWatch)
    {
        List<Actor> enemies = new List<Actor>();

        Collider[] hits = Physics.OverlapSphere(origin.position, radius, layers);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == tagToWatch)
            {
                enemies.Add(hits[i].GetComponent<Actor>());
            }
        }
        return enemies;
    }

    Actor FindClosestTarget(List<Actor> targets, Transform origin, float radius)
    {
        Actor closest = null;
        float closestRange = radius + 5;
        for (int i = 0; i < targets.Count; i++)
        {
            Actor select = targets[i];
            float dist = Vector3.Distance(origin.position, select.transform.position);
            if (dist <= closestRange)
            {
                closest = select;
                closestRange = dist;
            }
        }
        return closest;
    }

    void RunStates()
    {
        switch(currentState)
        {
            case States.Idle:
                actions.primaryDirection = Vector3.zero;
                if (currentTarget.GetComponent<MinionManager>())
                {
                    if (!withinCommanderRange || !withinGroupRange)
                    {
                        currentState = States.MoveToLocation;
                    }
                }
                else
                {
                    List<Actor> enemiesInRange = CheckForEnemies(transform, checkTargetRadius, targetLayers, validTargets);
                    if (enemiesInRange.Count > 0)
                    {
                        currentTarget = FindClosestTarget(enemiesInRange, transform, checkTargetRadius).transform;
                        previousTarget = currentTarget;
                        currentState = States.MoveToLocation;
                    }
                }
                break;
            case States.Attack:
                actions.primaryDirection = Vector3.zero;
                Attack(enemyTarget);
                if(currentTarget == null)
                {
                    currentState = States.Idle;
                }
                if(!withinAttackRange)
                {
                    currentState = States.MoveToLocation;
                }
                break;
            case States.MoveToLocation:
                if(currentTarget.tag == validTargets)
                {
                    FollowPath();
                    if(withinAttackRange)
                    {
                        currentState = States.Attack;
                    }
                }
                else if(currentTarget.GetComponent<MinionManager>())
                {
                    FollowPath();
                    if (withinCommanderRange)
                    {
                        currentState = States.Idle;
                    }
                }
                else
                {
                    FollowPath();
                    if(Vector3.Distance(transform.position, currentTarget.position) <= atPositionRadius)
                    {
                        currentState = States.Idle;
                    }
                }
                break;
        }
    }
}

