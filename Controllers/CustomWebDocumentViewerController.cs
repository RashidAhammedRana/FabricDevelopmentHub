using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using Microsoft.AspNetCore.Mvc;

[Route("DXXRDV")]
public class CustomWebDocumentViewerController
    : DevExpress.AspNetCore.Reporting.WebDocumentViewer.WebDocumentViewerController
{
    public CustomWebDocumentViewerController(
        IWebDocumentViewerMvcControllerService controllerService)
        : base(controllerService)
    {
    }
}