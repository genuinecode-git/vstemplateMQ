namespace TemplateMQ.Domain.Entities;

public partial class Sample
{
    public Sample( string name)
    {
        Validate( name);
        this.Name = name;
    }

    private void Validate( string name)
    {
        List<string> errors = [];
        if (string.IsNullOrEmpty(name)) errors.Add($"{nameof(name)} is required.");

        if (errors.Count != 0)
            throw new ArgumentException(string.Join(" ", errors));
    }
}
