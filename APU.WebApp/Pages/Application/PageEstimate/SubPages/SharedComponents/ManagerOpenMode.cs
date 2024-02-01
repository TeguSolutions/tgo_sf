namespace APU.WebApp.Pages.Application.PageEstimate.SubPages.SharedComponents;

// ReSharper disable once EmptyConstructor
public class ManagerOpenMode
{
    public ManagerOpenMode()
    {

    }

    public string Name { get; set; }
    public string Header { get; set; }
    public List<(string button, ManagerSubmitMode mode)> Buttons { get; set; }



    public static ManagerOpenMode Create { get; } = new()
    {
        Name = "Create",
        Header = "Create",
        Buttons = new List<(string button, ManagerSubmitMode mode)>()
        {
            ("Create", ManagerSubmitMode.Create)
        }
    };

    public static ManagerOpenMode Copy { get; } = new()
    {
        Name = "Copy",
        Header = "Copy",
        Buttons = new List<(string button, ManagerSubmitMode mode)>()
        {
            ("Copy", ManagerSubmitMode.Copy)
        }
    };

    public static ManagerOpenMode Update { get; } = new()
    {
        Name = "Update",
        Header = "Update",
        Buttons = new List<(string button, ManagerSubmitMode mode)>()
        {
            ("Update", ManagerSubmitMode.Update)
        }
    };

    public static ManagerOpenMode UpdateMix { get; } = new()
    {
        Name = "Update",
        Header = "Update",
        Buttons = new List<(string button, ManagerSubmitMode mode)>()
        {
            ("Update", ManagerSubmitMode.Update),
            ("Update with Base", ManagerSubmitMode.UpdateProjectAndBase)
        }
    };

    public static ManagerOpenMode Duplicate { get; } = new()
    {
        Name = "Duplicate",
        Header = "Duplicate",
        Buttons = new List<(string button, ManagerSubmitMode mode)>()
        {
            ("Duplicate", ManagerSubmitMode.Duplicate)
        }
    };
}