namespace NuFridge.Service.Model
{
    /// <summary>
    /// A non-instantiable base entity which defines 
    /// members available across all entities.
    /// </summary>
    public  interface IEntityBase 
    {
        string Id { get; set; }
    }
}
