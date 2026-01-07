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

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class AbilityCommands
{
  // public command for users to set currently selected R spell to Q
  [Command("ability set primary", "ability set q", "Set a spells to Q and E slot", adminOnly: false)]
  public static void SetSpellsCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    // we get the spell prefabs from what they have in their R and C slots
    PrefabGUID spellPrefabGuid = AbilityService.getEquippedAbilityPrefabGuid(playerEntity, AbilityTypeEnum.SpellSlot1);

    string currentWeaponCategory = AbilityService.CATEGORY_UNARMED;
    AbilityService.setAbility(playerEntity, AbilityTypeEnum.Secondary, spellPrefabGuid, currentWeaponCategory);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(spellPrefabGuid);
    ctx.Reply($"Set Q to {prefabName}. Swap weapons to update your UI.");
  }

  // public command for users to set currently selected C spell to E
  [Command("ability set secondary", "ability set e", "Set a spells to E slot", adminOnly: false)]
  public static void SetSecondarySpellCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    // we get the spell prefab from what they have in their R and C slots
    PrefabGUID spellPrefabGuid = AbilityService.getEquippedAbilityPrefabGuid(playerEntity, AbilityTypeEnum.SpellSlot2);

    string currentWeaponCategory = AbilityService.CATEGORY_UNARMED;
    AbilityService.setAbility(playerEntity, AbilityTypeEnum.Power, spellPrefabGuid, currentWeaponCategory);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(spellPrefabGuid);
    ctx.Reply($"Set E to {prefabName}. Swap weapons to update your UI.");
  }

  // public command for users to clear their Q slot
  [Command("ability clear primary", "ability clear q", "Clear the Q ability slot", adminOnly: false)]
  public static void ClearPrimaryAbilityCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    string currentWeaponCategory = AbilityService.CATEGORY_UNARMED;
    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Secondary, currentWeaponCategory);
    ctx.Reply($"Cleared Q ability slot. Swap weapons to update your UI.");
  }

  // public command for users to clear their E slot
  [Command("ability clear secondary", "ability clear e", "Clear the E ability slot", adminOnly: false)]
  public static void ClearSecondaryAbilityCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    string currentWeaponCategory = AbilityService.CATEGORY_UNARMED;
    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Power, currentWeaponCategory);
    ctx.Reply($"Cleared E ability slot. Swap weapons to update your UI.");
  }

  [Command("ability set", "as", "Set an ability to a slot", adminOnly: true)]
  public static void SetAbilityCommand(ChatCommandContext ctx, int placementId, int prefabGuid, string playerName = null)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }
    Entity playerEntity;
    Entity userEntity;
    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }

    string currentWeaponCategory = PlayerService.GetEquippedWeaponCategory(playerEntity);
    AbilityService.setAbility(playerEntity, slot, new PrefabGUID(prefabGuid), currentWeaponCategory);
    ctx.Reply($"Set ability slot {slot} to prefab GUID {prefabGuid} for player '{playerName ?? playerEntity.GetUser().CharacterName}'.");
  }

  [Command("ability clear", "ac", "Clear an ability slot", adminOnly: true)]
  public static void ClearAbilitySlotCommand(ChatCommandContext ctx, int placementId, string playerName = null)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }
    Entity playerEntity;
    Entity userEntity;
    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }
    string currentWeaponCategory = PlayerService.GetEquippedWeaponCategory(playerEntity);
    AbilityService.ClearAbilitySlot(playerEntity, slot, currentWeaponCategory);
    ctx.Reply($"Cleared ability slot {slot} for player '{playerName ?? playerEntity.GetUser().CharacterName}'.");
  }

  [Command("ability clearall", "ac", "Clear all ability slots", adminOnly: true)]
  public static void ClearAllAbilitySlotsCommand(ChatCommandContext ctx, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;
    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }
    string currentWeaponCategory = PlayerService.GetEquippedWeaponCategory(playerEntity);
    AbilityService.ClearAllAbilitySlots(playerEntity, currentWeaponCategory);
    ctx.Reply($"Cleared all ability slots for player '{playerName ?? playerEntity.GetUser().CharacterName}'.");
  }
}