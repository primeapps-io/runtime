using System;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AssemblyVersionStaticAttribute : Attribute
{
    private string _version;

    public AssemblyVersionStaticAttribute(string version)
    {
        _version = version;
    }

    public string Version
    {
        get { return _version; }
    }
}