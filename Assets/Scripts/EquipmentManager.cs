using System.Collections.Generic;
using System.Linq;



public class EquipmentManager : Service
{

    readonly Actor owner;
    readonly Dictionary<EquipmentSlotType, EquipmentSlot> slots = new();
        
    // ===============================================================================

    public EquipmentManager(Actor actor)
    {
        owner = actor;

        slots[EquipmentSlotType.Head    ] = new() { SlotType = EquipmentSlotType.Head       };
        slots[EquipmentSlotType.Cloak   ] = new() { SlotType = EquipmentSlotType.Cloak      };
        slots[EquipmentSlotType.MainHand] = new() { SlotType = EquipmentSlotType.MainHand   };
        slots[EquipmentSlotType.OffHand ] = new() { SlotType = EquipmentSlotType.OffHand    };
        slots[EquipmentSlotType.Dash    ] = new() { SlotType = EquipmentSlotType.Dash       };

        owner.Emit.Link.Local<Message<Request, EquipEvent>>(HandleEquipEvent);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void Equip(Equipment item)
    {
        EquipOrSwap(item);
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

    // ===============================================================================
    
    void EquipOrSwap(Equipment item)
    {
        slots.TryGetValue(item.SlotType, out var slot);
        
        if (slot.HasEquipped())
            UnequipItem(item.SlotType);
        
        slot.Equip(item);

        owner.Emit.Local(Publish.Equipped, new EquipmentChangeEvent(owner, item, item.SlotType));

        DebugLog();
    }
    
    public void EquipItem(Equipment item)
    {
        slots.TryGetValue(item.SlotType, out var slot);
        
        slot.Equip(item);

        owner.Emit.Local(Publish.Equipped, new EquipmentChangeEvent(owner, item, item.SlotType));

        DebugLog();
    }    

    public Equipment UnequipItem(EquipmentSlotType slotType)
    {
        slots.TryGetValue(slotType, out var slot);
        
        var item = slot.Unequip();

        if (item != null)
            owner.Emit.Local(Publish.Unequipped, new EquipmentChangeEvent(owner, item, item.SlotType));
        
        DebugLog();
        return item;
    }

    // ===============================================================================

    void HandleEquipEvent(Message<Request, EquipEvent> message)
    {
        EquipOrSwap(message.Payload.Equipment);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Equipment);

    void DebugLog()
    {
        Log.Debug("Head",      () => slots[EquipmentSlotType.Head]?.Equipped       );
        Log.Debug("Cloak",     () => slots[EquipmentSlotType.Cloak]?.Equipped      );
        Log.Debug("MainHand",  () => slots[EquipmentSlotType.MainHand]?.Equipped   );
        Log.Debug("Offhand",   () => slots[EquipmentSlotType.OffHand]?.Equipped    );
        Log.Debug("Dash",      () => slots[EquipmentSlotType.Dash]?.Equipped       );
    }

    public override void Dispose()
    {
        // NO OP   
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum EquipmentSlotType
{
    Head,
    Cloak,
    MainHand,
    OffHand,
    Dash
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Classes
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EquipmentSlot
{
    public EquipmentSlotType SlotType   { get; init; }
    public Equipment Equipped           { get; private set; }
    
    // ===============================================================================
    //  Public API
    // ===============================================================================

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

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct EquipEvent
{
    public readonly Actor Owner              { get; init; }
    public readonly Equipment Equipment      { get; init; }

    public EquipEvent(Actor owner, Equipment equipment)
    {
        Owner       = owner;
        Equipment   = equipment;
    }
}


public readonly struct EquipmentChangeEvent
{
    public readonly Actor Owner              { get; init; }
    public readonly Equipment Equipment      { get; init; }
    public readonly EquipmentSlotType Slot   { get; init; }

    public EquipmentChangeEvent(Actor owner, Equipment equipment, EquipmentSlotType slot)
    {
        Owner       = owner;
        Equipment   = equipment;
        Slot        = slot;
    }
}