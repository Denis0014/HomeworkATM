using System;
using System.Net.Cache;
using System.Reflection.Emit;
using System.Text;

namespace HomeworkATM
{
    #region Banknote
    /// <summary>
    /// Класс банкноты
    /// </summary>
    public class Banknote
    {
        private static Random rnd = new Random();
        /// <summary>
        /// Номинал банкноты - неизменяемое свойство
        /// </summary>
        public int Nominal { get; }
        /// <summary>
        /// Серия банкноты - состоит из 2 букв и 9 цифр, неизменяемое свойство
        /// </summary>
        public string Series { get; }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Banknote(int nominal, string series)
        {
            Nominal = nominal;
            Series = series;
        }
        /// <summary>
        /// Переопределение функции ToString
        /// </summary>
        public override string ToString() => $"Nominal: {Nominal}, Series: {Series}";
    }
    #endregion
    #region Card
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
    #endregion
    #region ATM
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
        private Dictionary<int, Stack<Banknote>> cassette = new Dictionary<int, Stack<Banknote>>();
        /// <summary>
        /// История транзакций - список строк формата [<карта>: <вид транзакции> (<сумма>) => <ответ банкомата>].
        /// </summary>
        private List<string> history = new List<string>();
        /// <summary>
        /// Сюда будут записываться всё серии упюр, которые держит в себе автомат
        /// </summary>
        private HashSet<string> magazine = new HashSet<string>();
        /// <summary>
        /// Секретный ключ для валидации инкассаторов - фиксированная строка
        /// </summary>
        private string key = "74306199653196";
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ATM(string bank, Dictionary<int, Stack<Banknote>> getCassette)
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
        public int CashAmount() => cassette.Sum(x => x.Key * x.Value.Count);

