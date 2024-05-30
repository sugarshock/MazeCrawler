using System;
using Godot;
[GlobalClass]
public partial class Memory : Resource
{
    [Export] public String Text;
    [Export] public Texture2D Image;
    [Export] public AudioStream Audio;
    [Export] public String EpicId;
    [Export] public String SoulId;
    

}