using System;
using System.ComponentModel.DataAnnotations;
using Database.Enums;

namespace Database.Entities
{
    public class DBLine
    {
        public DBLine()
        {
            Timestamp = DateTimeOffset.Now;
        }

        public int Id { get; set; }
        public double? T1 { get; set; }
        public double? T2 { get; set; }
        public double? T3 { get; set; }
        public int? WinMaxRiskStake { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }
        [Required]
        public EDBLineType Type { get; set; }
        [Required]
        public EDBLine Line { get; set; }

        public long EventId { get; set; }
        public virtual DBEvent Event { get; set; }
    }
}