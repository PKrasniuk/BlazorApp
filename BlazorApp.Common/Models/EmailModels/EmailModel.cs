namespace BlazorApp.Common.Models.EmailModels
{
    public class EmailModel
    {
        private string _name;

        public string ToAddress { get; set; }

        public string ToName
        {
            get => string.IsNullOrEmpty(_name) ? ToAddress : _name;
            set => _name = value;
        }

        public string FromName { get; set; }

        public string FromAddress { get; set; }

        public string ReplyToAddress { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string TemplateName { get; set; }
    }
}