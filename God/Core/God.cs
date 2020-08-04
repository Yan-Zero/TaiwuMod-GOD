using BepInEx;
using BepInEx.Logging;
using GameData;
using God.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.GameObjects;
using YanLib.DataManipulator;
using YanLib.ModHelper;

namespace God.Core
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
        public const string Version = "1.1.1.0";
        /// <summary>GUID</summary>
        public const string GUID = "Yan.God";
        /// <summary>日志</summary>
        public static new ManualLogSource Logger;
        public static ModHelper Mod;
        public static Setting Setting;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Logger = base.Logger;
            Setting = new Setting();
            Setting.Init(Config);

            Thread athread = new Thread(new ThreadStart(HarmonyPatches.Init));
            athread.IsBackground = true;
            athread.Start();

            GameSpeeder.Load();
            Mod = new ModHelper(GUID, "GOD");
            Mod.SettingUI = new BoxAutoSizeModelGameObject()
            {
                Name = GUID,
                Group =
                {
                    Direction = UnityUIKit.Core.Direction.Vertical,
                    Spacing = 5,
                },
                SizeFitter =
                {
                    VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                },
                Children =
                {
                    new Container()
                    {
                        Name = "DelActor",
                        Element = { PreferredSize = { 0,60 }, },
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
                                Element = { PreferredSize = { 200 , 0 } },
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
                                Element = { PreferredSize = { 150 , 0 } },
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
                    },
                    new BoxAutoSizeModelGameObject()
                    {
                        Name = "GameSpeeder",
                        Group =
                        {
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            Spacing = 5
                        },
                        SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                        Children =
                        {
                            new TaiwuButton()
                            {
                                Name = "Button-Show",
                                Text = "变速器",
                                UseBoldFont = true,
                                UseOutline = true,
                                FontColor = Color.white,
                                OnClick = (Button button) =>
                                {
                                    for(int i = 1;i<button.Parent.Children.Count;i++)
                                    {
                                        button.Parent.Children[i].SetActive(!button.Parent.Children[i].IsActive);
                                    }
                                },
                                Element =
                                {
                                    PreferredSize = { 0 , 50 }
                                }
                            },
                            new BoxAutoSizeModelGameObject()
                            {
                                Name = "BaseSetting",
                                Group=
                                {
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    Spacing = 3
                                },
                                SizeFitter =
                                {
                                    VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                                },
                                Children =
                                {
                                    new TaiwuButton()
                                    {
                                        Name = "Button-Show",
                                        Text = "---基本配置---",
                                        UseBoldFont = true,
                                        UseOutline = true,
                                        OnClick = (Button button) =>
                                        {
                                            for(int i = 1;i<button.Parent.Children.Count;i++)
                                            {
                                                button.Parent.Children[i].SetActive(!button.Parent.Children[i].IsActive);
                                            }
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0 , 50 }
                                        },
                                        FontColor = Color.white,
                                    },
                                    new Container()
                                    {
                                        Name = "Enable",
                                        Group =
                                        {
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 5,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0 , 50 }
                                        },
                                        Children =
                                        {
                                            new TaiwuToggle()
                                            {
                                                Name = "Enable",
                                                Text = Setting.enabled.Value ? "变速已激活" : "变速未激活",
                                                isOn = Setting.enabled.Value,
                                                FontColor = Setting.enabled.Value ? Color.green : Color.red,
                                                onValueChanged = (bool value,Toggle tg) =>
                                                {
                                                    Setting.enabled.Value = value;
                                                    tg.FontColor = Setting.enabled.Value ? Color.green : Color.red;
                                                    tg.Text = Setting.enabled.Value ? "变速已激活" : "变速未激活";
                                                },
                                                Element = { PreferredSize = { 250 , 0 } }
                                            },
                                            new Label()
                                            {
                                                Name = "Space",
                                                Text = "              ",
                                            },
                                            new TaiwuLabel()
                                            {
                                                Name = "Label",
                                                Text = "变速倍率",
                                                Element = { PreferredSize = { 150 , 0 } },
                                                UseBoldFont = true,
                                                UseOutline = true
                                            },
                                            new TaiwuLabel()
                                            {
                                                Name = "Value",
                                                Text = Setting.speedScale.Value.ToString(),
                                                BackgroundStyle = TaiwuLabel.Style.Value,
                                                Element = { PreferredSize = { 100 , 0 } }
                                            },
                                            new TaiwuSlider()
                                            {
                                                WholeNumber = true,
                                                Direction = UnityEngine.UI.Slider.Direction.LeftToRight,
                                                MaxValue = 16,
                                                MinValue = 1,
                                                Value = Setting.speedScale.Value,
                                                OnValueChanged = (float value , Slider sl) =>
                                                {
                                                    var lable = sl.Parent.Children[3] as TaiwuLabel;
                                                    lable.Text = value.ToString("n0");
                                                    Setting.speedScale.Value = value;
                                                }
                                            }
                                        },
                                        DefaultActive = false,
                                    }
                                },
                                DefaultActive = false,
                            },
                            new BoxAutoSizeModelGameObject()
                            {
                                Name = "MoreSetting",
                                Group =
                                {
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    Spacing = 3
                                },
                                SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                                Children =
                                {
                                    new TaiwuButton()
                                    {
                                        Name = "Button-Show",
                                        Text = "---扩展配置---",
                                        UseBoldFont = true,
                                        UseOutline = true,
                                        OnClick = (Button button) =>
                                        {
                                            for(int i = 1;i<button.Parent.Children.Count;i++)
                                                button.Parent.Children[i].SetActive(!button.Parent.Children[i].IsActive);
                                        },
                                        Element = { PreferredSize = { 0 , 50 } },
                                        FontColor = Color.white,
                                    },
                                    new BoxAutoSizeModelGameObject()
                                    {
                                        Name = "More",
                                        Group =
                                        {
                                            Direction = UnityUIKit.Core.Direction.Vertical,
                                            Spacing = 2
                                        },
                                        SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                                        Children =
                                        {
                                            new Container()
                                            {
                                                Name = "Setting 1",
                                                Element = { PreferredSize = { 0 , 50 } },
                                                Group =
                                                {
                                                    Spacing = 5,
                                                    Direction = UnityUIKit.Core.Direction.Horizontal
                                                },
                                                Children =
                                                {
                                                    new TaiwuLabel()
                                                    {
                                                        Name = "战斗首次有高于主角精纯",
                                                        Text = "战斗首次有高于主角精纯暂停变速",
                                                        UseBoldFont = true,
                                                        UseOutline = true,
                                                        Element = { PreferredSize = { 350, 0 } }
                                                    },
                                                    new TaiwuToggle()
                                                    {
                                                        Text = Setting.stopOnHiJingcunEnemy.Value ? "开" : "关",
                                                        isOn = Setting.stopOnHiJingcunEnemy.Value,
                                                        onValueChanged = (bool value, Toggle tg) =>
                                                        {
                                                            Setting.stopOnHiJingcunEnemy.Value = value;
                                                            tg.Text = Setting.stopOnHiJingcunEnemy.Value ? "开" : "关";
                                                        },
                                                        Element = { PreferredSize = { 50 } }
                                                    },
                                                    new TaiwuLabel()
                                                    {
                                                        Name = "最小精纯",
                                                        Text = "最小精纯",
                                                        UseBoldFont = true,
                                                        UseOutline = true,
                                                        Element = { PreferredSize = { 150, 0 } }
                                                    },
                                                    new TaiwuLabel()
                                                    {
                                                        Name = "Value",
                                                        Text = Setting.minJingcunExc.Value.ToString(),
                                                        BackgroundStyle = TaiwuLabel.Style.Value,
                                                        Element = { PreferredSize = { 100 , 0 } }
                                                    },
                                                    new TaiwuSlider()
                                                    {
                                                        WholeNumber = true,
                                                        Direction = UnityEngine.UI.Slider.Direction.LeftToRight,
                                                        MaxValue = 100,
                                                        MinValue = 1,
                                                        Value = Setting.minJingcunExc.Value,
                                                        OnValueChanged = (float value , Slider sl) =>
                                                        {
                                                            var lable = sl.Parent.Children[3] as TaiwuLabel;
                                                            lable.Text = value.ToString("n0");
                                                            Setting.minJingcunExc.Value = (int)value;
                                                        }
                                                    }
                                                }
                                            },
                                            new Container()
                                            {
                                                Element = { PreferredSize = { 0, 50 }},
                                                Group =
                                                {
                                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                                    Spacing = 5
                                                },
                                                Children =
                                                {
                                                    new TaiwuLabel()
                                                    {
                                                        Text = "读书开启时自动暂停变速",
                                                        UseBoldFont = true,
                                                        UseOutline = true,
                                                    },
                                                    new TaiwuToggle()
                                                    {
                                                        Text = Setting.stopOnReading.Value ? "开" : "关",
                                                        isOn = Setting.stopOnReading.Value,
                                                        onValueChanged = (bool value, Toggle tg) =>
                                                        {
                                                            Setting.stopOnReading.Value = value;
                                                            tg.Text = Setting.stopOnReading.Value ? "开" : "关";
                                                        },
                                                        Element = { PreferredSize = { 50 } }
                                                    },
                                                    new TaiwuLabel()
                                                    {
                                                        Text = "捕促织时自动暂停变速",
                                                        UseBoldFont = true,
                                                        UseOutline = true,
                                                    },
                                                    new TaiwuToggle()
                                                    {
                                                        Text = Setting.stopOnCatching.Value ? "开" : "关",
                                                        isOn = Setting.stopOnCatching.Value,
                                                        onValueChanged = (bool value, Toggle tg) =>
                                                        {
                                                            Setting.stopOnCatching.Value = value;
                                                            tg.Text = Setting.stopOnCatching.Value ? "开" : "关";
                                                        },
                                                        Element = { PreferredSize = { 50 } }
                                                    },
                                                }
                                            }
                                        },
                                        DefaultActive = false,
                                    }
                                },
                                DefaultActive = false
                            },
                        }
                    },
                    new BoxAutoSizeModelGameObject()
                    {
                        Name = "人物生育",
                        Group =
                        {
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            Spacing = 5
                        },
                        SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                        Children =
                        {
                            new TaiwuButton()
                            {
                                Name = "Button-Show",
                                Text = "人物生育",
                                UseBoldFont = true,
                                UseOutline = true,
                                FontColor = Color.white,
                                OnClick = (Button button) =>
                                {
                                    for(int i = 1;i<button.Parent.Children.Count;i++)
                                    {
                                        button.Parent.Children[i].SetActive(!button.Parent.Children[i].IsActive);
                                    }
                                },
                                Element =
                                {
                                    PreferredSize = { 0 , 50 }
                                }
                            },
                            new Container()
                            {
                                Name = "生育控制",
                                Group =
                                {
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 5,
                                },
                                Element =
                                {
                                    PreferredSize = { 0 , 50 }
                                },
                                Children =
                                {
                                    new TaiwuLabel()
                                    {
                                        Text = "不会怀蛐蛐",
                                        Element = { PreferredSize = { 200 , 0 }},
                                    },
                                    new TaiwuToggle()
                                    {
                                        Name = "怀蛐蛐的开关",
                                        isOn = Setting.ChildIsntQuqu.Value,
                                        Text = Setting.ChildIsntQuqu.Value ? "开" : "关",
                                        onValueChanged = (bool value,Toggle toggle) =>
                                        {
                                            toggle.Text = Setting.ChildIsntQuqu.Value ? "开" : "关";
                                            Setting.ChildIsntQuqu.Value = value;
                                            if(value)
                                                HarmonyPatches.PatchHandler["莫得蛐蛐"].Patch(HarmonyPatches.harmony);
                                            else
                                                HarmonyPatches.PatchHandler["莫得蛐蛐"].Unpatch(HarmonyPatches.harmony);
                                        },
                                        Element = { PreferredSize = { 50 , 50 } }
                                    },
                                    new TaiwuLabel()
                                    {
                                        Text = "同性生娃",
                                        Element = { PreferredSize = { 200 , 0 }},
                                    },
                                    new TaiwuToggle()
                                    {
                                        Name = "同性生娃的开关",
                                        isOn = Setting.ConceiveIgnoreSex.Value,
                                        Text = Setting.ConceiveIgnoreSex.Value ? "开" : "关",
                                        onValueChanged = (bool value,Toggle toggle) =>
                                        {
                                            toggle.Text = Setting.ConceiveIgnoreSex.Value ? "开" : "关";
                                            Setting.ConceiveIgnoreSex.Value = value;
                                        },
                                        Element = { PreferredSize = { 50 , 50 } },
                                        TipTitle = "同性生子",
                                        TipContant = "对太吾和 NPC 都有效"
                                    },
                                    new TaiwuLabel()
                                    {
                                        Text = "最大子嗣数量",
                                        Element = { PreferredSize = { 150 , 0 }},
                                    },
                                    new TaiwuToggle()
                                    {
                                        Name = "最大子嗣数量的开关",
                                        isOn = Setting.ChildrenMaxCountChange.Value,
                                        Text = Setting.ChildrenMaxCountChange.Value ? "开" : "关",
                                        onValueChanged = (bool value,Toggle toggle) =>
                                        {
                                            toggle.Text = Setting.ChildrenMaxCountChange.Value ? "开" : "关";
                                            Setting.ChildrenMaxCountChange.Value = value;
                                            if(value)
                                                HarmonyPatches.PatchHandler["最大孩子数修改"].Patch(HarmonyPatches.harmony);
                                            else
                                                HarmonyPatches.PatchHandler["最大孩子数修改"].Unpatch(HarmonyPatches.harmony);
                                            (toggle.Parent.Children.Last() as TaiwuInputField).ReadOnly = !value;
                                        },
                                        Element = { PreferredSize = { 50 , 50 }},
                                        TipTitle = "子嗣数量修改（太污是他们的 2.5 倍）",
                                        TipContant = "举个例子，当一对夫妻没有再婚，那么他们原本最多只会有一个子嗣，而太污是他们的 2.5 倍。修改后，他们最多的子嗣数量变为指定值。",
                                    },
                                    new TaiwuInputField()
                                    {
                                        ContentType = UnityEngine.UI.InputField.ContentType.IntegerNumber,
                                        InputType = UnityEngine.UI.InputField.InputType.AutoCorrect,
                                        Text = Setting.ChildrenMaxCount.Value.ToString(),
                                        OnEndEdit = (string value,InputField IF) =>
                                        {
                                            Setting.ChildrenMaxCount.Value = value.ParseInt();
                                        }
                                    }
                                },
                                DefaultActive = false,
                            },
                            new Container()
                            {
                                Name = "无中生有限制",
                                Group =
                                {
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 5,
                                },
                                Element =
                                {
                                    PreferredSize = { 0 , 50 }
                                },
                                Children =
                                {
                                    new TaiwuLabel()
                                    {
                                        Text = "没有无中生有的 Level 9",
                                        //Element = { PreferredSize = { 200 , 0 } }
                                    },
                                    new TaiwuToggle()
                                    {
                                        Name = "没有无中生有的 Level 9的开关",
                                        isOn = Setting.DontMakeLevel_9.Value,
                                        Text = Setting.DontMakeLevel_9.Value ? "开" : "关",
                                        onValueChanged = (bool value,Toggle toggle) => Setting.DontMakeLevel_9.Value = value,
                                        Element = { PreferredSize = { 50 , 50 } }
                                    },
                                },
                                DefaultActive = false,
                            }
                        }
                    }
                }
            };
        }
    }
}
