using System;

namespace NuFridge.DataAccess.Model
{
    /// <summary>
    /// A non-instantiable base entity which defines 
    /// members available across all entities.
    /// </summary>
    public  interface IEntityBase 
    {
        Guid Id { get; set; }
    }
}
