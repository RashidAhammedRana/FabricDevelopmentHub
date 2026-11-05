using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblLabTestGroup
{
    public int Ltgid { get; set; }

    public DateOnly? Date { get; set; }

    public string? GroupName { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<TblLabTestName> TblLabTestNames { get; set; } = new List<TblLabTestName>();
}
