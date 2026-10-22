using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblOtherProcess
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public double? AopType { get; set; }

    public double? NoOfColorW { get; set; }

    public double? RepeatSizeW { get; set; }

    public string? AopRemarks { get; set; }

    public double? WashType { get; set; }

    public double? GmtColorBw { get; set; }

    public double? GmtColorAw { get; set; }

    public double? WashRemarks { get; set; }

    public double? PrintType { get; set; }

    public double? NoOfColorP { get; set; }

    public double? RepeatSizeP { get; set; }

    public string? PrintRemarks { get; set; }

    public byte[]? ImageP { get; set; }

    public byte[]? ImageW { get; set; }
}
