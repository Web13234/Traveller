﻿using ColdMint.scripts.inventory;
using ColdMint.scripts.pickable;
using ColdMint.scripts.utils;

using Godot;
using PacksackUi = ColdMint.scripts.loader.uiLoader.PacksackUi;

namespace ColdMint.scripts.item;

/// <summary>
/// <para>packsack</para>
/// <para>背包</para>
/// </summary>
public partial class Packsack : PickAbleTemplate
{
    private PackedScene? _packedScene;
    private PacksackUi? _packsackUi;
    [Export]
    public int NumberSlots { get; set; }

    public override bool CanPutInPack => false;

    public override void Destroy()
    {
        if (ItemContainer == null) return;
        foreach (var itemSlot in ItemContainer)
        {
            itemSlot.ClearSlot();
        }

        QueueFree();
    }

    public override void Use(Node2D? owner, Vector2 targetGlobalPosition)
    {
        if (_packedScene == null)
        {
            return;
        }

        if (_packsackUi == null)
        {
            _packsackUi = NodeUtils.InstantiatePackedScene<PacksackUi>(_packedScene,this);
            if (_packsackUi != null)
            {
                _packsackUi.Title = Name;
                _packsackUi.ItemContainer = ItemContainer;
            }
        }
        GameSceneNodeHolder.BackpackUiContainer?.Show();
        _packsackUi?.Show();
    }

    public IItemContainer? ItemContainer { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        ItemContainer = new UniversalItemContainer();
        //When the backpack is created, the item slot is generated.
        //当背包被创建时，物品槽就被生成出来了。
        for (var i = 0; i < NumberSlots; i++)
        {
            ItemContainer.AddItemSlot(this)?.Hide();
        }
        _packedScene = GD.Load<PackedScene>("res://prefab/ui/packsackUI.tscn");
    }
}