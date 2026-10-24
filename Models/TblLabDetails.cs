using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabDetails
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public string? LabDipNo { get; set; }

    public string? LabProductName { get; set; }

    public double? Ratio { get; set; }
}
