using System;
using System.Collections.Generic;

namespace ProcessImage.Entities;

public partial class TipProcesare
{
    public long Id { get; set; }

    public string Nume { get; set; } = null!;

    public virtual ICollection<SubscripteProcesare> SubscripteProcesares { get; set; } = new List<SubscripteProcesare>();
}
