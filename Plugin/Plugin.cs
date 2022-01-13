using BepInEx;
using BepInEx.Configuration;
using Bounce.Singletons;
using Bounce.Unmanaged;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    public partial class HideVolumeMenuPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Hide Volume Menu Plug-In";
        public const string Guid = "org.lordashes.plugins.hidevolumemenu";
        public const string Version = "1.0.1.0"; 

        public class StateHideVolume
        {
            public HideVolumeItem Volume { get; set; } = null;
            private Action _callback = null;

            public StateHideVolume(HideVolumeItem hvi, Action callback)
            {
                this.Volume = hvi;
                this.Volume.name = "Hide Volume:On";
                this._callback = callback;
            }

            public string Name
            {
                get
                {
                    string[] info = this.Volume.name.Split(':');
                    return info[0];
                }
                set
                {
                    string[] info = this.Volume.name.Split(':');
                    if (this.Volume.name != value + ":" + info[1])
                    {
                        this.Volume.name = value + ":" + info[1];
                        this._callback();
                    }
                }
            }

            public bool State
            {
                get
                {
                    string[] info = this.Volume.name.Split(':');
                    return (info[1]=="On");
                }
                set
                {
                    string[] info = this.Volume.name.Split(':');
                    string state = (value) ? "On" : "Off";
                    if (this.Volume.name != info[0] + ":" + state)
                    {
                        this.Volume.name = info[0] + ":" + state;
                        this._callback();
                    }
                }
            }
        }

        // Configuration
        static string data = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/CustomData/";

        private ConfigEntry<KeyboardShortcut> triggerKey { get; set; }
        private static bool boardDataLoaded = false;
        private static bool menuOpen = false;
        private static bool hideVolumesShowing = false;
        private Dictionary<NGuid, StateHideVolume> currentStates = new Dictionary<NGuid, StateHideVolume>();

        private static class MenuLayout
        {
            public static int VerticalStart { get; set; } = 60;
            public static int VerticalOffset { get; set; } = 30;
            public static int VerticalCount { get; set; } = 34;
            public static int HorizontalStart { get; set; } = 10;
            public static int HorizontalOffset { get; set; } = 480;
        }
        private static class LabelInfo
        {
            public static float VerticalOffset { get; set; } = 0.25f;
            public static float FontSize = 6.0f;
        }

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Hide Volume Menu Plugin: Active.");

            triggerKey = Config.Bind("Hotkeys", "States Activation", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl));

            MenuLayout.VerticalStart = Config.Bind("Menu Layout", "Vertical Start", 60).Value;
            MenuLayout.VerticalOffset = Config.Bind("Menu Layout", "Vertical Offset", 30).Value;
            MenuLayout.VerticalCount = Config.Bind("Menu Layout", "Vertical Item Count", 34).Value;
            MenuLayout.HorizontalStart = Config.Bind("Menu Layout", "Horizontal Start", 10).Value;
            MenuLayout.HorizontalOffset = Config.Bind("Menu Layout", "Horizontal Osset", 480).Value;

            LabelInfo.VerticalOffset = Config.Bind("Labels", "Vertical Offset", 0.25f).Value;
            LabelInfo.FontSize = Config.Bind("Labels", "Font Size", 6.0f).Value;

            var harmony = new Harmony(Guid);
            harmony.PatchAll();

            Utility.PostOnMainPage(this.GetType());
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                if (Utility.StrictKeyCheck(triggerKey.Value))
                {
                    menuOpen = !menuOpen;
                    Debug.Log("Hide Volume Menu: Toggling Menu To " + menuOpen);
                }
            }
            else
            {
                boardDataLoaded = false;
            }
        }

        void OnGUI()
        {
            if (menuOpen)
            {
                HideVolumeManager hvm = HideVolumeManager.Instance;

                Transform[] hvs = null;

                foreach (Transform t in hvm.transform.Children())
                {
                    if (t.name.ToUpper() == "ROOT") { hvs = t.Children().ToArray(); break; }
                }

                if (hvs != null)
                {
                    int counter = 0;
                    int offsetX = MenuLayout.HorizontalStart;
                    int offsetY = MenuLayout.VerticalStart;
                    foreach (Transform hvt in hvs)
                    {
                        HideVolumeItem hv = hvt.GetComponent<HideVolumeItem>();
                        if (!currentStates.ContainsKey(hv.HideVolume.Id))
                        {
                            currentStates.Add(hv.HideVolume.Id, new StateHideVolume(hv, ()=> { Save(); }));
                        }
                        StateHideVolume state = currentStates[hv.HideVolume.Id];
                        state.State = GUI.Toggle(new Rect(offsetX, offsetY, 20, 20), state.State, "");
                        if(GUI.Button(new Rect(offsetX+30, offsetY, MenuLayout.HorizontalOffset-35, 20), state.Name))
                        {
                            SystemMessage.AskForTextInput("Hide Voumes...", "Hide Volume Name:", "OK", (s) => { state.Name = s; Save(); }, null, "Cancel", null, state.Name);
                        }
                        offsetY = offsetY + MenuLayout.VerticalOffset;
                        counter++;
                        if (counter >= MenuLayout.VerticalCount) { offsetY = MenuLayout.VerticalStart; offsetX = offsetX + MenuLayout.HorizontalOffset; }
                    }

                    if (!boardDataLoaded)
                    {
                        boardDataLoaded = true;
                        if (System.IO.File.Exists(data + BoardSessionManager.CurrentBoardInfo.BoardName + "(" + BoardSessionManager.CurrentBoardInfo.Id + ").json"))
                        {
                            Debug.Log("Hide Volume Menu: Loading Hide Volume Names And States...");
                            Dictionary<string, string> names = new Dictionary<string, string>();
                            names = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(data + BoardSessionManager.CurrentBoardInfo.BoardName + "(" + BoardSessionManager.CurrentBoardInfo.Id + ").json"));

                            foreach (KeyValuePair<string, string> item in names)
                            {
                                NGuid guid = NGuid.Empty;
                                NGuid.TryParse(item.Key, out guid);
                                if (guid != NGuid.Empty)
                                {
                                    Debug.Log("Hide Volume Menu: Restoring Hide Volume '" + guid.ToString() + "' Name And State...");
                                    if (currentStates.ContainsKey(guid))
                                    {
                                        currentStates[guid].Volume.name = item.Value;

                                        SimpleSingletonBehaviour<HideVolumeManager>.Instance.SetHideVolumeState(currentStates[guid].Volume.HideVolume);
                                        GameObject goLabel = GameObject.Find("HideVolume:Label:" + currentStates[guid].Volume.HideVolume.Id);
                                        TextMeshPro label = null;
                                        if (goLabel == null)
                                        {
                                            Debug.Log("Hide Volume Menu: Creating Label For Hide Volume '" + currentStates[guid].Name + "'...");
                                            goLabel = new GameObject();
                                            goLabel.name = "HideVolume:Label:" + currentStates[guid].Volume.HideVolume.Id;
                                            label = goLabel.AddComponent<TextMeshPro>();
                                            label.alignment = TextAlignmentOptions.Center;
                                            label.fontSize = 0.0f;
                                            label.transform.position = new Vector3(currentStates[guid].Volume.HideVolume.Bounds.center.x, currentStates[guid].Volume.HideVolume.Bounds.max.y + LabelInfo.VerticalOffset, currentStates[guid].Volume.HideVolume.Bounds.center.z);
                                        }
                                        else
                                        {
                                            label = goLabel.GetComponent<TextMeshPro>();
                                        }
                                        label.text = currentStates[guid].Name;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if(hideVolumesShowing)
            {
                foreach (StateHideVolume item in currentStates.Values)
                {
                    GameObject go = GameObject.Find("HideVolume:Label:" + item.Volume.HideVolume.Id);
                    if (go != null)
                    {
                        TextMeshPro label = go.GetComponent<TextMeshPro>();
                        label.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                    }
                }
            }
        }

        private void Save()
        {
            Debug.Log("Hide Volume Menu: Saving Hide Volume Changes...");
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (StateHideVolume item in currentStates.Values)
            {
                names.Add(item.Volume.HideVolume.Id.ToString(), item.Volume.name);
                Debug.Log("Hide Volume Menu: Setting '" + item.Name + "' To '" + item.State + "'...");
                item.Volume.ChangeIsActive(item.State);
                SimpleSingletonBehaviour<HideVolumeManager>.Instance.SetHideVolumeState(item.Volume.HideVolume);
                GameObject goLabel = GameObject.Find("HideVolume:Label:" + item.Volume.HideVolume.Id);
                if (goLabel != null)
                {
                    TextMeshPro label = goLabel.GetComponent<TextMeshPro>();
                    if (label != null)
                    {
                        label.text = item.Name;
                    }
                }
            }
            System.IO.File.WriteAllText(data + BoardSessionManager.CurrentBoardInfo.BoardName + "(" + BoardSessionManager.CurrentBoardInfo.Id + ").json", JsonConvert.SerializeObject(names, Formatting.Indented));
        }
    }
}

