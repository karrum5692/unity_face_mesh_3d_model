using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEnableGo : MonoBehaviour
{
    [System.Serializable]
    public class Target
    {
        public GameObject TargetGO;
    }
    public List<Target> Targets;
    public bool _localAuto = false;
    private TDActiveElement activeParent;
    private TDScene tdscene;
    private bool _waiting;


    void Start()
    {
        // find references for vars
        activeParent = this.GetComponent<TDActiveElement>();
        tdscene = GameObject.FindWithTag("TdLevelManager").GetComponent<TDScene>();

        // check if global auto enabled
        if (activeParent.Automatic)
            _localAuto = true;
    }

    void Update()
    {
        // check if character hit collider
        if (activeParent.ActiveCollider.actived)
        {
            // check if character has activated action
            if(_localAuto && !_waiting || Input.GetKeyDown(tdscene.ActiveKey))
            {
                // do action
                foreach (Target tar in Targets)
                {
                    if (tar.TargetGO.activeInHierarchy)
                        tar.TargetGO.SetActive(false);
                    else
                        tar.TargetGO.SetActive(true);
                    // on waiting state
                    _waiting = true;
                }
            }
        }
        // check if character is leaving scene with automode
        else if (_localAuto && activeParent.ActiveCollider.hasexit && _waiting)
        {
            if (activeParent.AutoOnExit)
            {
                // do action
                foreach (Target tar in Targets)
                {
                    if (tar.TargetGO.activeInHierarchy)
                        tar.TargetGO.SetActive(false);
                    else
                        tar.TargetGO.SetActive(true);
                    // off waiting state
                    _waiting = false;
                }
            }
            else _waiting = false;
        }
    }
}
