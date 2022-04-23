using REGOVID.Properties.Interface.Database.Operation;
using REGOVID.Properties.Enum.Database;
using REGOVID.Properties.Enum.Database.Operation;
using REGOVID.Properties.Struct.Database.Tab;

namespace REGOVID.Properties.Struct.Database.Tab
{
    readonly struct Patient : IRecord, IField
    {
        public Patient(in string slf, in string birthYear, in string homeAddr, in string company, in string occupation)
        {
            IDN = Record.UN;
            Slf = slf;
            BirthYear = birthYear;
            HomeAddr = homeAddr;
            Company = company;
            Occupation = occupation;
        }
        private readonly object IDN;
        private readonly string Slf;
        private readonly string BirthYear;
        private readonly string HomeAddr;
        private readonly string Company;
        private readonly string Occupation;
        public static readonly char[] tagSlf = new char[3] { (char)0x0424, (char)0x0418, (char)0x041E };
        public static readonly char[] tagBirthYear = new char[13] { (char)0x0414, (char)0x0430, (char)0x0442, (char)0x0430, (char)0x0020, (char)0x0440, (char)0x043E, (char)0x0436, (char)0x0434, (char)0x0435, (char)0x043D, (char)0x0438, (char)0x044F };
        public static readonly char[] tagHomeAddr = new char[16] { (char)0x0410, (char)0x0434, (char)0x0440, (char)0x0435, (char)0x0441, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0436, (char)0x0438, (char)0x0432, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x044F };
        public static readonly char[] tagCompany = new char[12] { (char)0x041C, (char)0x0435, (char)0x0441, (char)0x0442, (char)0x043E, (char)0x0020, (char)0x0440, (char)0x0430, (char)0x0431, (char)0x043E, (char)0x0442, (char)0x044B };
        public static readonly char[] tagOccupation = new char[20] { (char)0x0417, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x043C, (char)0x0430, (char)0x0435, (char)0x043C, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x043D, (char)0x043E, (char)0x0441, (char)0x0442, (char)0x044C };
        public string FindByUN => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Field.IDN}] LIKE '%{IDN}%'";
        string IField.FindByUN => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[Серия препарата],[{Field.Приём}],[Фактическая дата],[Плановая дата] " +
                                  $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.IDN}] LIKE '%{IDN}%' AND [{Field.IDN}]=[{Field.UNN}] ORDER BY [{Field.ФИО}] ASC";
        public string FindBySlf => $"SELECT DISTINCT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{Slf}%' AND [{Field.IDN}]=[{Field.UNN}]";
        public string GetNotExisted => $"SELECT DISTINCT [{Field.UNN}] FROM [{Sheet.Вакцина}{(char)36}] WHERE NOT EXISTS (SELECT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Sheet.Вакцина}{(char)36}].[{Field.UNN}]=[{Field.IDN}])";   // gather UNNs which are not in IDN column
        public string GetNext => $"SELECT MAX([{Field.IDN}]) + 1 FROM [{Sheet.Пациент}{(char)36}]";
        public string Insert => $"INSERT INTO [{Sheet.Пациент}{(char)36}] VALUES ('{IDN}','{Slf}','{BirthYear}','{HomeAddr}','{Company}','{Occupation}')";
        public string Update => $"UPDATE [{Sheet.Пациент}{(char)36}] SET [{Field.ФИО}]='{Slf}',[{Field.Год}]='{BirthYear}',[{Field.Адрес}]='{HomeAddr}',[{Field.Организация}]='{Company}',[{Field.Должность}]='{Occupation}' WHERE [{Field.IDN}]={IDN}";
        public string Search => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{Slf}%' AND [{Field.Год}] " +
                              $"LIKE '%{BirthYear}%' AND [{Field.Адрес}] LIKE '%{HomeAddr}%' AND [{Field.Организация}] LIKE '%{Company}%' AND [{Field.Должность}] LIKE '%{Occupation}%' ORDER BY [{Field.ФИО}] ASC";
        public string Delete => $"UPDATE [{Sheet.Пациент}{(char)36}] SET [{Field.IDN}]=NULL,[{Field.ФИО}]=NULL,[{Field.Год}]=NULL,[{Field.Адрес}]=NULL,[{Field.Организация}]=NULL,[{Field.Должность}]=NULL WHERE [{Field.IDN}]={IDN}";
        public override string ToString() => Slf + (char)0x3B + BirthYear + (char)0x3B + HomeAddr + (char)0x3B + Company + (char)0x3B + Occupation;
    }
    readonly struct Vaccine : IRecord, IField
    {
        public Vaccine(in string name, in string serial, in string numbAppoint, in string dateActual, in string datePlan)
        {
            UNN = Record.UN;
            Name = name;
            Serial = serial;
            NumbAppoint = numbAppoint;
            DateActual = dateActual;
            DatePlan = datePlan;
        }
        private readonly object UNN;
        private readonly string Name;
        private readonly string Serial;
        private readonly string NumbAppoint;
        private readonly string DateActual;
        private readonly string DatePlan;
        public static readonly char[] tagName = new char[22] { (char)0x041D, (char)0x0430, (char)0x0438, (char)0x043C, (char)0x0435, (char)0x043D, (char)0x043E, (char)0x0432, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x0435, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0435, (char)0x043F, (char)0x0430, (char)0x0440, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagSerial = new char[15] { (char)0x0421, (char)0x0435, (char)0x0440, (char)0x0438, (char)0x044F, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0435, (char)0x043F, (char)0x0430, (char)0x0440, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagNumbAppoint = new char[17] { (char)0x041A, (char)0x043E, (char)0x043B, (char)0x0438, (char)0x0447, (char)0x0435, (char)0x0441, (char)0x0442, (char)0x0432, (char)0x043E, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0438, (char)0x0451, (char)0x043C, (char)0x0430 };
        public static readonly char[] tagDateActual = new char[16] { (char)0x0424, (char)0x0430, (char)0x043A, (char)0x0442, (char)0x0438, (char)0x0447, (char)0x0435, (char)0x0441, (char)0x043A, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagDatePlan = new char[13] { (char)0x041F, (char)0x043B, (char)0x0430, (char)0x043D, (char)0x043E, (char)0x0432, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x0442, (char)0x0430 };
        public string FindByUN => $"SELECT [{Field.Наименование}],[Серия препарата],[{Field.Приём}],[Фактическая дата],[Плановая дата] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Field.UNN}] LIKE '%{UNN}%'";
        public string GetNotExisted => $"SELECT DISTINCT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}] WHERE NOT EXISTS (SELECT [{Field.UNN}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Sheet.Пациент}{(char)36}].[{Field.IDN}]=[{Field.UNN}])";   // gather IDNs which are not in UNN column
        public string GetNext => $"SELECT MAX([{Field.UNN}]) + 1 FROM [{Sheet.Вакцина}{(char)36}]";
        public string Insert => $"INSERT INTO [{Sheet.Вакцина}{(char)36}] VALUES ('{UNN}','{Name}','{Serial}','{NumbAppoint}','{DateActual}','{DatePlan}'){(char)59}";
        public string Update => $"UPDATE [{Sheet.Вакцина}{(char)36}] SET [{Field.Наименование}]='{Name}',[Серия препарата]='{Serial}',[{Field.Приём}]='{NumbAppoint}',[Плановая дата]='{DatePlan}' WHERE [{Field.UNN}]={UNN} AND [Фактическая дата]='{DateActual}'";
        public string Search => $"SELECT [{Field.Наименование}],[Серия препарата],[{Field.Приём}],[Фактическая дата],[Плановая дата] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Field.Наименование}] LIKE '%{Name}%' AND " +
                              $"[Серия препарата] LIKE '%{Serial}%' AND [{Field.Приём}] LIKE '%{NumbAppoint}%' AND [Фактическая дата] LIKE '%{DateActual}%' AND [Плановая дата] LIKE '%{DatePlan}%' ORDER BY [{Field.Наименование}] ASC";
        public string Delete => $"UPDATE [{Sheet.Вакцина}{(char)36}] SET [{Field.UNN}]=NULL,[{Field.Наименование}]=NULL,[Серия препарата]=NULL,[{Field.Приём}]=NULL,[Фактическая дата]=NULL,[Плановая дата]=NULL WHERE [{Field.UNN}]={UNN}";
        public override string ToString() => Name + (char)0x3B + Serial + (char)0x3B + NumbAppoint + (char)0x3B + DateActual + (char)0x3B + DatePlan;
    }
}
namespace REGOVID.Properties.Struct.Database
    {
    readonly ref struct Record
    {
        public Record(params IRecord[] sender)
        {
            switch (sender)
            {
                case var _ when sender.Length == 2 && sender[0] is Patient && sender[1] is Vaccine:
                    break;
                default:
                    Slf = string.Empty;
                    BirthYear = string.Empty;
                    HomeAddr = string.Empty;
                    Company = string.Empty;
                    Occupation = string.Empty;
                    NameVaccine = string.Empty;
                    Serial = string.Empty;
                    NumbAppoint = string.Empty;
                    DateActual = string.Empty;
                    DatePlan = string.Empty;
                    return;
            }
            Slf = sender[0].ToString().Split((char)0x3B)[0];
            BirthYear = sender[0].ToString().Split((char)0x3B)[1];
            HomeAddr = sender[0].ToString().Split((char)0x3B)[2];
            Company = sender[0].ToString().Split((char)0x3B)[3];
            Occupation = sender[0].ToString().Split((char)0x3B)[4];
            NameVaccine = sender[1].ToString().Split((char)0x3B)[0];
            Serial = sender[1].ToString().Split((char)0x3B)[1];
            NumbAppoint = sender[1].ToString().Split((char)0x3B)[2];
            DateActual = sender[1].ToString().Split((char)0x3B)[3];
            DatePlan = sender[1].ToString().Split((char)0x3B)[4];
        }
        public static object UN { get; set; }   // stands for Unique Number
        internal string Slf { get; }
        internal string BirthYear { get; }
        internal string HomeAddr { get; }
        internal string Company { get; }
        internal string Occupation { get; }
        internal string NameVaccine { get; }
        internal string Serial { get; }
        internal string NumbAppoint { get; }
        internal string DateActual { get; }
        internal string DatePlan { get; }
        public object this[in Field index]
        {
            get
            {
                switch(index)
                {
                    case Field.ФИО:
                        return Slf;
                    case Field.Год:
                        return BirthYear;
                    case Field.Адрес:
                        return HomeAddr;
                    case Field.Организация:
                        return Company;
                    case Field.Должность:
                        return Occupation;
                    case Field.Наименование:
                        return NameVaccine;
                    case Field.СерияПрепарата:
                        return Serial;
                    case Field.Приём:
                        return NumbAppoint;
                    case Field.ФактическаяДата:
                        return DateActual;
                    case Field.ПлановаяДата:
                        return DatePlan;
                    default:
                        return null;
                }
            }
        }
        public object this[in Action act, in IRecord sender]
        {
            get
            {
                switch ($"{act},{sender.GetType().Name}")
                {
                    case "Записать,Patient":
                        return new Patient(Slf, BirthYear, HomeAddr, Company, Occupation).Insert;
                    case "Записать,Vaccine":
                        return new Vaccine(NameVaccine, Serial, NumbAppoint, DateActual, DatePlan).Insert;
                    case "Поиск,Patient":
                        return new Patient(Slf, BirthYear, HomeAddr, Company, Occupation).Search;
                    case "Поиск,Vaccine":
                        return new Vaccine(NameVaccine, Serial, NumbAppoint, DateActual, DatePlan).Search;
                    case "Удалить,Patient":
                        return new Patient(Slf, BirthYear, HomeAddr, Company, Occupation).Delete;
                    case "Удалить,Vaccine":
                        return new Vaccine(NameVaccine, Serial, NumbAppoint, DateActual, DatePlan).Delete;
                    default:
                        return null;
                }
            }
        }
        public object this[in Action act, in byte indRequest, in IField sender]
        {
            get
            {
                switch ($"{act},{indRequest},{sender.GetType().Name}")
                {
                    case "Поиск,0,Patient":
                        return ((IField)new Patient(NameVaccine, Serial, NumbAppoint, DateActual, DatePlan)).FindByUN;
                    case "Поиск,1,Patient":
                        return new Patient(Slf, BirthYear, HomeAddr, Company, Occupation).FindByUN;
                    case "Поиск,1,Vaccine":
                        return new Vaccine(NameVaccine, Serial, NumbAppoint, DateActual, DatePlan).FindByUN;
                    case "Поиск,2,Patient":
                        return $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[Серия препарата],[{Field.Приём}],[Фактическая дата],[Плановая дата] " +
                               $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{Slf}%' AND [{Field.Год}] LIKE '%{BirthYear}%' AND [{Field.Адрес}] LIKE '%{HomeAddr}%' AND [{Field.Организация}] " +
                               $"LIKE '%{Company}%' AND [{Field.Должность}] LIKE '%{Occupation}%' AND [{Field.Наименование}] LIKE '%{NameVaccine}%' AND [Серия препарата] LIKE '%{Serial}%' " +
                               $"AND [{Field.Приём}] LIKE '%{NumbAppoint}%' AND [Фактическая дата] LIKE '%{DateActual}%' AND [Плановая дата] LIKE '%{DatePlan}%' AND [{Field.IDN}]=[{Field.UNN}] ORDER BY [{Field.ФИО}] ASC";
                    default:
                        return null;
                }
            }
        }
        public override string ToString() => Slf + (char)0x3B + BirthYear + (char)0x3B + HomeAddr + (char)0x3B + Company + (char)0x3B + Occupation + (char)0x3B + NameVaccine + (char)0x3B + Serial + (char)0x3B + NumbAppoint + (char)0x3B + DateActual + (char)0x3B + DatePlan;
    }
}