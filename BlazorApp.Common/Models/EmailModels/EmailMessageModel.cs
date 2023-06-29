using System.Collections.Generic;

namespace BlazorApp.Common.Models.EmailModels;

public class EmailMessageModel
{
    public EmailMessageModel()
    {
        ToAddresses = new List<EmailAddressModel>();

        FromAddresses = new List<EmailAddressModel>();

        CcAddresses = new List<EmailAddressModel>();

        BccAddresses = new List<EmailAddressModel>();
    }

    public List<EmailAddressModel> ToAddresses { get; set; }

    public List<EmailAddressModel> FromAddresses { get; set; }

    public List<EmailAddressModel> BccAddresses { get; set; }

    public List<EmailAddressModel> CcAddresses { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsHtml { get; set; } = true;
}