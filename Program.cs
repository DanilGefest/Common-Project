using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Observer 
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

public class ConsoleGradeObserver : IObserver
{
    public void Update(string studentName, int grade)
    {
        Console.WriteLine($"Оценка добавлена: Студент {studentName}, Оценка: {grade}");
    }
}

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


// Strategy 
public interface IReportStrategy
{
    void GenerateReport(List<Student> students);
}

public class AllGradesReportStrategy : IReportStrategy
{
    public void GenerateReport(List<Student> students)
    {
        Console.WriteLine("=== Отчет: Все оценки ===");
        foreach (var student in students)
        {
            Console.WriteLine($"Студент: {student.FirstName} {student.LastName}");
            Console.WriteLine("Оценки:");
            foreach (var grade in student.Grades)
            {
                Console.Write(grade + " ");
            }
            Console.WriteLine();
        }
    }
}

public class AverageGradeReportStrategy : IReportStrategy
{
    public void GenerateReport(List<Student> students)
    {
        Console.WriteLine("=== Отчет: Средний балл ===");
        foreach (var student in students)
        {
            double averageGrade = student.Grades.Average();
            Console.WriteLine($"Студент: {student.FirstName} {student.LastName}, Средний балл: " + string.Format("{0:F2}", averageGrade));
        }
    }
}

// Composite 
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
        throw new InvalidOperationException("Невозможно добавить к листу");
    }

    public override void Remove(Component component)
    {
        throw new InvalidOperationException("Невозможно удалить с листа");
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
            Console.WriteLine("4. Сохранить данные в файл");
            Console.WriteLine("5. Загрузить данные из файла");
            Console.WriteLine("6. Выйти из программы");
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
                    Console.WriteLine("Студент успешно добавлен.");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "2":
                    Console.Write("Введите имя студента: ");
                    string studentFirstName = Console.ReadLine();
                    Console.Write("Введите фамилию студента: ");
                    string studentLastName = Console.ReadLine();
                    Console.Write("Введите оценку: ");
                    int grade = int.Parse(Console.ReadLine());
                    controller.AddGrade(studentFirstName, studentLastName, grade);
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "3":
                    Console.WriteLine("Выберите тип отчета:");
                    Console.WriteLine("1. Все оценки");
                    Console.WriteLine("2. Средний балл");
                    string reportType = Console.ReadLine();
                    IReportStrategy reportStrategy;
                    if (reportType == "1")
                    {
                        reportStrategy = new AllGradesReportStrategy();
                    }
                    else if (reportType == "2")
                    {
                        reportStrategy = new AverageGradeReportStrategy();
                    }
                    else
                    {
                        Console.WriteLine("Неправильный выбор.");
                        continue;
                    }
                    controller.GenerateReport(reportStrategy);
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "4":
                    Console.Write("Введите имя файла для сохранения данных: ");
                    string saveFileName = Console.ReadLine();
                    controller.SaveData(saveFileName);
                    Console.WriteLine("Данные успешно сохранены.");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "5":
                    Console.Write("Введите имя файла для загрузки данных: ");
                    string loadFileName = Console.ReadLine();
                    controller.LoadData(loadFileName);
                    Console.WriteLine("Данные успешно загружены.");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Неправильный выбор.");
                    Console.ReadKey();
                    Console.Clear();
                    break;
            }
            Console.WriteLine();
        }
    }
}
