using System;
using System.Collections.Generic;
using System.Linq;

using ACE.Common;
using ACE.Database.Models.World;
using ACE.Entity.Enum;
using ACE.Server.Entity;
using ACE.Server.Factories;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        public List<WorldObject> GetCreateList(DestinationType type)
        {
            var items = new List<WorldObject>();

            foreach (var item in Biota.PropertiesCreateList.Where(x => x.DestinationType == type))
            {
                var wo = WorldObjectFactory.CreateNewWorldObject(item.WeenieClassId);

                if (item.Palette > 0)
                    wo.PaletteTemplate = item.Palette;

                if (item.Shade > 0)
                    wo.Shade = item.Shade;

                if (item.StackSize > 0)
                    wo.SetStackSize(item.StackSize);

                items.Add(wo);
            }
            return items;
        }

        public List<WorldObject> GenerateWieldedTreasureSets(TreasureWieldedTable table)
        {
            var wieldedTreasure = new List<WorldObject>();

            foreach (var set in table.Sets)
                wieldedTreasure.AddRange(GenerateWieldedTreasureSet(set));

            return wieldedTreasure;
        }

        public List<WorldObject> GenerateWieldedTreasureSet(TreasureWieldedSet set)
        {
            var wieldedTreasure = new List<WorldObject>();

            var rng = ThreadSafeRandom.Next(0.0f, set.TotalProbability);
            var probability = 0.0f;

            foreach (var item in set.Items)
            {
                probability += item.Item.Probability;
                if (rng > probability) continue;

                // item roll successful, spawn item in creature inventory
                var wo = CreateWieldedTreasure(item.Item);
                if (wo == null) continue;

                wieldedTreasure.Add(wo);

                // traverse into possible subsets
                if (item.Subset != null)
                    wieldedTreasure.AddRange(GenerateWieldedTreasureSet(item.Subset));

                break;
            }
            return wieldedTreasure;
        }

        public WorldObject CreateWieldedTreasure(TreasureWielded item)
        {
            var wo = WorldObjectFactory.CreateNewWorldObject(item.WeenieClassId);
            if (wo == null) return null;

            if (item.PaletteId > 0)
                wo.PaletteTemplate = (int)item.PaletteId;

            if (item.Shade > 0)
                wo.Shade = item.Shade;

            if (item.StackSize > 0)
            {
                var stackSize = item.StackSize;

                var hasVariance = item.StackSizeVariance > 0;
                if (hasVariance)
                {
                    var minStack = (int)Math.Max(Math.Round(item.StackSize * item.StackSizeVariance), 1);
                    var maxStack = item.StackSize;
                    stackSize = ThreadSafeRandom.Next(minStack, maxStack);
                }
                wo.SetStackSize(stackSize);
            }
            return wo;
        }

        public virtual void OnWield(Creature creature)
        {
            EmoteManager.OnWield(creature);
        }

        public virtual void OnUnWield(Creature creature)
        {
            EmoteManager.OnUnwield(creature);
        }
    }
}
