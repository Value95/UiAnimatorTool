using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public UiAnimator uianim;
    

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            uianim.ChanageState(UIAnimTimeLineWindow.AnimState.Playing);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            uianim.ChanageState(UIAnimTimeLineWindow.AnimState.Pause);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            uianim.ChanageState(UIAnimTimeLineWindow.AnimState.ReSet);
        }
        
    }
}
