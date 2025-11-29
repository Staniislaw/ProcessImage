using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProcessImage.Entities;

public partial class Utilizator
{
    public long Id { get; set; }

    public string Nume { get; set; } = null!;

    public string Parola { get; set; } = null!;

    public long SubscriptieId { get; set; }

    public string Email { get; set; } = null!;
    public bool isActive { get; set; } = true;
    public long? RolId { get; set; }
    public virtual Subscriptie Subscriptie { get; set; } = null!;
    public virtual Rol Rol { get; set; } = null!;
}
