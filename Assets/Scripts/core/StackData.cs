using System;

[Serializable]
public class StackData
{
    public PlayerColor owner;
    public int height;

    public StackData(PlayerColor owner, int height)
    {
        this.owner = owner;
        this.height = height;
    }
}