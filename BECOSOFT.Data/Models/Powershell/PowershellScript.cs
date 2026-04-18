using System;
using System.Collections;

namespace BECOSOFT.Data.Models.Powershell {
    /// <summary>
    /// Model for Powershell scripts
    /// </summary>
    public class PowershellScript {
        /// <summary>
        /// The script
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// The parameters
        /// </summary>
        public IDictionary Parameters { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="script">The script</param>
        /// <param name="parameters">The parameters</param>
        public PowershellScript(IPowerShellScript script, IDictionary parameters) {
            Script = script.GetScript();
            Parameters = parameters;
        }
    }
}
