using Unity.Entities;

namespace SoVUtilities.Services.Buffs;

public interface ICustomBuff
{
  void ApplyCustomBuff(Entity targetEntity);
  bool RemoveBuff(Entity entity);
  bool HasBuff(Entity entity);
  string BuffId { get; }
}

public class HumanCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => HumanBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => HumanBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => HumanBuff.HasBuff(entity);
  public string BuffId => BuffService.HumanBuffId;
}

public class HideNameplateCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => HideNameplateBuff.ApplyCustomBuff(targetEntity);
  public bool RemoveBuff(Entity entity) => HideNameplateBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => HideNameplateBuff.HasBuff(entity);
  public string BuffId => BuffService.HideNameplateBuffId;
}

public class AfflictedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => AfflictedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => AfflictedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => AfflictedBuff.HasBuff(entity);
  public string BuffId => BuffService.AfflictedBuffId;
}

public class BeholdenCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => BeholdenBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => BeholdenBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => BeholdenBuff.HasBuff(entity);
  public string BuffId => BuffService.BeholdenBuffId;
}

public class EncasedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => EncasedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => EncasedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => EncasedBuff.HasBuff(entity);
  public string BuffId => BuffService.EncasedBuffId;
}

public class ConsumedCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => ConsumedBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => ConsumedBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => ConsumedBuff.HasBuff(entity);
  public string BuffId => BuffService.ConsumedBuffId;
}

public class SeededCustomBuff : ICustomBuff
{
  public void ApplyCustomBuff(Entity targetEntity) => SeededBuff.ApplyCustomBuff(targetEntity).Start();
  public bool RemoveBuff(Entity entity) => SeededBuff.RemoveBuff(entity);
  public bool HasBuff(Entity entity) => SeededBuff.HasBuff(entity);
  public string BuffId => BuffService.SeededBuffId;
}