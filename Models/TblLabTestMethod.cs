using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabTestMethod
{
    public int Ltmid { get; set; }

    public DateOnly? Date { get; set; }

    public string? MethodName { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public int? Ltnid { get; set; }

    public virtual TblLabTestName? Ltn { get; set; }
}
