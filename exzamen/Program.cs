namespace exzamen
{
    class DictionaryApp
    {
        private Dictionary<string, List<string>> dictionary;
        private string filePath;

        public DictionaryApp(string filePath)
        {
            this.filePath = filePath;
            dictionary = LoadDictionaryFromFile(filePath);
        }

        public void AddWord(string word, List<string> translations)
        {
            if (dictionary.ContainsKey(word))
            {
                dictionary[word].AddRange(translations);
            }
            else
            {
                dictionary[word] = new List<string>(translations);
            }
            SaveDictionaryToFile();
        }

        public void RemoveWord(string word)
        {
            if (dictionary.ContainsKey(word))
            {
                dictionary.Remove(word);
                SaveDictionaryToFile();
            }
            else
            {
                Console.WriteLine("Слово не найдено!");
            }
        }

        public void ReplaceTranslation(string word, string oldTranslation, string newTranslation)
        {
            if (dictionary.ContainsKey(word))
            {
                var translations = dictionary[word];
                if (translations.Contains(oldTranslation))
                {
                    translations[translations.IndexOf(oldTranslation)] = newTranslation;
                    SaveDictionaryToFile();
                }
                else
                {
                    Console.WriteLine("Перевод не найден!");
                }
            }
            else
            {
                Console.WriteLine("Слово не найдено!");
            }
        }

        public void FindWord(string word)
        {
            if (dictionary.ContainsKey(word))
            {
                Console.WriteLine($"Переводы для '{word}': {string.Join(", ", dictionary[word])}");
            }
            else
            {
                Console.WriteLine("Слово не найдено!");
            }
        }

        private void SaveDictionaryToFile()
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var entry in dictionary)
                {
                    string line = $"{entry.Key}:{string.Join(",", entry.Value)}";
                    writer.WriteLine(line);
                }
            }
        }

        private Dictionary<string, List<string>> LoadDictionaryFromFile(string path)
        {
            var dict = new Dictionary<string, List<string>>();

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        string word = parts[0];
                        var translations = parts[1].Split(',');
                        dict[word] = new List<string>(translations);
                    }
                }
            }
            return dict;
        }
    }

    class Program
    {
        static void Main()
        {
            string filePath = "dictionary.txt";
            DictionaryApp app = new DictionaryApp(filePath);

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Добавить слово");
                Console.WriteLine("2. Удалить слово");
                Console.WriteLine("3. Заменить перевод");
                Console.WriteLine("4. Найти слово");
                Console.WriteLine("5. Выйти");
                Console.Write("Выберите опцию: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Введите слово: ");
                        string word = Console.ReadLine();
                        Console.Write("Введите переводы (через запятую): ");
                        var translations = Console.ReadLine().Split(',');
                        app.AddWord(word, new List<string>(translations));
                        break;

                    case "2":
                        Console.Write("Введите слово для удаления: ");
                        string wordToRemove = Console.ReadLine();
                        app.RemoveWord(wordToRemove);
                        break;

                    case "3":
                        Console.Write("Введите слово: ");
                        string wordToReplace = Console.ReadLine();
                        Console.Write("Введите старый перевод: ");
                        string oldTranslation = Console.ReadLine();
                        Console.Write("Введите новый перевод: ");
                        string newTranslation = Console.ReadLine();
                        app.ReplaceTranslation(wordToReplace, oldTranslation, newTranslation);
                        break;

                    case "4":
                        Console.Write("Введите слово для поиска: ");
                        string wordToFind = Console.ReadLine();
                        app.FindWord(wordToFind);
                        break;

                    case "5":
                        return;

                    default:
                        Console.WriteLine("Неверный выбор, попробуйте снова.");
                        break;
                }
            }
        }
    }

}
