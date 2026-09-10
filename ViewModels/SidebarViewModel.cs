namespace FabricDevelopmentHub.ViewModels
{
    public class SidebarViewModel
    {
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }

        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? Url { get; set; }
        public int ActionId { get; set; }
    }
}
