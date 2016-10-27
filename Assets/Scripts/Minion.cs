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

    [Header("Ranges")]
    public float checkTargetRadius;
    public float attackRadius;
    public float commanderRange;
    public bool withinCommanderRange
    {
        get
        {
            return Vector3.Distance(transform.position, commander.transform.position) <= commanderRange;
        }
    }
    private bool withinGroupRange
    {
        get
        {
            for (int i = 0; i < commander.activeMinions.Count; i++)
            {
                Minion skel = commander.activeMinions[i];
                if (Vector3.Distance(transform.position, skel.transform.position) <= commanderRange &&
                    skel.withinCommanderRange)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Header("Targeting")]
    public LayerMask targetLayers;
    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, actions.target.position) <= attackRadius;
        }
    }

    private Transform previousTarget;

    //State Control
    public enum States
    {
        Idle,
        MoveToLocation,
        Attack
    }
    public States currentState;

    public enum Targets
    {
        Friendly,
        Hostile
    }

    public Targets targetType
    {
        get
        {
            int mask = (1 << actions.target.gameObject.layer);
            if ((targetLayers.value & mask) > 0)
            {
                return Targets.Hostile;
            }
            else
            {
                return Targets.Friendly;
            }
        }
    }
    void Start()
    {
        //Events.Attack += FriendliesAttacked;

        currentStats.speed += Random.Range(-speedVariation, speedVariation);
        UpdateBaseStats();
    }

    void OnEnable()
    {
        actions.target = commander.transform;
        previousTarget = commander.transform;
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    public override void RecieveDamage(object source, AttackMessage e)
    {
        base.RecieveDamage(source, e);
        if (currentState == States.Idle)
        {
            actions.target = (Transform)source;
            previousTarget = actions.target;
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
    public void RecieveTarget(Transform target)
    {
        if (target.GetComponent<MinionCommander>() == commander)
        {
            actions.target = target;
            previousTarget = actions.target;
        }
        else
        {
            Transform enemy = CheckForClosestEnemy(transform, checkTargetRadius, targetLayers);
            if (enemy == null)
            {
                actions.target = target;
                previousTarget = actions.target;
            }
            else
            {
                if (enemy != previousTarget)
                {
                    actions.target = enemy;
                    previousTarget = actions.target;
                }
            }
        }
    }

    //Targeting Logic
    Transform CheckForClosestEnemy(Transform origin, float radius, LayerMask layers)
    {
        List<Actor> enemies = new List<Actor>();

        Collider[] hits = Physics.OverlapSphere(origin.position, radius, layers);
        for (int i = 0; i < hits.Length; i++)
        {
            enemies.Add(hits[i].GetComponent<Actor>());
        }

        Transform closest = null;
        float closestRange = radius + 5;
        for (int i = 0; i < enemies.Count; i++)
        {
            Actor select = enemies[i];
            float dist = Vector3.Distance(origin.position, select.transform.position);
            if (dist <= closestRange)
            {
                closest = select.transform;
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
                actions.primaryAction = false;
                actions.secondaryAction = false;
                if (actions.target == null)
                {
                    actions.target = CheckForClosestEnemy(transform, checkTargetRadius, targetLayers);
                }
                else
                {
                    currentState = States.MoveToLocation;
                }
                break;
            case States.Attack:
                actions.primaryAction = false;
                actions.secondaryAction = true;
                if (actions.target == null)
                {
                    currentState = States.Idle;
                }
                else
                {
                    if (!withinAttackRange)
                    {
                        currentState = States.MoveToLocation;
                    }
                }
                break;
            case States.MoveToLocation:
                actions.primaryAction = true;
                actions.secondaryAction = false;
                if (actions.target == null)
                {
                    currentState = States.Idle;
                }
                else
                {
                    if (targetType == Targets.Hostile && withinAttackRange)
                    {
                        currentState = States.Attack;
                    }
                }
                break;
        }
    }
}

