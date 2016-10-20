using UnityEngine;
using System.Collections.Generic;

public class MinionManager : Action 
{
    public InputActions.Actions raiseAction;
    public GameObject minion;

    public List<Minion> activeMinions = new List<Minion>();

    public List<Minion> followingMinions = new List<Minion>();

    protected override void Execute(InputActions actions)
    {
        bool invokePrimary = false;
        switch(raiseAction)
        {
            case InputActions.Actions.PrimaryAction:
                invokePrimary = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invokePrimary = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                invokePrimary = actions.tertiaryAction;
                break;
        }

        if(invokePrimary && activeMinions.Count < actions.stats.maxSkeletons)
        {
            //Temporary Positioning
            Vector3 direction = Random.insideUnitCircle * 2;
            direction.z = direction.y;
            direction.y = 0;
            Vector3 position = transform.position + direction;

            GameObject newSkel = (GameObject)Instantiate(minion, position, Quaternion.identity);
            Minion skel = newSkel.GetComponent<Minion>();

            activeMinions.Add(skel);
            followingMinions.Add(skel);

            skel.commander = this;
            skel.gameObject.SetActive(true);
        }
    }
}
