using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Observer pattern
public interface IObserver
{
    void Update(string studentName, int grade);
}

public interface ISubject
{
    void RegisterObserver(IObserver observer);
    void RemoveObserver(IObserver observer);
    void NotifyObservers(string studentName, int grade);
}

// Concrete Observer
public class ConsoleGradeObserver : IObserver
{
    public void Update(string studentName, int grade)
    {
        Console.WriteLine($"Оценка добавлена: Студент {studentName}, Оценка: {grade}");
    }
}

// Concrete Subject
public class GradeTracker : ISubject
{
    private List<IObserver> observers;

    public GradeTracker()
    {
        observers = new List<IObserver>();
    }

    public void RegisterObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(string studentName, int grade)
    {
        foreach (var observer in observers)
        {
            observer.Update(studentName, grade);
        }
    }
}

// Strategy pattern
public interface IReportStrategy
{
    void GenerateReport(List<Student> students);
}

// Concrete Strategies
public class TextReportStrategy : IReportStrategy
{
    public void GenerateReport(List<Student> students)
    {
        Console.WriteLine("=== Текстовый отчет ===");
        foreach (var student in students)
        {
            double averageGrade = student.Grades.Average();
            Console.WriteLine($"Студент: {student.FirstName} {student.LastName}, Средний балл: {averageGrade}");
        }
    }
}

public class ChartReportStrategy : IReportStrategy
{
    public void GenerateReport(List<Student> students)
    {
        Console.WriteLine("=== Отчет в виде графика ===");
        foreach (var student in students)
        {
            double averageGrade = student.Grades.Average();
            string chart = new string('*', (int)averageGrade);
            Console.WriteLine($"Студент: {student.FirstName} {student.LastName}, График: {chart}");
        }
    }
}

// Visitor pattern
public interface IVisitor
{
    void Visit(Student student);
}

// Concrete Visitor
public class CalculationVisitor : IVisitor
{
    public void Visit(Student student)
    {
        double averageGrade = student.Grades.Average();
        Console.WriteLine($"Студент: {student.FirstName} {student.LastName}, Средний балл: {averageGrade}");
    }
}

// Composite pattern
public abstract class Component
{
    protected string name;

    public Component(string name)
    {
        this.name = name;
    }

    public abstract void Add(Component component);
    public abstract void Remove(Component component);
    public abstract void Display(int depth);
}

public class Composite : Component
{
    private List<Component> children;

    public Composite(string name) : base(name)
    {
        children = new List<Component>();
    }

    public override void Add(Component component)
    {
        children.Add(component);
    }

    public override void Remove(Component component)
    {
        children.Remove(component);
    }

    public override void Display(int depth)
    {
        Console.WriteLine(new string('-', depth) + name);

        foreach (var component in children)
        {
            component.Display(depth + 2);
        }
    }
}

public class Leaf : Component
{
    public Leaf(string name) : base(name)
    {
    }

    public override void Add(Component component)
    {
        throw new InvalidOperationException("Cannot add to a leaf");
    }

    public override void Remove(Component component)
    {
        throw new InvalidOperationException("Cannot remove from a leaf");
    }

    public override void Display(int depth)
    {
        Console.WriteLine(new string('-', depth) + name);
    }
}

// Models
public class Student
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<int> Grades { get; }

    public Student(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Grades = new List<int>();
    }
}

// Controllers
public class Controller
{
    private List<Student> students;
    private GradeTracker gradeTracker;

    public Controller()
    {
        students = new List<Student>();
        gradeTracker = new GradeTracker();
    }

    public void RegisterObserver(IObserver observer)
    {
        gradeTracker.RegisterObserver(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        gradeTracker.RemoveObserver(observer);
    }

    public void AddStudent(string firstName, string lastName)
    {
        var student = new Student(firstName, lastName);
        students.Add(student);
    }

    public void AddGrade(string firstName, string lastName, int grade)
    {
        var student = FindStudent(firstName, lastName);
        if (student != null)
        {
            student.Grades.Add(grade);
            gradeTracker.NotifyObservers($"{student.FirstName} {student.LastName}", grade);
        }
        else
        {
            throw new ArgumentException("Студент не найден.");
        }
    }

    public void GenerateReport(IReportStrategy reportStrategy)
    {
        reportStrategy.GenerateReport(students);
    }

    public void VisitStudents(IVisitor visitor)
    {
        foreach (var student in students)
        {
            visitor.Visit(student);
        }
    }

    public List<Student> GetStudentsWithHighestGrade()
    {
        var maxGrade = students.Max(s => s.Grades.Max());
        return students.Where(s => s.Grades.Contains(maxGrade)).ToList();
    }

    public void SaveData(string fileName)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach (var student in students)
            {
                writer.WriteLine($"{student.FirstName},{student.LastName},{string.Join(",", student.Grades)}");
            }
        }
    }

    public void LoadData(string fileName)
    {
        students.Clear();
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string firstName = parts[0];
                    string lastName = parts[1];
                    var student = new Student(firstName, lastName);
                    for (int i = 2; i < parts.Length; i++)
                    {
                        if (int.TryParse(parts[i], out int grade))
                        {
                            student.Grades.Add(grade);
                        }
                    }
                    students.Add(student);
                }
            }
        }
    }

    private Student FindStudent(string firstName, string lastName)
    {
        return students.FirstOrDefault(s => s.FirstName == firstName && s.LastName == lastName);
    }
}

