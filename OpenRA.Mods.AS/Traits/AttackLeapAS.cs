#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.AS.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.AS.Traits
{
	[Desc("Dogs use this attack model.")]
	class AttackLeapASInfo : AttackFrontalInfo
	{
		[Desc("Leap speed (in units/tick).")]
		public readonly WDist Speed = new WDist(426);

		public readonly WAngle Angle = WAngle.FromDegrees(20);

		[Desc("Types of damage that this trait causes. Leave empty for no damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default(BitSet<DamageType>);

		public override object Create(ActorInitializer init) { return new AttackLeapAS(init.Self, this); }
	}

	class AttackLeapAS : AttackFrontal
	{
		readonly AttackLeapASInfo info;

		public AttackLeapAS(Actor self, AttackLeapASInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override void DoAttack(Actor self, Target target, IEnumerable<Armament> armaments = null)
		{
			if (target.Type != TargetType.Actor || !CanAttack(self, target))
				return;

			var a = ChooseArmamentsForTarget(target, true).FirstOrDefault();
			if (a == null)
				return;

			if (!target.IsInRange(self.CenterPosition, a.MaxRange()))
				return;

			self.CancelActivity();
			self.QueueActivity(new LeapAS(self, target.Actor, a, info.Speed, info.Angle, info.DamageTypes));
		}
	}
}
