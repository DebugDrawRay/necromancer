using UnityEngine;
using System.Collections;

public class CommandCursor : MonoBehaviour 
{
    public static CommandCursor instance;
    public Animator animator;
    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

}
