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
            this.slf = slf;
            this.birthYear = birthYear;
            this.homeAddr = homeAddr;
            this.company = company;
            this.occupation = occupation;
        }
        public Patient(in object IDN)
        {
            this.IDN = IDN;
            slf = string.Empty;
            birthYear = string.Empty;
            homeAddr = string.Empty;
            company = string.Empty;
            occupation = string.Empty;
        }
        private readonly object IDN;
        private readonly string slf;
        private readonly string birthYear;
        private readonly string homeAddr;
        private readonly string company;
        private readonly string occupation;
        public static readonly char[] tagSlf = new char[0x03] { (char)0x0424, (char)0x0418, (char)0x041E };
        public static readonly char[] tagBirthYear = new char[0x0D] { (char)0x0414, (char)0x0430, (char)0x0442, (char)0x0430, (char)0x0020, (char)0x0440, (char)0x043E, (char)0x0436, (char)0x0434, (char)0x0435, (char)0x043D, (char)0x0438, (char)0x044F };
        public static readonly char[] tagHomeAddr = new char[0x10] { (char)0x0410, (char)0x0434, (char)0x0440, (char)0x0435, (char)0x0441, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0436, (char)0x0438, (char)0x0432, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x044F };
        public static readonly char[] tagCompany = new char[0x0C] { (char)0x041C, (char)0x0435, (char)0x0441, (char)0x0442, (char)0x043E, (char)0x0020, (char)0x0440, (char)0x0430, (char)0x0431, (char)0x043E, (char)0x0442, (char)0x044B };
        public static readonly char[] tagOccupation = new char[0x14] { (char)0x0417, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x043C, (char)0x0430, (char)0x0435, (char)0x043C, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x043D, (char)0x043E, (char)0x0441, (char)0x0442, (char)0x044C };
        public object this[in Field field, in Action act]
        {
            get
            {
                switch ($"{field},{act}")
                {
                    case "IDN,Поиск":   // instantiate the latest IDN by UN
                        return new Patient(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty).FindByUN;
                    // useless indexer(this) might be used to handle iteratively(private fields) and to optimize the code
                    case "ФИО,Поиск":
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{field}] LIKE '%{slf}%'";
                    case "Год,Поиск":
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{field}] LIKE '%{birthYear}%'";
                    case "Адрес,Поиск":
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{field}] LIKE '%{homeAddr}%'";
                    case "Организация,Поиск":
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{field}] LIKE '%{company}%'";
                    case "Должность,Поиск":
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{field}] LIKE '%{occupation}%'";
                    default:
                        return default;
                }
            }
        }
        public string FindByUN => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Field.IDN}] LIKE '%{IDN}%'";
        string IField.FindByUN => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] " +
                                  $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.IDN}] LIKE '%{IDN}%' AND [{Field.IDN}]=[{Field.UNN}] ORDER BY [{Field.ФИО}] ASC";
        public string FindByName => $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Field.ФИО}]='{slf}'";
        string IField.FindByName => $"SELECT DISTINCT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{slf}%' AND [{Field.IDN}]=[{Field.UNN}]";
        public string GetNotExisted => $"SELECT DISTINCT [{Field.UNN}] FROM [{Sheet.Вакцина}{(char)36}] WHERE NOT EXISTS (SELECT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Sheet.Вакцина}{(char)36}].[{Field.UNN}]=[{Field.IDN}])";   // gather UNNs which are not in IDN column
        public string GetNext => $"SELECT MAX([{Field.IDN}]) + 1 FROM [{Sheet.Пациент}{(char)36}]";
        public string Insert => $"INSERT INTO [{Sheet.Пациент}{(char)36}] VALUES ('{IDN}','{slf}','{birthYear}','{homeAddr}','{company}','{occupation}')";
        public string Update => $"UPDATE [{Sheet.Пациент}{(char)36}] SET [{Field.ФИО}]='{slf}',[{Field.Год}]='{birthYear}',[{Field.Адрес}]='{homeAddr}',[{Field.Организация}]='{company}',[{Field.Должность}]='{occupation}' WHERE [{Field.IDN}]={IDN}";
        public string Search => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}] FROM [{Sheet.Пациент}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{slf}%' AND [{Field.Год}] " +
                              $"LIKE '%{birthYear}%' AND [{Field.Адрес}] LIKE '%{homeAddr}%' AND [{Field.Организация}] LIKE '%{company}%' AND [{Field.Должность}] LIKE '%{occupation}%' ORDER BY [{Field.ФИО}] ASC";
        public string Delete => $"UPDATE [{Sheet.Пациент}{(char)36}] SET [{Field.IDN}]=NULL,[{Field.ФИО}]=NULL,[{Field.Год}]=NULL,[{Field.Адрес}]=NULL,[{Field.Организация}]=NULL,[{Field.Должность}]=NULL WHERE [{Field.IDN}]={IDN}";
        public override string ToString() => slf + (char)0x3B + birthYear + (char)0x3B + homeAddr + (char)0x3B + company + (char)0x3B + occupation;
    }
    readonly struct Vaccine : IRecord, IField
    {
        public Vaccine(in string name, in string serial, in string numbAppoint, in string dateActual, in string datePlan)
        {
            UNN = Record.UN;
            this.name = name;
            this.serial = serial;
            this.numbAppoint = numbAppoint;
            this.dateActual = dateActual;
            this.datePlan = datePlan;
        }
        public Vaccine(in object UNN, in string dateActual)
        {
            this.UNN = UNN;
            name = string.Empty;
            serial = string.Empty;
            numbAppoint = string.Empty;
            this.dateActual = dateActual;
            datePlan = string.Empty;
        }
        private readonly object UNN;
        private readonly string name;
        private readonly string serial;
        private readonly string numbAppoint;
        private readonly string dateActual;   // reserved for additional uniqueness(right after UNN)
        private readonly string datePlan;
        public static readonly char[] tagName = new char[0x16] { (char)0x041D, (char)0x0430, (char)0x0438, (char)0x043C, (char)0x0435, (char)0x043D, (char)0x043E, (char)0x0432, (char)0x0430, (char)0x043D, (char)0x0438, (char)0x0435, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0435, (char)0x043F, (char)0x0430, (char)0x0440, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagSerial = new char[0x0F] { (char)0x0421, (char)0x0435, (char)0x0440, (char)0x0438, (char)0x044F, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0435, (char)0x043F, (char)0x0430, (char)0x0440, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagNumbAppoint = new char[0x11] { (char)0x041A, (char)0x043E, (char)0x043B, (char)0x0438, (char)0x0447, (char)0x0435, (char)0x0441, (char)0x0442, (char)0x0432, (char)0x043E, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x0438, (char)0x0451, (char)0x043C, (char)0x0430 };
        public static readonly char[] tagDateActual = new char[0x10] { (char)0x0424, (char)0x0430, (char)0x043A, (char)0x0442, (char)0x0438, (char)0x0447, (char)0x0435, (char)0x0441, (char)0x043A, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x0442, (char)0x0430 };
        public static readonly char[] tagDatePlan = new char[0x0D] { (char)0x041F, (char)0x043B, (char)0x0430, (char)0x043D, (char)0x043E, (char)0x0432, (char)0x0430, (char)0x044F, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x0442, (char)0x0430 };
        public object this[in Field field, in Action act]
        {
            get
            {
                switch ($"{field},{act}")
                {
                    case "UNN,Поиск":   // instantiate the latest UNN by UN
                        return new Vaccine(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty).FindByUN;
                    case "Наименование,Поиск":
                        return FindByName;
                    // useless indexer(this) might be used to handle iteratively(private fields) and to optimize the code
                    case "СерияПрепарата,Поиск":
                        return $"SELECT [{Field.UNN}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{field}] LIKE '%{serial}%'";
                    case "Приём,Поиск":
                        return $"SELECT [{Field.UNN}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{field}] LIKE '%{numbAppoint}%'";
                    case "ФактическаяДата,Поиск":
                        return $"SELECT [{Field.UNN}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{field}] LIKE '%{dateActual}%'";
                    case "ПлановаяДата,Поиск":
                        return $"SELECT [{Field.UNN}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{field}] LIKE '%{datePlan}%'";
                    default:
                        return default;
                }
            }
        }
        public string FindByUN => $"SELECT [{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Field.UNN}] LIKE '%{UNN}%'";
        string IField.FindByUN => $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] " +
                                  $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.UNN}] LIKE '%{UNN}%' AND [{Field.UNN}]=[{Field.IDN}] AND [{Field.ФактическаяДата}] LIKE '{dateActual}' ORDER BY [{Field.Наименование}] ASC";
        public string FindByName => $"SELECT [{Field.UNN}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Field.Наименование}] LIKE '%{name}%'";
        public string GetNotExisted => $"SELECT DISTINCT [{Field.IDN}] FROM [{Sheet.Пациент}{(char)36}] WHERE NOT EXISTS (SELECT [{Field.UNN}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Sheet.Пациент}{(char)36}].[{Field.IDN}]=[{Field.UNN}])";   // gather IDNs which are not in UNN column
        public string GetNext => $"SELECT MAX([{Field.UNN}]) + 1 FROM [{Sheet.Вакцина}{(char)36}]";
        public string Insert => $"INSERT INTO [{Sheet.Вакцина}{(char)36}] VALUES ('{UNN}','{name}','{serial}','{numbAppoint}','{dateActual}','{datePlan}'){(char)59}";
        public string Update => $"UPDATE [{Sheet.Вакцина}{(char)36}] SET [{Field.Наименование}]='{name}',[{Field.СерияПрепарата}]='{serial}',[{Field.Приём}]='{numbAppoint}',[{Field.ПлановаяДата}]='{datePlan}' WHERE [{Field.UNN}]={UNN} AND [{Field.ФактическаяДата}]='{dateActual}'";
        public string Search => $"SELECT [{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] FROM [{Sheet.Вакцина}{(char)36}] WHERE [{Field.Наименование}] LIKE '%{name}%' AND " +
                              $"[{Field.СерияПрепарата}] LIKE '%{serial}%' AND [{Field.Приём}] LIKE '%{numbAppoint}%' AND [{Field.ФактическаяДата}] LIKE '%{dateActual}%' AND [{Field.ПлановаяДата}] LIKE '%{datePlan}%' ORDER BY [{Field.Наименование}] ASC";
        public string Delete => $"UPDATE [{Sheet.Вакцина}{(char)36}] SET [{Field.UNN}]=NULL,[{Field.Наименование}]=NULL,[{Field.СерияПрепарата}]=NULL,[{Field.Приём}]=NULL,[{Field.ФактическаяДата}]=NULL,[{Field.ПлановаяДата}]=NULL WHERE [{Field.UNN}]={UNN}";
        string IRecord.Delete => $"UPDATE [{Sheet.Вакцина}{(char)36}] SET [{Field.UNN}]=NULL,[{Field.Наименование}]=NULL,[{Field.СерияПрепарата}]=NULL,[{Field.Приём}]=NULL,[{Field.ФактическаяДата}]=NULL,[{Field.ПлановаяДата}]=NULL WHERE [{Field.UNN}]={UNN} AND [{Field.ФактическаяДата}]='{dateActual}'";
        public override string ToString() => name + (char)0x3B + serial + (char)0x3B + numbAppoint + (char)0x3B + dateActual + (char)0x3B + datePlan;
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
                case var _ when sender.Length == 0x02 && sender[default] is Patient && sender[0x01] is Vaccine:
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
            Slf = sender[default].ToString().Split((char)0x3B)[default];
            BirthYear = sender[default].ToString().Split((char)0x3B)[0x01];
            HomeAddr = sender[default].ToString().Split((char)0x3B)[0x02];
            Company = sender[default].ToString().Split((char)0x3B)[0x03];
            Occupation = sender[default].ToString().Split((char)0x3B)[0x04];
            NameVaccine = sender[0x01].ToString().Split((char)0x3B)[default];
            Serial = sender[0x01].ToString().Split((char)0x3B)[0x01];
            NumbAppoint = sender[0x01].ToString().Split((char)0x3B)[0x02];
            DateActual = sender[0x01].ToString().Split((char)0x3B)[0x03];
            DatePlan = sender[0x01].ToString().Split((char)0x3B)[0x04];
        }
        public static object UN { get; set; }   // stands in for Unique Number
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
                switch (index)
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
        public object this[in Action act]
        {
            get
            {
                switch (act)
                {
                    case Action.Поиск:
                        return $"SELECT [{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] " +
                               $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{Slf}%' AND [{Field.Год}] LIKE '%{BirthYear}%' AND [{Field.Адрес}] LIKE '%{HomeAddr}%' AND [{Field.Организация}] " +
                               $"LIKE '%{Company}%' AND [{Field.Должность}] LIKE '%{Occupation}%' AND [{Field.Наименование}] LIKE '%{NameVaccine}%' AND [{Field.СерияПрепарата}] LIKE '%{Serial}%' " +
                               $"AND [{Field.Приём}] LIKE '%{NumbAppoint}%' AND [{Field.ФактическаяДата}] LIKE '%{DateActual}%' AND [{Field.ПлановаяДата}] LIKE '%{DatePlan}%' AND [{Field.IDN}]=[{Field.UNN}] ORDER BY [{Field.ФИО}] ASC";
                    case Action.Удалить:   // the case shouldn't be taken off unless Action.Поиск has no IDN on SQL output fields
                        return $"SELECT [{Field.IDN}],[{Field.ФИО}],[{Field.Год}],[{Field.Адрес}],[{Field.Организация}],[{Field.Должность}],[{Field.Наименование}],[{Field.СерияПрепарата}],[{Field.Приём}],[{Field.ФактическаяДата}],[{Field.ПлановаяДата}] " +
                               $"FROM [{Sheet.Пациент}{(char)36}],[{Sheet.Вакцина}{(char)36}] WHERE [{Field.ФИО}] LIKE '%{Slf}%' AND [{Field.Год}] LIKE '%{BirthYear}%' AND [{Field.Адрес}] LIKE '%{HomeAddr}%' AND [{Field.Организация}] " +
                               $"LIKE '%{Company}%' AND [{Field.Должность}] LIKE '%{Occupation}%' AND [{Field.Наименование}] LIKE '%{NameVaccine}%' AND [{Field.СерияПрепарата}] LIKE '%{Serial}%' " +
                               $"AND [{Field.Приём}] LIKE '%{NumbAppoint}%' AND [{Field.ФактическаяДата}] LIKE '%{DateActual}%' AND [{Field.ПлановаяДата}] LIKE '%{DatePlan}%' AND [{Field.IDN}]=[{Field.UNN}] ORDER BY [{Field.ФИО}] ASC";
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
        public override string ToString() => Slf + (char)0x3B + BirthYear + (char)0x3B + HomeAddr + (char)0x3B + Company + (char)0x3B + Occupation + (char)0x3B + NameVaccine + (char)0x3B + Serial + (char)0x3B + NumbAppoint + (char)0x3B + DateActual + (char)0x3B + DatePlan;
    }
}
namespace REGOVID.Properties.Struct.Database.Info
{
    readonly ref struct Message
    {
        public char[] this[in byte id]
        {
            get
            {
                switch (id)
                {
                    case 0xFF:
                        return new char[0x29] { (char)0x0412, (char)0x043E, (char)0x0437, (char)0x043D, (char)0x0438, (char)0x043A, (char)0x043B, (char)0x0438, (char)0x0020, (char)0x043D, (char)0x0435, (char)0x043A, (char)0x043E, (char)0x0442, (char)0x043E, (char)0x0440,
                                                (char)0x044B, (char)0x0435, (char)0x0020, (char)0x0442, (char)0x0440, (char)0x0443, (char)0x0434, (char)0x043D, (char)0x043E, (char)0x0441, (char)0x0442, (char)0x0438, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440,
                                                (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0xFD:
                        return new char[0x1A] { (char)0x041D, (char)0x0435, (char)0x0442, (char)0x0020, (char)0x0438, (char)0x0437, (char)0x043C, (char)0x0435, (char)0x043D, (char)0x0435, (char)0x043D, (char)0x0438, (char)0x0439, (char)0x002C, (char)0x0020, (char)0x043F,
                                                (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0xFB:
                        return new char[0x1E] { (char)0x0423, (char)0x0442, (char)0x043E, (char)0x0447, (char)0x043D, (char)0x0438, (char)0x0442, (char)0x0435, (char)0x0020, (char)0x043A, (char)0x0440, (char)0x0438, (char)0x0442, (char)0x0435, (char)0x0440, (char)0x0438,
                                                (char)0x0439, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0xF9:
                        return new char[0x25] { (char)0x041A, (char)0x043E, (char)0x043D, (char)0x043A, (char)0x0440, (char)0x0435, (char)0x0442, (char)0x0438, (char)0x0437, (char)0x0438, (char)0x0440, (char)0x0443, (char)0x0439, (char)0x0442, (char)0x0435, (char)0x0020,
                                                (char)0x043A, (char)0x0440, (char)0x0438, (char)0x0442, (char)0x0435, (char)0x0440, (char)0x0438, (char)0x0439, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B,
                                                (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0xF7:
                        return new char[0x14] { (char)0x0411, (char)0x0435, (char)0x0437, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x043D, (char)0x043D, (char)0x044B, (char)0x0445, (char)0x0020, (char)0x043F, (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0435,
                                                (char)0x043D, (char)0x0442, (char)0x0430, (char)0x003F };
                    case 0xF5:
                        return new char[0x19] { (char)0x0411, (char)0x0435, (char)0x0437, (char)0x0020, (char)0x0438, (char)0x043D, (char)0x0444, (char)0x043E, (char)0x0440, (char)0x043C, (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0438, (char)0x0020, (char)0x043E,
                                                (char)0x0020, (char)0x0432, (char)0x0430, (char)0x043A, (char)0x0446, (char)0x0438, (char)0x043D, (char)0x0435, (char)0x003F };
                    case 0xF3:
                        return new char[0x18] { (char)0x0423, (char)0x0434, (char)0x0430, (char)0x043B, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x0437, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x043F,
                                                (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0435, (char)0x043D, (char)0x0442, (char)0x0430, (char)0x003F };
                    case 0xF1:
                        return new char[0x17] { (char)0x0423, (char)0x0434, (char)0x0430, (char)0x043B, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x0437, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0432,
                                                (char)0x0430, (char)0x043A, (char)0x0446, (char)0x0438, (char)0x043D, (char)0x044B, (char)0x003F };
                    case 0xEF:
                        return new char[0x27] { (char)0x0423, (char)0x0434, (char)0x0430, (char)0x043B, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x043F, (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0435, (char)0x043D, (char)0x0442, (char)0x0430,
                                                (char)0x0020, (char)0x0438, (char)0x0020, (char)0x0435, (char)0x0433, (char)0x043E, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x043D, (char)0x043D, (char)0x044B, (char)0x0435, (char)0x0020, (char)0x043F, (char)0x0440,
                                                (char)0x0438, (char)0x0432, (char)0x0438, (char)0x0432, (char)0x043E, (char)0x043A, (char)0x003F };
                    case 0xED:
                        return new char[0x1D] { (char)0x0421, (char)0x043E, (char)0x0435, (char)0x0434, (char)0x0438, (char)0x043D, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x0441, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x043D, (char)0x043D,
                                                (char)0x044B, (char)0x043C, (char)0x0438, (char)0x0020, (char)0x043F, (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0435, (char)0x043D, (char)0x0442, (char)0x0430, (char)0x003F };
                    case 0xEB:
                        return new char[0x22] { (char)0x0421, (char)0x043E, (char)0x0435, (char)0x0434, (char)0x0438, (char)0x043D, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x0441, (char)0x0020, (char)0x0438, (char)0x043D, (char)0x0444, (char)0x043E,
                                                (char)0x0440, (char)0x043C, (char)0x0430, (char)0x0446, (char)0x0438, (char)0x0435, (char)0x0439, (char)0x0020, (char)0x043E, (char)0x0020, (char)0x0432, (char)0x0430, (char)0x043A, (char)0x0446, (char)0x0438, (char)0x043D,
                                                (char)0x0435, (char)0x003F };
                    case 0xE9:
                        return new char[0x18] { (char)0x041F, (char)0x0440, (char)0x0438, (char)0x043C, (char)0x0435, (char)0x043D, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x0020, (char)0x044D, (char)0x0442, (char)0x043E, (char)0x0020, (char)0x0438, (char)0x0437,
                                                (char)0x043C, (char)0x0435, (char)0x043D, (char)0x0435, (char)0x043D, (char)0x0438, (char)0x0435, (char)0x003F };
                    case 0xE7:
                        return new char[0x2E] { (char)0x0417, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0441, (char)0x0020, (char)0x0442, (char)0x0430, (char)0x043A, (char)0x0438, (char)0x043C, (char)0x0020, (char)0x0424,
                                                (char)0x0418, (char)0x041E, (char)0x0020, (char)0x0443, (char)0x0436, (char)0x0435, (char)0x0020, (char)0x0441, (char)0x0443, (char)0x0449, (char)0x0435, (char)0x0441, (char)0x0442, (char)0x0432, (char)0x0443, (char)0x0435,
                                                (char)0x0442, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0x04:
                        return new char[0x23] { (char)0x0417, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0443, (char)0x0441, (char)0x043F, (char)0x0435, (char)0x0448, (char)0x043D, (char)0x043E, (char)0x0020, (char)0x0443,
                                                (char)0x0434, (char)0x0430, (char)0x043B, (char)0x0435, (char)0x043D, (char)0x0430, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438,
                                                (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0x02:
                        return new char[0x26] { (char)0x0417, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0443, (char)0x0441, (char)0x043F, (char)0x0435, (char)0x0448, (char)0x043D, (char)0x043E, (char)0x0020, (char)0x0438,
                                                (char)0x0441, (char)0x043F, (char)0x0440, (char)0x0430, (char)0x0432, (char)0x043B, (char)0x0435, (char)0x043D, (char)0x0430, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E,
                                                (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    case 0x00:
                        return new char[0x25] { (char)0x0417, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0443, (char)0x0441, (char)0x043F, (char)0x0435, (char)0x0448, (char)0x043D, (char)0x043E, (char)0x0020, (char)0x0434,
                                                (char)0x043E, (char)0x0431, (char)0x0430, (char)0x0432, (char)0x043B, (char)0x0435, (char)0x043D, (char)0x0430, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B,
                                                (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                    default:
                        return (string.Empty + char.MinValue).ToCharArray();
                }
            }
        }
    }
}