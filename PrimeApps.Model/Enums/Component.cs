using System;

namespace PrimeApps.Model.Enums
{
    /// <summary>
    /// These are our predefined components for our server and client infrastructure. We have 6 different component region.
    /// Every webservice method affiliated with one of these components or more of them. Same thing applies to the client side also.
    /// Every view on the client side have a relation with one or more of these components.
    /// On clientside we are checking these components for the views every time when we have an update. 
    /// It's necessary for not to update every view when we got an update on the client side. 
    /// </summary>
    [Flags]
    public enum Component
    {
        Avatars = 1,
        Tasks = 2,
        Documents = 4,
        Entities = 8,
        Users = 16,
        Licenses = 32
    }
}