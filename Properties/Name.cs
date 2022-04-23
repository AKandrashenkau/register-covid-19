namespace REGOVID.Properties.Enum.Database
{
    enum Sheet : byte { Пациент, Вакцина = Field.Наименование }   // associated constant value is a number of field where sheet starts from
    enum Field : byte { IDN = 0xFF, ФИО = 0x01, Год = 0x02, Адрес = 0x03, Организация = 0x04, Должность = 0x05, UNN = 0xFE, Наименование = 0x06, СерияПрепарата = 0x07, Приём = 0x08, ФактическаяДата = 0x09, ПлановаяДата = 0x0A}
}
namespace REGOVID.Properties.Enum.Database.Operation
{
    enum Action : byte { None, Записать, Изменить, Поиск, Удалить }
}