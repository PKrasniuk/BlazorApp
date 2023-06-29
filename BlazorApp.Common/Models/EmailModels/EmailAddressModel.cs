namespace BlazorApp.Common.Models.EmailModels;

public class EmailAddressModel
{
    private string _name;

    public EmailAddressModel(string name, string address)
    {
        _name = name;
        Address = address;
    }

    public string Name
    {
        get => string.IsNullOrEmpty(_name) ? Address : _name;
        set => _name = value;
    }

    public string Address { get; set; }
}