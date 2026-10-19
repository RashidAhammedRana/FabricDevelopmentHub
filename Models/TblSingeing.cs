using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblSingeing
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? BurnerTemp { get; set; }

    public double? FlameHeight { get; set; }

    public double? Speed { get; set; }

    public double? ASingeingGsm { get; set; }

    public double? ASingeingDia { get; set; }
}
