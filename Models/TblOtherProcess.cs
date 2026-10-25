using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblOtherProcess
{
    public int Trid { get; set; }

    public int? Ftdid { get; set; }

    public string? AopType { get; set; }

    public double? NoOfColorW { get; set; }

    public string? RepeatSizeW { get; set; }

    public string? AopRemarks { get; set; }

    public string? WashType { get; set; }

    public string? GmtColorBw { get; set; }

    public string? GmtColorAw { get; set; }

    public string? WashRemarks { get; set; }

    public string? PrintType { get; set; }

    public double? NoOfColorP { get; set; }

    public string? RepeatSizeP { get; set; }

    public string? PrintRemarks { get; set; }

    public byte[]? ImageP { get; set; }

    public byte[]? ImageW { get; set; }
}
