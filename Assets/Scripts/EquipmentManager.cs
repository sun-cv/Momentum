using System;
using System.Collections.Generic;
using System.Linq;





public enum EquipmentSlotType
{
    Head,
    Cloak,
    MainHand,
    OffHand,
    Dash
}


public class EquipmentSlot
{
    public EquipmentSlotType SlotType   { get; init; }
    public Equipment Equipped           { get; private set; }
    
    public bool CanEquip(Equipment item)
    {
        return item.SlotType == SlotType;
    }
    
    public bool Equip(Equipment item)
    {
        if (!CanEquip(item))
            return false;
            
        Equipped = item;
        return true;
    }
    
    public Equipment Unequip()
    {
        var item = Equipped;
        Equipped = null;
        return item;
    }

    public bool HasEquipped()
    {
        return Equipped != null;
    }
}


public class EquipmentManager
{
    readonly Logger Log = Logging.For(LogSystem.Equipment);

    readonly Actor owner;
    readonly Dictionary<EquipmentSlotType, EquipmentSlot> slots = new();
    
    public EquipmentManager(Actor actor)
    {
        owner = actor;

        slots[EquipmentSlotType.Head    ] = new() { SlotType = EquipmentSlotType.Head       };
        slots[EquipmentSlotType.Cloak   ] = new() { SlotType = EquipmentSlotType.Cloak      };
        slots[EquipmentSlotType.MainHand] = new() { SlotType = EquipmentSlotType.MainHand   };
        slots[EquipmentSlotType.OffHand ] = new() { SlotType = EquipmentSlotType.OffHand    };
        slots[EquipmentSlotType.Dash    ] = new() { SlotType = EquipmentSlotType.Dash       };
    }
    
    public bool Equip(Equipment item)
    {
        if (!slots.TryGetValue(item.SlotType, out var slot))
            return false;
        
        if (slot.HasEquipped())
            Unequip(item.SlotType);
        
        if (!slot.Equip(item))
            return false;

        EmitLocal<EquipmentPublish>(new(Guid.NewGuid(), Publish.Equipped, new() { Owner = owner, Equipment = item, Slot = item.SlotType } ));
        
        DebugLog();
        return true;
    }
    
    public Equipment Unequip(EquipmentSlotType slotType)
    {
        if (!slots.TryGetValue(slotType, out var slot))
            return null;
        
        var item = slot.Unequip();

        if (item != null)
            EmitLocal<EquipmentPublish>(new(Guid.NewGuid(), Publish.Unequipped, new() { Owner = owner, Equipment = item, Slot = item.SlotType } ));
        
        DebugLog();
        return item;
    }
    
    public Equipment GetEquipped(EquipmentSlotType slotType)
    {
        return slots.TryGetValue(slotType, out var slot) ? slot.Equipped : null;
    }
    
    public IEnumerable<Equipment> GetAllEquipped()
    {
        return slots.Values.Select(slot => slot.Equipped).Where(equipment => equipment != null);
    }

    public List<Weapon> GetEquippedWeapons()
    {
        var weapons = new List<Weapon>
        {
            (Weapon)GetEquipped(EquipmentSlotType.MainHand),
            (Weapon)GetEquipped(EquipmentSlotType.OffHand)
        };
        return weapons;
    }

    void DebugLog()
    {
        Log.Debug("Head",      () => slots[EquipmentSlotType.Head]?.Equipped       );
        Log.Debug("Cloak",     () => slots[EquipmentSlotType.Cloak]?.Equipped      );
        Log.Debug("MainHand",  () => slots[EquipmentSlotType.MainHand]?.Equipped   );
        Log.Debug("Offhand",   () => slots[EquipmentSlotType.OffHand]?.Equipped    );
        Log.Debug("Dash",      () => slots[EquipmentSlotType.Dash]?.Equipped       );
    }

    void EmitLocal <T>(T evt) where T : IEvent              => owner.Bus.Raise(evt);
}


public readonly struct EquipmentPayload
{
    public readonly Actor Owner              { get; init; }
    public readonly Equipment Equipment      { get; init; }
    public readonly EquipmentSlotType Slot   { get; init; }
}

public readonly struct EquipmentPublish : ISystemEvent
{
    public Guid Id                  { get; }
    public Publish Action           { get; }
    public EquipmentPayload Payload { get; }
    
    public EquipmentPublish(Guid id, Publish action, EquipmentPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}

