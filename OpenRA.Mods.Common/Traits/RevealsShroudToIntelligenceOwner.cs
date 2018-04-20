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
	public class RevealsShroudToIntelligenceOwnerInfo : RevealsShroudInfo
	{
		[FieldLoader.Require]
		[Desc("Types of intelligence this trait requires.")]
		public readonly HashSet<string> Types = new HashSet<string>();

		public override object Create(ActorInitializer init) { return new RevealsShroudToIntelligenceOwner(init.Self, this); }
	}

	public class RevealsShroudToIntelligenceOwner : RevealsShroud, INotifyAddedToWorld, ITick
	{
		readonly Actor self;
		readonly RevealsShroudToIntelligenceOwnerInfo info;
		public List<Player> intelOwners = new List<Player>();

		public RevealsShroudToIntelligenceOwner(Actor self, RevealsShroudToIntelligenceOwnerInfo info)
			: base(self, info)
		{
			this.info = info;
			this.self = self;
			self.World.ActorAdded += ActorAdded;
			self.World.ActorRemoved += ActorRemoved;
		}

		public void ActorAdded(Actor a)
		{
			if (self.Owner.NonCombatant)
				return;

			var givesIntel = a.TraitOrDefault<GivesIntelligence>();
			if (givesIntel != null && givesIntel.Types.Overlaps(info.Types))
			{
				var cells = ProjectedCells(self);
				if (!intelOwners.Contains(a.Owner))
				{
					intelOwners.Add(a.Owner);
					RemoveCellsFromPlayerShroud(self, a.Owner);
					AddCellsToPlayerShroud(self, a.Owner, cells);
				}
			}
		}

		public void ActorRemoved(Actor a)
		{
			if (self.Owner.NonCombatant)
				return;

			var givesIntel = a.TraitOrDefault<GivesIntelligence>();
			if (givesIntel != null && givesIntel.Types.Overlaps(info.Types))
			{
				// Ensure there is no other actor that gives intelligence
				if (!a.World.ActorsWithTrait<GivesIntelligence>().Where(t => t.Actor != a && t.Actor.Owner == a.Owner && t.Trait.Types.Overlaps(info.Types)).Any())
				{
					intelOwners.Remove(a.Owner);
					RemoveCellsFromPlayerShroud(self, a.Owner);
				}
			}
		}

		protected override void AddCellsToPlayerShroud(Actor self, Player p, PPos[] uv)
		{
			p.Shroud.AddSource(this, type, uv);
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld)
				return;

			if (self.Owner.NonCombatant)
				return;

			if (!intelOwners.Any())
				return;

			var centerPosition = self.CenterPosition;
			var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
			var projectedLocation = self.World.Map.CellContaining(projectedPos);
			var traitDisabled = IsTraitDisabled;

			if (cachedLocation == projectedLocation && traitDisabled == cachedTraitDisabled)
				return;

			cachedLocation = projectedLocation;
			cachedTraitDisabled = traitDisabled;

			var cells = ProjectedCells(self);
			foreach (var p in self.World.Players)
			{
				RemoveCellsFromPlayerShroud(self, p);
				if (intelOwners.Contains(p))
					AddCellsToPlayerShroud(self, p, cells);
			}
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			if (self.Owner.NonCombatant)
				return;

			var centerPosition = self.CenterPosition;
			var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
			cachedLocation = self.World.Map.CellContaining(projectedPos);
			cachedTraitDisabled = IsTraitDisabled;
			var cells = ProjectedCells(self);

			foreach (var p in self.World.Players)
			{
				var hasIntel = self.World.ActorsWithTrait<GivesIntelligence>().Where(t => t.Actor.Owner == p && t.Trait.Types.Overlaps(info.Types)).Any();

				if (hasIntel)
				{
					AddCellsToPlayerShroud(self, p, cells);

					intelOwners.Add(p);
				}
			}
		}
	}
}
