using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdDyeingBasic
{
    public int Trid { get; set; }

    public DateOnly? Trdate { get; set; }

    public string? BatchNo { get; set; }

    public string? MachineNo { get; set; }

    public double? InhouseDyeing { get; set; }

    public double? ShadeP { get; set; }

    public string? Shade { get; set; }

    public string? DyeingPart { get; set; }

    public string? IsNonRft { get; set; }
    public string? LabDipNo { get; set; }
    public string? ProductName { get; set; }
    public double? Ratio { get; set; }

    public double? EnzayemPer { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
