using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FabricDevelopmentHub.Models;

public partial class TblCompanyInfo
{
    public int Comid { get; set; }
    [Required(ErrorMessage = "This field is required")]
    public string? ComName { get; set; }

    public string? ComContact { get; set; }

    public string? ComEmail { get; set; }
    [Required(ErrorMessage = "This field is required")]
    public string? ComAddress { get; set; }
    [Required(ErrorMessage = "This field is required")]
    public string? Status { get; set; }

    public string? Remarks { get; set; }


}
