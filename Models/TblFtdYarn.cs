using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdYarn
{
    public int Trid { get; set; }

    public DateOnly? Trdate { get; set; }

    public int? Ftdid { get; set; }

    public string? Count { get; set; }

    public string? LotNo { get; set; }

    public string? Brand { get; set; }

    public string? Tpi { get; set; }

    public string? Std { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual TblFtdMaster? Ftd { get; set; }
}
