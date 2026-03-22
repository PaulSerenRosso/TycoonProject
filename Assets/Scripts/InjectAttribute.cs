using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public bool Optional { get; set; } = false;

    public InjectAttribute(bool optional = false)
    {
        Optional = optional;
    }
}
