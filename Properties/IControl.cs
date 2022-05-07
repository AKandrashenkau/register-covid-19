namespace REGOVID.Properties.Interface.Database.Operation
{
    interface IRecord
    {
        string Insert { get; }
        string Update { get; }
        string Search { get; }
        string Delete { get; }
    }
    interface IField
    {
        string GetNext { get; }   // fetch the highest unique value and increase its by one
        string GetNotExisted { get; }   // fetch missed unique value(s) by comparing with different sheet
        string FindByUN { get; }   // fetch record by unique value
        string FindByName { get; }   // fetch record by GUI input field - either ФИО or Наименование
    }
}