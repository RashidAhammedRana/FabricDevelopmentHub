using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblCompacting
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? Temp { get; set; }

    public double? Speed { get; set; }

    public double? OFeed { get; set; }

    public double? CompactionPer { get; set; }

    public double? Steam { get; set; }

    public double? ADia { get; set; }

    public double? AGsm { get; set; }

    public double? FGsm { get; set; }

    public double? RDia { get; set; }

    public double? RGsm { get; set; }

    public double? Shrinkage { get; set; }

    public double? Comments { get; set; }
}
