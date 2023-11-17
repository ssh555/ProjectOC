using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;


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
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B) && BM.Mode == BuildingMode.None)
        {
            ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();

            BM.Mode = BuildingMode.Interact;
            Debug.Log("进入建造模式");
        }
        if (Input.GetKeyDown(KeyCode.N) && BM.Mode == BuildingMode.Interact)
        {
            ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Enable();
            BM.Mode = BuildingMode.None;
            Debug.Log("退出建造模式");
        }
    }

}
#endif