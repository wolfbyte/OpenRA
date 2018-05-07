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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Primitives;

namespace OpenRA.Mods.Yupgi_alert.Traits
{
	[Desc("This actor places mines around itself, and replenishes them after a while.")]
	public class LaysMinefieldInfo : PausableConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Types of mines to place, if multipile is defined, a random one will be selected.")]
		public readonly HashSet<string> Mines = new HashSet<string>();

		[Desc("Range, in cells, to place mines around.")]
		public readonly int Range = 3;

		[Desc("Initial delay to create the mines.")]
		public readonly int InitialDelay = 1;

		[Desc("Recreate the mines, if they are destroyed after this much of time.")]
		public readonly int RecreationInterval = 250;

		[Desc("Remove the mines if the trait gets disabled.")]
		public readonly bool RemoveOnDisable = true;

		public override object Create(ActorInitializer init) { return new LaysMinefield(this); }
	}

	public class LaysMinefield : PausableConditionalTrait<LaysMinefieldInfo>, INotifyBuildComplete, INotifyKilled, INotifyActorDisposing, ITick, ISync
	{
		[Sync] int ticks;
		List<Actor> mines = new List<Actor>();

		public LaysMinefield(LaysMinefieldInfo info)
			: base(info)
		{
			ticks = Info.InitialDelay;
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitPaused || IsTraitDisabled)
				return;

			if (--ticks < 0)
			{
				ticks = Info.RecreationInterval;
				SpawnMinesPart1(self);
			}
		}

		public void SpawnMinesPart1(Actor self)
		{
			var building = self.TraitOrDefault<Building>();
			if (building != null && building.Locked)
				return;

			if (building != null)
			{ 
				foreach (var buildingCell in building.OccupiedCells())
				{ 
					foreach (var cell in self.World.Map.FindTilesInCircle(buildingCell.First, Info.Range))
					{
						SpawnMinesPart2(self, cell);
					}
				}
			}
			else
			{
				foreach (var cell in self.World.Map.FindTilesInCircle(self.World.Map.CellContaining(self.CenterPosition), Info.Range))
				{
					SpawnMinesPart2(self, cell);
				}
			}
		}

		public void SpawnMinesPart2(Actor self, CPos cell)
		{
			foreach (var actor in Info.Mines)
			{
				var ai = self.World.Map.Rules.Actors[actor];
				var ip = ai.TraitInfo<IPositionableInfo>();

				if (!ip.CanEnterCell(self.World, null, cell))
					continue;

				var mine = self.World.CreateActor(actor.ToLowerInvariant(), new TypeDictionary
					{
						new OwnerInit(self.Owner),
						new LocationInit(cell)
					});

				mines.Add(mine);
			}
		}

		public void RemoveMines()
		{
			foreach (var mine in mines)
				mine.Dispose();

			mines.Clear();
		}

		void INotifyBuildComplete.BuildingComplete(Actor self)
		{
			if (!IsTraitDisabled && !IsTraitPaused)
				SpawnMinesPart1(self);
		}

		protected override void TraitDisabled(Actor self)
		{
			ticks = Info.InitialDelay;

			if (Info.RemoveOnDisable)
				RemoveMines();
		}
		
		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			RemoveMines();
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			RemoveMines();
		}
	}
}
