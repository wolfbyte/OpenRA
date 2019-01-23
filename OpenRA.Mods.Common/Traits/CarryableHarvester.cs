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

using System.Linq;
using OpenRA.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class CarryableHarvesterInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new CarryableHarvester(); }
	}

	public class CarryableHarvester : INotifyCreated, INotifyHarvesterAction
	{
		ICallForTransport[] transports;

		void INotifyCreated.Created(Actor self)
		{
			transports = self.TraitsImplementing<ICallForTransport>().ToArray();
		}

		Activity INotifyHarvesterAction.MovingToResources(Actor self, CPos targetCell, Activity next)
		{
			foreach (var t in transports)
				t.RequestTransport(self, targetCell, next);

			return null;
		}

		Activity INotifyHarvesterAction.MovingToRefinery(Actor self, Actor refineryActor, Activity next)
		{
			var dock = refineryActor.TraitsImplementing<Dock>().First();
			var location = refineryActor.Location + dock.Info.DockOffset;
			foreach (var t in transports)
				t.RequestTransport(self, location, next);

			return null;
		}

		void INotifyHarvesterAction.MovementCancelled(Actor self)
		{
			foreach (var t in transports)
				t.MovementCancelled(self);
		}

		void INotifyHarvesterAction.Harvested(Actor self, ResourceType resource) { }
		void INotifyHarvesterAction.Docked() { }
		void INotifyHarvesterAction.Undocked() { }
	}
}
