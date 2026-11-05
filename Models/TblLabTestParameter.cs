using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabTestParameter
{
    public int ParameterId { get; set; }

    public int Ltnid { get; set; }

    public string ParameterGroup { get; set; } = null!;
    public string ParameterName { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public string? Unit { get; set; }

    public string? NormalRange { get; set; }

    public string DataType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<TblLabTestResult> TblLabTestResults { get; set; } = new List<TblLabTestResult>();
}
