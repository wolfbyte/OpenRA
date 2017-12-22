#region Copyright & License Information
/*
 * Modded by Boolbada of OP Mod.
 *  Sames as FlyAttack.cs but this makes the unit enter spawner when empty, not return to base.
 *  return ActivityUtils.SequenceActivities(new EnterSpawner(self, master, EnterBehaviour.Exit));
 *  
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Yupgi_alert.Traits;
using OpenRA.Traits;

/* Works with no base engine modification */

namespace OpenRA.Mods.Yupgi_alert.Activities
{
	public class SpawnedFlyAttack : Activity
	{
		readonly Target target;
		readonly Aircraft aircraft;
		readonly AttackPlane attackPlane;

		readonly bool autoReloads;
		int ticksUntilTurn;

		public SpawnedFlyAttack(Actor self, Target target)
		{
			this.target = target;
			aircraft = self.Trait<Aircraft>();
			attackPlane = self.TraitOrDefault<AttackPlane>();
			ticksUntilTurn = attackPlane.AttackPlaneInfo.AttackTurnDelay;
			autoReloads = self.TraitsImplementing<AmmoPool>().All(p => p.AutoReloads);
		}

		public override Activity Tick(Actor self)
		{
			if (!target.IsValidFor(self))
				return NextActivity;

			// If all valid weapons have depleted their ammo and RearmBuilding is defined, return to RearmBuilding to reload and then resume the activity
			if (!autoReloads && aircraft.Info.RearmBuildings.Any() && attackPlane.Armaments.All(x => x.IsTraitPaused || !x.Weapon.IsValidAgainst(target, self.World, self)))
				return ActivityUtils.SequenceActivities(new ReturnToBase(self, aircraft.Info.AbortOnResupply), this);

			if (attackPlane != null)
				attackPlane.DoAttack(self, target);

			if (ChildActivity == null)
			{
				if (IsCanceled)
					return NextActivity;

				// TODO: This should fire each weapon at its maximum range
				if (attackPlane != null && target.IsInRange(self.CenterPosition, attackPlane.Armaments.Select(a => a.Weapon.MinRange).Min()))
					ChildActivity = ActivityUtils.SequenceActivities(new FlyTimed(ticksUntilTurn, self), new Fly(self, target), new FlyTimed(ticksUntilTurn, self));
				else
					ChildActivity = ActivityUtils.SequenceActivities(new Fly(self, target), new FlyTimed(ticksUntilTurn, self));

				// HACK: This needs to be done in this round-about way because TakeOff doesn't behave as expected when it doesn't have a NextActivity.
				if (self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length < aircraft.Info.MinAirborneAltitude)
					ChildActivity = ActivityUtils.SequenceActivities(new TakeOff(self), ChildActivity);
			}

			ActivityUtils.RunActivity(self, ChildActivity);

			return this;
		}
	}
}
