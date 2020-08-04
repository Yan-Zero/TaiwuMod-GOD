using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace God.Core
{
    public class Setting
    {
        /// <summary>是否生效</summary>
        public ConfigEntry<bool> enabled;
        /// <summary>速度倍率</summary>
        public ConfigEntry<float> speedScale;
        /// <summary>最小精纯超出值</summary>
        public ConfigEntry<int> minJingcunExc;
        /// <summary>面对高精纯敌人时暂停变速</summary>
        public ConfigEntry<bool> stopOnHiJingcunEnemy;
        /// <summary>读书时暂停变速</summary>
        public ConfigEntry<bool> stopOnReading;
        /// <summary>捉蟋蟀时暂停变速</summary>
        public ConfigEntry<bool> stopOnCatching;
        /// <summary>激活变速热键</summary>
        public ConfigEntry<KeyboardShortcut> hotKeyEnable;

        /// <summary>
        /// 孩子8是🦗
        /// </summary>
        public ConfigEntry<bool> ChildIsntQuqu;
        /// <summary>
        /// 忽略性别
        /// </summary>
        public ConfigEntry<bool> ConceiveIgnoreSex;
        /// <summary>
        /// 最大的孩子数量
        /// </summary>
        public ConfigEntry<int> ChildrenMaxCount;
        /// <summary>
        /// 最大孩子数量修改
        /// </summary>
        public ConfigEntry<bool> ChildrenMaxCountChange;
        /// <summary>
        /// 莫得无中生有的 Level 9
        /// </summary>
        public ConfigEntry<bool> DontMakeLevel_9;

        /// <summary>
        /// 配置
        /// </summary>
        public ConfigFile Config { get; private set; }

        public void Init(ConfigFile Config)
        {
            enabled = Config.Bind("游戏变速", "enable", true, "是否开启 Mod");
            speedScale = Config.Bind("游戏变速", "speedScale", 1f, "速度倍率");
            minJingcunExc = Config.Bind("游戏变速", "minJingcunExc", 1, "最小精纯超出值");
            stopOnHiJingcunEnemy = Config.Bind("游戏变速", "stopOnHiJingcunEnemy", true, "面对高精纯敌人时暂停变速");
            stopOnReading = Config.Bind("游戏变速", "stopOnReading", true, "读书时暂停变速");
            stopOnCatching = Config.Bind("游戏变速", "stopOnCatching", true, "捉蟋蟀时暂停变速");
            hotKeyEnable = Config.Bind("游戏变速", "hotKeyEnable", new KeyboardShortcut(KeyCode.N, new KeyCode[] { KeyCode.LeftControl }), "激活变速热键");

            ChildrenMaxCount = Config.Bind("人物生育", "ChildrenMaxCount", 1, "NPC 最大的孩子数量");
            ChildrenMaxCountChange = Config.Bind("人物生育", "ChildrenMaxCountChange", true, "NPC 最大的孩子数量修改");
            if(ChildrenMaxCountChange.Value)
                HarmonyPatches.PatchHandler["最大孩子数修改"].Patch(HarmonyPatches.harmony);

            ChildIsntQuqu = Config.Bind("人物生育", "ChildIsntQuqu", true, "孩子不是蛐蛐");
            if(ChildIsntQuqu.Value)
                HarmonyPatches.PatchHandler["莫得蛐蛐"].Patch(HarmonyPatches.harmony);

            ConceiveIgnoreSex = Config.Bind("人物生育", "ConceiveIgnoreSex", true, "怀孕忽略性别");
            DontMakeLevel_9 = Config.Bind("人物生育", "DontMakeLevel_9", true, "莫得无中生有的 Level 9");
        }
    }
}