        #region Кeplenish
        /// <summary>
        /// Пополнение карты
        /// </summary>
        /// <param name="card">Исходная карта</param>
        /// <param name="cash">Словарь вносимых купюр</param>
        /// <returns>Успех проведённой операции</returns>
        public bool Кeplenish(Card card, Dictionary<int, Stack<Banknote>> cash)
        {
            double tax = 0;
            int sum = 0;
            var magazineTemp = magazine;
            foreach (var banknotes in cash)
            {
                if (!nominals.Contains(banknotes.Key))
                {
                    history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Неверный наминал купюры: {banknotes.Key}!");
                    Console.WriteLine($"Неверный наминал купюры: {banknotes.Key}! Пополнение будет завершино");
                    return false;
                }
                foreach (var banknote in banknotes.Value)
                {
                    if (magazine.Contains(banknote.Series))
                    {
                        history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Обнаружена купюры с одинаковыми сериями: {banknote.Series}!");
                        Console.WriteLine($"Неверная серия купюры! Пополнение будет завершино");
                        return false;
                    }
                    var dist = (Math.Abs(char.ToLower(banknote.Series[0]) - char.ToLower(banknote.Series[1])) - 1);
                    if (banknotes.Key >= 1000)
                    {
                        if (dist % 2 != 0 && dist != -1 || banknote.Series.Skip(2).Sum(x => int.Parse(x + "")) % 2 == 0)
                        {
                            history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Неверная серия купюры: {banknote.Series}!");
                            Console.WriteLine($"Неверная серия купюры! Пополнение будет завершино");
                            return false;
                        }
                    }
                    else
                    {
                        if (dist % 2 == 0 || dist == -1 || banknote.Series.Skip(2).Sum(x => int.Parse(x + "")) % 2 != 0)
                        {
                            history.Add($"{DateTime.Now}, {card.Number}: Пополнение 0 => Неверная серия купюры: {banknote.Series}!");
                            Console.WriteLine($"Неверная серия купюры! Пополнение будет завершино");
                            return false;
                        }
                    }
                    magazineTemp.Add(banknote.Series);
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
            foreach (var banknotes in cash)
            {
                foreach (var banknote in banknotes.Value)
                {
                    sum += banknotes.Key;
                    cassette[banknotes.Key].Push(banknote);
                }
            }
            card.Sum += (int)Math.Round(sum * (1 - tax));
            magazine = magazineTemp;
            Console.WriteLine($"Карта успешно пополнена!");
            history.Add($"{DateTime.Now}, {card.Number}: Пополнение {sum} => Карта успешно пополнена! Налог: {tax}%");
            return true;
        }
        #endregion
        #region Withdraw
        /// <summary>
        /// Снятие наличных с карты
        /// </summary>
        /// <param name="card">Исходная карта</param>
        /// <param name="sum">Сумма снятия</param>
        /// <returns>Успех проведённой операции</returns>
        public bool Withdraw(Card card, int sum)
        {
            double tax = 0;
            var sumTemp = sum;
            var cassetteTemp = cassette;
            var magazineTemp = magazine;
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
                var banknoteNominal = 0;
                foreach (var banknotes in cassette)
                {
                    if (banknotes.Value.Count != 0 && banknotes.Key <= sum && banknotes.Key > banknoteNominal)
                        banknoteNominal = banknotes.Key;
                }
                if (banknoteNominal == 0)
                {
                    history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Терминал не может выдать такую сумму!");
                    Console.WriteLine("Терминал не может выдать такую сумму! Снятие будет завершино");
                    return false;
                }
                sumTemp -= banknoteNominal;
                magazineTemp.Remove(cassetteTemp[banknoteNominal].Peek().Series);
                cassetteTemp[banknoteNominal].Pop();
            }
            sum = sumTemp;
            cassette = cassetteTemp;
            magazine = magazineTemp;
            Console.WriteLine($"Снятие прошло успешно!");
            history.Add($"{DateTime.Now}, {card.Number}: Снятие {sum} => Снятие прошло успешно! Налог: {tax}%");
            return true;
        }
        #endregion
        #region Pickup
        /// <summary>
        /// Обслуживание автомата
        /// </summary>
        /// <param name="keyGet">Секретный ключ</param>
        /// <returns>Успех проведённой операции</returns>
        public bool Pickup(string keyGet)
        {
            string keyBuilder = "";
            for (int i = 0; i < keyGet.Length - 2; i++)
                keyBuilder += (int.Parse(keyGet[i] + "") + int.Parse(keyGet[i + 1] + "") + int.Parse(keyGet[i + 2] + "")) % 10;
            if (keyBuilder.Equals(key))
            {
                Console.WriteLine($"Ключ верен");
                Console.WriteLine(string.Join("\n", history));
                history.Clear();
                Console.WriteLine($"История очищена");
                cassette.Clear();
                magazine.Clear();
                cassette.Add(100, new Stack<Banknote>());
                cassette.Add(200, new Stack<Banknote>());
                cassette.Add(500, new Stack<Banknote>());
                cassette.Add(1000, new Stack<Banknote>());
                cassette.Add(2000, new Stack<Banknote>());
                cassette.Add(5000, new Stack<Banknote>());
                for (int i = 10000; i > 0; i--)
                {
                    foreach (var banknotes in cassette)
                    {
                        if (i % banknotes.Key == 0)
                        {
                            a: var c1 = (char)rnd.Next(65, 90);
                            var c2 = (char)rnd.Next(65, 90);
                            var series = "";
                            for (int j = 1; j <= 9; j++)
                                series += rnd.Next(0, 9);
                            var dist = (Math.Abs(c1 - c2) - 1);
                            series = c1.ToString() + c2.ToString() + series;
                            if (((banknotes.Key >= 1000) == (dist % 2 != 0 && dist != -1 || series.Skip(2).Sum(x => int.Parse(x + "")) % 2 == 0)) || magazine.Contains(series))
                            {
                                goto a;
                            }
                            banknotes.Value.Push(new Banknote(banknotes.Key, series));
                            magazine.Add(series);
                        }
                    }
                }
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
        #endregion
    }
    #endregion
    internal class Program
    {
        static void Main(string[] args)
        {
            var cassette = new Dictionary<int, int>();
            var atm = new ATM("Bank", new Dictionary<int, Stack<Banknote>>());
            atm.Pickup("1428394874562349");
            Console.WriteLine(atm.CashAmount());
            var card = new Card("0000000000000000", "", 12, 2030, "Bank", 10000);
            atm.Withdraw(card, 5000);
        }
    }
}
/* В коде ОЧЕНЬ мого комментариев, так что вспомнитиь: что, где и как мне было просто;
 * Во основном названия переменных – говорящие */