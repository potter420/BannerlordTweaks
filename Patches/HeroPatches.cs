using HarmonyLib;
using ModLib.Debugging;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using SandBox;

namespace BannerlordTweaks.Patches
{
    // Modify hero skill xp gain
    [HarmonyPatch(typeof(Hero), "AddSkillXp")]
    public class AddSkillXpPatch
    {
        private static FieldInfo hdFieldInfo = null;

        //private static float GetMultiplier()
        //{
        // if (Settings.Instance.HeroSkillExperienceOverrideMultiplierEnabled)
        //return Settings.Instance.HeroSkillExperienceMultiplier;
        //else
        //    return Math.Max(1, 0.0315769 * Math.Pow(skillLevel, 1.020743));
        //}

        static bool Prefix(Hero __instance, SkillObject skill, float xpAmount)
        {
            try
            {
                if (hdFieldInfo == null) GetFieldInfo();

                HeroDeveloper hd = (HeroDeveloper)hdFieldInfo.GetValue(__instance);

                if (hd != null)
                {
                    if (xpAmount > 0)
                    {
                        float newXpAmount = (int)Math.Ceiling(xpAmount * Settings.Instance.HeroSkillExperienceMultiplier);
                        hd.AddSkillXp(skill, newXpAmount, true, true);
                    }
                    else
                        hd.AddSkillXp(skill, xpAmount, true, true);
                }
            }
            catch (Exception ex)
            {
                ModDebug.ShowError($"An exception occurred whilst trying to apply the hero xp multiplier.", "", ex);
            }
            return false;
        }

        static bool Prepare()
        {
            if (Settings.Instance.HeroSkillExperienceMultiplierEnabled)
                GetFieldInfo();
            return Settings.Instance.HeroSkillExperienceMultiplierEnabled;
        }

