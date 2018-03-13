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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Yupgi_alert.Traits
{
	[Desc("Gives additional cash when resources are delivered to refineries.")]
	public class ResourcePurifierInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Percentage value of the resource to grant as cash.")]
		public readonly int Modifier = 25;

		[Desc("Only apply the modifier, if the resources are dumped to one of this actors.")]
		public readonly HashSet<string> RefineriesToPurify = new HashSet<string>();

		[Desc("Only apply the modifier, if the resources are dumped by one of this actors.")]
		public readonly HashSet<string> HarvestersToPurify = new HashSet<string>();

		public readonly bool ShowTicks = true;

		[Desc("If true, show the ticks on the refinery, if false show it on this actor.")]
		public readonly bool ShowTicksOnRefinery = true;

		public readonly int TickLifetime = 30;
		public readonly int TickVelocity = 2;
		public readonly int TickRate = 10;

		public override object Create(ActorInitializer init) { return new ResourcePurifier(init.Self, this); }
	}

	public class ResourcePurifier : ConditionalTrait<ResourcePurifierInfo>, ITick, IResourcePurifier, INotifyOwnerChanged
	{
		readonly ResourcePurifierInfo info;

		PlayerResources playerResources;
		int currentDisplayTick = 0;
		int currentDisplayValue = 0;

		bool IResourcePurifier.ShowTicksOnRefinery { get { return info.ShowTicksOnRefinery; } }

		public ResourcePurifier(Actor self, ResourcePurifierInfo info)
			: base(info)
		{
			this.info = info;

			// Special case handling is required for the Player actor.
			// Created is called before Player.PlayerActor is assigned,
			// so we must query other player traits from self, knowing that
			// it refers to the same actor as self.Owner.PlayerActor
			var playerActor = self.Info.Name == "player" ? self : self.Owner.PlayerActor;

			playerResources = playerActor.Trait<PlayerResources>();
		}

		int IResourcePurifier.RefineAmount(int amount, string refinery, string harvester)
		{
			if (IsTraitDisabled)
				return 0;

			if (info.RefineriesToPurify.Any() && !info.RefineriesToPurify.Contains(refinery))
				return 0;

			if (info.HarvestersToPurify.Any() && !info.HarvestersToPurify.Contains(harvester))
				return 0;

			var cash = Util.ApplyPercentageModifiers(amount, new int[] { info.Modifier });
			playerResources.GiveCash(cash);

			if (info.ShowTicks && !info.ShowTicksOnRefinery)
				currentDisplayValue += cash;

			return cash;
		}

		void ITick.Tick(Actor self)
		{
			if (info.ShowTicks && !info.ShowTicksOnRefinery && currentDisplayValue > 0 && --currentDisplayTick <= 0)
			{
				var temp = currentDisplayValue;
				if (self.Owner.IsAlliedWith(self.World.RenderPlayer))
					self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.Owner.Color.RGB, FloatingText.FormatCashTick(temp), 30)));
				currentDisplayTick = info.TickRate;
				currentDisplayValue = 0;
			}
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			playerResources = newOwner.PlayerActor.Trait<PlayerResources>();
		}
	}
}
