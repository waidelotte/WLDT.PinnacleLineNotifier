namespace Database.Entities
{
    public class DBNotify
    {
        public int Id { get; set; }
        public bool IsExecuted { get; set; }

        public long EventId { get; set; }
        public virtual DBEvent Event { get; set; }
    }
}