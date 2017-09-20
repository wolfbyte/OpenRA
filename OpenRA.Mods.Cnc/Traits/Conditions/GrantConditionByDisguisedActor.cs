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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits.Render
{
	class GrantConditionByDisguisedActorInfo : ITraitInfo, Requires<DisguiseInfo>
	{
		[Desc("Conditions to grant when disguised as specified actor.",
			"A dictionary of [actor id]: [condition].")]
		public readonly Dictionary<string, string> Conditions = new Dictionary<string, string>();
		
		public object Create(ActorInitializer init) { return new GrantConditionByDisguisedActor(init.Self, this); }
	}

	class GrantConditionByDisguisedActor : ITick
	{
		readonly GrantConditionByDisguisedActorInfo info;
		readonly Disguise disguise;
		string intendedActorName;
		string condition;

		ConditionManager conditionManager;
		int conditionToken = ConditionManager.InvalidConditionToken;

		public GrantConditionByDisguisedActor(Actor self, GrantConditionByDisguisedActorInfo info)
		{
			this.info = info;
			disguise = self.Trait<Disguise>();
			intendedActorName = disguise.AsActorName;
			
			conditionManager = self.TraitOrDefault<ConditionManager>();
		}

		public void Tick(Actor self)
		{
			if (disguise.AsActorName != intendedActorName)
			{
				intendedActorName = disguise.AsActorName;

				if (conditionToken != ConditionManager.InvalidConditionToken)
					conditionToken = conditionManager.RevokeCondition(self, conditionToken);

				if (info.Conditions.TryGetValue(disguise.AsActorName, out condition))
					conditionToken = conditionManager.GrantCondition(self, condition);
			}
		}
	}
}
