using System;

namespace BECOSOFT.Data.Converters {
    internal interface IFastDelegate {

        /// <summary>
        /// Delegate to get
        /// </summary>
        Func<object, object> GetDelegate { get; }

        /// <summary>
        /// Delegate to set
        /// </summary>
        Action<object, object> SetDelegate { get; }
    }
}
