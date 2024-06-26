﻿namespace ColdMint.scripts.behaviorTree;

/// <summary>
/// <para>BehaviorTreeTemplate</para>
/// <para>行为树模板</para>
/// </summary>
public abstract class BehaviorTreeTemplate : IBehaviorTree
{
    private IBehaviorTreeNode? _root;
    private string? _id;
    public IBehaviorTreeNode? Root => _root;
    public string? Id => _id;

    public void Init()
    {
        _root = CreateRoot();
        _id = CreateId();
    }

    protected abstract IBehaviorTreeNode? CreateRoot();
    
    protected abstract string? CreateId();
}