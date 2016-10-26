using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Seeker))]
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

    [Header("Pathing")]
    public LayerMask checkAvoidanceMask;

    private Path currentPath;

    private Seeker seeker;
    private int currentNodeIndex;

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
    private Transform previousTarget;

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

    //State Control
    public enum States
    {
        Idle,
        MoveToLocation,
        Attack,
        Path
    }
    public States currentState;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
         
        seeker = GetComponent<Seeker>();
    }

    void Start()
    {
        //Events.Attack += FriendliesAttacked;

        currentStats.speed += Random.Range(-speedVariation, speedVariation);
        UpdateBaseStats();
    }

    void OnEnable()
    {
        currentTarget = commander.transform;
        previousTarget = commander.transform;
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    //Movement

    void Follow(Vector3 position)
    {
        //point to our target
        Vector3 dir = position - transform.position;

        //Check if target is in sight
        Ray sight = new Ray(transform.position, dir);

        //check if we see our leader
        if(Physics.Raycast(sight, Mathf.Infinity, checkAvoidanceMask))
        {
            if(currentPath == null)
            {
                seeker.StartPath(transform.position, position, RequestPath);
            }
            FollowPath(position);
        }
        else
        {
            currentPath = null;
            MoveInDirection(dir);
        }
    }

    void MoveInDirection(Vector3 dir)
    {
        dir = dir.normalized;
        dir.y = dir.z;
        /*dir.x = Mathf.Round(dir.x);
        dir.y = Mathf.Round(dir.y);
        dir.z = Mathf.Round(dir.z);*/

        actions.primaryDirection = dir;
    }

    void FollowPath(Vector3 target)
    {
        if (currentPath != null)
        {
            if (currentNodeIndex < currentPath.path.Count)
            {
                GraphNode node = currentPath.path[currentNodeIndex];

                Vector3 pos = (Vector3)node.position;
                Vector3 direction = pos - transform.position;
                MoveInDirection(direction);

                if (Vector3.Distance(transform.position, pos) < atNodeRange)
                {
                    currentNodeIndex++;
                }
            }
        }
    }

    void RequestPath(Path p)
    {
        currentPath = p;
        currentNodeIndex = 0;
    }
    
    //Combat
    void Attack(Actor target)
    {
        if (currentInterval > 0)
        {
            currentInterval -= Time.deltaTime;
        }
        else
        {
            Events.TriggerAttack(this, new AttackMessage(target, currentStats.strength));
            currentInterval = attackInterval;
        }
    }

    public override void RecieveDamage(object source, AttackMessage e)
    {
        base.RecieveDamage(source, e);
        if (currentState == States.Idle)
        {
            currentTarget = (Transform)source;
            previousTarget = currentTarget;
            currentState = States.MoveToLocation;
        }
    }

    void FriendliesAttacked(object source, AttackMessage message)
    {
        /*if (message.aggressor.tag != gameObject.tag && currentState == States.Idle || currentState == States.MoveToLocation)
        {
            currentTarget = message.aggressor.transform;
            previousTarget = currentTarget;
            currentState = States.MoveToLocation;
        }*/
    }

    //Commands
    public void ChangeLocation(Transform trans)
    {
        currentPath = null;
        List<Actor> enemies = CheckForEnemies(trans, checkTargetRadius, targetLayers, validTargets);
        if (enemies.Count > 0)
        {
            currentTarget = FindClosestTarget(enemies, trans, checkTargetRadius).transform;
            if (previousTarget != currentTarget)
            {
                previousTarget = currentTarget;
                currentState = States.MoveToLocation;
            }
        }
        else
        {
            currentTarget = trans;
            previousTarget = trans;
            currentState = States.MoveToLocation;
        }
    }

    public void ReturnHome(Transform home)
    {
        currentTarget = home;
        previousTarget = home;
        currentState = States.MoveToLocation;
    }

    //Enemy Logic
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
                currentPath = null;
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
                Follow(currentTarget.position);
                if (currentTarget.tag == validTargets && withinAttackRange)
                {
                    currentState = States.Attack;
                }
                else if (currentTarget.GetComponent<MinionManager>() && withinCommanderRange)
                {
                    currentState = States.Idle;
                }
                else if (Vector3.Distance(transform.position, currentTarget.position) <= atPositionRadius)
                {
                    currentState = States.Idle;
                }
                break;
        }
    }
}

