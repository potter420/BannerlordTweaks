// ModLib.SmeltingHelper
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace BannerlordTweaks
{
	public static class SmeltingHelper
	{
		public static IEnumerable<CraftingPiece> GetNewPartsFromSmelting(ItemObject item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			return from x in item.WeaponDesign.UsedPieces
				   select x.CraftingPiece into x
				   where x != null && x.IsValid && !Campaign.Current.GetCampaignBehavior<CraftingCampaignBehavior>().IsOpened(x)
				   select x;
		}
	}
}
