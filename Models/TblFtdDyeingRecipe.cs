using System;
using System.Collections.Generic;

namespace FabricDevelopmentHub.Models;

public partial class TblFtdDyeingRecipe
{
    public int Trid { get; set; }

    public DateOnly? Trdate { get; set; }

    public string? BatchNo { get; set; }

    public string? RecipeDetails { get; set; }

    public string? Ratio { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}
