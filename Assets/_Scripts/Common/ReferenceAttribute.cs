using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ReferenceAttribute : Attribute
{
    public string Path { get; }
    public bool Required { get; }

    public ReferenceAttribute(string path = "", bool required = true)
    {
        Path = path;
        Required = required;
    }
}
