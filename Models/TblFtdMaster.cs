using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdMaster
{
    public int Ftdid { get; set; }

    public DateOnly? Trdate { get; set; }

    public string? FtdNo { get; set; }

    public string? Company { get; set; }

    public string? Control { get; set; }

    public string? BatchNo { get; set; }

    public int? SysReq { get; set; }

    public string? Buyer { get; set; }

    public string? StyleRef { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<TblFtdFabric> TblFtdFabrics { get; set; } = new List<TblFtdFabric>();

    public virtual ICollection<TblFtdKnit> TblFtdKnits { get; set; } = new List<TblFtdKnit>();

    public virtual ICollection<TblFtdYarn> TblFtdYarns { get; set; } = new List<TblFtdYarn>();
}
