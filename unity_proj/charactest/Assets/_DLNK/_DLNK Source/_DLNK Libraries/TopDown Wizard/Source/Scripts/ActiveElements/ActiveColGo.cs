using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveColGo : MonoBehaviour
{
    public TDActiveElement activeParent;
    private TDScene tdscene;
    [HideInInspector]
    public bool actived;
    [HideInInspector]
    public bool hasexit;

    private void Start()
    {
        tdscene = GameObject.FindWithTag("TdLevelManager").GetComponent<TDScene>();
    }
    void OnTriggerEnter(Collider trig)
    {
        //Debug.Log("hey! im working! cuz" + trig + "has entered and my player is: " + tdscene.PlayerChar.gameObject.GetComponent<Collider>());
        //Check character in range and keycode pressed or automatic to start action
        if (trig.GetComponent<Collider>() == tdscene.PlayerChar)
        {
            hasexit = false;
            actived = true;
        }
    }
    void OnTriggerExit(Collider trig)
    {
        if (trig.GetComponent<Collider>() == tdscene.PlayerChar)
        {
            actived = false;
            hasexit = true;
        }

    }
}
