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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor can be captured by a unit with ExternalCaptures: trait.")]
	public class ExternalCapturableInfo : ConditionalTraitInfo
	{
		[Desc("CaptureTypes (from the ExternalCaptures trait) that are able to capture this.")]
		public readonly HashSet<string> Types = new HashSet<string>() { "building" };

		[Desc("What diplomatic stances can be captured by this actor.")]
		public readonly Stance ValidStances = Stance.Neutral | Stance.Enemy;

		[Desc("Seconds it takes to change the owner.", "You might want to add a ExternalCapturableBar: trait, too.")]
		public readonly int CaptureCompleteTime = 15;

		[Desc("Whether to prevent autotargeting this actor while it is being captured by an ally.")]
		public readonly bool PreventsAutoTarget = true;

		[GrantedConditionReference]
		[Desc("Grant this condition to self while being captured.")]
		public readonly string CaptureCondition = null;

		public bool CanBeTargetedBy(Actor captor, Player owner)
		{
			var c = captor.Info.TraitInfoOrDefault<ExternalCapturesInfo>();
			if (c == null)
				return false;

			var stance = owner.Stances[captor.Owner];
			if (!ValidStances.HasStance(stance))
				return false;

			if (!c.CaptureTypes.Overlaps(Types))
				return false;

			return true;
		}

		public override object Create(ActorInitializer init) { return new ExternalCapturable(init.Self, this); }
	}

	public class ExternalCapturable : ConditionalTrait<ExternalCapturableInfo>, ITick, ISync, INotifyCreated, IPreventsAutoTarget
	{
		[Sync] public int CaptureProgressTime = 0;
		[Sync] public Actor Captor;
		private Actor self;
		public bool CaptureInProgress { get { return Captor != null; } }

		ConditionManager conditionManager;
		int token = ConditionManager.InvalidConditionToken;

		public ExternalCapturable(Actor self, ExternalCapturableInfo info)
			: base(info)
		{
			this.self = self;
		}

		void INotifyCreated.Created(Actor self)
		{
			conditionManager = self.TraitOrDefault<ConditionManager>();
		}

		public void BeginCapture(Actor captor)
		{
			if (conditionManager != null && token == ConditionManager.InvalidConditionToken && !string.IsNullOrEmpty(Info.CaptureCondition))
				token = conditionManager.GrantCondition(self, Info.CaptureCondition);

			Captor = captor;
		}

		public void EndCapture()
		{
			if (token != ConditionManager.InvalidConditionToken)
				token = conditionManager.RevokeCondition(self, token);

			Captor = null;
		}

		void ITick.Tick(Actor self)
		{
			if (Captor != null && (!Captor.IsInWorld || Captor.IsDead))
				EndCapture();

			if (!CaptureInProgress)
				CaptureProgressTime = 0;
			else
				CaptureProgressTime++;
		}

		public bool PreventsAutoTarget(Actor self, Actor attacker)
		{
			return Info.PreventsAutoTarget && Captor != null && attacker.AppearsFriendlyTo(Captor);
		}

		public bool CanBeTargetedBy(Actor captor, Player owner)
		{
			if (IsTraitDisabled)
				return false;

			return Info.CanBeTargetedBy(captor, owner);
		}
	}
}
