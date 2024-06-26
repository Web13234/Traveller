﻿using ColdMint.scripts.item.itemStacks;

using Godot;

namespace ColdMint.scripts.item;

public interface IItem
{
    /// <summary>
    /// <para>ID of current item</para>
    /// <para>当前项目的ID</para>
    /// </summary>
    string Id { get; }
    /// <summary>
    /// <para>Icon of current item</para>
    /// <para>当前项目的图标</para>
    /// </summary>
    Texture2D Icon { get; }
    /// <summary>
    /// <para>Display name of current item</para>
    /// <para>显示当前Item的名称</para>
    /// </summary>
    string Name { get; }
    /// <summary>
    /// <para>Description of current item, which may show in inventory</para>
    /// <para>当前项目的描述</para>
    /// </summary>
    string? Description { get; }
    /// <summary>
    /// <para>
    ///     Whether the current item can be put into item containers like packsack.<br/>
    ///     This attribute is usually set to false for items of the backpack class to avoid pack nesting.
    /// </para>
    /// <para>当前物品是否可以放入背包类的容器中。一般将背包类物品的该属性设为false来避免背包嵌套。</para>
    /// </summary>
    bool CanPutInPack { get; }

    /// <summary>
    /// <para>Execute when current item is used <br/> e.g. when player clicks left mouse button with current item in hand</para>
    /// <para>当前项被使用时执行 <br/> e.g. 当玩家用鼠标左键点击当前物品时</para>
    /// </summary>
    /// <param name="owner">Owner of current item, if any</param>
    /// <param name="targetGlobalPosition">Target position, such as the position of the cursor when used by the player</param>
    void Use(Node2D? owner, Vector2 targetGlobalPosition);

    /// <summary>
    /// <para>Execute when current item be removed from game.</para>
    /// <para>当前物品从游戏中移除时执行。</para>
    /// </summary>
    void Destroy();

    /// <summary>
    /// <para>Return true if this item can be stacked with the given item in one stack</para>
    /// <para>若该物品是否能与给定物品堆叠在同一个物品堆上，返回true</para>
    /// </summary>
    /// <remarks>
    /// <para>
    ///     ! Note in the implementation:  the correspondence of this predicate must be an equivalence relation over the full set of stackable items
    ///     (i.e., be able to derive a division into the full set of stackable items).<br/>
    ///     Or if the item is not stackable, make it always return false.
    /// </para>
    /// <para>
    ///     ！实现时注意：该谓词的对应关系必须是在全部可堆叠的物品集合上的等价关系（即能导出一个对可堆叠物品全集的划分）。<br/>
    ///     或如果该物品不可堆叠，令其永远返回false。<br/>
    /// </para>
    /// <para>
    ///     If it is necessary to implement special stacking relationships (e.g. containers that can be stacked to hold items),
    ///     customize the special <see cref="IItemStack"/> type, implement the special stacking determination in it by overriding <see cref="IItemStack.CanAddItem"/>,
    ///     and override the <see cref="SpecialStack"/> method so that it returns an instance of that custom ItemStack.
    /// </para>
    /// <para>
    ///     如果有必要实现特殊的堆叠关系（如可以用堆叠来收纳物品的容器），请自定义特殊的<see cref="IItemStack"/>类型，在其中重写<see cref="IItemStack.CanAddItem"/>以实现特殊的堆叠判定，
    ///     并重写<see cref="SpecialStack"/>方法使其返回该自定义ItemStack实例。
    /// </para>
    /// </remarks>
    bool CanStackWith(IItem item);

    /// <summary>
    /// <para>If this item need a special stack type, return the special item stack instance that contains the item. If else, just leave this null.</para>
    /// <para>如果该项目需要特殊的物品堆类型，重写此方法来返回包含该物品的特殊物品堆实例。否则，保留原本的null返回值。</para>
    /// </summary>
    /// <seealso cref="CanStackWith"/>
    IItemStack? SpecialStack() => null;
}