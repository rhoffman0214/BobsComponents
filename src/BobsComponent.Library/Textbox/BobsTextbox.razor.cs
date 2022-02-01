using Microsoft.AspNetCore.Components;
using BobsComponent.Library.Constants;

namespace BobsComponent.Library.Textbox
{
    public partial class BobsTextbox
    {
        public TextBoxType TextBoxTypes { get; } = new();
        [Parameter]
        public string TextBoxType { get; set; } = string.Empty;
        [Parameter]
        public string Value { get; set; } = string.Empty;
        protected override void OnInitialized()
        {
            //give textbox the default type of text
            TextBoxType = TextBoxTypes.Text;
        }
    }
}