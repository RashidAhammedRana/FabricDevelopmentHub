namespace FabricDevelopmentHub.ViewModels
{
    public class PermissionPageVm
    {
        public string UserId { get; set; } = null!;

        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }

        public int MenuId { get; set; }
        public string? MenuName { get; set; }

        public int ActionId { get; set; }
        public string? ActionName { get; set; }

        public bool IsAllowed { get; set; }
    }
}