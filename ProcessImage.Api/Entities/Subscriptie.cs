using System;
using System.Collections.Generic;

namespace ProcessImage.Entities;

public partial class Subscriptie
{
    public long Id { get; set; }

    public string Tip { get; set; } = null!;

    public decimal Pret { get; set; }

    public decimal DimensiuneMaximaMb { get; set; }

    public long SubscriptieProcesareId { get; set; }

    public virtual ICollection<SubscripteProcesare> SubscripteProcesares { get; set; } = new List<SubscripteProcesare>();

    public virtual ICollection<Utilizator> Utilizators { get; set; } = new List<Utilizator>();
}
