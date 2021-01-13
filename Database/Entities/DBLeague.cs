using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class DBLeague
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }

        public int SportId { get; set; }
        public virtual DBSport Sport { get; set; }

        public virtual List<DBEvent> Events { get; set; }
    }
}