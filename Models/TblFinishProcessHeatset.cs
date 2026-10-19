using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFinishProcessHeatset
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? Noc { get; set; }

    public double? Nop { get; set; }

    public double? DweelTime { get; set; }

    public double? GreyDia { get; set; }

    public double? GreyGsm { get; set; }

    public double? Temp { get; set; }

    public double? Speed { get; set; }

    public double? OFeed { get; set; }

    public double? UFeed { get; set; }

    public double? Blower { get; set; }

    public double? Pp { get; set; }

    public double? Stretching { get; set; }

    public double? AHeatsetGsm { get; set; }

    public double? AHeatsetDia { get; set; }

    public double? Comments { get; set; }
}
