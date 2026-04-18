using BECOSOFT.Data.Services.Interfaces;
using System;

namespace BECOSOFT.Data.Validation {
    /// <summary>
    /// Options to manipulate <see cref="IService{T}"/>.<see cref="IService{T}.Save(T)"/> or <see cref="IValidator{T}"/>.<see cref="IValidator{T}.Validate(T)"/>.
    /// </summary>
    public interface ISaveOptions {
        SaveOptions GetDefault();

        /// <summary>
        /// Get options for the specified <paramref name="subType"/>.
        /// </summary>
        /// <param name="subType"></param>
        /// <returns></returns>
        ISaveOptions GetSubOptions(Type subType);
    }
}