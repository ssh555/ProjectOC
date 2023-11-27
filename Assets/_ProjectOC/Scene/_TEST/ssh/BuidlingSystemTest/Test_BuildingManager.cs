using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.BuildingSystem.BuildingArea;
using System;


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

        StartCoroutine(AddTestEvent());
    }

    private IEnumerator AddTestEvent()
    {
        while(BM.Placer == null)
        {
            yield return null;
        }


        BM.Placer.OnKeyComStart += () =>
        {
            Debug.Log("Start KeyCom");
        };

        BM.Placer.OnKeyComInProgress += (float cur, float total) =>
        {
            Debug.Log("KeyCom Progress: " + (cur / total));
        };

        BM.Placer.OnKeyComComplete += () =>
        {
            Debug.Log("Enter KeyCom");
        };

        BM.Placer.OnKeyComCancel += (float cur, float total) =>
        {
            Debug.Log("KeyCom Cancel: " + (cur / total));
        };

        BM.Placer.OnKeyComExit += () =>
        {
            Debug.Log("Exit KeyCom");
        };

        BM.Placer.OnEnterAppearance+=(bpart) =>
        {
            Debug.Log("Enter Appearance, Current Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnExitAppearance += (bpart) =>
        {
            Debug.Log("Exit Appearance, Current Selected BPart: " + bpart.Classification.ToString());
        };


        BM.Placer.OnDestroySelectedBPart +=(bpart) =>
        {
            Debug.Log("Destroy Selected BPart: " + bpart.Classification.ToString());
        };

        BM.Placer.OnEditModeEnter += (bpart) =>
        {
            Debug.Log("Enter EditMode, Current Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnEditModeExit += (bpart) =>
        {
            Debug.Log("Exit EditMode, Current Selected BPart: " + bpart.Classification.ToString());
        };

        BM.Placer.OnBuildSelectionEnter += (BuildingCategory[] c, int ic, BuildingType[] t, int it) =>
        {
            Debug.Log("Enter Building Selection");
        };
        BM.Placer.OnBuildSelectionExit += () =>
        {
            Debug.Log("Exit Building Selection");
        };
        BM.Placer.OnBuildSelectionComfirm += (bpart) =>
        {
            Debug.Log("Building Selection confirm, Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnBuildSelectionCancel += () =>
        {
            Debug.Log("Cancel Building Selection");
        };
        BM.Placer.OnBuildSelectionTypeChanged += (BuildingCategory category, BuildingType[] types, int index) =>
        {
            Debug.Log("Current Selected Category: " + category + "-Type: " + types[index]);
        };
        BM.Placer.OnPlaceModeEnter += (bpart) =>
        {
            Debug.Log("Enter PlaceMode, Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnPlaceModeExit += () =>
        {
            Debug.Log("Exit PlaceMode");
        };
        BM.Placer.OnPlaceModeChangeStyle += (bpart, isForward) =>
        {
            Debug.Log("PlaceMode Style Changed Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnPlaceModeChangeHeight += (bpart, isForward) =>
        {
            Debug.Log("PlaceMode Height Changed Selected BPart: " + bpart.Classification.ToString());
        };
        BM.Placer.OnPlaceModeSuccess += (bpart) =>
        {
            Debug.Log("BPart Place Success: " + bpart.Classification.ToString());
        };
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