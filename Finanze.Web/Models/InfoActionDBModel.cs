using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Finanze.Web.Models
{
    public class InfoActionDBModel
    {
        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; }

        public bool IsError { get; set; } = false;

        public MudBlazor.Severity Severity { get {
                return IsError ? MudBlazor.Severity.Error: MudBlazor.Severity.Success;
            }
        }

        public void SetError(string message)
        {
            IsError = true;
            Message = message;
            ShowMessage = true;
        }


        public void SetSuccess(string message)
        {
            IsError = false;
            Message = message;
            ShowMessage = true;
        }


        public RenderFragment Render => __builder =>
        {
            if (!ShowMessage)
                return;

            __builder.OpenComponent(0, typeof(MudItem));
            __builder.AddAttribute(1, "xs", "12");
            __builder.AddAttribute(2, "ChildContent", (RenderFragment)(__builder2 =>
            {
                __builder2.OpenComponent(3, typeof(MudAlert));
                __builder2.AddAttribute(4, "Severity", Severity);
                __builder2.AddAttribute(5, "ChildContent", (RenderFragment)(__builder3 =>
                {
                    __builder3.AddContent(6, Message);
                }));
                __builder2.CloseComponent();
            }));
            __builder.CloseComponent();
        };
    }

}

