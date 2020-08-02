using God.Core;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace God
{
    public partial class HarmonyPatches
    {
        private static bool GetQuquWindow_LateUpdate_stopPatching = false;
        private static float GetQuquWindow_LateUpdate_fixDeltaTime = 0;
        private static bool GetQuquWindow_LateUpdate_Prefix(GetQuquWindow __instance, MethodBase __originalMethod)
        {
            if (GetQuquWindow_LateUpdate_stopPatching)
                return true;

            // 没有激活变速时不patch避免功能未激活时对原游戏功能可能产生的影响
            if (!GameSpeeder.IsTimePatchEnable())
                return true;

            float realDeltaTime = Time.unscaledDeltaTime;
            GetQuquWindow_LateUpdate_fixDeltaTime += Time.deltaTime;
            float fFramePass = GetQuquWindow_LateUpdate_fixDeltaTime / realDeltaTime;
            int nFramePass = (int)Math.Floor(fFramePass);
            GetQuquWindow_LateUpdate_fixDeltaTime -= nFramePass * realDeltaTime;

            while (nFramePass-- > 0)
            {
                if (!__instance.getQuquWindow.activeSelf)
                    return false;

                GetQuquWindow_LateUpdate_stopPatching = true;
                __originalMethod.Invoke(__instance, null);
                GetQuquWindow_LateUpdate_stopPatching = false;
            }

            return false;
        }


        private static bool BattleSystem_GetEnemy__thisBattleAlreadySet = false;
        private static void BattleSystem_GetEnemy_Postfix(BattleSystem __instance, bool newBattle)
        {
            if (newBattle)
                BattleSystem_GetEnemy__thisBattleAlreadySet = false;
            else if (BattleSystem_GetEnemy__thisBattleAlreadySet)
                return;

            int nEnemyId = __instance.ActorId(false, false);
            int nPlayerId = __instance.ActorId(true, false);
            int enemyJingCunPoint = int.Parse(DateFile.instance.GetActorDate(nEnemyId, 901, false));
            int playerJingCunPoint = int.Parse(DateFile.instance.GetActorDate(nPlayerId, 901, false));
            // Main.Logger.Log("New Enemy Entered " + nEnemyId + " " + enemyJingCunPoint);
            if (God.Core.God.Setting.stopOnHiJingcunEnemy.Value
                && enemyJingCunPoint - playerJingCunPoint >= God.Core.God.Setting.minJingcunExc.Value)
            {
                BattleSystem_GetEnemy__thisBattleAlreadySet = true;
                GameSpeeder.ApplyTimeScale(false);
            }
        }

        #region Patches - Auto
        /// <summary>
        /// 解决某些情况下变招成功无法触发变招效果的问题
        /// </summary>
        /// <remarks>
        /// 猜测原作者此修复的原理:
        /// 游戏中<see cref="BattleSystem.AttackPartChooseEnd"/>是个协程，第一次运行时会等待现实时间的0.2s后继续执行，
        /// 加速后0.2s会发生很多游戏战斗事件，可能会在<see cref="BattleSystem.AttackPartChooseEnd"/>继续执行(触发
        /// 变招效果)之前覆盖变招效果，导致变招效果再也没机会触发。此修复是将<see cref="Time.timeScale"/> 设为0，此时
        /// 游戏中除frame rate dependent的事件外，全部事件暂停，协程<see cref="BattleSystem.AttackPartChooseEnd"/>
        /// 等待时间改为0，即为等待一帧后继续执行，可以避免变招效果在等待期间被覆盖。
        /// </remarks>
        [HarmonyPatch(typeof(BattleSystem), "SetChooseAttackPart")]
        public static class SetChooseAttackPartPatch
        {
            private static void Postfix() => Time.timeScale = 0; // Fix变招的逻辑step 1
        }

        [HarmonyPatch(typeof(BattleSystem), "AttackPartChooseEnd")]
        public static class AttackPartChooseEndPatch
        {
            private static void Prefix(ref float waitTime) => waitTime = 0; // Fix变招的逻辑step 2
        }

        [HarmonyPatch(typeof(BattleEndWindow), "BattleEnd")]
        public static class ExitBattlePatch
        {
            private static void Postfix() => GameSpeeder.ApplyTimeScale(God.Core.God.Setting.enabled.Value);
        }

        // 这个函数产生的协程使用了WaitForSecondsRealtime，timescale无法直接变速，故需patch it
        [HarmonyPatch(typeof(BattleSystem), "TimePause")]
        public static class TimePausePatch
        {
            private static void Prefix(ref float autoTime)
            {
                // 变速配置激活状态且倍速>1才改这个等待时间
                if (God.Core.God.Setting.enabled.Value && God.Core.God.Setting.speedScale.Value > 1)
                    autoTime /= God.Core.God.Setting.speedScale.Value;
            }
        }

        [HarmonyPatch(typeof(ReadBook), "SetReadBookWindow")]
        public static class StartReadBook
        {
            private static void Postfix() 
            {
                if (God.Core.God.Setting.stopOnReading.Value)
                    GameSpeeder.ApplyTimeScale(false);
            }
        }

        [HarmonyPatch(typeof(ReadBook), "CloseReadBookWindow")]
        public static class EndReadBook
        {
            private static void Postfix() => GameSpeeder.ApplyTimeScale(God.Core.God.Setting.enabled.Value);
        }

        [HarmonyPatch(typeof(GetQuquWindow), "ShowGetQuquWindow")]
        public static class StartCatching
        {
            private static void Postfix()
            {
                if (God.Core.God.Setting.stopOnCatching.Value)
                    GameSpeeder.ApplyTimeScale(false);

                GetQuquWindow_LateUpdate_fixDeltaTime = 0;
            }
        }

        [HarmonyPatch(typeof(GetQuquWindow), "CloseGetQuquWindow")]
        public static class EndCatching
        {
            private static void Postfix() => GameSpeeder.ApplyTimeScale(God.Core.God.Setting.enabled.Value);
        }
        #endregion
    }

    

    public static class GameSpeeder
    {
        public class GameSpeeder_Looper : MonoBehaviour
        {
            private void LateUpdate() => GameSpeeder.CheckPerFrame();
        }

        private static GameSpeeder_Looper _looper = null;

        /// <summary>当前设定的时间流逝速度</summary>
        public static float lastTimeScale = 1f;
        /// <summary>游戏中默认的时间流逝速度</summary>
        public static float realTimeScale = 1f;
        
        private static bool _isHotKeyHangUp = false;

        public static bool Load()
        {
            lastTimeScale = realTimeScale = Time.timeScale;
            if (_looper == null)
            {
                _looper = new GameObject().AddComponent(
                    typeof(GameSpeeder_Looper)) as GameSpeeder_Looper;
                UnityEngine.Object.DontDestroyOnLoad(_looper);
            }
            return true;
        }

        public static bool IsTimePatchEnable() => lastTimeScale != realTimeScale;

        public static void ApplyTimeScale(bool enable, bool updateSetting = false)
        {
            if (updateSetting)
                God.Core.God.Setting.enabled.Value = enable;
            if (lastTimeScale != Time.timeScale)
                realTimeScale = Time.timeScale;
            if (God.Core.God.Setting.enabled.Value)
                lastTimeScale = Time.timeScale = realTimeScale * God.Core.God.Setting.speedScale.Value * 1.00001f; // * 1.00001f方便检测之后游戏逻辑是否对Time.timeScale作了更改
            else
                lastTimeScale = Time.timeScale = realTimeScale;
        }
        /// <summary>当前状态有按键被按下</summary>
        private static bool _keyCurrentlyHeldDown = false;
        public static void CheckPerFrame()
        {
            if (lastTimeScale != Time.timeScale) // may be changed in game logic
            {
                ApplyTimeScale(God.Core.God.Setting.enabled.Value);
            }

            if (_isHotKeyHangUp)
                _isHotKeyHangUp = false;
            else if (God.Core.God.Setting.hotKeyEnable.Value.IsDown())
                ApplyTimeScale(!God.Core.God.Setting.enabled.Value, true);

            if (Input.anyKey) // 任何键盘按键按下期间停止变速
            {
                _keyCurrentlyHeldDown = true;
                lastTimeScale = Time.timeScale = realTimeScale;
            }
            else if (_keyCurrentlyHeldDown)
            {
                _keyCurrentlyHeldDown = false;
                ApplyTimeScale(God.Core.God.Setting.enabled.Value); // 恢复变速
            }
        }

    }
}


