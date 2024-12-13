using System;
using System.Collections.Generic;
using System.IO;

namespace exzamen_t2
{
    class Program
    {
        static void Main()
        {
            QuizApplication app = new QuizApplication();
            app.Run();
        }
    }

    public class QuizApplication
    {
        private UserManager userManager;
        private QuizManager quizManager;
        private ResultManager resultManager;
        private string currentUser;

        public QuizApplication()
        {
            userManager = new UserManager("users.txt");
            quizManager = new QuizManager("quizzes.txt");
            resultManager = new ResultManager("results.txt");
        }

        public void Run()
        {
            Console.WriteLine("Добро пожаловать в приложение 'Викторина'!");

            while (true)
            {
                if (currentUser == null)
                {
                    ShowMainMenu();
                }
                else
                {
                    ShowUserMenu();
                }
            }
        }

        private void ShowMainMenu()
        {
            Console.WriteLine("\n1. Войти");
            Console.WriteLine("2. Зарегистрироваться");
            Console.WriteLine("3. Выход");
            Console.Write("Выберите опцию: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    currentUser = userManager.Login();
                    break;
                case "2":
                    userManager.Register();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }

        private void ShowUserMenu()
        {
            Console.WriteLine($"\nПривет, {currentUser}!");
            Console.WriteLine("1. Начать викторину");
            Console.WriteLine("2. Посмотреть результаты");
            Console.WriteLine("3. Изменить настройки");
            Console.WriteLine("4. Выйти");
            Console.Write("Выберите опцию: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    quizManager.StartQuiz(currentUser, resultManager);
                    break;
                case "2":
                    resultManager.ViewResults(currentUser);
                    break;
                case "3":
                    userManager.UpdateSettings(currentUser);
                    break;
                case "4":
                    currentUser = null;
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }
    }

    public class UserManager
    {
        private Dictionary<string, (string Password, string DateOfBirth)> users;
        private string filePath;

        public UserManager(string filePath)
        {
            this.filePath = filePath;
            users = LoadUsers();
        }

        public string Login()
        {
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            if (users.ContainsKey(login) && users[login].Password == password)
            {
                Console.WriteLine("Успешный вход!");
                return login;
            }

            Console.WriteLine("Неверный логин или пароль.");
            return null;
        }

        public void Register()
        {
            Console.Write("Введите новый логин: ");
            string login = Console.ReadLine();

            if (users.ContainsKey(login))
            {
                Console.WriteLine("Этот логин уже занят.");
                return;
            }

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();
            Console.Write("Введите дату рождения (дд.мм.гггг): ");
            string dob = Console.ReadLine();

            users.Add(login, (password, dob));
            SaveUsers();
            Console.WriteLine("Регистрация прошла успешно!");
        }

        public void UpdateSettings(string currentUser)
        {
            Console.Write("Введите новый пароль: ");
            string newPassword = Console.ReadLine();
            Console.Write("Введите новую дату рождения (дд.мм.гггг): ");
            string newDob = Console.ReadLine();

            if (users.ContainsKey(currentUser))
            {
                users[currentUser] = (newPassword, newDob);
                SaveUsers();
                Console.WriteLine("Настройки успешно обновлены!");
            }
        }

        private Dictionary<string, (string, string)> LoadUsers()
        {
            var users = new Dictionary<string, (string, string)>();
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        users[parts[0]] = (parts[1], parts[2]);
                    }
                }
            }
            return users;
        }

