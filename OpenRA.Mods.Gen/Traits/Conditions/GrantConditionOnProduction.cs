#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.Yupgi_alert.Traits
{
	[Desc("Grants a condition when this actor produces a specific actor.")]
	public class GrantConditionOnProductionInfo : ITraitInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant")]
		public readonly string Condition = null;

		[ActorReference]
		[Desc("The actors to grant condition for. If empty condition will be granted for all actors.")]
		public readonly HashSet<string> Actors = new HashSet<string>(); 
		
		public object Create(ActorInitializer init) { return new GrantConditionOnProduction(init.Self, this); }
	}

	public class GrantConditionOnProduction : INotifyCreated, INotifyOtherProduction
	{
		readonly GrantConditionOnProductionInfo info;
		ConditionManager manager;

		int token = ConditionManager.InvalidConditionToken;

		public GrantConditionOnProduction(Actor self, GrantConditionOnProductionInfo info)
		{
			this.info = info;
		}
		
		void INotifyCreated.Created(Actor self)
		{
			manager = self.Trait<ConditionManager>();
		}

		void GrantCondition(Actor self, string cond)
		{
			if (manager == null)
				return;

			if (string.IsNullOrEmpty(cond))
				return;

			token = manager.GrantCondition(self, cond);
		}

		public void UnitProducedByOther(Actor self, Actor producer, Actor produced, string productionType)
		{
			// Only grant to self, not others.
			if (producer != self)
				return;

			if (!info.Actors.Any() || info.Actors.Contains(produced.Info.Name))
				if (token == ConditionManager.InvalidConditionToken)
					GrantCondition(self, info.Condition);
		}
	}
}
