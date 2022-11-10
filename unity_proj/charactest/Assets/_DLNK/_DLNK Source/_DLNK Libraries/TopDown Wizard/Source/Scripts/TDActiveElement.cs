using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDActiveElement : MonoBehaviour
{
    public ActiveColGo ActiveCollider;
    public bool Automatic;
    public bool AutoOnExit = true;
    [Header("Active Actions")]
    public bool EnableGo;
    public ActiveEnableGo scriptEnable;
    public bool SwitchMaterial;
    public ActiveMaterial scriptMat;
    public bool DoTransform;
    public ActiveTransform scriptTrans;
    public bool DoAnimate;
    public ActiveAnim scriptAnim;
    private TDScene tdscene;
    [HideInInspector]
    public GameObject TrigSelected;


    // Start is called before the first frame update
    void Start()
    {
        //Set scene local variable
        tdscene = GameObject.FindWithTag("TdLevelManager").GetComponent<TDScene>();
        //Secure Collider as trigger
        ActiveCollider.gameObject.GetComponent<Collider>().isTrigger = true;
        //make Collider object invisible
        if (!tdscene.VisibleTriggers)
            ActiveCollider.GetComponent<Renderer>().enabled = false;
    }

    public void OnCreateCollider()
    {
        // Create object
        GameObject activcol = GameObject.CreatePrimitive(PrimitiveType.Cube);
        activcol.transform.parent = this.transform;
        activcol.transform.position = this.transform.position;
        activcol.name = "[ColliderActive]";
        activcol.AddComponent<ActiveColGo>();
        activcol.GetComponent<ActiveColGo>().activeParent = this;
        //Set material
        activcol.GetComponent<MeshRenderer>().material = GameObject.FindWithTag("TdLevelManager").GetComponent<TDScene>().ActiveMat;
        activcol.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //Set in ActiveElement
        ActiveCollider = activcol.GetComponent<ActiveColGo>();

        //select trigger
        TrigSelected = activcol;
    }
}
