using Unity.Entities;

namespace SoVUtilities.Services.Buffs;

public enum GlobalStatBuffFlags
{
  None = 0,
  WolfFormSpeedBoost = 1,
}

public interface ICustomBuff
{
  void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null);
  bool RemoveBuff(Entity entity);
  bool HasBuff(Entity entity);
  string BuffId { get; }
}

public class GlobalStatsCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => GlobalStatsBuff.ApplyCustomBuff(targetEntity, flags).Start();
  public bool RemoveBuff(Entity entity) => GlobalStatsBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => GlobalStatsBuff.HasBuff(entity);
  public string BuffId => BuffService.GlobalStatsBuffId;
}

public class HumanCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => HumanBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => HumanBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => HumanBuff.HasBuff(entity);
  public string BuffId => BuffService.HumanBuffId;
}

public class HideNameplateCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => HideNameplateBuff.ApplyCustomBuff(targetEntity);
  public bool RemoveBuff(Entity entity) => HideNameplateBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => HideNameplateBuff.HasBuff(entity);
  public string BuffId => BuffService.HideNameplateBuffId;
}

public class AfflictedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => AfflictedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => AfflictedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => AfflictedBuff.HasBuff(entity);
  public string BuffId => BuffService.AfflictedBuffId;
}

public class BeholdenCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => BeholdenBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => BeholdenBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => BeholdenBuff.HasBuff(entity);
  public string BuffId => BuffService.BeholdenBuffId;
}

public class EncasedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => EncasedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => EncasedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => EncasedBuff.HasBuff(entity);
  public string BuffId => BuffService.EncasedBuffId;
}

public class ConsumedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => ConsumedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => ConsumedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => ConsumedBuff.HasBuff(entity);
  public string BuffId => BuffService.ConsumedBuffId;
}

public class SeededCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => SeededBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => SeededBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => SeededBuff.HasBuff(entity);
  public string BuffId => BuffService.SeededBuffId;
}

public class RelicCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => RelicBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => RelicBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => RelicBuff.HasBuff(entity);
  public string BuffId => BuffService.RelicBuffId;
}

public class SpiritChosenCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => SpiritChosenBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => SpiritChosenBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => SpiritChosenBuff.HasBuff(entity);
  public string BuffId => BuffService.SpiritChosenBuffId;
}

public class WerewolfCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => WerewolfBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => WerewolfBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => WerewolfBuff.HasBuff(entity);
  public string BuffId => BuffService.WerewolfBuffId;
}

public class WerewolfStatsCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => WerewolfStatsBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => WerewolfStatsBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => WerewolfStatsBuff.HasBuff(entity);
  public string BuffId => BuffService.WerewolfStatsBuffId;
}

public class RotlingCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => RotlingBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => RotlingBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => RotlingBuff.HasBuff(entity);
  public string BuffId => BuffService.RotlingBuffId;
}

public class WolfSpeedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null) => WolfSpeedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => WolfSpeedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => WolfSpeedBuff.HasBuff(entity);
  public string BuffId => BuffService.WolfSpeedBuffId;
}