namespace TemplateMQ.API.Application.Commands;

public class AddSampleCommand: IRequest<SampleViewModel>
{
    public string Name { get; }

    public AddSampleCommand(string name)
    {
            this.Name = name;
    }
}
