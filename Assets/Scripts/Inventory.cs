using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Inventory
{
    private readonly List<ItemInstance> items = new();
    
    // ===============================================================================
    //  Public API
    // ================================================================================

    public void Add(ItemDefinition definition, int quantity = 1)
    {
        var existing = items.FirstOrDefault(item => item.Definition.ID == definition.ID && item.Quantity < definition.MaxStackSize);
            
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            items.Add(new ItemInstance
            {
                Quantity    = quantity,
                Definition  = definition,
                Durability  = definition.MaxDurability,
            });
        }
    }
    
    public IEnumerable<ItemInstance> GetAll()
    {
        return items;
    }
    public IEnumerable<ItemInstance> GetByType(Type itemType)
    {
        return items.Where(i => itemType.IsAssignableFrom(i.Definition.GetType()));
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations                                      
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ItemDefinition : Definition
{
    public string Description;
    public Sprite Icon;
    public int MaxStackSize;
    public int MaxDurability;
}

public class ItemInstance : Item
{
    public ItemDefinition Definition;
    public int Quantity;
    public int Durability;
    public List<string> Enchantments;
}

