using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public bool visibleCursor;
    void Awake()
    {
        Cursor.visible = visibleCursor;
    }
}
