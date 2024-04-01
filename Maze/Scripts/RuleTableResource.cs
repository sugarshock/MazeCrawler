using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class RuleTableResource : Resource
{
    [Export] public Godot.Collections.Array<RuleResource> RuleTable = new Godot.Collections.Array<RuleResource>();
    
    public void AddUnique(RuleResource rule)
    {
        // check if the rule already exists in the RuleTable
        foreach (var r in RuleTable)
        {
            if (r.Id == rule.Id 
                && r.Orientation == rule.Orientation
                && r.Direction == rule.Direction 
                && r.NeighborId == rule.NeighborId 
                && r.NeighborOrientation == rule.NeighborOrientation)
                return;
        }
        RuleTable.Add(rule);
    }
    
    public List<int> GetIds()
    {
        return RuleTable.Select(x => x.Id).Distinct().ToList();
    }
    
    public List<int> GetOrientations()
    {
        return RuleTable.Select(x => x.Orientation).Distinct().ToList();
    }
    
}