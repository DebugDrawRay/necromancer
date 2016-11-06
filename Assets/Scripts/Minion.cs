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
        FollowCommander,
        MoveToLocation,
        AttackEnemy
    }
    public States currentState;

    public enum Targets
    {
        Commander,
        Neutral,
        Hostile
    }


    void Start()
    {
        Events.SubscribeAttack(FriendliesAttacked);

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
        UpdateAnimation(actions);
    }

    protected override void UpdateAnimation(InputActions actionsToAnimate)
    {
        if (actionsToAnimate.target != null)
        {
            Vector3 dir = actionsToAnimate.target.position - transform.position;
            dir = dir.normalized;
            if (dir.x != 0)
            {
                float sign = Mathf.Sign(dir.x);
                float abs = Mathf.Abs(dir.x);
                int x = Mathf.RoundToInt(Mathf.Ceil(abs) * sign);
                animator.SetFloat("x", x);
            }
            if (dir.z != 0)
            {
                float sign = Mathf.Sign(dir.z);
                float abs = Mathf.Abs(dir.z);
                int y = Mathf.RoundToInt(Mathf.Ceil(abs) * sign);
                animator.SetFloat("y", y);
            }
        }
        animator.SetBool("s_action", actionsToAnimate.secondaryAction);
        if (lastHealth != currentStats.health)
        {
            animator.SetTrigger("damaged");
            lastHealth = currentStats.health;
        }
    }

    public override void RecieveDamage(Transform source, float damage)
    {
        base.RecieveDamage(source, damage);
        if (source != previousTarget && currentState == States.MoveToLocation)
        {
            actions.target = source;
            previousTarget = source;
            currentState = States.AttackEnemy;
            Events.TriggerAttack(transform, new AttackMessage(source));
        }
    }

    void FriendliesAttacked(Transform sender, AttackMessage e)
    {
        if (sender.tag == gameObject.tag &&
            sender != transform &&
            currentState != States.AttackEnemy &&
            previousTarget != e.source &&
            Vector3.Distance(transform.position, sender.transform.position) <= commanderRange
            )
        {
            actions.target = e.source;
            previousTarget = e.source;
            currentState = States.AttackEnemy;
        }
    }

    void OnDestroy()
    {
        commander.activeMinions.Remove(this);
        if(commander.followingMinions.Contains(this))
        {
            commander.followingMinions.Remove(this);
        }
    }
    //Commands
    public void RecieveTarget(Transform target)
    {
        if (target == commander.transform)
        {
            actions.target = target;
            previousTarget = actions.target;
            currentState = States.FollowCommander;
        }
        else
        {
            Transform enemy = CheckForClosestEnemy(transform, checkTargetRadius, targetLayers);
            if (enemy == null)
            {
                actions.target = target;
                previousTarget = actions.target;
                currentState = States.MoveToLocation;
            }
            else
            {
                if (enemy != previousTarget)
                {
                    actions.target = enemy;
                    previousTarget = actions.target;
                    currentState = States.AttackEnemy;
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
            case States.FollowCommander:
                actions.primaryAction = true;
                actions.secondaryAction = false;
                break;
            case States.MoveToLocation:
                actions.primaryAction = true;
                actions.secondaryAction = false;
                Transform newTarget = CheckForClosestEnemy(transform, checkTargetRadius, targetLayers);
                if (newTarget != null)
                {
                    actions.target = newTarget;
                    previousTarget = actions.target;
                    currentState = States.AttackEnemy;
                }
                break;
            case States.AttackEnemy:
                if (actions.target != null)
                {
                    if (withinAttackRange)
                    {
                        actions.primaryAction = false;
                        actions.secondaryAction = true;
                    }
                    else
                    {
                        actions.primaryAction = true;
                        actions.secondaryAction = false;
                    }
                }
                else
                {
                    currentState = States.MoveToLocation;
                }
                break;
        }
    }

    public Targets targetType(Transform target)
    {
        int mask = (1 << actions.target.gameObject.layer);
        if ((targetLayers.value & mask) > 0)
        {
            return Targets.Hostile;
        }
        else if (commander.gameObject.layer == actions.target.gameObject.layer)
        {
            return Targets.Commander;
        }
        else
        {
            return Targets.Neutral;
        }
    }
}

