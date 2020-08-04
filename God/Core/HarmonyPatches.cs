using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using YanLib;

namespace God.Core
{
    public partial class HarmonyPatches
    {
        public static readonly Harmony harmony = new Harmony(God.GUID);
        public static readonly Type PatchesType = typeof(HarmonyPatches);
        public static readonly Dictionary<string, PatchHandler> AutoPatchHandlers = new Dictionary<string, PatchHandler>
        {
            { "使GetQuquWindow受变速影响", new PatchHandler
                {
                    TargetType = typeof(GetQuquWindow),
                    TargetMethonName = "LateUpdate",
                    Prefix = AccessTools.Method(PatchesType,"GetQuquWindow_LateUpdate_Prefix")
                }},
            { "战斗界面修正", new PatchHandler
            {
                TargetType = typeof(BattleSystem),
                TargetMethonName = "GetEnemy",
                Postfix = AccessTools.Method(PatchesType,"BattleSystem_GetEnemy_Postfix")
            }},
            { "男人怀孕", new PatchHandler()
                {
                    TargetType = typeof(PeopleLifeAI),
                    TargetMethonName = "AISetChildren",
                    Transpiler = AccessTools.Method(PatchesType,"ConceiveIgnoreSex_A")
            }},
            { "没有无中生有的 Level 9", new PatchHandler(){
                TargetType = typeof(DateFile),
                TargetMethonName = "MakeLevel9People",
                Prefix = AccessTools.Method(PatchesType,"DontMakeLevel9")
            }},
            { "同性怀孕", new PatchHandler()
                {
                    TargetType = typeof(PeopleLifeAI),
                    TargetMethonName = "DoTrunAIChange",
                    Transpiler = AccessTools.Method(PatchesType,"ConceiveIgnoreSex_B")
            }},
        };

        public static void Init()
        {
            God.Logger.LogInfo($"这玩意是多线程 Patch");
            foreach (var patch in AutoPatchHandlers)
            {
                try
                {
                    God.Logger.LogInfo($"Patch { patch.Key }");
                    patch.Value.Patch(harmony);
                }
                catch (Exception ex)
                {
                    God.Logger.LogError($"{ patch.Key } Patch Failed");
                    God.Logger.LogError(ex);
                }
            }

            harmony.PatchAll();
            God.Logger.LogInfo($"Patch 完了");
        }

        public static readonly Dictionary<string, PatchHandler> PatchHandler = new Dictionary<string, PatchHandler>()
        {
            {"莫得蛐蛐",new PatchHandler()
                {
                    TargetType = typeof(PeopleLifeAI),
                    TargetMethonName = "AISetChildren",
                    Transpiler = AccessTools.Method(PatchesType,"SetNotQuqu")
            }},
            {"最大孩子数修改",new PatchHandler()
                {
                    TargetType = typeof(PeopleLifeAI),
                    TargetMethonName = "AISetChildren",
                    Transpiler = AccessTools.Method(PatchesType,"ChildrenMaxCountChange")
            }}
        };

        static IEnumerable<CodeInstruction> SetNotQuqu(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var i in instructions)
            {
                if (i.opcode == OpCodes.Ldloc && ((LocalBuilder)i.operand).LocalIndex == 11)
                {
                    i.opcode = OpCodes.Ldc_I4_1;
                }
                yield return i;
            }
        }

        static IEnumerable<CodeInstruction> ConceiveIgnoreSex_A(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var i in instructions)
            {
                if (i.opcode == OpCodes.Ldloc && ((LocalBuilder)i.operand).LocalIndex == 4)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = AccessTools.Method(PatchesType, "ConceiveIgnoreSex");
                    yield return i;
                    yield return new CodeInstruction(OpCodes.Not);
                    continue;
                }
                yield return i;
            }
        }

        static IEnumerable<CodeInstruction> ConceiveIgnoreSex_B(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var i in instructions)
            {
                if(i.opcode == OpCodes.Ldloc && ((LocalBuilder)i.operand).LocalIndex == 799)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = AccessTools.Method(PatchesType, "ConceiveIgnoreSex");
                }
                yield return i;
            }
        }

        static IEnumerable<CodeInstruction> ChildrenMaxCountChange(IEnumerable<CodeInstruction> instructions)
        {
            bool foundStlocS7 = false;
            foreach(var i in instructions)
            {
                if (foundStlocS7)
                { 
                    i.opcode = OpCodes.Call;
                    i.operand = AccessTools.Method(PatchesType, "GetChildrenMaxValue");
                    foundStlocS7 = false;
                }
                if (i.opcode == OpCodes.Stloc_S && ((LocalBuilder)i.operand).LocalIndex == 7)
                    foundStlocS7 = true;
                yield return i;
            }
        }

        public static int GetChildrenMaxValue()
        {
            return God.Setting.ChildrenMaxCount.Value * 100;
        }

        public static bool ConceiveIgnoreSex()
        {
            return God.Setting.ConceiveIgnoreSex.Value;
        }

        static bool DontMakeLevel9()
        {
            return !God.Setting.DontMakeLevel_9.Value;
        }
    }
}
