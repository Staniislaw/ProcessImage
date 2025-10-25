using ProcessImage.Domain;

using System;
using System.Collections.Generic;

namespace ProcessImage.Entities;

public partial class SubscripteProcesare
{
    public long Id { get; set; }

    public long SubscriptieId { get; set; }

    public long TipProcesareId { get; set; }

    public int? LimitaMax { get; set; }

    public virtual Subscriptie Subscriptie { get; set; } = null!;

    public virtual TipProcesare TipProcesare { get; set; } = null!;
}
