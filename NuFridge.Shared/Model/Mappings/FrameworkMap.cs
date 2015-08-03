using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class FrameworkMap : EntityMapping<IFramework>
    {
        public FrameworkMap()
        {
            TableName = "Framework";
            Column(f => f.Name);
        }
    }
}
