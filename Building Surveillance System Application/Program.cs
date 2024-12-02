
Console.Clear();

SecuritySurveillanceHub securityHub = new SecuritySurveillanceHub();

EmployeeNotify employeeNotify = new EmployeeNotify(new Employee
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    JobTitle = "Software Engineer",
    
});

EmployeeNotify employeeNotify2 = new EmployeeNotify(new Employee
{
    Id = 2,
    FirstName = "Davy",
    LastName = "Jones",
    JobTitle = "Civil Engineer",
    
});

SecurityNotify securityNotify = new SecurityNotify();
employeeNotify.Subscribe(securityHub);
employeeNotify2.Subscribe(securityHub);
securityNotify.Subscribe(securityHub);

securityHub.ConfirmExternalVisitorEntersBuilding(1, "Anne", "Tate", "Messenger", "Jitu", DateTime.Parse("2 December 2024 11:00"), 1);
securityHub.ConfirmExternalVisitorEntersBuilding(2, "Donald", "Trump", "President", "USA", DateTime.Parse("2 December 2024 11:00"), 2);

securityHub.ConfirmExternalVisitorExitsBuilding(1,  DateTime.Parse("2 December 2024 14:00"));
// securityHub.ConfirmExternalVisitorExitsBuilding(2,  DateTime.Parse("2 December 2024 15:00"));

securityHub.BuildingEntryCutOffTimeReached();

public interface IEmployee
{
    int Id { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string JobTitle { get; set; }
}

public class Employee : IEmployee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string JobTitle { get; set; }
}

public abstract class Observer : IObserver<ExternalVisitor>
{
    private IDisposable _cancellation;
    protected List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();

    public abstract void OnCompleted();
    public abstract void OnError(Exception error);
    public abstract void OnNext(ExternalVisitor value);

    public void Subscribe(IObservable<ExternalVisitor> provider)
    {
        _cancellation = provider.Subscribe(this);
    }

    public void Unsubscribe()
    {
        _cancellation.Dispose();
        _externalVisitors.Clear();
    }
}

public class EmployeeNotify : Observer
{
    IEmployee _employee = null;


    public EmployeeNotify(IEmployee employee)
    {
        _employee = employee;
    }

    public override void OnCompleted()
    {
        string heading = $"{_employee.FirstName} {_employee.LastName} Daily Visitors Report.";
        Console.WriteLine();
        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        Console.WriteLine();

        foreach (var externalVisitor in _externalVisitors)
        {
            externalVisitor.InBuilding = false;

            Console.WriteLine(
                $"{externalVisitor.Id,-6} {externalVisitor.FirstName,-10} {externalVisitor.LastName,-15} {externalVisitor.EntryDateTime.ToString("yyyy MMMM dd hh:mm:ss"),-25} {externalVisitor.ExitDateTime.ToString("yyyy MMMM dd hh:mm:ss tt"),-25}");
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    public override void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(ExternalVisitor value)
    {
        if (value.EmployeeContactId == _employee.Id)
        {
            var externalVisitorsListItem = _externalVisitors.FirstOrDefault(x => x.Id == value.Id);

            if (externalVisitorsListItem == null)
            {
                _externalVisitors.Add(value);
                OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Employee);
                Console.WriteLine(
                    $"{_employee.FirstName} {_employee.LastName}, your visitor has arrived. Visitor Id: {value.Id}, Last Name: {value.LastName}, Job Title: {value.JobTitle}, entered the building, Datetime: {value.EntryDateTime.ToString("dd MMMM yyyy HH:mm:ss tt")}");
                OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Normal);
                Console.WriteLine();
            }
            else
            {
                externalVisitorsListItem.InBuilding = value.InBuilding;
                externalVisitorsListItem.ExitDateTime = value.ExitDateTime;
            }
        }
    }
}

public class SecurityNotify : Observer
{
    List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();
    private IDisposable _cancellation;

