using System;
using System.Net.Cache;
using System.Text;

namespace HomeworkATM
{
    /// <summary>
    /// Клаас банковской карты
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Номер карты - 16 цифр, неизменяемо
        /// </summary>
        public string Number { get; } = "0000000000000000";
        /// <summary>
        /// Имя владельца - неизменяемо
        /// </summary>
        public string Name { get; } = "";
        /// <summary>
        /// Месяц и год окончания действия карты - неизменяемо
        /// </summary>
        public (int month, int year) Validity { get; }
        /// <summary>
        /// Банк-эмитент карты - неизменяемо
        /// </summary>
        public string Bank { get; } = "";
        /// <summary>
        /// Сумма денег на счету - переменная
        /// </summary>
        private int sum;
        /// <summary>
        /// Сумма денег на счету - изменяемо, есть проверка на корректность устанавливаемого значения
        /// </summary>
        public int Sum { get { return sum; } set { sum = (value > 0) ? value : sum; } }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Card(string number, string name, int month, int year, string bank, int sum)
        {
            Number = number;
            Name = name;
            Validity = (month, year);
            Bank = bank;
            Sum = sum;
        }
        /// <summary>
        /// Переопределение функции ToString
        /// </summary>
        public override string ToString() => $"Number: {Number}, Name: {Name}, Validity: {Validity.month}/{Validity.year}, Bank: {Bank}, Sum: {Sum}";
    }
    /// <summary>
    /// Класс банкомата
    /// </summary>
    public class ATM
    {
        private static int[] nominals = new int[] { 10, 50, 100, 200, 500, 1000, 2000, 5000 };
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        private static Random rnd = new Random();
        /// <summary>
        /// ID банкомата - генерируется случайным образом при создании объекта класса, неизменяемо
        /// </summary>
        public long ID { get; } = rnd.Next(1, 1000000);
        /// <summary>
        /// Банк, которому принадлежит банкомат - неизменяемо
        /// </summary>
        public string Bank { get; } = "";
        /// <summary>
        /// Кассета с деньгами - словарь с ключами - достоинством банкнот и значениями - количеством банкнот в банкомате. 
        /// </summary>
        private Dictionary<int, int> cassette = new Dictionary<int, int>();
        /// <summary>
        /// История транзакций - список строк формата [<карта>: <вид транзакции> (<сумма>) => <ответ банкомата>].
        /// </summary>
        private List<string> history = new List<string>();
        /// <summary>
        /// Секретный ключ для валидации инкассаторов - фиксированная строка 👻
        /// </summary>
        private string key = "ghost";
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ATM(string bank, Dictionary<int, int> getCassette)
        {
            Bank = bank;
            cassette = getCassette;
        }
        /// <summary>
        /// Переопределение функции ToString
        /// </summary>
        public override string ToString() => $"ID: {ID}, Bank: {Bank}";
        /// <summary>
        /// Возвращает общее количество денег в кассетах банкомата
        /// </summary>
        /// <returns>Целое цисло деняг</returns>
        public int CashAmount() => cassette.Sum(x => x.Key * x.Value);
        /// <summary>
        /// Пополнение карты
        /// </summary>
        /// <param name="card">Исходная карта</param>
        /// <param name="cash">Словарь вносимых купюр</param>
        /// <returns>Успех проведённой операции</returns>
        public bool Кeplenish(Card card, Dictionary<int, int> cash)
        {
            double tax = 0;
            int sum = 0;
            foreach (var banknote in cash)
            {
                if (!nominals.Contains(banknote.Key))
                {
                    history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Неверный наминал купюры: {banknote.Key}!");
                    Console.WriteLine($"Неверный наминал купюры: {banknote.Key}! Пополнение будет завершино");
                    return false;
                }
            }
            if (card.Validity.year < DateTime.Now.Year || card.Validity.year == DateTime.Now.Year && card.Validity.month < DateTime.Now.Month)
            {
                history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Истёк срок карты ({card.Validity.month}/{card.Validity.year})!");
                Console.WriteLine($"Истёк срок карты ({card.Validity.month}/{card.Validity.year})! Пополнение будет завершино");
                return false;
            }
            if (card.Bank == Bank)
            {
                Console.WriteLine("Пополнение пройдёт без комиссии");
            }
            else
            {
                tax = 0.05;
                Console.WriteLine("За пополнение будет взиматься комиссия 5%");
            }
            foreach (var banknote in cash)
            {
                sum += banknote.Key * banknote.Value;
                if (cassette.ContainsKey(banknote.Key)) cassette[banknote.Key] += banknote.Value;
                else cassette[banknote.Key] = banknote.Value;
            }
            card.Sum += (int)Math.Round(sum * (1 - tax));
            Console.WriteLine($"Карта успешно пополнена!");
            history.Add($"{DateTime.Now}, {card.Number}: Пополнение {sum} => Карта успешно пополнена! Налог: {tax}%");
            return true;
        }
        /// <summary>
        /// Снятие наличных с карты
        /// </summary>
        /// <param name="card">Исходная карта</param>
        /// <param name="sum">Сумма снятия</param>
        /// <returns>Успех проведённой операции</returns>
        public bool Withdraw(Card card, int sum)
        {
            double tax = 0;
            var temp = cassette;
            if (card.Validity.year < DateTime.Now.Year || card.Validity.year == DateTime.Now.Year && card.Validity.month < DateTime.Now.Month)
            {
                history.Add($"{DateTime.Now}, {card.Number}: Снятие 0 => Истёк срок карты ({card.Validity.month}/{card.Validity.year})!");
                Console.WriteLine($"Истёк срок карты ({card.Validity.month}/{card.Validity.year})! Снятие будет завершино");
                return false;
            }
            if (card.Sum < sum)
            {
                history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Недостаточно средств на карте для снятия такой суммы!");
                Console.WriteLine($"Недостаточно средств на карте для снятия такой суммы! Снятие будет завершино");
                return false;
            }
            if (card.Bank == Bank)
            {
                Console.WriteLine("Снятие пройдёт без комиссии");
            }
            else
            {
                tax = 0.05;
                sum = (int)Math.Round(sum * (1 - tax));
                Console.WriteLine("За cнятие будет взиматься комиссия 5%");
            }
            if (this.CashAmount() < sum)
            {
                history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Недостаточно средств в банкомате для снятия такой суммы!");
                Console.WriteLine("Недостаточно средств в банкомате для снятия такой суммы! Снятие будет завершино");
                return false;
            }
            while (sum > 0)
            {
                var banknote = 0;
                foreach (var x in cassette)
                {
                    if (x.Value != 0 && x.Key <= sum && x.Key > banknote)
                        banknote = x.Key;
                }
                if (banknote == 0)
                {
                    history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Терминал не может выдать такую сумму!");
                    Console.WriteLine("Терминал не может выдать такую сумму! Снятие будет завершино");
                    return false;
                }
                sum -= banknote;
                cassette[banknote] -= 1;
            }
            Console.WriteLine($"Снятие прошло успешно!");
            history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Снятие прошло успешно! Налог: {tax}%");
            return true;
        }
        public bool Pickup(string keyGet)
        {
            StringBuilder keyBuilder = new StringBuilder(keyGet);
            for (int i = 0; i < keyBuilder.Length; i++)
                keyBuilder[i] = (char)(((int)keyBuilder[i] - 4) > 97 ? (int)keyBuilder[i] - 4 : 22 + (int)keyBuilder[i]);
            if (keyBuilder.Equals(key))
            {
                Console.WriteLine($"Ключ верен");
                Console.WriteLine(string.Join("\n", history));
                history.Clear();
                Console.WriteLine($"История очищена");
                cassette.Clear();
                cassette.Add(100, 30);
                cassette.Add(200, 20);
                cassette.Add(500, 10);
                cassette.Add(1000, 5);
                cassette.Add(2000, 4);
                cassette.Add(5000, 2);
                Console.WriteLine($"Замена касеты прошла успешно");
                return true;
            }
            else
            {
                Console.WriteLine($"Неверный ключ!");
                history.Add($"{DateTime.Now}: Неверный ключ! Возможно попытка взлома");
                return false;
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var cassette = new Dictionary<int, int>();
            cassette.Add(100, 5);
            cassette.Add(500, 4);
            cassette.Add(1000, 10);
            cassette.Add(5000, 2);
            var atm = new ATM("Bank", cassette);
            Console.WriteLine(atm.CashAmount());
            var card = new Card("0000000000000000", "", 12, 2030, "Bank", 10000);
            atm.Withdraw(card, 5000);
            atm.Pickup("klswx");
        }
    }
}