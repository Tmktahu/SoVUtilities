using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;
using ProjectM;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class StructureCommands
{
  // command to toggle container name visibility
  [Command("structure container name", "Toggles the visibility of a container's name")]
  public static void ToggleContainerNameVisibility(ChatCommandContext ctx)
  {
    // first we want to get the closest container structure to the command sender
    Entity senderEntity = ctx.Event.SenderCharacterEntity;
    Entity structureEntity = StructureService.GetClosestContainerEntity(senderEntity, 5f);

    if (structureEntity == Entity.Null)
    {
      ctx.Reply("No container structure found nearby.");
      return;
    }

    StructureService.ToggleContainerNameVisibility(structureEntity);
  }
}