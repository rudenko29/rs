using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace redsoft.Models
{
    public class AccountHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public Account Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
