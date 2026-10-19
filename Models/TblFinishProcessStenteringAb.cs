using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFinishProcessStenteringAb
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? Temp { get; set; }

    public double? Speed { get; set; }

    public double? OFeed { get; set; }

    public double? StretchPer { get; set; }

    public double? PPressure1 { get; set; }

    public double? PPressure2 { get; set; }

    public double? Chemical { get; set; }

    public double? BlowerSpeed { get; set; }

    public double? Noc { get; set; }

    public double? ADia { get; set; }

    public double? AGsm { get; set; }

    public double? AShrinkage { get; set; }

    public string? Comments { get; set; }
}
