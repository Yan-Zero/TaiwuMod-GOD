using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YanLib;

namespace God.Core
{
    public partial class HarmonyPatches
    {
        public static readonly Harmony harmony = new Harmony(God.GUID);
        public static readonly Type PatchesType = typeof(HarmonyPatches);
        public static readonly Dictionary<string, YanLib.PatchHandler> PatchHandles = new Dictionary<string, PatchHandler>
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
        };

        public static void Init()
        {
            foreach (var patch in PatchHandles)
            {
                try
                {
                    patch.Value.Patch(harmony);
                }
                catch (Exception ex)
                {
                    God.Logger.LogError($"{ patch.Key } Patch Failed");
                    God.Logger.LogError(ex);
                }
            }

            harmony.PatchAll();
        }
    }
}
