using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblSqueezing
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? Pp { get; set; }

    public string? DiaType { get; set; }
    public string? Speed { get; set; }
    public string? Tension { get; set; }
    public string? WetDia { get; set; }
    public string? WetGsm { get; set; }
    public string? ChemicalName { get; set; }
    public double? Ratio { get; set; }
}
