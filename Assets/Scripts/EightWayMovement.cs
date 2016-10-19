using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EightWayMovement : Action 
{
    [Range(0,1)]
    public float acceleration;
    private Rigidbody rigid;
    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        rigid = GetComponent<Rigidbody>();
    }
    protected override void Execute(InputActions action)
    {
        Vector3 forward = transform.forward * action.primaryDirection.y * action.stats.speed;
        Vector3 right = transform.right * action.primaryDirection.x * action.stats.speed;
        Vector3 move = forward + right;
        move.y = rigid.velocity.y;

        
        rigid.velocity = Vector3.Lerp(rigid.velocity, move, acceleration);
    }

}
