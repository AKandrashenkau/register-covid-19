using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using name = REGOVID.Properties.Enum.Database;
using ops = REGOVID.Properties.Enum.Database.Operation;
using iops = REGOVID.Properties.Interface.Database.Operation;
using reg = REGOVID.Properties.Struct.Database;
using inf = REGOVID.Properties.Struct.Database.Info;

namespace REGOVID
{
    partial class Form1 : Form
    {
        private readonly Timer timer1;
        private readonly int width;    // reserved for working on initialized clientWidth
        private readonly int height;   // reserved for working on initialized clientHeight
        private readonly string dtBase;
        private readonly string[] custContainer;
        private const char separator = (char)59;   // divides customized container into human-readable view that is consisting of GUI input fields
        private delegate void Action(in byte index, in object sender);
        private readonly Action hint;
        private bool snap;   // reserved for resetting flashing colours
        public Form1()
        {
            dtBase = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{Environment.GetEnvironmentVariable(string.Empty + (char)0x0050 + (char)0x0072 + (char)0x006F + (char)0x0067 + (char)0x0072 + (char)0x0061 + (char)0x006D +
                        (char)0x0044 + (char)0x0061 + (char)0x0074 + (char)0x0061) + (char)0x005C + (char)0x0056 + (char)0x0069 + (char)0x006C + (char)0x0043 + (char)0x0052 + (char)0x0042 + (char)0x005C + (char)0x0064 +
                        (char)0x0074 + (char)0x0042 + (char)0x0061 + (char)0x0073 + (char)0x0065 + (char)0x002E + (char)0x0078 + (char)0x006C + (char)0x0073 + (char)0x0062}';Extended Properties='Excel 12.0;HDR=Yes'";
            hint = SetFieldsColour;   // serves for action - "Изменить" to indicate where data amendment is applied
            hint += SetFieldsText;   // serves for action - "Изменить" to indicate what data amendment is applied
            custContainer = new string[0x03]
            {
                string.Empty,   // on action - "Изменить" for gathering indices of GUI input fields from user
                string.Empty,   // on action - "Изменить" for taking text of GUI input fields from user
                string.Empty    // on action - "Изменить" for taking complete record from database
            };
            InitializeComponent();
            timer1 = new Timer(components)
            {
                Interval = 0x01F4   // a half of 0x01 sec
            };
            width = Math.Abs(Width);
            height = Math.Abs(Height);
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            SetGUI(default, default);
        }
        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAction(ops.Action.Записать);
        }
        private void изменитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetAction(ops.Action.Изменить);
        }
        private void найтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAction(ops.Action.Поиск);
        }
        private void удалитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetAction(ops.Action.Удалить);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var msg = new inf.Message();
            var patient = new reg.Tab.Patient(textBox1.Text, label1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
            var vaccine = new reg.Tab.Vaccine(textBox5.Text, textBox6.Text, label2.Text, label3.Text, label4.Text);
            var senders = new reg.Record(patient, vaccine);
            var condition = new char[(byte)name.Field.ПлановаяДата]   // on instantiation: the size of GUI input fields
            {
                (char)49,   // ФИО
                (char)48,   // Год
                (char)54,   // Адрес
                (char)50,   // Организация
                (char)52,   // Должность
                (char)51,   // Наименование
                (char)53,   // СерияПрепарата
                (char)51,   // Приём
                (char)52,   // ФактическаяДата
                (char)52    // ПлановаяДата
            };
            switch (sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Split((char)0x0020)[0x01])   // determine action by GUI output
            {
                case "Записать":
                    switch (Handler((byte)ops.Action.Записать, out byte sheet, condition, senders.Slf, senders.BirthYear, senders.HomeAddr, senders.Company, senders.Occupation, senders.NameVaccine, senders.Serial, senders.NumbAppoint, senders.DateActual, senders.DatePlan))
                    {
                        case true:
                            var nxtIDN = ExecSQLSingle(patient.GetNext);   // increase MAX(IDN) by one
                            var nxtUNN = ExecSQLSingle(vaccine.GetNext);   // increase MAX(UNN) by one
                            switch (sheet)
                            {
                                case 0x00:   // concerning Пациент
                                    ExecSQLtoSet(patient.GetNotExisted, out Queue container);   // stay advised that from this point the "Формат ячеек" of cell should be "Числовой" in Excel
                                    switch (container.Count == default)
                                    {
                                        case true:
                                            reg.Record.UN = nxtIDN;
                                            break;
                                        default:   // there are some unpaired UNNs
                                            switch (string.IsNullOrEmpty(string.Empty + container.Peek()))
                                            {
                                                case true:   // IDN has nothing(UNN column is empty) to compare with
                                                    reg.Record.UN = DBNull.Value.Equals(nxtIDN) ? uint.MinValue : GetMax(nxtIDN, uint.MinValue);
                                                    break;
                                                default:
                                                    SetGUI(default, default);   // prepare for chance to output SQL result
                                                    for (; container.Count > uint.MinValue;)
                                                    {
                                                        reg.Record.UN = container.Dequeue();   // take one off
                                                        dataGridView1.DataSource = ExecSQLtoSet(string.Empty + vaccine[(name.Field)0xFE, ops.Action.Поиск]).DefaultView;   // SQL result
                                                        switch (GetMsg(GetTextFormatted(msg[0xEB], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)   // suggest unpaired IDN to connect with UNN
                                                        {
                                                            case true:
                                                                container.Clear();   // interrupt the read-next
                                                                continue;
                                                        }
                                                        reg.Record.UN = default;   // mark off a deal with increased MAX(IDN) by one
                                                    }
                                                    switch (Equals(reg.Record.UN, default))   // the deal
                                                    {
                                                        case true:
                                                            reg.Record.UN = Convert.ToInt32(GetMax(nxtIDN, uint.MinValue)) == default ? uint.MinValue : GetMax(nxtIDN, reg.Record.UN);
                                                            reg.Record.UN = Convert.ToUInt32(reg.Record.UN).Equals(Convert.ToUInt32(nxtUNN) - 0x01) ? nxtUNN : reg.Record.UN;   // the increased MAX(IDN) by one should not coincide with missed MAX(UNN)
                                                        break;
                                                    }
                                                    SetGUI(true, ops.Action.Записать);
                                                    dataGridView1.DataSource = default;
                                                    break;
                                            }
                                            break;
                                    }
                                    ExecSQLPlural(string.Empty + senders[ops.Action.Записать, patient]);
                                    break;
                                case 0x05:   // concerning Вакцина
                                    ExecSQLtoSet(vaccine.GetNotExisted, out container);   // stay advised that from this point the "Формат ячеек" of cell should be "Числовой" in Excel
                                    switch (container.Count == default)
                                    {
                                        case true:
                                            reg.Record.UN = nxtUNN;
                                            break;
                                        default:   // there are some unpaired IDNs
                                            switch (string.IsNullOrEmpty(string.Empty + container.Peek()))
                                            {
                                                case true:   // UNN has nothing(IDN column is empty) to compare with
                                                    reg.Record.UN = DBNull.Value.Equals(nxtUNN) ? uint.MinValue : GetMax(uint.MinValue, nxtUNN);
                                                    break;
                                                default:
                                                    SetGUI(default, default);   // prepare for chance to output SQL result
                                                    for (; container.Count > uint.MinValue;)
                                                    {
                                                        reg.Record.UN = container.Dequeue();   // take one off
                                                        dataGridView1.DataSource = ExecSQLtoSet(string.Empty + patient[(name.Field)0xFF, ops.Action.Поиск]).DefaultView;   // SQL result
                                                        switch (GetMsg(GetTextFormatted(msg[0xED], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)   // suggest unpaired UNN to connect with IDN
                                                        {
                                                            case true:
                                                                container.Clear();   // interrupt the read-next
                                                                continue;
                                                        }
                                                        reg.Record.UN = default;   // mark off a deal with increased MAX(UNN) by one
                                                    }
                                                    switch (Equals(reg.Record.UN, default))   // the deal
                                                    {
                                                        case true:
                                                            reg.Record.UN = Convert.ToInt32(GetMax(uint.MinValue, nxtUNN)) == default ? uint.MinValue : GetMax(reg.Record.UN, nxtUNN);
                                                            reg.Record.UN = Convert.ToUInt32(reg.Record.UN).Equals(Convert.ToUInt32(nxtIDN) - 0x01) ? nxtIDN : reg.Record.UN;   // the increased MAX(UNN) by one should not coincide with missed MAX(IDN)
                                                        break;
                                                    }
                                                    SetGUI(true, ops.Action.Записать);
                                                    dataGridView1.DataSource = default;
                                                    break;
                                            }
                                            break;
                                    }
                                    ExecSQLPlural(string.Empty + senders[ops.Action.Записать, vaccine]);
                                    break;
                                default:
                                    reg.Record.UN = DBNull.Value.Equals(nxtIDN) ? GetMax(uint.MinValue, nxtUNN) : GetMax(nxtIDN, uint.MinValue);
                                    ExecSQLPlural(string.Empty + senders[ops.Action.Записать, patient], string.Empty + senders[ops.Action.Записать, vaccine]);
                                    break;
                            }
                            SetScene(GetMsg(GetTextFormatted(msg[0x00], (string.Empty + char.MinValue).ToCharArray())));
                            return;
                        default:
                            SetScene(GetMsg(GetTextFormatted(msg[0xFF], (string.Empty + char.MinValue).ToCharArray())));
                            return;
                    }
                case "Изменить":   // handle only complete record(e.g. IDN matches UNN)
                    switch (Handler((byte)ops.Action.Изменить, out _, condition, senders.Slf))
                    {
                        case true:
                            for (byte indStart = (byte)name.Field.Год - 0x01, indEnd = (byte)name.Field.ПлановаяДата; indStart < indEnd; indStart++)   // validity-check of inputs in range from Год to ПлановаяДата GUI fields
                                ConfineFields(senders.ToString(), indStart, condition);   // gather those indices of GUI input fields that are planning to be modified
                            switch (custContainer[default].Length == default)   // less than one valid GUI input field to change
                            {
                                case true:
                                    return;
                            }
                            custContainer[0x01] = senders.ToString();
                            ExecSQLtoSet(((iops.IField)patient).FindByName, out Queue container);   // seek by input - ФИО, GUI field
                            timer1.Tick += new EventHandler(SetHint);
                            for (timer1.Start(); container.Count > uint.MinValue; snap = default)   // on iteration: reset sign of hint for those GUI input fields to its default
                            {
                                reg.Record.UN = container.Dequeue();   // take one off
                                var store = ExecSQLtoSet(((iops.IField)new reg.Tab.Patient(reg.Record.UN)).FindByUN);   // fetch the complete record
                                foreach (DataRow element in store.Rows)   // element supposes to have 0x0A columns by SQL output fields
                                {
                                    for (var index = byte.MinValue; index < element.Table.Columns.Count; index++)   // on init: 0th index has relation is one-to-many
                                        SetFieldsText(index, element[index]);   // 8th index has relation is many-to-one
                                    custContainer[0x02] = textBox1.Text + separator + label1.Text + separator + textBox2.Text + separator + textBox3.Text + separator + textBox4.Text + separator + textBox5.Text + separator + textBox6.Text + separator + label2.Text + separator + label3.Text + separator + label4.Text;
                                    switch (GetMsg(GetTextFormatted(msg[0xE9], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            timer1.Stop();   // cease hinting
                                            timer1.Tick -= new EventHandler(SetHint);
                                            var objSet = new Queue();
                                            for (var fieldInd = byte.MinValue; fieldInd < (byte)name.Field.ПлановаяДата; fieldInd++)   // on init: counter for GUI index fields
                                            {
                                                for (var capacityFieldInd = (byte)(custContainer[default].Split(separator).Length - 0x01); capacityFieldInd > byte.MinValue; capacityFieldInd--)   // on init: amount of wanted GUI index fields
                                                    switch (fieldInd == Convert.ToByte(custContainer[default].Split(separator)[capacityFieldInd - 0x01]))
                                                    {
                                                        case true:   // wanted index is found
                                                            objSet.Enqueue(custContainer[0x01].Split(separator)[Convert.ToByte(custContainer[default].Split(separator)[capacityFieldInd - 0x01])]);
                                                            fieldInd++;
                                                            continue;
                                                    }
                                                objSet.Enqueue(custContainer[0x02].Split(separator)[fieldInd]);
                                            }
                                            patient = new reg.Tab.Patient(string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue());
                                            vaccine = new reg.Tab.Vaccine(string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue(), string.Empty + objSet.Dequeue());
                                            ExecSQLPlural(patient.Update, vaccine.Update);
                                            SetFieldsColour(SystemColors.Window);   // restore colour back
                                            CLS();   // reset GUI input(by user|fetched) of fields
                                            SetScene(GetMsg(GetTextFormatted(msg[0x02], (string.Empty + char.MinValue).ToCharArray())), senders);
                                            return;   // finish iterating element up here due to foreach-loop
                                        default:   // decline
                                            custContainer[0x02] = string.Empty;
                                            break;
                                    }
                                    SetFieldsColour(SystemColors.Window);   // restore colour of those GUI input fields
                                    CLS();   // reset GUI input(by user|fetched) of fields
                                    continue;
                                }
                            }
                            timer1.Stop();   // cease hinting
                            timer1.Tick -= new EventHandler(SetHint);
                            switch (string.IsNullOrEmpty(custContainer[0x02]))
                            {
                                case true:
                                    SetScene(GetMsg(GetTextFormatted(msg[0xFD], (string.Empty + char.MinValue).ToCharArray())), senders);
                                    return;
                                default:
                                    SetScene(GetMsg(GetTextFormatted(msg[0x02], (string.Empty + char.MinValue).ToCharArray())), senders);
                                    return;
                            }
                        default:
                            SetScene(GetMsg(GetTextFormatted(msg[0xFF], (string.Empty + char.MinValue).ToCharArray())));
                            return;
                    }
                case "Поиск":
                    switch (Handler((byte)ops.Action.Поиск, out sheet, null, senders.Slf, senders.BirthYear, senders.HomeAddr, senders.Company, senders.Occupation, senders.NameVaccine, senders.Serial, senders.NumbAppoint, senders.DateActual, senders.DatePlan))
                    {
                        case true:
                            switch (sheet)
                            {
                                case 0x00:   // concerning Пациент
                                    dataGridView1.DataSource = ExecSQLtoSet(string.Empty + senders[ops.Action.Поиск, patient]).DefaultView;
                                    break;
                                case 0x05:   // concerning Вакцина
                                    dataGridView1.DataSource = ExecSQLtoSet(string.Empty + senders[ops.Action.Поиск, vaccine]).DefaultView;
                                    break;
                                default:
                                    dataGridView1.DataSource = ExecSQLtoSet(string.Empty + senders[ops.Action.Поиск]).DefaultView;
                                    break;
                            }
                            break;
                    }
                    SetGUI(default, default);
                    return;
                case "Удалить":
                    switch (Handler((byte)ops.Action.Удалить, out sheet, null, senders.Slf, senders.BirthYear, senders.HomeAddr, senders.Company, senders.Occupation, senders.NameVaccine, senders.Serial, senders.NumbAppoint, senders.DateActual, senders.DatePlan))
                    {
                        case true:
                            var index = byte.MinValue;   // reserved for amount of valid criteria
                            switch (sheet)
                            {
                                case 0x00:   // Пациент record by any criterion(GUI input field)
                                    var store = new DataTable[0x05];   // on instantiation: the size of intended criteria within Пациент
                                    var listUN = string.Empty;   // despite being declared, reserved for enumerating DISTINCT(IDN)s
                                    for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                        switch (string.IsNullOrEmpty(string.Empty + senders[indField]))
                                        {
                                            case false:
                                                store[index] = ExecSQLtoSet(string.Empty + patient[indField, ops.Action.Поиск]);   // fetch Пациент record
                                                switch (store[index].Rows.Count == uint.MinValue)   // exclude nonexistent criteria
                                                {
                                                    case true:
                                                        SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criteria
                                                        SetScene(GetMsg(GetTextFormatted(msg[0xFB], (string.Empty + char.MinValue).ToCharArray())));
                                                        return;   // interrupt it once to let apply corrections
                                                }
                                                for (var number = store[index].Rows.Count - 0x01; number >= uint.MinValue; number--)   // on init: amount of received rows in current Table - SQL output
                                                    switch (listUN.Contains(string.Empty + store[index].Rows[number][string.Empty + name.Field.IDN]))   // seen such IDN, already
                                                    {
                                                        case true:
                                                            store[index].Rows.Remove(store[index].Rows[number]);   // delete(roll back) duplicate row in the Table
                                                            continue;
                                                        default:
                                                            listUN += string.Empty + store[index].Rows[number][string.Empty + name.Field.IDN] + (char)0x0020;   // add new value of IDN to list
                                                            continue;
                                                    }
                                                switch (store[index].Rows.Count == byte.MinValue)   // current Table has still leftover rows
                                                {
                                                    case false:
                                                        index++;
                                                        continue;
                                                }
                                                continue;
                                        }
                                    for (; ~index < -0x01; index--)   // on iteration: read the SQL outputs backwards by each criterion
                                        for (var number = store[index - 0x01].Rows.Count - 0x01; number >= uint.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                        {
                                            for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                                SetFieldsText((byte)(indField - 0x01), store[index - 0x01].Rows[number][string.Empty + indField]);   // display the Table on GUI input fields
                                            switch (GetMsg(GetTextFormatted(msg[0xF3], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                            {
                                                case true:
                                                    reg.Record.UN = store[index - 0x01].Rows[number][string.Empty + name.Field.IDN];
                                                    ExecSQLPlural(string.Empty + senders[ops.Action.Удалить, patient]);
                                                    CLS();   // display the action - "Удалить" on GUI
                                                    SetScene(GetMsg(GetTextFormatted(msg[0x04], (string.Empty + char.MinValue).ToCharArray())), senders);
                                                    return;
                                            }
                                        }
                                    SetScene(string.Empty, senders);
                                    break;
                                case 0x05:   // Вакцина record by any criterion(GUI input field)
                                    store = new DataTable[0x05];   // on instantiation: the size of intended criteria within Вакцина
                                    listUN = string.Empty;   // reserved for enumerating DISTINCT(UNN)s
                                    for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                        switch (string.IsNullOrEmpty(string.Empty + senders[indField]))
                                        {
                                            case false:
                                                store[index] = ExecSQLtoSet(string.Empty + vaccine[indField, ops.Action.Поиск]);   // fetch Вакцина record
                                                switch (store[index].Rows.Count == uint.MinValue)   // exclude nonexistent criteria
                                                {
                                                    case true:
                                                        SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criteria
                                                        SetScene(GetMsg(GetTextFormatted(msg[0xFB], (string.Empty + char.MinValue).ToCharArray())));
                                                        return;   // interrupt it once to let apply corrections
                                                }
                                                for (var number = store[index].Rows.Count - 0x01; number >= uint.MinValue; number--)   // on init: amount of received rows in current Table - SQL output
                                                    switch (listUN.Contains(string.Empty + store[index].Rows[number][string.Empty + name.Field.UNN]))   // seen such UNN, already
                                                    {
                                                        case true:
                                                            store[index].Rows.Remove(store[index].Rows[number]);   // delete(roll back) duplicate row in the Table
                                                            continue;
                                                        default:
                                                            listUN += string.Empty + store[index].Rows[number][string.Empty + name.Field.UNN] + (char)0x0020;   // add new value of UNN to list
                                                            continue;
                                                    }
                                                switch (store[index].Rows.Count == byte.MinValue)   // current Table has still leftover rows
                                                {
                                                    case false:
                                                        index++;
                                                        continue;
                                                }
                                                continue;
                                        }
                                    for (; ~index < -0x01; index--)   // on iteration: read the SQL outputs backwards by each criterion
                                        for (var number = store[index - 0x01].Rows.Count - 0x01; number >= uint.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                        {
                                            for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                                SetFieldsText((byte)(indField - 0x01), store[index - 0x01].Rows[number][string.Empty + indField]);   // display the Table on GUI input fields
                                            switch (GetMsg(GetTextFormatted(msg[0xF1], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                            {
                                                case true:
                                                    ExecSQLPlural(((iops.IRecord)new reg.Tab.Vaccine(store[index - 0x01].Rows[number][string.Empty + name.Field.UNN], label3.Text)).Delete);
                                                    CLS();   // display the action - "Удалить" on GUI
                                                    SetScene(GetMsg(GetTextFormatted(msg[0x04], (string.Empty + char.MinValue).ToCharArray())), senders);
                                                    return;
                                            }
                                        }
                                    SetScene(string.Empty, senders);
                                    break;
                                default:   // the complete record that belongs to single unique value by any criterion(GUI input field)
                                    store = new DataTable[0x01];   // on instantiation: the size of one criterion that consists of GUI input fields
                                    listUN = string.Empty;   // reserved for enumerating complete records DISTINCT(UN)
                                    store[index] = ExecSQLtoSet(string.Empty + senders[ops.Action.Удалить]);   // fetch complete record
                                    switch (store[index].Rows.Count == uint.MinValue)   // exclude nonexistent criteria
                                    {
                                        case true:
                                            for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                                switch (string.IsNullOrEmpty(string.Empty + senders[indField]))
                                                {
                                                    case false:
                                                        var objField = ExecSQLtoSet(string.Empty + patient[indField, ops.Action.Поиск]);
                                                        switch (objField.Rows.Count == uint.MinValue)
                                                        {
                                                            case true:   // there is no such criterion, literally
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criterion
                                                                SetScene(GetMsg(GetTextFormatted(msg[0xFB], (string.Empty + char.MinValue).ToCharArray())));
                                                                return;   // interrupt it once to let apply corrections
                                                        }
                                                        switch (ExecSQLtoSet(((iops.IField)new reg.Tab.Patient(objField.Rows[byte.MinValue][string.Empty + name.Field.IDN])).FindByUN).Rows.Count == uint.MinValue)
                                                        {
                                                            case true:   // there is no relationship with Вакцина, just
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Silver);   // mark out inconsistent criterion
                                                                SetScene(GetMsg(GetTextFormatted(msg[0xF9], (string.Empty + char.MinValue).ToCharArray())));
                                                                return;   // interrupt it once to let specify criterion
                                                        }
                                                        continue;
                                                }
                                            for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                                switch (string.IsNullOrEmpty(string.Empty + senders[indField]))
                                                {
                                                    case false:
                                                        var objField = ExecSQLtoSet(string.Empty + vaccine[indField, ops.Action.Поиск]);
                                                        switch (objField.Rows.Count == uint.MinValue)
                                                        {
                                                            case true:   // there is no such criterion, literally
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criterion
                                                                SetScene(GetMsg(GetTextFormatted(msg[0xFB], (string.Empty + char.MinValue).ToCharArray())));
                                                                return;   // interrupt it once to let apply corrections
                                                        }
                                                        switch (ExecSQLtoSet(((iops.IField)new reg.Tab.Vaccine(objField.Rows[byte.MinValue][string.Empty + name.Field.UNN], string.Empty)).FindByUN).Rows.Count == uint.MinValue)
                                                        {
                                                            case true:   // there is no relationship with Пациент, just
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Silver);   // mark out inconsistent criterion
                                                                SetScene(GetMsg(GetTextFormatted(msg[0xF9], (string.Empty + char.MinValue).ToCharArray())));
                                                                return;   // interrupt it once to let specify criterion
                                                        }
                                                        continue;
                                                }
                                            return;
                                    }
                                    for (var number = store[index].Rows.Count - 0x01; number >= uint.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                    {
                                        switch (listUN.Contains(string.Empty + store[index].Rows[number][string.Empty + name.Field.IDN]))   // seen such complete record(its unique value), already
                                        {
                                            case true:
                                                continue;
                                        }
                                        listUN += string.Empty + store[index].Rows[number][string.Empty + name.Field.IDN] + (char)0x0020;   // gather new unique values
                                        dataGridView1.DataSource = ExecSQLtoSet(((iops.IField)new reg.Tab.Patient(store[index].Rows[number][string.Empty + name.Field.IDN])).FindByUN);   // SQL result
                                        SetGUI(default, default);   // display the SQL result
                                        switch (GetMsg(GetTextFormatted(msg[0xEF], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                        {
                                            case true:
                                                reg.Record.UN = store[index].Rows[number][string.Empty + name.Field.IDN];   // supposes that IDN matches UNN as a complete record
                                                ExecSQLPlural(string.Empty + senders[ops.Action.Удалить, patient], string.Empty + senders[ops.Action.Удалить, vaccine]);
                                                SetGUI(true, ops.Action.Удалить);
                                                dataGridView1.DataSource = default;
                                                SetScene(GetMsg(GetTextFormatted(msg[0x04], (string.Empty + char.MinValue).ToCharArray())));
                                                return;
                                        }
                                    }
                                    SetGUI(true, ops.Action.Удалить);
                                    dataGridView1.DataSource = default;
                                    break;
                            }
                            return;
                        default:
                            SetScene(GetMsg(GetTextFormatted(msg[0xFF], (string.Empty + char.MinValue).ToCharArray())));
                            return;
                    }
            }
        }
        private void button1_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Empty + (char)0x0412 + (char)0x044B + (char)0x043F + (char)0x043E + (char)0x043B + (char)0x043D + (char)0x0438 + (char)0x0442 + (char)0x044C;
        }
        private void button1_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Empty;
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    for (var index = byte.MinValue; index < custContainer.Length; index++)
                        custContainer[index] = string.Empty;
                    return;
            }
        }
        private object GetMax(in object objA, in object objB)
        {
            switch (DBNull.Value.Equals(objA) || DBNull.Value.Equals(objB))   // is there uninitialized one
            {
                case true:
                    return uint.MinValue;
                default:
                    return (Convert.ToInt32(objA) >= Convert.ToInt32(objB)) ? objA : objB;
            }
        }
        private void FirstScene()
        {
            Width = width;
            Height = height;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            dataGridView1.Visible = true;
        }
        private void SecondScene()
        {
            Width = width / 0x02;
            Height = height / 0x02;
            FormBorderStyle = default;
            dataGridView1.Visible = default;
        }
        private void SetGUI(in bool swap, in ops.Action act)
        {
            switch (swap)
            {
                case true:
                    SecondScene();
                    break;
                default:
                    FirstScene();
                    break;
            }
            textBox1.Visible = swap;
            switch (textBox1.Visible)
            {
                case true:
                    textBox1.Focus();
                    break;
            }
            textBox2.Visible = swap;
            textBox3.Visible = swap;
            textBox4.Visible = swap;
            textBox5.Visible = swap;
            textBox6.Visible = swap;
            comboBox1.Visible = swap;
            dateTimePicker1.Visible = swap;
            dateTimePicker2.Visible = swap;
            dateTimePicker3.Visible = swap;
            label1.Visible = swap;
            label2.Visible = swap;
            label3.Visible = swap;
            label4.Visible = swap;
            button1.Text = string.Empty + act;
            button1.Visible = swap;
            statusStrip1.Visible = swap;
        }
        private void SetScene(in string ask)
        {
            switch (ask == string.Empty + (char)0x004E + (char)0x006F)
            {
                case true:
                    SetGUI(default, default);
                    return;
                default:   // stay on same
                    return;
            }
        }
        private void SetScene(in string ask, in reg.Record sender)
        {
            switch (ask == string.Empty + (char)0x004E + (char)0x006F)
            {
                case true:
                    SetGUI(default, default);
                    return;
                default:   // stay on same
                    for (var index = name.Field.ФИО; index <= name.Field.ПлановаяДата; index++)
                        SetFieldsText((byte)(index - 0x01), sender[index]);   // restore text of those GUI input fields that user had entered
                    return;
            }
        }
        private void SetAction(in ops.Action act)
        {
            CLS();
            SetFieldsColour(SystemColors.Window);
            SetGUI(true, act);
        }
        private void SetHint(object sender, EventArgs e)
        {
            switch (!string.IsNullOrEmpty(custContainer[default]))
            {
                case true:
                    switch (snap ^= true)
                    {
                        case false:   // off
                            SetFieldsHint(Convert.ToByte(custContainer[default].Split(separator).Length - 0x01), SystemColors.Window);   // amount of intended GUI input fields as indices. Colour supposes to be default
                            return;
                        default:   // on
                            SetFieldsHint(Convert.ToByte(custContainer[default].Split(separator).Length - 0x01), Color.Lime);   // amount of intended GUI input fields as indices. Colour supposes to hint
                            return;
                    }
            }
        }
        private void SetFieldsHint(in byte capacity, in Color colour)
        {
            for (var index = byte.MinValue; index < capacity; index++)
                hint?.Invoke(Convert.ToByte(custContainer[default].Split(separator)[index]), colour);
        }
        private OleDbConnection GetConnect()
        {
            return new OleDbConnection(dtBase);
        }
        private OleDbCommand GetCmd(in string sqlQuery, in OleDbConnection dbConn)
        {
            return new OleDbCommand(sqlQuery, dbConn);
        }
        private OleDbCommand GetCmd(in string sqlQuery, in OleDbConnection dbConn, in OleDbTransaction go)
        {
            return new OleDbCommand(sqlQuery, dbConn, go);
        }
        private string GetMsg(in string text)
        {
            return string.Empty + MessageBox.Show(text, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        private DataTable ExecSQLtoSet(in string sqlQuery)   // may be rebuild to produce at once plural DataSet tables
        {
            using (var dbConn = GetConnect())
            using (var datAdapter = new OleDbDataAdapter(sqlQuery, dbConn))   // execute query without dbConn.Open and allocate result to cache
            {
                var datSet = new DataSet();   // a store
                datAdapter.Fill(datSet);   // copy cached data to the store - DataSet
                return datSet.Tables[default(int)];   // resulting table
            }
        }
        private void ExecSQLtoSet(in string sqlQuery, out Queue container)
        {
            using (var dbConn = GetConnect())
            using (var cmd = GetCmd(sqlQuery, dbConn))
            {
                container = new Queue();
                dbConn.Open();
                using (var cmdR = cmd.ExecuteReader() as OleDbDataReader)
                    do
                        while (cmdR.Read())   // skim data
                            container.Enqueue(cmdR[default(int)]);   // put data(resulting column) into container
                    while (cmdR.NextResult());   // move on to next output column
            }
        }
        private object ExecSQLSingle(in string sqlQuery)
        {
            using (var dbConn = GetConnect())
            using (var cmd = GetCmd(sqlQuery, dbConn))
            {
                dbConn.Open();
                return cmd.ExecuteScalar();
            }
        }
        private void ExecSQLPlural(params string[] sqlQuery)
        {
            using (var dbConn = GetConnect())
            {
                dbConn.Open();
                using (var trans = dbConn.BeginTransaction() as OleDbTransaction)
                using (var cmd = GetCmd(string.Empty, dbConn, trans))
                {
                    for (var index = ushort.MinValue; index < sqlQuery.Length; index++)
                    {
                        cmd.CommandText = sqlQuery[index];   // enclose more, queries
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();   // if error then try catch and finally Rollback() transaction changes
                }
            }
        }
        private void CLS()
        {
            label1.Text = string.Empty;
            label2.Text = string.Empty;
            label3.Text = string.Empty;
            label4.Text = string.Empty;
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            textBox5.Text = string.Empty;
            textBox6.Text = string.Empty;
            dateTimePicker1.Text = string.Empty;
            dateTimePicker2.Text = string.Empty;
            dateTimePicker3.Text = string.Empty;
            toolStripStatusLabel1.Text = string.Empty;
        }
        private string GetFieldsText(in byte index, in object sender)
        {

            switch (string.Empty + sender)
            {
                case "Color [Window]":
                    return custContainer[0x01].Split(separator)[index];
                case "Color [Lime]":
                    switch (!string.IsNullOrEmpty(custContainer[0x02]))   // on action - "Изменить" the timer is launched before custContainer of second index is filled in
                    {
                        case true:
                            return custContainer[0x02].Split(separator)[index];
                    }
                    return string.Empty;
                default:
                    return string.Empty + sender;
            }
        }
        private unsafe string GetTextFormatted(in char[] text, in char[] symb)
        {
            fixed (char* letter = &text[default])
            {
                bool isSymb = default;
                for (var i = byte.MinValue; i < text.Length; i++, isSymb = default)
                    for (var j = byte.MinValue; j < symb.Length; j++)
                        switch (letter[i] == symb[j])
                        {
                            case true:
                                letter[i + 0x01] = char.ToUpper(letter[i + 0x01]);
                                isSymb = true;
                                continue;
                            default:
                                switch (isSymb == char.IsUpper(letter[i]))
                                {
                                    case true:
                                        letter[i] = char.ToLower(letter[i]);
                                        continue;
                                }
                                continue;
                        }
                return new string(letter).Trim(symb);   // trimming out those wanted characters on output
            }
        }
        private void SetFieldsText(in byte index, in object sender)
        {
            switch (index)
            {
                case 0x00:
                    switch (string.Empty + sender == "Color [Window]" || string.Empty + sender == "Color [Lime]")
                    {
                        case true:
                            textBox1.Text = custContainer[0x02].Split(separator)[index];
                            return;
                        default:
                            textBox1.Text = string.Empty + sender;
                            return;
                    }
                case 0x01:
                    label1.Text = GetFieldsText(index, sender);
                    return;
                case 0x02:
                    textBox2.Text = GetFieldsText(index, sender);
                    return;
                case 0x03:
                    textBox3.Text = GetFieldsText(index, sender);
                    return;
                case 0x04:
                    textBox4.Text = GetFieldsText(index, sender);
                    return;
                case 0x05:
                    textBox5.Text = GetFieldsText(index, sender);
                    return;
                case 0x06:
                    textBox6.Text = GetFieldsText(index, sender);
                    return;
                case 0x07:
                    label2.Text = GetFieldsText(index, sender);
                    return;
                case 0x08:
                    label3.Text = GetFieldsText(index, sender);
                    return;
                case 0x09:
                    label4.Text = GetFieldsText(index, sender);
                    return;
            }
        }
        private void SetFieldTextsColour(in byte index, in object sender)
        {
            switch (index)
            {
                case 0x00:
                    textBox1.ForeColor = (Color)sender;
                    return;
                case 0x01:
                    label1.ForeColor = (Color)sender;
                    return;
                case 0x02:
                    textBox2.ForeColor = (Color)sender;
                    return;
                case 0x03:
                    textBox3.ForeColor = (Color)sender;
                    return;
                case 0x04:
                    textBox4.ForeColor = (Color)sender;
                    return;
                case 0x05:
                    textBox5.ForeColor = (Color)sender;
                    return;
                case 0x06:
                    textBox6.ForeColor = (Color)sender;
                    return;
                case 0x07:
                    label2.ForeColor = (Color)sender;
                    return;
                case 0x08:
                    label3.ForeColor = (Color)sender;
                    return;
                case 0x09:
                    label4.ForeColor = (Color)sender;
                    return;
            }
        }
        private void SetFieldsColour(in Color colour)
        {
            textBox1.BackColor = colour;
            label1.BackColor = colour;
            textBox2.BackColor = colour;
            textBox3.BackColor = colour;
            textBox4.BackColor = colour;
            textBox5.BackColor = colour;
            textBox6.BackColor = colour;
            label2.BackColor = colour;
            label3.BackColor = colour;
            label4.BackColor = colour;
        }
        private void SetFieldsColour(in byte index, in object sender)
        {
            switch (index)
            {
                case 0x00:
                    textBox1.BackColor = (Color)sender;
                    return;
                case 0x01:
                    label1.BackColor = (Color)sender;
                    return;
                case 0x02:
                    textBox2.BackColor = (Color)sender;
                    return;
                case 0x03:
                    textBox3.BackColor = (Color)sender;
                    return;
                case 0x04:
                    textBox4.BackColor = (Color)sender;
                    return;
                case 0x05:
                    textBox5.BackColor = (Color)sender;
                    return;
                case 0x06:
                    textBox6.BackColor = (Color)sender;
                    return;
                case 0x07:
                    label2.BackColor = (Color)sender;
                    return;
                case 0x08:
                    label3.BackColor = (Color)sender;
                    return;
                case 0x09:
                    label4.BackColor = (Color)sender;
                    return;
            }
        }
        private void ConfineFields(in string container, in byte index, in char[] condition)
        {
            switch (container.ToString().Split(separator)[index].Length > byte.Parse(string.Empty + condition[index]))   // deal with length of GUI input fields
            {
                case true:
                    custContainer[default] += string.Empty + index + separator;
                    return;
            }
        }
        private bool ConfineFields(in byte indStart, in byte indEnd, in object[] sender, in char[] condition)
        {
            switch (condition)
            {
                case null:
                    return default;
                default:
                    for (var index = indStart; index < indEnd; index++)
                        switch (sender[index].ToString().Length > byte.Parse(string.Empty + condition[index]))   // deal with length of GUI input fields
                        {
                            case false:
                                SetFieldsColour(index, Color.Salmon);   // incorrect GUI input field
                                return default;   // interrupt it once
                            default:
                                continue;
                        }
                    return true;
            }
        }
        private byte CountUp(in object sender)
        {
            switch (sender.ToString().Length == default)
            {
                case true:
                    return default;   // empty GUI input field
                default:
                    return 0x01;   // not empty one
            }
        }
        private void SetFieldsSum(in object[] sender, in byte index, ref byte[] amount)
        {
            amount[default] += CountUp(sender[index]);   // sum up, literally, filled GUI input fields
            switch (index)
            {
                case 0x04:   // reached the index - Должность of GUI input field
                    amount[0x01] = amount[default];
                    return;
                case 0x09:   // reached the index - ПлановаяДата of GUI input field
                    amount[0x02] = (byte)(amount[default] - amount[0x01]);
                    return;
            }
        }
        private bool Handler(in byte act, out byte onset, in char[] condition, params object[] sender)   // not willing to re-new IWin32Window.Handle method from Control class
        {
            var msg = new inf.Message();
            var summary = new byte[0x03]
            {
                default,   // reserved for range of GUI input fields
                default,   // reserved for range of Пациент input fields
                default    // reserved for range of Вакцина input fields
            };
            for (var index = byte.MinValue; index < sender.Length; index++)
                SetFieldsSum(sender, index, ref summary);
            onset = (byte)name.Field.ПлановаяДата;   // on init: amount of GUI input fields
            switch (act)
            {
                case 0x01:
                    switch (sender.Length == onset)   // does it equal to sent GUI input fields
                    {
                        case true:
                            switch (summary[0x01] > summary[0x02])   // concerning Пациент
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF5], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            return ConfineFields(onset = (byte)name.Field.ФИО - 0x01, (byte)name.Field.Должность, sender, condition);   // determine GUI input fields within the sheet, in order
                                    }
                                    break;
                            }
                            switch (summary[0x02] > summary[0x01])   // concerning Вакцина
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF7], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            return ConfineFields(onset = (byte)name.Field.Наименование - 0x01, (byte)name.Field.ПлановаяДата, sender, condition);   // determine GUI input fields within the sheet, in order
                                    }
                                    break;
                            }
                            return ConfineFields((byte)name.Field.ФИО - 0x01, onset, sender, condition);   // determine GUI input fields, in order
                    }
                    return default;
                case 0x02:
                    switch (summary[default] == default)   // does it equal to less than ФИО GUI input field
                    {
                        case true:
                            SetFieldsColour(default, Color.Salmon);   // useless ConfineFields(onset = (byte)name.Field.ФИО - 0x01, ++summary[default], sender, condition) might be used to optimize the code
                            return default;
                        default:
                            return ConfineFields(default, summary[default], sender, condition);
                    }
                case 0x03:
                    switch (sender.Length == onset)   // does it equal to sent GUI input fields
                    {
                        case true:
                            switch (summary[0x01] > summary[0x02])   // concerning Пациент
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF5], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            onset = (byte)name.Field.ФИО - 0x01;
                                            break;
                                    }
                                    break;
                            }
                            switch (summary[0x02] > summary[0x01])   // concerning Вакцина
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF7], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            onset = (byte)name.Field.Наименование - 0x01;
                                            break;
                                    }
                                    break;
                            }
                            return true;   // let GUI input fields be empty. Useless ConfineFields(default, default, sender, condition) might be used to optimize the code
                    }
                    return default;
                case 0x04:
                    switch (sender.Length == onset)   // does it equal to sent GUI input fields
                    {
                        case true:
                            switch (summary[0x01] > summary[0x02])   // concerning Пациент
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF5], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            onset = (byte)name.Field.ФИО - 0x01;
                                            break;
                                    }
                                    break;
                            }
                            switch (summary[0x02] > summary[0x01])   // concerning Вакцина
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(msg[0xF7], (string.Empty + char.MinValue).ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            onset = (byte)name.Field.Наименование - 0x01;
                                            break;
                                    }
                                    break;
                            }
                            return (summary[default] != default) & true;   // do not let GUI input fields be empty at all
                    }
                    return default;
                default:   // unknown action
                    return default;
            }
        }
        private bool GetKBRDSet(in KeyPressEventArgs e, params char[] sender)
        {
            switch (sender)
            {
                case var _ when sender.Length == default && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar):
                    return true;
                case var _ when sender.Length == 0x01 && !char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && e.KeyChar != sender[default]:
                    return true;
                case var _ when sender.Length == 0x02 && !char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && e.KeyChar != sender[default]:   // reserved for GUI input field - Наименование
                    return true;
                case var _ when sender.Length == 0x03 && !char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && e.KeyChar != sender[default] && e.KeyChar != sender[0x01] && e.KeyChar != sender[0x02]:
                    return true;
                default:
                    return default;
            }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45);
        }
        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagSlf, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(default, SystemColors.Window);
            SetFieldTextsColour(default, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagSlf, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
            switch (text.Length == default)
            {
                case true:
                    return;
            }
            SetFieldsText(default, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x0020 + (char)0x002D).ToCharArray()));
        }
        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x01, SystemColors.Window);
            SetFieldTextsColour(0x01, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagBirthYear, (string.Empty + char.MinValue).ToCharArray());
            label1.Visible = default;
        }
        private void dateTimePicker1_Leave(object sender, EventArgs e)
        {
            label1.Text = $"{dateTimePicker1.Value.Year}";
            label1.Visible = true;
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45, (char)47, (char)46);
        }
        private void textBox2_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagHomeAddr, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox2_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x02, SystemColors.Window);
            SetFieldTextsColour(0x02, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagHomeAddr, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox2_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x02, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x0020).ToCharArray()));
                    return;
            }
        }
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45, (char)47, (char)46);
        }
        private void textBox3_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagCompany, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox3_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x03, SystemColors.Window);
            SetFieldTextsColour(0x03, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagCompany, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox3_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x03, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x0020).ToCharArray()));
                    return;
            }
        }
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45);
        }
        private void textBox4_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagOccupation, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox4_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x04, SystemColors.Window);
            SetFieldTextsColour(0x04, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagOccupation, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox4_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x04, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x002D).ToCharArray()));
                    return;
            }
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x04, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x0020).ToCharArray()));
                    return;
            }
        }
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45, (char)45);
        }
        private void textBox5_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagName, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox5_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x05, SystemColors.Window);
            SetFieldTextsColour(0x05, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagName, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox5_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x05, GetTextFormatted(text.ToCharArray(), (string.Empty + (char)0x0020).ToCharArray()));
                    return;
            }
        }
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e);
        }
        private void textBox6_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagSerial, (string.Empty + char.MinValue).ToCharArray());
        }
        private void textBox6_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x06, SystemColors.Window);
            SetFieldTextsColour(0x06, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagSerial, (string.Empty + char.MinValue).ToCharArray());
        }
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            SetFieldsColour(0x07, SystemColors.Window);
            SetFieldTextsColour(0x07, SystemColors.WindowText);
            label2.Text = comboBox1.Text;   // on init: to measure Text.Length of the further event
        }
        private void comboBox1_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagNumbAppoint, (string.Empty + char.MinValue).ToCharArray());
            label2.Visible = default;
        }
        private void comboBox1_Leave(object sender, EventArgs e)
        {
            label2.Text = comboBox1.Text;
            label2.Visible = true;
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    switch (label3.Text.Length == default)
                    {
                        case true:
                            label4.Text = string.Empty;   // changed its mind about date assignment
                            return;
                        default:   // a date was picked up before, already
                            switch (comboBox1.Text.Length)
                            {
                                case 0x00:
                                    label3.Text = string.Empty;   // changed its mind about picking up a date
                                    label4.Text = string.Empty;   // changed its mind about date assignment
                                    return;
                                case 0x04:
                                    label4.Text = label3.Text;
                                    return;
                                default:
                                    byte tmp = 0x15;   // on init: days after which are planned to repeat again
                                    label4.Text = Convert.ToDateTime($"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}").ToString().Substring(0x00, 0x0A);   // keep leading zeros in front of day/month
                                    return;
                            }
                    }
            }
        }
        private void dateTimePicker2_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDateActual, (string.Empty + char.MinValue).ToCharArray());
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    switch (comboBox1.Text.Length == default)
                    {
                        case true:
                            label3.Visible = true;
                            return;
                    }
                    break;
            }
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    label3.Visible = true;   // should not be redone
                    return;
            }
            label3.Visible = default;
        }
        private void dateTimePicker2_Leave(object sender, EventArgs e)
        {
            SetFieldsColour(0x08, SystemColors.Window);
            SetFieldTextsColour(0x08, SystemColors.WindowText);
            label3.Text = dateTimePicker2.Text;
            label3.Visible = true;
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    switch (comboBox1.Text.Length)
                    {
                        case 0x00:
                            label3.Text = string.Empty;
                            return;
                        case 0x04:
                            label4.Text = dateTimePicker2.Text;
                            return;
                        default:
                            byte tmp = 0x15;   // on init: days after which are planned to repeat again
                            label4.Text = Convert.ToDateTime($"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}").ToString().Substring(0x00, 0x0A);   // keep leading zeros in front of day/month
                            return;
                    }
            }
            switch (button1.Text == string.Empty + ops.Action.Изменить)
            {
                case true:
                    label3.Text = string.Empty;   // reserved for additional uniqueness(right after UNN)
                    return;
            }
        }
        private void dateTimePicker3_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDatePlan, (string.Empty + char.MinValue).ToCharArray());
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    switch (comboBox1.Text.Length)
                    {
                        case 0x00:
                            label4.Text = string.Empty;
                            return;
                        case 0x04:
                            label4.Text = label3.Text;
                            return;
                        default:
                            byte tmp = 0x15;   // on init: days after which are planned to repeat again
                            label4.Text = Convert.ToDateTime($"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}").ToString().Substring(0x00, 0x0A);   // keep leading zeros in front of day/month
                            return;
                    }
            }
            label4.Visible = default;
        }
        private void dateTimePicker3_Leave(object sender, EventArgs e)
        {
            SetFieldsColour(0x09, SystemColors.Window);
            SetFieldTextsColour(0x09, SystemColors.WindowText);
            switch (button1.Text == string.Empty + ops.Action.Записать)
            {
                case true:
                    return;
            }
            label4.Text = dateTimePicker3.Text;
            label4.Visible = true;
        }
        private void label1_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagBirthYear, (string.Empty + char.MinValue).ToCharArray());
        }
        private void label2_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagNumbAppoint, (string.Empty + char.MinValue).ToCharArray());
        }
        private void label3_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDateActual, (string.Empty + char.MinValue).ToCharArray());
        }
        private void label4_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDatePlan, (string.Empty + char.MinValue).ToCharArray());
        }
    }
}