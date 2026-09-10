using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdKnit
{
    public int Trid { get; set; }

    public DateOnly? Trdate { get; set; }

    public int? Ftdid { get; set; }

    public string? KnitCom { get; set; }

    public string? Source { get; set; }

    public string? McNo { get; set; }

    public string? Sl { get; set; }

    public string? McBrand { get; set; }

    public int? McGauge { get; set; }

    public double? Rpm { get; set; }

    public int? McDia { get; set; }

    public string? StripeMeasure { get; set; }

    public byte[]? YarnPhoto { get; set; }

    public string? YarnPhotoContentType { get; set; }

    public byte[]? GsPhoto { get; set; }

    public string GsPhotoContentType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual TblFtdMaster? Ftd { get; set; }
}