    public override void OnCompleted()
    {
        string heading = $"Security Daily Visitors Report.";
        Console.WriteLine();
        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        Console.WriteLine();

        foreach (var externalVisitor in _externalVisitors)
        {
            externalVisitor.InBuilding = false;

            Console.WriteLine(
                $"{externalVisitor.Id,-6} {externalVisitor.FirstName,-10} {externalVisitor.LastName,-15} {externalVisitor.EntryDateTime.ToString("yyyy MMMM dd hh:mm:ss tt"),-25} {externalVisitor.ExitDateTime.ToString("yyyy MMMM dd hh:mm:ss tt"),-25}");
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    public override void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(ExternalVisitor value)
    {
        var externalVisitor = value;
        var externalVisitorsListItem = _externalVisitors.FirstOrDefault(x => x.Id == externalVisitor.Id);

        if (externalVisitorsListItem == null)
        {
            _externalVisitors.Add(externalVisitor);
            OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Security);
            Console.WriteLine(
                $"Security Notification. Visitor Id: {externalVisitor.Id}, Last Name: {externalVisitor.LastName}, Job Title: {externalVisitor.JobTitle}, entered the building, Datetime: {externalVisitor.EntryDateTime.ToString("dd MMMM yyyy HH:mm:ss tt")}");
            OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Normal);
            Console.WriteLine();
        }
        else
        {
            if (externalVisitorsListItem.InBuilding == false)
            {
                externalVisitorsListItem.InBuilding = false;
                externalVisitorsListItem.ExitDateTime = externalVisitor.ExitDateTime;

                Console.WriteLine(
                    $"Security Notification. Visitor Id: {externalVisitor.Id}, Last Name: {externalVisitor.LastName}, Job Title: {externalVisitor.JobTitle}, exited the building, Datetime: {externalVisitor.ExitDateTime.ToString("dd MMMM yyyy HH:mm:ss tt")}");
                Console.WriteLine();
            }
        }
    }
}

public class UnSubscriber<ExternalVisitor> : IDisposable
{
    private List<IObserver<ExternalVisitor>> _observers;
    private IObserver<ExternalVisitor> _observer;


    public UnSubscriber(List<IObserver<ExternalVisitor>> observers, IObserver<ExternalVisitor> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose()
    {
        if (_observers.Contains(_observer))
        {
            _observers.Remove(_observer);
        }
    }
}

public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
{
    private List<ExternalVisitor> _externalVisitors;
    private List<IObserver<ExternalVisitor>> _observers;

    public SecuritySurveillanceHub()
    {
        _externalVisitors = new List<ExternalVisitor>();
        _observers = new List<IObserver<ExternalVisitor>>();
    }


    public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }

        foreach (var externalVisitor in _externalVisitors)
        {
            observer.OnNext(externalVisitor);
        }

        return new UnSubscriber<ExternalVisitor>(_observers, observer);
    }

    public void ConfirmExternalVisitorEntersBuilding(int Id, string FirstName, string LastName, string JobTitle,
        string CompamyName, DateTime EntryDateTime, int EmployeeContactId)
    {
        ExternalVisitor externalVisitor = new ExternalVisitor
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            JobTitle = JobTitle,
            CompanyName = CompamyName,
            EntryDateTime = EntryDateTime,
            EmployeeContactId = EmployeeContactId,
            InBuilding = true
        };
        _externalVisitors.Add(externalVisitor);

        foreach (var observer in _observers)
        {
            observer.OnNext(externalVisitor);
        }
    }

    public void ConfirmExternalVisitorExitsBuilding(int externalVisitorId, DateTime ExitDateTime)
    {
        ExternalVisitor externalVisitor = _externalVisitors.FirstOrDefault(x => x.Id == externalVisitorId);

        if (externalVisitor != null)
        {
            externalVisitor.ExitDateTime = ExitDateTime;
            externalVisitor.InBuilding = false;

            foreach (var observer in _observers)
            {
                observer.OnNext(externalVisitor);
            }
        }
    }

    public void BuildingEntryCutOffTimeReached()
    {
        if (_externalVisitors.Where(e => e.InBuilding == true).ToList().Count() == 0)
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}

public static class OutputFormatter
{
    public enum TextOutputTheme
    {
        Security,
        Employee,
        Normal
    }

    public static void ChangeOutputTheme(TextOutputTheme textOutputTheme)
    {
        if (textOutputTheme == TextOutputTheme.Security)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (textOutputTheme == TextOutputTheme.Employee)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ResetColor();
        }
    }
}

public class ExternalVisitor
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string JobTitle { get; set; }
    public string CompanyName { get; set; }
    public DateTime EntryDateTime { get; set; }
    public DateTime ExitDateTime { get; set; }
    public int EmployeeContactId { get; set; }
    public bool InBuilding { get; set; }
}