        private static void GetFieldInfo()
        {
            hdFieldInfo = typeof(Hero).GetField("_heroDeveloper", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
    // modify Hero learning limits
    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningLimit", new Type[]
    {
    typeof(Hero),
    typeof(SkillObject),
    typeof(StatExplainer)
    })]
    internal class AddCalculateLearningLimitPatchFirst
    {
        public static void Postfix(ref int __result)
        {
            bool learningPatch = Settings.Instance.HeroSkillExperienceMultiplierEnabled;
            if (learningPatch)
            {
                float learningLimitMultiplier = 20;
                int minimumLearningLimit = 200;
                int num = __result;
                __result = MBMath.ClampInt((int)Math.Ceiling((float)num * learningLimitMultiplier), minimumLearningLimit, 99999);
            }
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningLimit", new Type[]
{
    typeof(int),
    typeof(int),
    typeof(TextObject),
    typeof(StatExplainer)
})]
    internal class AddCalculateLearningLimitPatchSecond
    {
        public static void Postfix(ref int __result)
        {
            bool learningPatch = Settings.Instance.HeroSkillExperienceMultiplierEnabled;
            if (learningPatch)
            {
                float learningLimitMultiplier = 20;
                int minimumLearningLimit = 200;
                int num = __result;
                __result = MBMath.ClampInt((int)Math.Ceiling((float)num * learningLimitMultiplier), minimumLearningLimit, 99999);
            }
            
        }
    }
    // Change perk parameters
    [HarmonyPatch(typeof(DefaultPerks), "InitializePerks")]
    public class PerkMechanicChangePatch
    {
        public static void Postfix(DefaultPerks __instance)
        {
        int[] _tierSkillRequirements = new int[12]
            {
            25,
            50,
            75,
            100,
            125,
            150,
            175,
            200,
            225,
            250,
            275,
            300
        };
        FieldInfo skillInstance = typeof(DefaultPerks).GetField("AthleticsDexterous", BindingFlags.Instance | BindingFlags.NonPublic);
        PerkObject AthleticsDexterous = (PerkObject)skillInstance.GetValue(__instance);
        skillInstance = typeof(DefaultPerks).GetField("AthleticsEndurance", BindingFlags.Instance | BindingFlags.NonPublic);
        PerkObject AthleticsEndurance = (PerkObject)skillInstance.GetValue(__instance);
        AthleticsDexterous.Initialize("{=aEWuaOgQ}Dexterous", "{=gd5HkorH}+30% movement speed.", DefaultSkills.Athletics, _tierSkillRequirements[3-1], AthleticsEndurance, SkillEffect.PerkRole.PartyMember, 0.03f, SkillEffect.PerkRole.None, 0f, SkillEffect.EffectIncrementType.AddFactor);
        }
    }
    // Apply god mode
    [HarmonyPatch(typeof(SandboxAgentStatCalculateModel), "UpdateHumanStats")]
    public class AgentStatCalculateModelPatch
    {
        public static void Postfix(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            
            if (agent != null && agentDrivenProperties!= null)  
            {
                if (!agent.IsHero && agent.IsHuman)
                {
                    try
                    {
                        Agent leader = agent.Team.Leader;
                        Equipment leader_equipment = leader.SpawnEquipment;
                        float total_armor = leader_equipment.GetHeadArmorSum() + leader_equipment.GetHumanBodyArmorSum() +
                            leader_equipment.GetLegArmorSum() + leader_equipment.GetArmArmorSum();
                        EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                        MissionEquipment equipment = agent.Equipment;
                        WeaponComponentData weaponComponentData = (wieldedItemIndex3 != EquipmentIndex.None) ? equipment[wieldedItemIndex3].CurrentUsageItem : null;
                        if (total_armor > 9000) {
                            agentDrivenProperties.ArmorHead += leader_equipment.GetHeadArmorSum() / 10;
                            agentDrivenProperties.ArmorTorso += leader_equipment.GetHumanBodyArmorSum() / 10;
                            agentDrivenProperties.ArmorLegs += leader_equipment.GetLegArmorSum() / 10;
                            agentDrivenProperties.ArmorArms += leader_equipment.GetArmArmorSum() / 10;
                            agent.HealthLimit = Math.Min(agent.HealthLimit + (total_armor / 100) * (total_armor > 9000 ? 3f : 0.5f), 50000f);
                            agent.Health = agent.HealthLimit;
                            agentDrivenProperties.SwingSpeedMultiplier += (total_armor > 9000 ? 1f : 0f);
                            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier += (total_armor > 9000 ? 1f : 0f);
                            agentDrivenProperties.WeaponInaccuracy *= (total_armor > 9000 ? 0.01f : 1f);
                            agentDrivenProperties.MaxSpeedMultiplier *= (total_armor > 9000 ? 2f : 1f);
                            agentDrivenProperties.WeaponUnsteadyBeginTime = (total_armor > 9000 ? 0.01f : 1f);
                            agentDrivenProperties.WeaponUnsteadyEndTime = (total_armor > 9000 ? 0.05f : 1f);
                            agentDrivenProperties.ReloadSpeed = (total_armor > 9000 ? 0.01f : 0.93f);
                        }
                        

                        //ModDebug.ShowMessage($"Test: ArmorHead {agentDrivenProperties.ArmorHead} | ArmorTorso {agentDrivenProperties.ArmorTorso} | ArmorLegs {agentDrivenProperties.ArmorLegs} | ArmorArms {agentDrivenProperties.ArmorArms}" +
                        //    $"Test: HealthLimit {agent.HealthLimit}"
                        //        , "Test");
                    }
                    catch (Exception ex)
                    {
                        ModDebug.ShowError($"An exception occurred whilst trying to apply the god mode", "", ex);
                    }
                }
            }
        }
    }
    // Combat simulation
    [HarmonyPatch(typeof(DefaultCombatSimulationModel), nameof(DefaultCombatSimulationModel.SimulateHit))]
    public static class SimulateHitPatches
    {
        private static bool Prefix(ref int __result, CharacterObject strikerTroop, CharacterObject strikedTroop, PartyBase strikerParty, PartyBase strikedParty, float strikerAdvantage, MapEvent battle)
        {
            // When attacking
            Equipment attacker_owner_equipment = strikerParty.Owner.BattleEquipment;
            float total_attacker_armor = attacker_owner_equipment.GetHeadArmorSum() + attacker_owner_equipment.GetHumanBodyArmorSum() +
                        attacker_owner_equipment.GetLegArmorSum() + attacker_owner_equipment.GetArmArmorSum();
            // When defending
            Equipment defender_owner_equipment = strikedParty.Owner.BattleEquipment;
            float total_defender_armor = defender_owner_equipment.GetHeadArmorSum() + defender_owner_equipment.GetHumanBodyArmorSum() +
                        defender_owner_equipment.GetLegArmorSum() + defender_owner_equipment.GetArmArmorSum();
            if (total_attacker_armor > 9000)
            {
                __result = (int)((strikerTroop.GetPower() / strikedTroop.GetPower()) * strikerAdvantage * total_attacker_armor/50);
                
                return false;
            }
            if (total_defender_armor > 9000)
            {
                __result = Math.Max((int)(((strikerTroop.GetPower() / strikedTroop.GetPower()) * strikerAdvantage) - total_defender_armor/10 ),0);
                return false;
            }
            return true;
        }
    }
    // More movement speed
    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), nameof(DefaultPartySpeedCalculatingModel.CalculateFinalSpeed))]
    public static class PartyCalculateFinalSpeedPatches
    {
        public static void Postfix(ref float __result, MobileParty mobileParty, float baseSpeed, StatExplainer explanation)
        {
            if (mobileParty != null)
            {
                try
                {
                    Hero owner = mobileParty.Party.Owner;
                    if (mobileParty.LeaderHero == null) return;
                    Hero leader = mobileParty.LeaderHero;
                    Equipment owner_equipment = owner.BattleEquipment;
                    float total_owner_armor = owner_equipment.GetHeadArmorSum() + owner_equipment.GetHumanBodyArmorSum() +
                        owner_equipment.GetLegArmorSum() + owner_equipment.GetArmArmorSum();
                    Equipment leader_equipment = owner.BattleEquipment;
                    float total_leader_armor = leader_equipment.GetHeadArmorSum() + leader_equipment.GetHumanBodyArmorSum() +
                        leader_equipment.GetLegArmorSum() + leader_equipment.GetArmArmorSum();
                    float modSpeed = 0f;
                    if (total_owner_armor > 9000)
                    {
                        modSpeed += 2f;
                    }
                    if (total_leader_armor > 9000)
                    {
                        modSpeed += 2f;
                        if (leader.IsFactionLeader)
                        {
                            modSpeed += 3f;
                        }
                        __result += modSpeed;
                    }
                }
                catch (Exception ex)
                {
                    ModDebug.ShowError($"An exception occurred whilst trying to apply the hero xp multiplier.", "", ex);
                }

            }


        }
    }
    [HarmonyPatch(typeof(MobileParty), "DailyTick")]
    public static class TroopDailyXpPatches
    {
        public static void Postfix(MobileParty __instance)
        {
            if (__instance != null)
            {
                Hero owner = __instance.Party.Owner;
                if (owner == null) return;
                Equipment owner_equipment = owner.BattleEquipment;
                float total_owner_armor = owner_equipment.GetHeadArmorSum() + owner_equipment.GetHumanBodyArmorSum() +
                    owner_equipment.GetLegArmorSum() + owner_equipment.GetArmArmorSum();
                if (total_owner_armor > 9000)
                {
                    foreach (CharacterObject troop in __instance.MemberRoster.Troops)
                    {
                        int troopPerksXp = 50000;
                        __instance.Party.MemberRoster.AddXpToTroop(troopPerksXp, troop);
                    }
                }
            }
        }
    }
}