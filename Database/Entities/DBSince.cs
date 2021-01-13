using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class DBSince
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string For { get; set; }

        public long? Value { get; set; }
    }
}