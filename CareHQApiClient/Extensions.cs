using System.Text.Json;

namespace CareHQ
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a property of a <see cref="JsonElement"/> as
        /// a nullable <see cref="JsonElement"/>
        /// </summary>
        /// <param name="jsonElement"></param>
        /// <param name="propertyName"></param>
        /// <returns>A nullable version of <see cref="JsonElement"/></returns>
        public static JsonElement? GetPropertyNullable(
            this JsonElement jsonElement, 
            string propertyName
        )
        {
            if (
                jsonElement.TryGetProperty(
                    propertyName, 
                    out JsonElement returnElement
                )
            )
            {
                if (returnElement.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }

                return returnElement;
            }

            return null;
        }
    }
}
