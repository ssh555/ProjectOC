using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.BuildingSystem.BuildingArea;


#if UNITY_EDITOR
public class Test_BuildingManager : MonoBehaviour
{
    public BuildingManager BM;

    public List<BuildingPart> BPartList = new List<BuildingPart>();



    void Start()
    {
        ML.Engine.Manager.GameManager.Instance.RegisterLocalManager(BM);

        foreach (var bpart in BPartList)
        {
            BM.RegisterBPartPrefab(bpart);
        }

        BM.OnModeChanged += BM_OnModeChanged;
    }

    private void BM_OnModeChanged(BuildingMode pre, BuildingMode cur)
    {
        // 不启用
        if (cur == BuildingMode.None && pre != BuildingMode.None)
        {
            Debug.Log("退出建造模式");
        }
        // 启用
        else if (pre == BuildingMode.None && cur != BuildingMode.None)
        {
            Debug.Log("进入建造模式");
        }
    }
    public BuildingArea area;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B) && BM.Mode == BuildingMode.None)
        {
            ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
            area.gameObject.SetActive(true);
            BM.Mode = BuildingMode.Interact;
        }
        if (Input.GetKeyDown(KeyCode.N) && BM.Mode == BuildingMode.Interact)
        {
            ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Enable();
            BM.Mode = BuildingMode.None;
        }
    }

}
#endif