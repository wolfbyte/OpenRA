﻿using System.Drawing;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Yupgi_alert.Traits;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.Yupgi_alert.Activities
{
    class DeliverGoods : Activity
    {
        readonly SupplyCollector collector;
		readonly SupplyCollectorInfo collectorInfo;
        readonly IMove move;
		readonly Mobile mobile;
		readonly IPathFinder pathFinder;

		public DeliverGoods(Actor self)
		{
			collector = self.Trait<SupplyCollector>();
			collectorInfo = self.Info.TraitInfo<SupplyCollectorInfo>();
            move = self.Trait<IMove>();
			mobile = self.TraitOrDefault<Mobile>();
			pathFinder = self.World.WorldActor.Trait<IPathFinder>();
		}

        public override Activity Tick(Actor self)
        {
            if (IsCanceled || NextActivity != null)
                return NextActivity;

            if (collector.deliveryBuilding == null || !collector.deliveryBuilding.IsInWorld)
            {
				collector.deliveryBuilding = collector.ClosestDeliveryBuilding(self);
            }

            if (collector.deliveryBuilding == null || !collector.deliveryBuilding.IsInWorld)
            {
                return ActivityUtils.SequenceActivities(new Wait(collectorInfo.SearchForDeliveryBuildingDelay), this);
            }

			var center = collector.deliveryBuilding;
			self.SetTargetLine(Target.FromActor(center), Color.Green, false);

			CPos cell;
			var random = new MersenneTwister();
			var centerTrait = center.Trait<SupplyCenter>();
			if (mobile != null)
				cell = self.ClosestCell(centerTrait.Info.DeliveryOffsets.Where(c => mobile.CanEnterCell(center.Location + c)).Select(c => center.Location + c));
			else
				cell = self.ClosestCell(centerTrait.Info.DeliveryOffsets.Select(c => center.Location + c));

			if (!centerTrait.Info.DeliveryOffsets.Select(o => center.Location + o).Contains(self.Location))
            {
                return ActivityUtils.SequenceActivities(move.MoveTo(cell, 2), this);
            }

			if (self.Trait<IFacing>() != null)
			{
				if (centerTrait.Info.Facing >= 0 && self.Trait<IFacing>().Facing != centerTrait.Info.Facing)
				{
					return ActivityUtils.SequenceActivities(new Turn(self, centerTrait.Info.Facing), this);
				}
				else if (centerTrait.Info.Facing == -1)
				{
					var facing = (center.CenterPosition - self.CenterPosition).Yaw.Facing;
					if (self.Trait<IFacing>().Facing != facing)
					{
						return ActivityUtils.SequenceActivities(new Turn(self, facing), this);
					}
				}
			}

			if (!collector.Waiting)
			{
				collector.Waiting = true;
				return ActivityUtils.SequenceActivities(new Wait(collectorInfo.DeliveryDelay), this);
			}

			var amount = collector.Amount;
			if (amount < 0)
				return new FindGoods(self);
			
			if (centerTrait.CanGiveResource(amount))
            {
				var wsb = self.TraitsImplementing<WithSpriteBody>().Where(t => !t.IsTraitDisabled).FirstOrDefault();
				var wsda = self.Info.TraitInfoOrDefault<WithSupplyDeliveryAnimationInfo>();
				var rs = self.TraitOrDefault<RenderSprites>();
				if (rs != null && wsb != null && wsda != null && !collector.DeliveryAnimPlayed)
				{
					wsb.PlayCustomAnimation(self, wsda.DeliverySequence);
					collector.DeliveryAnimPlayed = true;
					return ActivityUtils.SequenceActivities(new Wait(wsda.WaitDelay), this);
				}

				var wsdo = self.TraitOrDefault<WithSupplyDeliveryOverlay>();
				if (wsb != null && wsdo != null && !collector.DeliveryAnimPlayed)
				{
					if (!wsdo.Visible)
					{
						wsdo.Visible = true;
						wsdo.Anim.PlayThen(wsdo.Info.Sequence, () => wsdo.Visible = false);
						collector.DeliveryAnimPlayed = true;
						var sequence = wsb.DefaultAnimation.GetSequence(wsdo.Info.Sequence);
						return ActivityUtils.SequenceActivities(new Wait(wsdo.Info.WaitDelay), this);
					}
				}

				collector.Waiting = false;
				collector.DeliveryAnimPlayed = false;
				centerTrait.GiveResource(amount, self.Info.Name);

				collector.Amount = 0;
				collector.CheckConditions(self);
			}
			else
				return ActivityUtils.SequenceActivities(new Wait(collectorInfo.DeliveryDelay), this);

			return new FindGoods(self);
        }
    }
}
