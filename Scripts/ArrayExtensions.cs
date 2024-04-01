
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

public static class ArrayExtensions
{
    
    public static void AddByPath(this Dictionary dict, List<Variant> path, Variant value)
    {
        Dictionary current = dict;
        for (int i = 0; i < path.Count; i++)
        {
            Variant key = path[i];
            if (i == path.Count - 1)
            {   
                if(!current.ContainsKey(key))
                {
                    current[key] = new Array();
                }

                var array = ((Array)current[key]);
                if(!array.Contains(value))
                       array.Add(value);
            }
            else
            {
                if (!current.ContainsKey(key))
                {
                    current[key] = new Godot.Collections.Dictionary<string, Variant>();
                }
                current = (Godot.Collections.Dictionary)current[key];
            }
        }
    }
    
    public static Variant GetByPath(this Dictionary dict, List<Variant> path)
    {
        Dictionary current = dict;
        for (int i = 0; i < path.Count; i++)
        {
            Variant key = path[i];
            if (!current.ContainsKey(key))
            {
                return new Array();
            }
            if (i == path.Count - 1)
            {
                return current[key];
            }
            else
            {
                current = (Godot.Collections.Dictionary)current[key];
            }
        }
        return new Array();
    }
}