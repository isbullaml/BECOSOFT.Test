using System.Collections.Generic;
using System.Management.Automation;

namespace BECOSOFT.Data.Models.Powershell {
    public class PowershellResult {
        public List<PSObject> Objects { get; set; }
    }
}