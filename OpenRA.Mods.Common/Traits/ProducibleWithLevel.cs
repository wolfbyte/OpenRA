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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Actors possessing this trait should define the GainsExperience trait. When the prerequisites are fulfilled, ",
		"this trait grants a level-up to newly spawned actors. If additionally the actor's owning player defines the ProductionIconOverlay ",
		"trait, the production queue icon renders with an overlay defined in that trait.")]
	public class ProducibleWithLevelInfo : ITraitInfo, Requires<GainsExperienceInfo>
	{
		public readonly string[] Prerequisites = { };

		[Desc("Only grant this level for certain factions.")]
		public readonly HashSet<string> Factions = new HashSet<string>();

		[Desc("Should it recheck everything when it is captured?")]
		public readonly bool ResetOnOwnerChange = false;

		[Desc("Number of levels to give to the actor on creation.")]
		public readonly int InitialLevels = 1;

		[Desc("Should the level-up animation be suppressed when actor is created?")]
		public readonly bool SuppressLevelupAnimation = true;

		public object Create(ActorInitializer init) { return new ProducibleWithLevel(init, this); }
	}

	public class ProducibleWithLevel : INotifyCreated, INotifyOwnerChanged
	{
		readonly ProducibleWithLevelInfo info;
		string faction;

		public ProducibleWithLevel(ActorInitializer init, ProducibleWithLevelInfo info)
		{
			this.info = info;

			faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : init.Self.Owner.Faction.InternalName;
		}

		public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (info.ResetOnOwnerChange)
				faction = newOwner.Faction.InternalName;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (info.Factions.Any() && !info.Factions.Contains(faction))
				return;

			if (info.Prerequisites.Any() && !self.Owner.PlayerActor.Trait<TechTree>().HasPrerequisites(info.Prerequisites))
				return;

			var ge = self.Trait<GainsExperience>();
			if (!ge.CanGainLevel)
				return;

			ge.GiveLevels(info.InitialLevels, info.SuppressLevelupAnimation);
		}
	}
}
