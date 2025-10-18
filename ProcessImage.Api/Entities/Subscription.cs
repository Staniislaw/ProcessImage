using System;
using System.Collections.Generic;

namespace ProcessImage.Entities;

public partial class Subscription
{
    public int Id { get; set; }

    public string? Tip { get; set; }

    public decimal Pret { get; set; }

    public int DimensiuneMaximaMb { get; set; }

    public int? SubscriptieProcesareId { get; set; }
}
