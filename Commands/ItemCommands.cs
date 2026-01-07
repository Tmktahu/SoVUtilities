using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;
using ProjectM;
using SoVUtilities.Services.Buffs;
using SoVUtilities.Resources;
using ProjectM.Terrain;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class ItemCommands
{
  // command to set an ability on an item
  [Command("item ability set", "ias", "Set abilities on the currently held item", adminOnly: true)]
  public static void SetItemAbilitiesCommand(ChatCommandContext ctx, int placementId, int prefabGUID)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }

    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    AbilityService.SetItemAbility(heldItemEntity, slot, new PrefabGUID(prefabGUID));
    AbilityService.RefreshEquipBuff(playerEntity).Start();
    ctx.Reply($"Set ability {prefabGUID} on held item in slot {placementId}.");
  }

  // command to clear a target ability slot on an item
  [Command("item ability clear", "iac", "Clear abilities on the currently held item", adminOnly: true)]
  public static void ClearItemAbilitiesCommand(ChatCommandContext ctx, int placementId)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }

    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    AbilityService.ClearItemAbility(heldItemEntity, slot);
    AbilityService.RefreshEquipBuff(playerEntity).Start();
    ctx.Reply($"Cleared ability on held item in slot {placementId}.");
  }

  // command to clear all ability slots on an item
  [Command("item ability clearall", "iac", "Clear all abilities on the currently held item", adminOnly: true)]
  public static void ClearAllItemAbilitiesCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    AbilityService.ClearAllItemAbilities(heldItemEntity);
    AbilityService.RefreshEquipBuff(playerEntity).Start();
    ctx.Reply("Cleared all abilities on held item.");
  }
}