class Program
{
    static void Main(string[] args)
    {
        var controller = new Controller();
        var consoleObserver = new ConsoleGradeObserver();
        controller.RegisterObserver(consoleObserver);

        while (true)
        {
            Console.WriteLine("==== Система оценивания студентов ====");
            Console.WriteLine("1. Добавить студента");
            Console.WriteLine("2. Добавить оценку студенту");
            Console.WriteLine("3. Сгенерировать отчет");
            Console.WriteLine("4. Выполнить вычисления");
            Console.WriteLine("5. Сохранить данные в файл");
            Console.WriteLine("6. Загрузить данные из файла");
            Console.WriteLine("7. Выйти из программы");
            Console.Write("Введите номер операции: ");
            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.Write("Введите имя студента: ");
                    string firstName = Console.ReadLine();
                    Console.Write("Введите фамилию студента: ");
                    string lastName = Console.ReadLine();
                    controller.AddStudent(firstName, lastName);
                    Console.WriteLine("Студент добавлен успешно.");
                    break;
                case "2":
                    Console.Write("Введите имя студента: ");
                    string studentFirstName = Console.ReadLine();
                    Console.Write("Введите фамилию студента: ");
                    string studentLastName = Console.ReadLine();
                    Console.Write("Введите оценку: ");
                    int grade = int.Parse(Console.ReadLine());
                    try
                    {
                        controller.AddGrade(studentFirstName, studentLastName, grade);
                        Console.WriteLine("Оценка добавлена успешно.");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "3":
                    Console.WriteLine("Выберите тип отчета:");
                    Console.WriteLine("1. Текстовый отчет");
                    Console.WriteLine("2. Отчет в виде графика");
                    Console.Write("Введите номер типа отчета: ");
                    string reportType = Console.ReadLine();
                    switch (reportType)
                    {
                        case "1":
                            controller.GenerateReport(new TextReportStrategy());
                            break;
                        case "2":
                            controller.GenerateReport(new ChartReportStrategy());
                            break;
                        default:
                            Console.WriteLine("Неверный номер типа отчета.");
                            break;
                    }
                    break;
                case "4":
                    Console.WriteLine("Выберите тип вычислений:");
                    Console.WriteLine("1. Средний балл");
                    Console.WriteLine("2. Высший балл");
                    Console.Write("Введите номер типа вычислений: ");
                    string calculationType = Console.ReadLine();
                    switch (calculationType)
                    {
                        case "1":
                            controller.VisitStudents(new CalculationVisitor());
                            break;
                        case "2":
                            var composite = new Composite("Студенты с высшим баллом");
                            foreach (var student in controller.GetStudentsWithHighestGrade())
                            {
                                composite.Add(new Leaf($"{student.FirstName} {student.LastName}"));
                            }
                            Console.WriteLine("Студенты с высшим баллом:");
                            composite.Display(1);
                            break;
                        default:
                            Console.WriteLine("Неверный номер типа вычислений.");
                            break;
                    }
                    break;
                case "5":
                    Console.Write("Введите имя файла для сохранения данных: ");
                    string fileName = Console.ReadLine();
                    try
                    {
                        controller.SaveData(fileName);
                        Console.WriteLine("Данные сохранены успешно.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                    }
                    break;
                case "6":
                    Console.Write("Введите имя файла для загрузки данных: ");
                    fileName = Console.ReadLine();
                    try
                    {
                        controller.LoadData(fileName);
                        Console.WriteLine("Данные загружены успешно.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                    }
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Неверный номер операции.");
                    break;
            }

            Console.WriteLine();
        }
    }
}
