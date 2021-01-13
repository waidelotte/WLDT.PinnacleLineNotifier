using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class DBEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        [Required]
        public string Home { get; set; }
        [Required]
        public string Away { get; set; }
        [Required]
        public bool IsLive { get; set; }
        [Required]
        public bool IsOpen { get; set; }
        [Required]
        public DateTimeOffset StartAt { get; set; }

        public int LeagueId { get; set; }
        public virtual DBLeague League { get; set; }

        public virtual List<DBLine> Lines { get; set; }
    }
}