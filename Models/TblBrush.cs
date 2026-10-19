using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblBrush
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? UPile { get; set; }

    public double? LPile { get; set; }

    public double? UCp { get; set; }

    public double? LCp { get; set; }

    public double? UDrumSpeed { get; set; }

    public double? LDrumSpeed { get; set; }

    public double? UTension { get; set; }

    public double? LTension { get; set; }

    public double? McSpeed { get; set; }

    public double? ADia { get; set; }

    public double? AGsm { get; set; }
}
