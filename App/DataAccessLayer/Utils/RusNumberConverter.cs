using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public enum TextCase
    {
        Nominative/*Кто? Что?*/, 
        Genitive/*Кого? Чего?*/, 
        Dative/*Кому? Чему?*/, 
        Accusative/*Кого? Что?*/, 
        Instrumental/*Кем? Чем?*/, 
        Prepositional/*О ком? О чём?*/
    };

    public class LanguageNames
    {
        public string Culture { get; set; }
        public string[] Names { get; set; }

        public LanguageNames(string culture, params string[] names)
        {
            Culture = culture;
            Names = names;
        }
    }

    public class RusNumberConverter
    {
        private static readonly string[] MonthNamesGenitive =
        {
            "", "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября",
            "ноября", "декабря"
        };

        const string Zero = "ноль";
        const string FirstMale = "один";
        const string FirstFemale = "одна";
        const string FirstFemaleAccusative = "одну";
        const string FirstMaleGenetive = "одно";
        const string SecondMale = "два";
        const string SecondFemale = "две";
        const string SecondMaleGenetive = "двух";
        const string SecondFemaleGenetive = "двух";

        private static readonly string[] From3Till19 =
            {
                "", "три", "четыре", "пять", "шесть",
                "семь", "восемь", "девять", "десять", "одиннадцать",
                "двенадцать", "тринадцать", "четырнадцать", "пятнадцать",
                "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
            };

        private static readonly string[] From3Till19Genetive =
            {
                "", "трех", "четырех", "пяти", "шести",
                "семи", "восеми", "девяти", "десяти", "одиннадцати",
                "двенадцати", "тринадцати", "четырнадцати", "пятнадцати",
                "шестнадцати", "семнадцати", "восемнадцати", "девятнадцати"
            };

        private static readonly string[] Tens =
            {
                "", "двадцать", "тридцать", "сорок", "пятьдесят",
                "шестьдесят", "семьдесят", "восемьдесят", "девяносто"
            };

        private static readonly string[] TensGenetive =
            {
                "", "двадцати", "тридцати", "сорока", "пятидесяти",
                "шестидесяти", "семидесяти", "восьмидесяти", "девяноста"
            };

        private static readonly string[] Hundreds =
            {
                "", "сто", "двести", "триста", "четыреста",
                "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"
            };

        private static readonly string[] HundredsGenetive =
            {
                "", "ста", "двухсот", "трехсот", "четырехсот",
                "пятисот", "шестисот", "семисот", "восемисот", "девятисот"
            };

        private static readonly string[] Thousands =
            {
                "", "тысяча", "тысячи", "тысяч"
            };

        private static readonly string[] ThousandsAccusative =
            {
                "", "тысячу", "тысячи", "тысяч"
            };

        private static readonly string[] Millions =
            {
                "", "миллион", "миллиона", "миллионов"
            };

        private static readonly string[] Billions =
            {
                "", "миллиард", "миллиарда", "миллиардов"
            };

        private static readonly string[] Trillions =
            {
                "", "трилион", "трилиона", "триллионов"
            };

        public static IList<LanguageNames> Currencies { get { return _currencies; } }
        public static IList<LanguageNames> Coins { get { return _coins; } }

        private static readonly string[] Rubles =
            {
                "", "рубль", "рубля", "рублей"
            };

        private static readonly string[] Soms =
            {
                "", "сом", "сома", "сомов"
            };

        private static readonly string[] Copecks =
            {
                "", "копейка", "копейки", "копеек"
            };

        private static readonly string[] Tyiyns =
            {
                "", "тыйын", "тыйына", "тыйын"
            };

        private static readonly IList<LanguageNames> _currencies =
            new List<LanguageNames>(new[] { new LanguageNames("ru-RU", Rubles), new LanguageNames("ky-KG", Soms) });

        private static readonly IList<LanguageNames> _coins =
            new List<LanguageNames>(new[] {new LanguageNames("ru-RU", Copecks), new LanguageNames("ky-KG", Tyiyns)});

        /// <summary>
        /// «07» января 2004 [+ _year(:года)]
        /// </summary>
        /// <param name="date"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string DateToTextLong(DateTime date, string year)
        {
            return String.Format("«{0}» {1} {2}",
                                 date.Day.ToString("D2"),
                                 MonthName(date.Month, TextCase.Genitive),
                                 date.Year.ToString()) + ((year.Length != 0) ? " " : "") + year;
        }

        /// <summary>
        /// «07» января 2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextLong(DateTime date)
        {
            return String.Format("«{0}» {1} {2}",
                                    date.Day.ToString("D2"),
                                    MonthName(date.Month, TextCase.Genitive),
                                    date.Year.ToString());
        }
        /// <summary>
        /// II квартал 2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextQuarter(DateTime date)
        {
            return NumeralsRoman(DateQuarter(date)) + " квартал " + date.Year.ToString();
        }

        /// <summary>
        /// 07.01.2004
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateToTextSimple(DateTime date)
        {
            return String.Format("{0:dd.MM.yyyy}", date);
        }

        public static int DateQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        static bool IsPluralGenitive(int digits)
        {
            if (digits >= 5 || digits == 0)
                return true;

            return false;
        }

        static bool IsSingularGenitive(int digits)
        {
            if (digits >= 2 && digits <= 4)
                return true;

            return false;
        }

        static int LastDigit(long _amount)
        {
            long amount = _amount;

            if (amount >= 100)
                amount = amount % 100;

            if (amount >= 20)
                amount = amount % 10;

            return (int)amount;
        }

        /// <summary>
        /// Десять тысяч рублей 67 копеек
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxt(double amount, bool firstCapital)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var currencyNames = Currencies.FirstOrDefault(n => n.Culture == culture.Name) ?? Currencies[0];
            var coinNames = Coins.FirstOrDefault(n => n.Culture == culture.Name) ?? Coins[0];

            //Десять тысяч рублей 67 копеек
            var rublesAmount = (long) Math.Floor(amount);
            var copecksAmount = ((long) Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital) + " ";

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += currencyNames.Names[3];  //Soms[3] + " "; // Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += currencyNames.Names[2];  //Soms[2] + " "; // Rubles[2] + " ";
            }
            else
            {
                s += currencyNames.Names[1] + " ";  //Soms[1] + " "; // Rubles[1] + " ";
            }

            s += String.Format("{0:00} ", copecksAmount);

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += coinNames.Names[3] + " "; //Tyiyns[3] + " "; // Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += coinNames.Names[2] + " ";  //Tyiyns[2] + " "; // Copecks[2] + " ";
            }
            else
            {
                s += coinNames.Names[1] + " "; //Tyiyns[1] + " "; // Copecks[1] + " ";
            }

            return s.Trim();
        }

        /// <summary>
        /// 10 000 (Десять тысяч) рублей 67 копеек
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxtFull(double amount, bool firstCapital)
        {
            //10 000 (Десять тысяч) рублей 67 копеек
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = String.Format("{0:N0} ({1}) ", rublesAmount, NumeralsToTxt(rublesAmount, TextCase.Nominative, true, firstCapital));

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += Soms[3] + " "; // Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += Soms[2] + " "; // Rubles[2] + " ";
            }
            else
            {
                s += Soms[1] + " "; // Rubles[1] + " ";
            }

            s += String.Format("{0:00} ", copecksAmount);

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += Tyiyns[3] + " "; // Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += Tyiyns[2] + " "; // Copecks[2] + " ";
            }
            else
            {
                s += Tyiyns[1] + " "; // Copecks[1] + " ";
            }

            return s.Trim();
        }

        /// <summary>
        /// 10 000 рублей 67 копеек  
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string CurrencyToTxtShort(double amount, bool firstCapital)
        {
            //10 000 рублей 67 копеек        
            var rublesAmount = (long)Math.Floor(amount);
            var copecksAmount = ((long)Math.Round(amount * 100)) % 100;
            var lastRublesDigit = LastDigit(rublesAmount);
            var lastCopecksDigit = LastDigit(copecksAmount);

            var s = String.Format("{0:N0} ", rublesAmount);

            if (IsPluralGenitive(lastRublesDigit))
            {
                s += Soms[3] + " "; // Rubles[3] + " ";
            }
            else if (IsSingularGenitive(lastRublesDigit))
            {
                s += Soms[2] + " "; // Rubles[2] + " ";
            }
            else
            {
                s += Soms[1] + " "; // Rubles[1] + " ";
            }

            s += String.Format("{0:00} ", copecksAmount);

            if (IsPluralGenitive(lastCopecksDigit))
            {
                s += Tyiyns[3] + " "; // Copecks[3] + " ";
            }
            else if (IsSingularGenitive(lastCopecksDigit))
            {
                s += Tyiyns[2] + " "; // Copecks[2] + " ";
            }
            else
            {
                s += Tyiyns[1] + " "; // Copecks[1] + " ";
            }

            return s.Trim();
        }

        static string MakeText(int _digits, string[] hundreds, string[] tens, string[] from3Till19, string second, string first, string[] power)
        {
            string s = "";
            int digits = _digits;

            if (digits >= 100)
            {
                s += hundreds[digits / 100] + " ";
                digits = digits % 100;
            }
            if (digits >= 20)
            {
                s += tens[digits / 10 - 1] + " ";
                digits = digits % 10;
            }

            if (digits >= 3)
            {
                s += from3Till19[digits - 2] + " ";
            }
            else if (digits == 2)
            {
                s += second + " ";
            }
            else if (digits == 1)
            {
                s += first + " ";
            }

            if (_digits != 0 && power.Length > 0)
            {
                digits = LastDigit(_digits);

                if (IsPluralGenitive(digits))
                {
                    s += power[3] + " ";
                }
                else if (IsSingularGenitive(digits))
                {
                    s += power[2] + " ";
                }
                else
                {
                    s += power[1] + " ";
                }
            }

            return s;
        }

        /// <summary>
        /// реализовано для падежей: именительный (nominative), родительный (Genitive),  винительный (accusative)
        /// </summary>
        /// <param name="sourceNumber"></param>
        /// <param name="_case"></param>
        /// <param name="isMale"></param>
        /// <param name="firstCapital"></param>
        /// <returns></returns>
        public static string NumeralsToTxt(long sourceNumber, TextCase _case, bool isMale, bool firstCapital)
        {
            var s = "";
            var number = sourceNumber;
            var power = 0;

            if ((number >= (long) Math.Pow(10, 15)) || number < 0)
            {
                return "";
            }

            while (number > 0)
            {
                var remainder = (int)(number % 1000);
                number = number / 1000;

                switch (power)
                {
                    case 12:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Trillions) + s;
                        break;
                    case 9:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Billions) + s;
                        break;
                    case 6:
                        s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondMale, FirstMale, Millions) + s;
                        break;
                    case 3:
                        switch (_case)
                        {
                            case TextCase.Accusative:
                                s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondFemale, FirstFemaleAccusative, ThousandsAccusative) + s;
                                break;
                            default:
                                s = MakeText(remainder, Hundreds, Tens, From3Till19, SecondFemale, FirstFemale, Thousands) + s;
                                break;
                        }
                        break;
                    default:
                        string[] powerArray = { };
                        switch (_case)
                        {
                            case TextCase.Genitive:
                                s = MakeText(remainder, HundredsGenetive, TensGenetive, From3Till19Genetive, isMale ? SecondMaleGenetive : SecondFemaleGenetive, isMale ? FirstMaleGenetive : FirstFemale, powerArray) + s;
                                break;
                            case TextCase.Accusative:
                                s = MakeText(remainder, Hundreds, Tens, From3Till19, isMale ? SecondMale : SecondFemale, isMale ? FirstMale : FirstFemaleAccusative, powerArray) + s;
                                break;
                            default:
                                s = MakeText(remainder, Hundreds, Tens, From3Till19, isMale ? SecondMale : SecondFemale, isMale ? FirstMale : FirstFemale, powerArray) + s;
                                break;
                        }
                        break;
                }

                power += 3;
            }

            if (sourceNumber == 0)
            {
                s = Zero + " ";
            }

            if (s != "" && firstCapital)
                s = s.Substring(0, 1).ToUpper() + s.Substring(1);

            return s.Trim();
        }

        public static string NumeralsDoubleToTxt(double sourceNumber, int @decimal, TextCase _case, bool firstCapital)
        {
            var decNum = (long)Math.Round(sourceNumber * Math.Pow(10, @decimal)) % (long)(Math.Pow(10, @decimal));

            var s = String.Format(" {0} целых {1} сотых", NumeralsToTxt((long) sourceNumber, _case, true, firstCapital),
                                  NumeralsToTxt(decNum, _case, true, false));
            return s.Trim();
        }

        /// <summary>
        /// название м-ца
        /// </summary>
        /// <param name="month">с единицы</param>
        /// <param name="_case"></param>
        /// <returns></returns>
        public static string MonthName(int month, TextCase _case)
        {
            var s = "";

            if (month > 0 && month <= 12)
            {
                switch (_case)
                {
                    case TextCase.Genitive:
                        s = MonthNamesGenitive[month];
                        break;
                }
            }

            return s;
        }

        public static string NumeralsRoman(int number)
        {
            var s = "";

            switch (number)
            {
                case 1: s = "I"; break;
                case 2: s = "II"; break;
                case 3: s = "III"; break;
                case 4: s = "IV"; break;
            }

            return s;
        }
    }
}
