#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("When killed, this actor causes the attacking player to receive money.")]
	class GivesBountyInfo : ConditionalTraitInfo
	{
		[Desc("Type of bounty. Used for targerting along with 'TakesBounty' trait on actors.")]
		public readonly string Type = "Bounty";

		[Desc("Stance the attacking player needs to receive the bounty.")]
		public readonly Stance ValidStances = Stance.Neutral | Stance.Enemy;

		[Desc("Whether to show a floating text announcing the won bounty.")]
		public readonly bool ShowBounty = true;

		[Desc("DeathTypes for which a bounty should be granted.",
			"Use an empty list (the default) to allow all DeathTypes.")]
		public readonly HashSet<string> DeathTypes = new HashSet<string>();

		public override object Create(ActorInitializer init) { return new GivesBounty(this); }
	}

	class GivesBounty : ConditionalTrait<GivesBountyInfo>, INotifyKilled
	{
		GainsExperience gainsExp;
		Cargo cargo;

		public GivesBounty(GivesBountyInfo info)
			: base(info) { }

		protected override void Created(Actor self)
		{
			base.Created(self);

			gainsExp = self.TraitOrDefault<GainsExperience>();
			cargo = self.TraitOrDefault<Cargo>();
		}

		// Returns 100's as 1, so as to keep accuracy for longer.
		int GetMultiplier(TakesBounty activeAttackerTakesBounty)
		{
			if (gainsExp == null)
				return 100;

			var slevel = gainsExp.Level;
			return (slevel > 0) ? slevel * activeAttackerTakesBounty.Info.LevelMod : 100;
		}

		int GetBountyValue(Actor self, TakesBounty activeAttackerTakesBounty)
		{
			// Divide by 10000 because of GetMultiplier and info.Percentage.
			return self.GetSellValue() * GetMultiplier(activeAttackerTakesBounty) * activeAttackerTakesBounty.Info.Percentage / 10000;
		}

		int GetDisplayedBountyValue(Actor self, TakesBounty activeAttackerTakesBounty)
		{
			var bounty = GetBountyValue(self, activeAttackerTakesBounty);
			if (cargo == null)
				return bounty;

			foreach (var a in cargo.Passengers)
			{
				var givesBounty = a.TraitOrDefault<GivesBounty>();
				if (givesBounty != null)
					bounty += givesBounty.GetDisplayedBountyValue(a, activeAttackerTakesBounty);
			}

			return bounty;
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (e.Attacker == null || e.Attacker.Disposed || IsTraitDisabled)
				return;

			var attackerTakesBounty = e.Attacker.TraitsImplementing<TakesBounty>().ToArray();
			var activeAttackerTakesBounty =	attackerTakesBounty.FirstOrDefault(tb => !tb.IsTraitDisabled && tb.Info.ValidTypes.Contains(Info.Type));
			if (activeAttackerTakesBounty == null)
				return;

			if (!Info.ValidStances.HasStance(e.Attacker.Owner.Stances[self.Owner]))
				return;

			if (Info.DeathTypes.Count > 0 && !e.Damage.DamageTypes.Overlaps(Info.DeathTypes))
				return;

			var displayedBounty = GetDisplayedBountyValue(self, activeAttackerTakesBounty);
			if (Info.ShowBounty && self.IsInWorld && displayedBounty > 0 && e.Attacker.Owner.IsAlliedWith(self.World.RenderPlayer))
				e.Attacker.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, e.Attacker.Owner.Color.RGB, FloatingText.FormatCashTick(displayedBounty), 30)));

			e.Attacker.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(GetBountyValue(self, activeAttackerTakesBounty));
		}
	}
}
