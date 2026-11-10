using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabTestResult
{
    public int ResultId { get; set; }

    public int? Ltnid { get; set; }
    public int? Ftdid { get; set; }

    public int? ParameterId { get; set; }

    public string? ResultValue { get; set; }

    public DateTime EntryDate { get; set; }

    public string? Remarks { get; set; }

    public virtual TblLabTestParameter Parameter { get; set; } = null!;
}
