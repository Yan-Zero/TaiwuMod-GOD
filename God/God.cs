using BepInEx;
using BepInEx.Logging;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.GameObjects;
using YanLib.DataManipulator;
using YanLib.ModHelper;

namespace God
{
    /// <summary>
    /// Mod 入口
    /// </summary>
    [BepInPlugin(GUID, "Yan.God", Version)]
    [BepInProcess("The Scroll Of Taiwu Alpha V1.0.exe")]
    [BepInDependency("0.0Yan.Lib")]
    public class God : BaseUnityPlugin
    {
        /// <summary>版本</summary>
        public const string Version = "1.0.0.0";
        /// <summary>GUID</summary>
        public const string GUID = "Yan.God";
        /// <summary>日志</summary>
        public static new ManualLogSource Logger;
        public static ModHelper Mod;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Logger = base.Logger;
            Mod = new ModHelper(GUID, "GOD");
            Mod.SettingUI = new Container()
            {
                Name = GUID,
                Group =
                {
                    Direction = UnityUIKit.Core.Direction.Vertical,
                    Spacing = 5,
                },
                Element = { PreferredSize = { 0, 60 } },
                Children =
                {
                    new Container()
                    {
                        Name = "DelActor",
                        Element =
                        {
                            PreferredSize = { 0,60 },
                        },
                        Group =
                        {
                            Direction = UnityUIKit.Core.Direction.Horizontal,
                            Spacing = 3,
                        },
                        Children =
                        {
                            new TaiwuLabel()
                            {
                                Name = "Label",
                                Text = "删除的角色",
                                Element =
                                {
                                    PreferredSize = { 200 , 0}
                                },
                                UseBoldFont = true,
                                UseOutline = true,
                            },
                            new TaiwuInputField()
                            {
                                Name = "Input",
                                Placeholder = "请输入人物ID/名字",
                            },
                            new TaiwuButton()
                            {
                                Name = "Button",
                                Text = "删除角色",
                                Element =
                                {
                                    PreferredSize = { 150 , 0 }
                                },
                                FontColor = Color.white,
                                OnClick = (Button bt) =>
                                {
                                    var IF = bt.Parent.Children.Find(it => it.Name == "Input") as TaiwuInputField;

                                    IF.Text = IF.Text.Replace(" ","");
                                    List<int> ActorIDs = new List<int>();
                                    if(int.TryParse(IF.Text,out int id))
                                        ActorIDs.Add(id);
                                    else
                                        foreach(int ActorID in Characters.GetAllCharIds())
                                            if(DateFile.instance.GetActorName(ActorID) == IF.Text)
                                                ActorIDs.Add(ActorID);

                                    foreach(var ActorID in ActorIDs)
                                        Actor.DelActor(ActorID);
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
