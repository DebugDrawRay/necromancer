using UnityEngine;
using System.Collections.Generic;

public class RaiseSkeleton : Action 
{
    public GameObject skeleton;
    public List<Skeleton> activeSkeletons;

    protected override void Execute(InputActions actions)
    {
        bool invoke = false;
        switch(primaryActionToRegister)
        {
            case InputActions.Actions.PrimaryAction:
                invoke = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invoke = actions.secondaryAction;
                break;
        }

        if(invoke && activeSkeletons.Count < actions.stats.maxSkeletons)
        {
            //Temporary Positioning
            Vector3 direction = Random.insideUnitCircle * 2;
            direction.z = direction.y;
            direction.y = 0;
            Vector3 position = transform.position + direction;

            GameObject newSkel = (GameObject)Instantiate(skeleton, position, Quaternion.identity);
            Skeleton skel = newSkel.GetComponent<Skeleton>();
            activeSkeletons.Add(skel);
            skel.necro = this;
            skel.gameObject.SetActive(true);
        }
    }
}
