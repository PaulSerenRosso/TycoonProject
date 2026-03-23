public abstract class Entity
{
    public string Id { get; protected set; }
    
    protected Entity(string id)
    {
        Id = id ?? System.Guid.NewGuid().ToString();
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Entity entity)
            return Id == entity.Id;
        return false;
    }
    
    public override int GetHashCode() => Id.GetHashCode();
}
