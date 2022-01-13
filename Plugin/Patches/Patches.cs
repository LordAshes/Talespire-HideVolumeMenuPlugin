using BepInEx;
using HarmonyLib;

using System;
using System.Reflection;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace LordAshes
{
    public partial class HideVolumeMenuPlugin : BaseUnityPlugin
    {
		[HarmonyPatch(typeof(HideVolumeItem), "VisibilityChange")]
        public static class Patches
        {
            public static bool Prefix(bool visibility, ref HideVolumeItem __instance)
            {
                Debug.Log("Hide Volume Menu: Patch: Visibility Toggled To " + visibility + " For "+__instance.HideVolume.Id);
                GameObject go = GameObject.Find("HideVolume:Label:" + __instance.HideVolume.Id);
                if(go!=null)
                {
                    TextMeshPro label = go.GetComponent<TextMeshPro>();
                    label.fontSize = (visibility) ? LabelInfo.FontSize : 0.0f;
                }
                hideVolumesShowing = visibility;
                return true;
            }
		}
    }
}
