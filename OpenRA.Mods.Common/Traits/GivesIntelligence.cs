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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor activates other player's actors with 'RevealsShroudToIntelligenceOwner' trait to its owner.")]
	public class GivesIntelligenceInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Types of intelligence this actor gives.")]
		public readonly HashSet<string> Types = new HashSet<string>();

		public override object Create(ActorInitializer init) { return new GivesIntelligence(this); }
	}

	public class GivesIntelligence : ConditionalTrait<GivesIntelligenceInfo>
	{
		public GivesIntelligence(GivesIntelligenceInfo info)
			: base(info) { }

		readonly HashSet<string> noTypes = new HashSet<string>();

		public HashSet<string> Types { get { return !IsTraitDisabled ? Info.Types : noTypes; } }
	}
}
