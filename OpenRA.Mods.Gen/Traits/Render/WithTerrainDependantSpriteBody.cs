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

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Yupgi_alert
{
	[Desc("Renders a different sequence depending on terrain actor is on.")]
	public class WithTerrainDependantSpriteBodyInfo : WithSpriteBodyInfo
	{
		public override object Create(ActorInitializer init) { return new WithTerrainDependantSpriteBody(init, this); }
	}

	public class WithTerrainDependantSpriteBody : WithSpriteBody
	{
		string terrain;
		string sequence;
		string startSequence;

		public WithTerrainDependantSpriteBody(ActorInitializer init, WithTerrainDependantSpriteBodyInfo info)
			: this(init, info, () => 0) { }

		protected WithTerrainDependantSpriteBody(ActorInitializer init, WithTerrainDependantSpriteBodyInfo info, Func<int> baseFacing)
			: base(init, info)
		{
			terrain = init.World.Map.GetTerrainInfo(init.Self.Location).Type;

			sequence = DefaultAnimation.HasSequence(info.Sequence + "-" + terrain) ? info.Sequence + "-" + terrain : info.Sequence;

			if (info.StartSequence != null)
			{
				startSequence = DefaultAnimation.HasSequence(info.StartSequence + "-" + terrain) ? info.StartSequence + "-" + terrain : info.StartSequence;

				PlayCustomAnimation(init.Self, startSequence,
					() => PlayCustomAnimationRepeating(init.Self, sequence));
			}
			else
				DefaultAnimation.PlayRepeating(NormalizeSequence(init.Self, sequence));
		}

		public override void PlayCustomAnimation(Actor self, string name, Action after = null)
		{
			var anim = DefaultAnimation.HasSequence(name + "-" + terrain) ? name + "-" + terrain : name;
			DefaultAnimation.PlayThen(NormalizeSequence(self, anim), () =>
			{
				CancelCustomAnimation(self);
				if (after != null)
					after();
			});
		}

		public override void PlayCustomAnimationRepeating(Actor self, string name)
		{
			var anim = DefaultAnimation.HasSequence(name + "-" + terrain) ? name + "-" + terrain : name;
			var sequence = NormalizeSequence(self, anim);
			DefaultAnimation.PlayThen(sequence, () => PlayCustomAnimationRepeating(self, sequence));
		}

		public override void PlayCustomAnimationBackwards(Actor self, string name, Action after = null)
		{
			var anim = DefaultAnimation.HasSequence(name + "-" + terrain) ? name + "-" + terrain : name;
			DefaultAnimation.PlayBackwardsThen(NormalizeSequence(self, anim), () =>
			{
				CancelCustomAnimation(self);
				if (after != null)
					after();
			});
		}

		public override void CancelCustomAnimation(Actor self)
		{
			DefaultAnimation.PlayRepeating(NormalizeSequence(self, sequence));
		}

		protected override void DamageStateChanged(Actor self)
		{
			if (DefaultAnimation.CurrentSequence != null)
				DefaultAnimation.ReplaceAnim(NormalizeSequence(self, DefaultAnimation.CurrentSequence.Name));
		}
	}
}
