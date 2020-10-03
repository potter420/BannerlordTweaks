/*using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerlordTweaks.Patches
{
    [HarmonyPatch(typeof(DefaultTroopCountLimitModel), "GetHideoutBattlePlayerMaxTroopCount")]
    public class DefaultTroopCountLimitModelPatch
    {
        static bool Prefix(ref int __result)
        {
            __result = Math.Min(Settings.Instance.HideoutBattleTroopLimit, 90);
            return false;
        }

        static bool Prepare()
        {
            return Settings.Instance.HideoutBattleTroopLimitTweakEnabled;
        }
    }
}
*/
using HarmonyLib;
using ModLib.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.AiBehaviors;
using TaleWorlds.Core;


namespace BannerlordTweaks.Patches
{

	[HarmonyPatch(typeof(AiMilitaryBehavior), "AiHourlyTick")]
	[HarmonyPatch(new Type[]
{
	typeof(MobileParty),
	typeof(PartyThinkParams)
})]
	public class AiMilitaryBehaviorHourlyTickPatch
	{
		private static void Postfix(MobileParty mobileParty, PartyThinkParams p)
		{
			Hero owner = mobileParty.Party.Owner;
			if (mobileParty?.LeaderHero == null) return;
			Equipment owner_equipment = owner.BattleEquipment;
			float total_owner_armor = owner_equipment.GetHeadArmorSum() + owner_equipment.GetHumanBodyArmorSum() +
				owner_equipment.GetLegArmorSum() + owner_equipment.GetArmArmorSum();
			Equipment leader_equipment = owner.BattleEquipment;
			float total_leader_armor = leader_equipment.GetHeadArmorSum() + leader_equipment.GetHumanBodyArmorSum() +
				leader_equipment.GetLegArmorSum() + leader_equipment.GetArmArmorSum();
			if (total_owner_armor < 9000) return;
			foreach (KeyValuePair<AIBehaviorTuple, float> keyValuePair in p.AIBehaviorScores.ToList())
			{
				float value = keyValuePair.Value;
				IMapPoint target = keyValuePair.Key.Party;
				if (keyValuePair.Key.AiBehavior == AiBehavior.GoToSettlement)
				{
					p.AIBehaviorScores[keyValuePair.Key] = value * 1.7f;
				}
				else if (keyValuePair.Key.AiBehavior == AiBehavior.DefendSettlement || keyValuePair.Key.AiBehavior == AiBehavior.PatrolAroundPoint)
				{
					if (((Settlement)keyValuePair.Key.Party).OwnerClan == mobileParty.LeaderHero.Clan)
					{
						p.AIBehaviorScores[keyValuePair.Key] = value * 1.4f;
					}
					else
					{
						p.AIBehaviorScores[keyValuePair.Key] = value * 1.4f;
					}
				}
				else if (keyValuePair.Key.AiBehavior == AiBehavior.BesiegeSettlement || keyValuePair.Key.AiBehavior == AiBehavior.AssaultSettlement)
				{
					p.AIBehaviorScores[keyValuePair.Key] = value * 2.0f;
				}
				else if (keyValuePair.Key.AiBehavior == AiBehavior.RaidSettlement)
				{
					p.AIBehaviorScores[keyValuePair.Key] = 0f;
				}
				else if (keyValuePair.Key.AiBehavior == AiBehavior.EngageParty)
				{
					InformationManager.DisplayMessage(new InformationMessage("EngageParty: " + keyValuePair.Key.Party.Name?.ToString() + " " + keyValuePair.Value));
				}
			}
		}

		private static void Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				ModDebug.ShowError($"An exception occurred whilst trying to apply aI behaviours.", "", __exception);
			}
		}

		private static bool Prepare()
		{
			return true;
		}
	}
}