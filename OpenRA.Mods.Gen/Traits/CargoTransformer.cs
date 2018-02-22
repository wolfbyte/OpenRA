#region Copyright & License Information
/*
 * Modded by Boolbada of Over Powered Mod.
 * Contains some copy and paste code from OpenRA base mod.
 * (Erm... hardly any by now but using OpenRA API of course)
 *
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Yupgi_alert.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

/* Works without base engine modification */

namespace OpenRA.Mods.Yupgi_alert.Traits
{
	[Desc("Transforms cargo inside when some predefined combination is met.")]
	public class CargoTransformerInfo : ITraitInfo, Requires<CargoInfo>
	{
		[Desc("Unit type to emit when the combination is met")]
		public readonly Dictionary<string, string[]> Combinations;

		[Desc("The sound played when the transform stars.")]
		public readonly string[] TransformSound = null;

		[Desc("The animation sequence to play when transforming.")]
		[SequenceReference] public readonly string ActiveSequence = "active";

		public object Create(ActorInitializer init) { return new CargoTransformer(init.Self, this); }
	}

	public class CargoTransformer : INotifyPassengerEntered, INotifyPassengerExited, IAcceptsRallyPoint
	{
		public readonly CargoTransformerInfo Info;
		Cargo cargo;

		public CargoTransformer(Actor self, CargoTransformerInfo info)
		{
			Info = info;
			cargo = self.Trait<Cargo>(); // I required CargoInfo so this will always work.
		}

		void SpawnUnit(Actor self, string unit)
		{
			// Play transformation sound
			Game.Sound.Play(SoundType.World, Info.TransformSound.Random(self.World.SharedRandom), self.CenterPosition);

			// Kill cargo and spawn new unit

			// For dramatic cargo, lets have those ejected and killed!
			// Actually not a good idea. I get unit lost sound. :(
			// Just dispose silently.
			while (!cargo.IsEmpty(self))
			{
				var c = cargo.Unload(self);
				c.Dispose();
			}

			/*
			// I attempted "production" but it wasn't a good idea.
			// When the exit is blocked, the unit disappears.
			// Blocking is better handled by unload.
			*/

			var td = new TypeDictionary
			{
				new OwnerInit(self.Owner),
			};
			var newUnit = self.World.CreateActor(false, unit.ToLowerInvariant(), td);
			cargo.Load(self, newUnit);

			var wsb = self.TraitOrDefault<WithSpriteBody>();
			if (wsb != null && wsb.DefaultAnimation.HasSequence(Info.ActiveSequence))
				wsb.PlayCustomAnimation(self, Info.ActiveSequence, () => self.QueueActivity(new UnloadCargo(self, true)));
		}

		void INotifyPassengerExited.OnPassengerExited(Actor self, Actor passenger)
		{
			var rp = self.TraitOrDefault<RallyPoint>();
			if (rp == null)
				return;

			// Make the unloaded passenger follow the passenger.
			// Yes, you see this correct. We queue the move activity after two end frames.
			// If you do this in just one frame end task, it collides with the AddFrameEndTask defined in UnloadCargo.cs
			// Just like a LaTeX trick AfterPage'ing twice.
			self.World.AddFrameEndTask(w1 =>
				self.World.AddFrameEndTask(w2 =>
				{
					if (passenger.Disposed)
						return;

					var move = passenger.Trait<IMove>();

					if (rp != null)
						rp.QueueRallyOrder(self, passenger);
					else
						passenger.QueueActivity(new AttackMoveActivity(passenger, move.MoveTo(rp.Location, 1)));

					passenger.SetTargetLine(Target.FromCell(w2, rp.Location), Color.Green, false);
				}));
		}

		void INotifyPassengerEntered.OnPassengerEntered(Actor self, Actor passenger)
		{
			// get rules entry name for each passenger.
			var names = cargo.Passengers.OrderBy(x => x.Info.Name).Select(x => x.Info.Name).ToArray();

			// Lets examine the contents.
			foreach (var kv in Info.Combinations)
			{
				if (Enumerable.SequenceEqual(names, kv.Value))
				{
					SpawnUnit(self, kv.Key);
					return; // no need to examine any other combination.
				}
			}
		}

		bool IAcceptsRallyPoint.IsAcceptableActor(Actor produced, Actor dest)
		{
			return produced.TraitOrDefault<Passenger>() != null;
		}

		Activity IAcceptsRallyPoint.RallyActivities(Actor produced, Actor dest)
		{
			var passenger = produced.TraitOrDefault<Passenger>();
			if (passenger == null)
				throw new InvalidProgramException("Is acceptable check didn't filter this?");

			return new EnterTransportWithWait(produced, dest);
		}
	}
}
