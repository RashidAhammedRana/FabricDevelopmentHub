using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabTestName
{
    public int Ltnid { get; set; }

    public DateOnly? Date { get; set; }

    public string? TestName { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public int? Ltgid { get; set; }

    public virtual TblLabTestGroup? Ltg { get; set; }

    public virtual ICollection<TblLabTestMethod> TblLabTestMethods { get; set; } = new List<TblLabTestMethod>();
}
