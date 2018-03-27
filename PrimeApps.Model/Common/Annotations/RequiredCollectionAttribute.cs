using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.Model.Common.Annotations
{
    /// <summary>
    /// Checks collection has at least one element
    /// </summary>
    public class RequiredCollectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var collection = value as ICollection;

            return collection?.Count > 0;
        }
    }
}