namespace BECOSOFT.Data.Models.Base {
    /// <summary>
    /// BaseResult represents an entity without identity.
    /// BaseResult maps to a query result.
    /// </summary>
    public abstract class BaseResult : IEntity {
        /// <summary>
        /// Get the entity's full type name.
        /// </summary>
        /// <returns>The entity's full type name</returns>
        public virtual string ToValidationLogString() {
            return GetType().FullName;
        }
    }
}