        private void SaveUsers()
        {
            var lines = new List<string>();
            foreach (var user in users)
            {
                lines.Add($"{user.Key}|{user.Value.Password}|{user.Value.DateOfBirth}");
            }
            File.WriteAllLines(filePath, lines);
        }
    }

    public class QuizManager
    {
        private Dictionary<string, List<Question>> quizzes;
        private string filePath;

        public QuizManager(string filePath)
        {
            this.filePath = filePath;
            quizzes = LoadQuizzes();
        }

        public void StartQuiz(string currentUser, ResultManager resultManager)
        {
            Console.WriteLine("\nДоступные темы:");
            foreach (var quiz in quizzes.Keys)
            {
                Console.WriteLine($"- {quiz}");
            }

            Console.Write("Введите тему викторины или 'Смешанная': ");
            string topic = Console.ReadLine();

            if (!quizzes.ContainsKey(topic) && topic != "Смешанная")
            {
                Console.WriteLine("Тема не найдена.");
                return;
            }

            List<Question> questions = (topic == "Смешанная") ? GetMixedQuestions() : quizzes[topic];

            int correctAnswers = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                Console.WriteLine($"\n{i + 1}. {questions[i].Text}");
                for (int j = 0; j < questions[i].Options.Count; j++)
                {
                    Console.WriteLine($"{j + 1}. {questions[i].Options[j]}");
                }

                Console.Write("Ваш ответ (через запятую, если несколько): ");
                var input = Console.ReadLine();
                var answers = ParseAnswers(input);

                if (answers.SetEquals(questions[i].CorrectOptions))
                {
                    correctAnswers++;
                }
            }

            Console.WriteLine($"\nВы правильно ответили на {correctAnswers} из {questions.Count} вопросов.");
            resultManager.SaveResult(new Result { User = currentUser, Topic = topic, Score = correctAnswers });
        }

        private List<Question> GetMixedQuestions()
        {
            var mixedQuestions = new List<Question>();
            foreach (var quiz in quizzes.Values)
            {
                mixedQuestions.AddRange(quiz);
            }

            var rnd = new Random();
            for (int i = mixedQuestions.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var temp = mixedQuestions[i];
                mixedQuestions[i] = mixedQuestions[j];
                mixedQuestions[j] = temp;
            }

            return mixedQuestions.GetRange(0, Math.Min(20, mixedQuestions.Count));
        }

        private HashSet<int> ParseAnswers(string input)
        {
            var answers = new HashSet<int>();
            foreach (var part in input.Split(','))
            {
                if (int.TryParse(part.Trim(), out int result))
                {
                    answers.Add(result - 1);
                }
            }
            return answers;
        }

        private Dictionary<string, List<Question>> LoadQuizzes()
        {
            var quizzes = new Dictionary<string, List<Question>>();
            if (!File.Exists(filePath)) return quizzes;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    var questions = new List<Question>();
                    foreach (var questionData in parts[1].Split(';'))
                    {
                        var qParts = questionData.Split('~');
                        questions.Add(new Question
                        {
                            Text = qParts[0],
                            Options = new List<string>(qParts[1].Split(',')),
                            CorrectOptions = new HashSet<int>(Array.ConvertAll(qParts[2].Split(','), int.Parse))
                        });
                    }
                    quizzes[parts[0]] = questions;
                }
            }
            return quizzes;
        }
    }

    public class ResultManager
    {
        private List<Result> results;
        private string filePath;

        public ResultManager(string filePath)
        {
            this.filePath = filePath;
            results = LoadResults();
        }

        public void SaveResult(Result result)
        {
            results.Add(result);
            SaveResults();
        }

        public void ViewResults(string currentUser)
        {
            Console.WriteLine("\nВаши результаты:");
            foreach (var result in results)
            {
                if (result.User == currentUser)
                {
                    Console.WriteLine($"Тема: {result.Topic}, Баллы: {result.Score}");
                }
            }
        }

        private List<Result> LoadResults()
        {
            var results = new List<Result>();
            if (!File.Exists(filePath)) return results;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                results.Add(new Result { User = parts[0], Topic = parts[1], Score = int.Parse(parts[2]) });
            }
            return results;
        }

        private void SaveResults()
        {
            var lines = new List<string>();
            foreach (var result in results)
            {
                lines.Add($"{result.User}|{result.Topic}|{result.Score}");
            }
            File.WriteAllLines(filePath, lines);
        }
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public HashSet<int> CorrectOptions { get; set; }
    }

    public class Result
    {
        public string User { get; set; }
        public string Topic { get; set; }
        public int Score { get; set; }
    }
}
