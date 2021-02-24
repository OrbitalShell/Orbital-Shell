using System.Collections.Generic;
using System.Linq;

namespace OrbitalShell.Component.Shell.Module.Data
{
    public class ModuleList
    {
        public List<ModuleReference> Modules
            = new List<ModuleReference>();    

        public void Merge(ModuleList modList)
        {
            foreach ( var modRef in modList.Modules )
            {
                if (!Modules.Where(x => x.ModuleId == modRef.ModuleId).Any())
                    Modules.Add(modRef);
            }
        }

    }
}
