using Microsoft.AspNetCore.Identity;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() { }
        public ApplicationRole(string name) { Name = name; }
    }
}
