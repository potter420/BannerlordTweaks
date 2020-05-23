﻿using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerlordTweaks
{
    public class DailyTroopExperienceTweak
    {
        public static void Apply(Campaign campaign)
        {
            var obj = new DailyTroopExperienceTweak();
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(obj, (MobileParty mp) => { obj.DailyTick(mp); });
        }

        private void DailyTick(MobileParty party)
        {
            if (party.LeaderHero != null)
            {
                int count = party.MemberRoster.Troops.Count();
                if (party.LeaderHero == Hero.MainHero || Settings.Instance.DailyTroopExperienceApplyToAllNPC ||
                    Settings.Instance.DailyTroopExperienceApplyToPlayerClanMembers && party.LeaderHero.Clan == Clan.PlayerClan)
                {
                    int experienceAmount = ExperienceAmount(party.LeaderHero);
                    if (experienceAmount > 0)
                    {
                        foreach (var troop in party.MemberRoster.Troops)
                        {
                            party.MemberRoster.AddXpToTroop(experienceAmount, troop);
                        }

                        if (Settings.Instance.DisplayMessageDailyExperienceGain)
                        {
                            string troops = count == 1 ? "soldier" : "troops";
                            //Debug
                            //InformationManager.DisplayMessage(new InformationMessage($"{party.LeaderHero.Name}'s party granted {experienceAmount} experience to {count} {troops}."));
                            if (party.LeaderHero == Hero.MainHero)
                                InformationManager.DisplayMessage(new InformationMessage($"Granted {experienceAmount} experience to {count} {troops}."));
                        }
                    }
                }
            }
        }

        private static int ExperienceAmount(Hero h)
        {
            int leadership = h.GetSkillValue(DefaultSkills.Leadership);
            if (leadership >= Settings.Instance.DailyTroopExperienceRequiredLeadershipLevel)
                return (int)(Settings.Instance.LeadershipPercentageForDailyExperienceGain * leadership);
            return 0;
        }
    }
}
