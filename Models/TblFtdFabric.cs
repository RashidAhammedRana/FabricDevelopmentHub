using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdFabric
{
    public int Trid { get; set; }

    public DateOnly? Trdate { get; set; }

    public int? Ftdid { get; set; }

    public string? Color { get; set; }

    public string? ColorType { get; set; }

    public string? Fabrication { get; set; }

    public double? ReqDia { get; set; }

    public string? DiaType { get; set; }

    public double? ReqGsm { get; set; }

    public double? FinDia { get; set; }

    public string? FinDiaType { get; set; }

    public double? FinGsm { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual TblFtdMaster? Ftd { get; set; }
}
