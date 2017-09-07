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
using Eluant;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("Ability")]
	public class CaptureProperties : ScriptActorProperties
	{
		readonly Captures[] captures;
		readonly ExternalCaptures[] externalCaptures;

		public CaptureProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			captures = Self.TraitsImplementing<Captures>().ToArray();
			externalCaptures = Self.TraitsImplementing<ExternalCaptures>().ToArray();
		}

		[Desc("Captures the target actor.")]
		public void Capture(Actor target)
		{
			var capturable = target.TraitsImplementing<Capturable>().ToArray();
			var activeCapturable = capturable.FirstOrDefault(c => !c.IsTraitDisabled);

			if (activeCapturable != null)
			{
				if (captures.Any(x => !x.IsTraitDisabled && x.Info.CaptureTypes.Contains(activeCapturable.Info.Type)))
				{
					Self.QueueActivity(new CaptureActor(Self, target));
					return;
				}
			}

			var externalCapturable = target.Info.TraitInfoOrDefault<ExternalCapturableInfo>();

			if (externalCapturable != null)
			{
				if (externalCaptures.Any(x => !x.IsTraitDisabled && x.Info.CaptureTypes.Contains(externalCapturable.Type)))
				{
					Self.QueueActivity(new ExternalCaptureActor(Self, Target.FromActor(target)));
					return;
				}
			}
			else
				throw new LuaException("Actor '{0}' cannot capture actor '{1}'!".F(Self, target));
		}
	}
}
