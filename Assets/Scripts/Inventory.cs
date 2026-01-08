using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





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


public class Inventory
{
    private List<ItemInstance> items = new();
    
    public void Add(ItemDefinition definition, int quantity = 1)
    {
        var existing = items.FirstOrDefault(i => 
            i.Definition.ID == definition.ID && 
            i.Quantity < definition.MaxStackSize);
            
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            items.Add(new ItemInstance
            {
                Definition = definition,
                Quantity = quantity,
                Durability = definition.MaxDurability
            });
        }
    }
    
    public IEnumerable<ItemInstance> GetAll() => items;
    
    public IEnumerable<ItemInstance> GetByType(Type itemType)
    {
        return items.Where(i => itemType.IsAssignableFrom(i.Definition.GetType()));
    }
}