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

using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits.Render
{
	class WithDisguisingFacingSpriteBodyInfo : WithFacingSpriteBodyInfo, Requires<DisguiseInfo>
	{
		public override object Create(ActorInitializer init) { return new WithDisguisingFacingSpriteBody(init, this); }
	}

	class WithDisguisingFacingSpriteBody : WithFacingSpriteBody, ITick
	{
		readonly WithDisguisingFacingSpriteBodyInfo info;
		readonly Disguise disguise;
		readonly RenderSprites rs;
		string intendedSprite;

		public WithDisguisingFacingSpriteBody(ActorInitializer init, WithDisguisingFacingSpriteBodyInfo info)
			: base(init, info)
		{
			this.info = info;
			rs = init.Self.Trait<RenderSprites>();
			disguise = init.Self.Trait<Disguise>();
			intendedSprite = disguise.AsSprite;
		}

		public void Tick(Actor self)
		{
			if (disguise.AsSprite != intendedSprite)
			{
				intendedSprite = disguise.AsSprite;
				DefaultAnimation.ChangeImage(intendedSprite ?? rs.GetImage(self), DefaultAnimation.CurrentSequence.Name);
				rs.UpdatePalette();
			}
		}
	}
}
