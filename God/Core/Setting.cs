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
        /// 配置
        /// </summary>
        public ConfigFile Config { get; private set; }

        public void Init(ConfigFile Config)
        {
            enabled = Config.Bind("Setting", "enable", true, "是否开启 Mod");
            speedScale = Config.Bind("Setting", "speedScale", 8f, "速度倍率");
            minJingcunExc = Config.Bind("Setting", "minJingcunExc", 1, "最小精纯超出值");
            stopOnHiJingcunEnemy = Config.Bind("Setting", "stopOnHiJingcunEnemy", true, "面对高精纯敌人时暂停变速");
            stopOnReading = Config.Bind("Setting", "stopOnReading", true, "读书时暂停变速");
            stopOnCatching = Config.Bind("Setting", "stopOnCatching", true, "捉蟋蟀时暂停变速");
            hotKeyEnable = Config.Bind("Setting", "hotKeyEnable", new KeyboardShortcut(KeyCode.N,new KeyCode[] { KeyCode.LeftControl }), "激活变速热键");
        }
    }
}
