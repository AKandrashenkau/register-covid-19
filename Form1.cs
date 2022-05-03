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

namespace REGOVID
{
    partial class Form1 : Form
    {
        private readonly Timer timer1;
        private readonly int width;    // reserved for working on initialized clientWidth
        private readonly int height;   // reserved for working on initialized clientHeight
        private readonly string dtBase;
        private readonly string[] custContainer;   // serves for action - "Изменить" to gather indexes, GUI input fields and database output
        private const char nil = (char)0;
        private const char separator = (char)59;   // divides customized container into human-readable view that is consisting of GUI input fields
        private delegate void Action(in byte index, in object sender);
        private readonly Action hint;   // serves for action - "Изменить" to indicate where data amendment is applied
        private bool hintSign;   // serves for action - "Изменить"
        public Form1()
        {
            dtBase = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{Environment.GetEnvironmentVariable(string.Empty + (char)0x0050 + (char)0x0072 + (char)0x006F + (char)0x0067 + (char)0x0072 + (char)0x0061 + (char)0x006D +
                        (char)0x0044 + (char)0x0061 + (char)0x0074 + (char)0x0061) + (char)0x005C + (char)0x0056 + (char)0x0069 + (char)0x006C + (char)0x0043 + (char)0x0052 + (char)0x0042 + (char)0x005C + (char)0x0064 +
                        (char)0x0074 + (char)0x0042 + (char)0x0061 + (char)0x0073 + (char)0x0065 + (char)0x002E + (char)0x0078 + (char)0x006C + (char)0x0073 + (char)0x0062}';Extended Properties='Excel 12.0;HDR=Yes'";
            hint = SetFieldsColour;
            hint += SetFieldsText;
            custContainer = new string[0x03]
            {
                string.Empty,   // on action - "Изменить" for taking indexes of GUI input fields from user
                string.Empty,   // on action - "Изменить" for taking text of GUI input fields from user
                string.Empty    // on action - "Изменить" for taking complete record - fields from database
            };
            InitializeComponent();
            timer1 = new Timer(components)
            {
                Interval = 0x01F4   // a half of 0x01 sec
            };
            width = Math.Abs(Width);
            height = Math.Abs(Height);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Tick += new EventHandler(SetHint);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Tick -= new EventHandler(SetHint);   // in case of window-prompt the snippet of code should move on to FormClosed event
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
            switch (sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Split((char)0x0020)[0x01])   // determine action by GUI item
            {
                case "Записать":
                    switch (Handler((byte)ops.Action.Записать, out byte sheet, condition, senders.Slf, senders.BirthYear, senders.HomeAddr, senders.Company, senders.Occupation, senders.NameVaccine, senders.Serial, senders.NumbAppoint, senders.DateActual, senders.DatePlan))
                    {
                        case true:
                            switch (sheet)
                            {
                                case 0x00:   // concerning Пациент
                                    ExecSQLtoSet(patient.GetNotExisted, out Queue container);
                                    switch (string.IsNullOrEmpty(container.Peek().ToString()))
                                    {
                                        case true:   // IDN has nothing(UNN column is empty) to compare with
                                            reg.Record.UN = ExecSQLSingle(patient.GetNext);   // increase MAX(IDN) by one
                                            break;
                                        default:
                                            SetGUI(default, default);   // prepare for chance to output SQL result
                                            for (var IDN = ExecSQLSingle(patient.GetNext); container.Count > byte.MinValue;)   // on init: increase MAX(IDN) by one
                                            {
                                                reg.Record.UN = container.Dequeue();   // take one off
                                                dataGridView1.DataSource = ExecSQLtoSet(vaccine[(name.Field)0xFE, ops.Action.Поиск].ToString()).DefaultView;   // SQL result
                                                switch (GetMsg(string.Empty + (char)0x0421 + (char)0x043E + (char)0x0435 + (char)0x0434 + (char)0x0438 + (char)0x043D + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x0441 +   // Suggest unpaired IDN to connect with UNN
                                                               (char)0x0020 + (char)0x0438 + (char)0x043D + (char)0x0444 + (char)0x043E + (char)0x0440 + (char)0x043C + (char)0x0430 + (char)0x0446 + (char)0x0438 + (char)0x0435 + (char)0x0439 +
                                                               (char)0x0020 + (char)0x043E + (char)0x0020 + (char)0x0432 + (char)0x0430 + (char)0x043A + (char)0x0446 + (char)0x0438 + (char)0x043D + (char)0x0435 + (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                                {
                                                    case true:
                                                        container.Clear();   // interrupt the read-next
                                                        break;
                                                    default:   // initialize the increased IDN
                                                        reg.Record.UN = byte.MinValue + string.Empty + IDN;   // IDN column might have no numbers. Be advised that from this point the "Формат ячеек" of cell should be "Числовой" in Excel
                                                        break;
                                                }
                                            }
                                            SetGUI(true, ops.Action.Записать);
                                            dataGridView1.DataSource = default;
                                            break;
                                    }
                                    ExecSQLPlural(senders[ops.Action.Записать, patient].ToString());
                                    break;
                                case 0x05:   // concerning Вакцина
                                    ExecSQLtoSet(vaccine.GetNotExisted, out container);
                                    switch (string.IsNullOrEmpty(container.Peek().ToString()))
                                    {
                                        case true:   // UNN has nothing(IDN column is empty) to compare with
                                            reg.Record.UN = ExecSQLSingle(vaccine.GetNext);   // increase MAX(UNN) by one
                                            break;
                                        default:
                                            SetGUI(default, default);   // prepare for chance to output SQL result
                                            for (var UNN = ExecSQLSingle(vaccine.GetNext); container.Count > byte.MinValue;)   // on init: increase MAX(UNN) by one
                                            {
                                                reg.Record.UN = container.Dequeue();   // take one off
                                                dataGridView1.DataSource = ExecSQLtoSet(patient[(name.Field)0xFF, ops.Action.Поиск].ToString()).DefaultView;   // SQL result
                                                switch (GetMsg(string.Empty + (char)0x0421 + (char)0x043E + (char)0x0435 + (char)0x0434 + (char)0x0438 + (char)0x043D + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x0441 +   // suggest unpaired UNN to connect with IDN
                                                               (char)0x0020 + (char)0x0434 + (char)0x0430 + (char)0x043D + (char)0x043D + (char)0x044B + (char)0x043C + (char)0x0438 + (char)0x0020 + (char)0x043F + (char)0x0430 + (char)0x0446 +
                                                               (char)0x0438 + (char)0x0435 + (char)0x043D + (char)0x0442 + (char)0x0430 + (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                                {
                                                    case true:
                                                        container.Clear();   // interrupt the read-next
                                                        break;
                                                    default:   // initialize the increased UNN
                                                        reg.Record.UN = byte.MinValue + string.Empty + UNN;   // UNN column might have no numbers. Be advised that from this point the "Формат ячеек" of cell should be "Числовой" in Excel
                                                        break;
                                                }
                                            }
                                            SetGUI(true, ops.Action.Записать);
                                            dataGridView1.DataSource = default;
                                            break;
                                    }
                                    ExecSQLPlural(senders[ops.Action.Записать, vaccine].ToString());
                                    break;
                                default:
                                    reg.Record.UN = GetMax(ExecSQLSingle(patient.GetNext), ExecSQLSingle(vaccine.GetNext));   // equate common unique number to the highest one
                                    ExecSQLPlural(senders[ops.Action.Записать, patient].ToString(), senders[ops.Action.Записать, vaccine].ToString());
                                    break;
                            }
                            SetScene(GetMsg(string.Empty + (char)0x0417 + (char)0x0430 + (char)0x043F + (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x0020 + (char)0x0443 + (char)0x0441 + (char)0x043F + (char)0x0435 + (char)0x0448 +
                                                (char)0x043D + (char)0x043E + (char)0x0020 + (char)0x0434 + (char)0x043E + (char)0x0431 + (char)0x0430 + (char)0x0432 + (char)0x043B + (char)0x0435 + (char)0x043D + (char)0x0430 + (char)0x002C +
                                                (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x003F));
                            return;
                        default:
                            SetScene(GetMsg(string.Empty + (char)0x0412 + (char)0x043E + (char)0x0437 + (char)0x043D + (char)0x0438 + (char)0x043A + (char)0x043B + (char)0x0438 + (char)0x0020 + (char)0x0442 + (char)0x0440 + (char)0x0443 +
                                                (char)0x0434 + (char)0x043D + (char)0x043E + (char)0x0441 + (char)0x0442 + (char)0x0438 + (char)0x0020 + (char)0x0437 + (char)0x0430 + (char)0x043F + (char)0x0438 + (char)0x0441 + (char)0x0430 +
                                                (char)0x0442 + (char)0x044C + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 + (char)0x0442 +
                                                (char)0x044C + (char)0x003F));
                            return;
                    }
                case "Изменить":   // handle only complete record(e.g. IDN matches UNN)
                    switch (Handler((byte)ops.Action.Изменить, out _, condition, senders.Slf))
                    {
                        case true:
                            for (byte indStart = (byte)name.Field.Год - 0x01, indEnd = (byte)name.Field.ПлановаяДата; indStart < indEnd; indStart++)   // validity-check of inputs in range from Год to ПлановаяДата GUI fields
                                ConfineFields(senders.ToString(), indStart, condition);   // gather those indexes of GUI input fields that are planning to be modified, otherwise ignore input from the range of fields. No system interrupts as consequence
                            custContainer[0x01] = senders.ToString();
                            ExecSQLtoSet(((iops.IField)patient).FindByName, out Queue container);   // seek by input - ФИО, GUI field
                            for (timer1.Start(); container.Count > uint.MinValue; hintSign = default)   // on iteration: reset sign of hint for those GUI input fields to its default
                            {
                                var store = ExecSQLtoSet(((iops.IField)new reg.Tab.Patient(container.Dequeue())).FindByUN);   // fetch the complete record
                                foreach (DataRow element in store.Rows)   // element supposes to have 0x0A columns by SQL output fields
                                {
                                    for (var index = byte.MinValue; index < element.Table.Columns.Count; index++)   // on init: 0th index has relation is one-to-many
                                        SetFieldsText(index, element[index]);   // 8th index has relation is many-to-one
                                    custContainer[0x02] = textBox1.Text + separator + label1.Text + separator + textBox2.Text + separator + textBox3.Text + separator + textBox4.Text + separator + textBox5.Text + separator + textBox6.Text + separator + label2.Text + separator + label3.Text + separator + label4.Text;
                                    switch (GetMsg(string.Empty + (char)0x041F + (char)0x0440 + (char)0x0438 + (char)0x043C + (char)0x0435 + (char)0x043D + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x044D +
                                                   (char)0x0442 + (char)0x043E + (char)0x0020 + (char)0x0438 + (char)0x0437 + (char)0x043C + (char)0x0435 + (char)0x043D + (char)0x0435 + (char)0x043D + (char)0x0438 + (char)0x0435 +
                                                   (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            timer1.Stop();   // cease hinting
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
                                            patient = new reg.Tab.Patient(objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString());
                                            vaccine = new reg.Tab.Vaccine(objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString(), objSet.Dequeue().ToString());
                                            ExecSQLPlural(patient.Update, vaccine.Update);
                                            SetFieldsColour(SystemColors.Window);   // restore colour back
                                            CLS();   // reset GUI input(by user|fetched) of fields
                                            SetScene(GetMsg(string.Empty + (char)0x0417 + (char)0x0430 + (char)0x043F + (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x0020 + (char)0x0443 + (char)0x0441 + (char)0x043F +
                                                            (char)0x0435 + (char)0x0448 + (char)0x043D + (char)0x043E + (char)0x0020 + (char)0x0438 + (char)0x0441 + (char)0x043F + (char)0x0440 + (char)0x0430 + (char)0x0432 +
                                                            (char)0x043B + (char)0x0435 + (char)0x043D + (char)0x0430 + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E +
                                                            (char)0x043B + (char)0x0436 + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x003F), senders);
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
                            switch (string.IsNullOrEmpty(custContainer[0x02]))
                            {
                                case true:
                                    SetScene(GetMsg(string.Empty + (char)0x041D + (char)0x0438 + (char)0x0447 + (char)0x0435 + (char)0x0433 + (char)0x043E + (char)0x0020 + (char)0x043D + (char)0x0435 + (char)0x0020 + (char)0x043D +
                                                    (char)0x0430 + (char)0x0439 + (char)0x0434 + (char)0x0435 + (char)0x043D + (char)0x043E + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 +
                                                    (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x003F), senders);
                                    return;
                                default:
                                    SetScene(GetMsg(string.Empty + (char)0x0417 + (char)0x0430 + (char)0x043F + (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x0020 + (char)0x0443 + (char)0x0441 + (char)0x043F + (char)0x0435 +
                                                    (char)0x0448 + (char)0x043D + (char)0x043E + (char)0x0020 + (char)0x0438 + (char)0x0441 + (char)0x043F + (char)0x0440 + (char)0x0430 + (char)0x0432 + (char)0x043B + (char)0x0435 +
                                                    (char)0x043D + (char)0x0430 + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 +
                                                    (char)0x0442 + (char)0x044C + (char)0x003F), senders);
                                    return;
                            }
                        default:
                            SetScene(GetMsg(string.Empty + (char)0x0412 + (char)0x043E + (char)0x0437 + (char)0x043D + (char)0x0438 + (char)0x043A + (char)0x043B + (char)0x0438 + (char)0x0020 + (char)0x043D + (char)0x0435 + (char)0x043A +
                                            (char)0x043E + (char)0x0442 + (char)0x043E + (char)0x0440 + (char)0x044B + (char)0x0435 + (char)0x0020 + (char)0x0442 + (char)0x0440 + (char)0x0443 + (char)0x0434 + (char)0x043D + (char)0x043E +
                                            (char)0x0441 + (char)0x0442 + (char)0x0438 + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 +
                                            (char)0x0442 + (char)0x044C + (char)0x003F));
                            return;
                    }
                case "Поиск":
                    switch (Handler((byte)ops.Action.Поиск, out sheet, null, senders.Slf, senders.BirthYear, senders.HomeAddr, senders.Company, senders.Occupation, senders.NameVaccine, senders.Serial, senders.NumbAppoint, senders.DateActual, senders.DatePlan))
                    {
                        case true:
                            switch (sheet)
                            {
                                case 0x00:   // concerning Пациент
                                    dataGridView1.DataSource = ExecSQLtoSet(senders[ops.Action.Поиск, patient].ToString()).DefaultView;
                                    break;
                                case 0x05:   // concerning Вакцина
                                    dataGridView1.DataSource = ExecSQLtoSet(senders[ops.Action.Поиск, vaccine].ToString()).DefaultView;
                                    break;
                                default:
                                    dataGridView1.DataSource = ExecSQLtoSet(senders[ops.Action.Поиск].ToString()).DefaultView;
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
                            var tabWarn = new char[0x23] { (char)0x0417, (char)0x0430, (char)0x043F, (char)0x0438, (char)0x0441, (char)0x044C, (char)0x0020, (char)0x0443, (char)0x0441, (char)0x043F, (char)0x0435, (char)0x0448, (char)0x043D,
                                                           (char)0x043E, (char)0x0020, (char)0x0443, (char)0x0434, (char)0x0430, (char)0x043B, (char)0x0435, (char)0x043D, (char)0x0430, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440,
                                                           (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                            var tabWarnField = new char[0x1E] { (char)0x0423, (char)0x0442, (char)0x043E, (char)0x0447, (char)0x043D, (char)0x0438, (char)0x0442, (char)0x0435, (char)0x0020, (char)0x043A, (char)0x0440, (char)0x0438,
                                                                (char)0x0442, (char)0x0435, (char)0x0440, (char)0x0438, (char)0x0439, (char)0x002C, (char)0x0020, (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E,
                                                                (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                            var tabWarnSheet = new char[0x25] { (char)0x041A, (char)0x043E, (char)0x043D, (char)0x043A, (char)0x0440, (char)0x0435, (char)0x0442, (char)0x0438, (char)0x0437, (char)0x0438, (char)0x0440, (char)0x0443, (char)0x0439,
                                                                (char)0x0442, (char)0x0435, (char)0x0020, (char)0x043A, (char)0x0440, (char)0x0438, (char)0x0442, (char)0x0435, (char)0x0440, (char)0x0438, (char)0x0439, (char)0x002C, (char)0x0020,
                                                                (char)0x043F, (char)0x0440, (char)0x043E, (char)0x0434, (char)0x043E, (char)0x043B, (char)0x0436, (char)0x0438, (char)0x0442, (char)0x044C, (char)0x003F };
                            var index = byte.MinValue;   // reserved for amount of valid criteria
                            switch (sheet)
                            {
                                case 0x00:   // Пациент record by any criterion(GUI input field)
                                    var listUN = string.Empty;   // despite being declared, reserved for enumerating DISTINCT(IDN)s
                                    var store = new DataTable[0x05];   // on instantiation: the size of intended criteria within Пациент
                                    for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                        switch (string.IsNullOrEmpty(senders[indField].ToString()))
                                        {
                                            case false:
                                                store[index] = ExecSQLtoSet(patient[indField, ops.Action.Поиск].ToString());   // fetch Пациент record
                                                switch (store[index].Rows.Count == byte.MinValue)   // exclude non-existent criteria
                                                {
                                                    case true:
                                                        SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criteria
                                                        SetScene(GetMsg(GetTextFormatted(tabWarnField, nil.ToString().ToCharArray())));
                                                        return;   // interrupt it once to let apply corrections
                                                }
                                                for (var number = store[index].Rows.Count - 0x01; number >= byte.MinValue; number--)   // on init: amount of received rows in current Table - SQL output
                                                    switch (listUN.Contains(store[index].Rows[number][name.Field.IDN.ToString()].ToString()))   // seen such IDN, already
                                                    {
                                                        case true:
                                                            store[index].Rows.Remove(store[index].Rows[number]);   // delete(roll back) duplicate row in the Table
                                                            continue;
                                                        default:
                                                            listUN += store[index].Rows[number][name.Field.IDN.ToString()].ToString() + (char)0x0020;   // add new value of IDN to list
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
                                        for (var number = store[index - 0x01].Rows.Count - 0x01; number >= byte.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                        {
                                            for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                                SetFieldsText((byte)(indField - 0x01), store[index - 0x01].Rows[number][indField.ToString()]);   // display the Table on GUI input fields
                                            switch (GetMsg(string.Empty + (char)0x0423 + (char)0x0434 + (char)0x0430 + (char)0x043B + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x0437 + (char)0x0430 + (char)0x043F +
                                                           (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x0020 + (char)0x043F + (char)0x0430 + (char)0x0446 + (char)0x0438 + (char)0x0435 + (char)0x043D + (char)0x0442 + (char)0x0430 + (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                            {
                                                case true:
                                                    reg.Record.UN = store[index - 0x01].Rows[number][name.Field.IDN.ToString()];
                                                    ExecSQLPlural(senders[ops.Action.Удалить, patient].ToString());
                                                    CLS();   // display the action - "Удалить" on GUI
                                                    SetScene(GetMsg(GetTextFormatted(tabWarn, nil.ToString().ToCharArray())), senders);
                                                    return;
                                            }
                                        }
                                    break;
                                case 0x05:   // Вакцина record by any criterion(GUI input field)
                                    store = new DataTable[0x05];   // on instantiation: the size of intended criteria within Вакцина
                                    listUN = string.Empty;   // reserved for enumerating DISTINCT(UNN)s
                                    for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                        switch (string.IsNullOrEmpty(senders[indField].ToString()))
                                        {
                                            case false:
                                                store[index] = ExecSQLtoSet(vaccine[indField, ops.Action.Поиск].ToString());   // fetch Вакцина record
                                                switch (store[index].Rows.Count == byte.MinValue)   // exclude non-existent criteria
                                                {
                                                    case true:
                                                        SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criteria
                                                        SetScene(GetMsg(GetTextFormatted(tabWarnField, nil.ToString().ToCharArray())));
                                                        return;   // interrupt it once to let apply corrections
                                                }
                                                for (var number = store[index].Rows.Count - 0x01; number >= byte.MinValue; number--)   // on init: amount of received rows in current Table - SQL output
                                                    switch (listUN.Contains(store[index].Rows[number][name.Field.UNN.ToString()].ToString()))   // seen such UNN, already
                                                    {
                                                        case true:
                                                            store[index].Rows.Remove(store[index].Rows[number]);   // delete(roll back) duplicate row in the Table
                                                            continue;
                                                        default:
                                                            listUN += store[index].Rows[number][name.Field.UNN.ToString()].ToString() + (char)0x0020;   // add new value of IDN to list
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
                                        for (var number = store[index - 0x01].Rows.Count - 0x01; number >= byte.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                        {
                                            for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                                SetFieldsText((byte)(indField - 0x01), store[index - 0x01].Rows[number][indField.ToString()]);   // display the Table on GUI input fields
                                            switch (GetMsg(string.Empty + (char)0x0423 + (char)0x0434 + (char)0x0430 + (char)0x043B + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x0437 + (char)0x0430 + (char)0x043F +
                                                           (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x0020 + (char)0x0432 + (char)0x0430 + (char)0x043A + (char)0x0446 + (char)0x0438 + (char)0x043D + (char)0x044B + (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                            {
                                                case true:
                                                    ExecSQLPlural(new reg.Tab.Vaccine(store[index - 0x01].Rows[number][name.Field.UNN.ToString()], label3.Text).Delete);
                                                    CLS();   // display the action - "Удалить" on GUI
                                                    SetScene(GetMsg(GetTextFormatted(tabWarn, nil.ToString().ToCharArray())), senders);
                                                    return;
                                            }
                                        }
                                    break;
                                default:   // the complete record that holds one(at once) unique number by any criterion(GUI input field)
                                    store = new DataTable[0x01];   // on instantiation: the size of one criterion that consists of GUI input fields
                                    store[index] = ExecSQLtoSet(senders[ops.Action.Удалить].ToString());   // fetch complete record
                                    switch (store[index++].Rows.Count == byte.MinValue)   // exclude non-existent criteria
                                    {
                                        case true:
                                            for (var indField = name.Field.ФИО; indField <= name.Field.Должность; indField++)
                                                switch (string.IsNullOrEmpty(senders[indField].ToString()))
                                                {
                                                    case false:
                                                        var objField = ExecSQLtoSet(patient[indField, ops.Action.Поиск].ToString());
                                                        switch (objField.Rows.Count == byte.MinValue)
                                                        {
                                                            case true:   // there is no such criterion, literally
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criterion
                                                                SetScene(GetMsg(GetTextFormatted(tabWarnField, nil.ToString().ToCharArray())));
                                                                return;   // interrupt it once to let apply corrections
                                                        }
                                                        switch (ExecSQLtoSet(((iops.IField)new reg.Tab.Patient(objField.Rows[byte.MinValue][name.Field.IDN.ToString()])).FindByUN).Rows.Count == byte.MinValue)
                                                        {
                                                            case true:   // there is no relationship with Вакцина, just
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Silver);   // mark out inconsistent criterion
                                                                SetScene(GetMsg(GetTextFormatted(tabWarnSheet, nil.ToString().ToCharArray())));
                                                                return;   // interrupt it once to let specify criterion
                                                        }
                                                        continue;
                                                }
                                            for (var indField = name.Field.Наименование; indField <= name.Field.ПлановаяДата; indField++)
                                                switch (string.IsNullOrEmpty(senders[indField].ToString()))
                                                {
                                                    case false:
                                                        var objField = ExecSQLtoSet(vaccine[indField, ops.Action.Поиск].ToString());
                                                        switch (objField.Rows.Count == byte.MinValue)
                                                        {
                                                            case true:
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Salmon);   // mark out invalid criterion
                                                                SetScene(GetMsg(GetTextFormatted(tabWarnField, nil.ToString().ToCharArray())));
                                                                return;   // interrupt it once to let apply corrections
                                                        }
                                                        switch (ExecSQLtoSet(((iops.IField)new reg.Tab.Vaccine(objField.Rows[byte.MinValue][name.Field.UNN.ToString()], string.Empty)).FindByUN).Rows.Count == byte.MinValue)
                                                        {
                                                            case true:   // there is no relationship with Пациент, just
                                                                SetFieldTextsColour((byte)(indField - 0x01), Color.Silver);   // mark out inconsistent criterion
                                                                SetScene(GetMsg(GetTextFormatted(tabWarnSheet, nil.ToString().ToCharArray())));
                                                                return;   // interrupt it once to let specify criterion
                                                        }
                                                        continue;
                                                }
                                            return;
                                    }
                                    for (; ~index < -0x01; index--)   // on iteration: read the SQL outputs backwards by each criterion
                                        for (var number = store[index - 0x01].Rows.Count - 0x01; number >= byte.MinValue; number--)   // on init: amount of received rows in certain Table - SQL output
                                        {
                                            for (var indField = name.Field.ФИО; indField <= name.Field.ПлановаяДата; indField++)
                                                SetFieldsText((byte)(indField - 0x01), store[index - 0x01].Rows[number][indField.ToString()]);   // display the Table on GUI input fields
                                            switch (GetMsg(string.Empty + (char)0x0423 + (char)0x0434 + (char)0x0430 + (char)0x043B + (char)0x0438 + (char)0x0442 + (char)0x044C + (char)0x0020 + (char)0x0437 + (char)0x0430 + (char)0x043F +
                                                           (char)0x0438 + (char)0x0441 + (char)0x044C + (char)0x003F) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                            {
                                                case true:
                                                    reg.Record.UN = store[index - 0x01].Rows[number][name.Field.IDN.ToString()];   // supposes that IDN matches UNN as a complete record
                                                    ExecSQLPlural(senders[ops.Action.Удалить, patient].ToString(), ((iops.IRecord)new reg.Tab.Vaccine(reg.Record.UN, label3.Text)).Delete);
                                                    CLS();   // display the action - "Удалить" on GUI
                                                    SetScene(GetMsg(GetTextFormatted(tabWarn, nil.ToString().ToCharArray())), senders);
                                                    return;
                                            }
                                        }
                                    break;
                            }
                            for (var indField = name.Field.ФИО; indField <= name.Field.ПлановаяДата; indField++)
                                SetFieldsText((byte)(indField - 0x01), senders[indField]);   // restore text of those GUI input fields that user had entered. Useless SetScene(string.Empty, senders) might be used to optimize the code
                            return;
                        default:
                            SetScene(GetMsg(string.Empty + (char)0x0412 + (char)0x043E + (char)0x0437 + (char)0x043D + (char)0x0438 + (char)0x043A + (char)0x043B + (char)0x0438 + (char)0x0020 + (char)0x043D + (char)0x0435 + (char)0x043A +
                                            (char)0x043E + (char)0x0442 + (char)0x043E + (char)0x0440 + (char)0x044B + (char)0x0435 + (char)0x0020 + (char)0x0442 + (char)0x0440 + (char)0x0443 + (char)0x0434 + (char)0x043D + (char)0x043E +
                                            (char)0x0441 + (char)0x0442 + (char)0x0438 + (char)0x002C + (char)0x0020 + (char)0x043F + (char)0x0440 + (char)0x043E + (char)0x0434 + (char)0x043E + (char)0x043B + (char)0x0436 + (char)0x0438 +
                                            (char)0x0442 + (char)0x044C + (char)0x003F));
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
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    for (var index = byte.MinValue; index < custContainer.Length; index++)
                        custContainer[index] = string.Empty;
                    return;
            }
        }
        private object GetMax(in object objA, in object objB)
        {
            switch (Convert.ToUInt32(objA) >= Convert.ToUInt32(objB))
            {
                case true:
                    return objA;
                default:
                    return objB;
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
            button1.Text = act.ToString();
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
                    switch (hintSign ^= true)
                    {
                        case false:   // off
                            SetFieldsHint(Convert.ToByte(custContainer[default].Split(separator).Length - 0x01), SystemColors.Window);   // amount of intended GUI input fields as indexes. Colour supposes to be default
                            return;
                        default:   // on
                            SetFieldsHint(Convert.ToByte(custContainer[default].Split(separator).Length - 0x01), Color.Lime);   // amount of intended GUI input fields as indexes. Colour supposes to hint
                            return;
                    }
            }
        }
        private void SetFieldsHint(in byte capacity, in Color colour)
        {
            for (var index = byte.MinValue; index < capacity; index++)
            {
                hint?.Invoke(Convert.ToByte(custContainer[default].Split(separator)[index]), colour);
            }
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
            return MessageBox.Show(text, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question).ToString();
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
                    for (var index = byte.MinValue; index < sqlQuery.Length; index++)
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

            switch (sender.ToString())
            {
                case "Color [Window]":
                    return custContainer[0x01].Split(separator)[index];
                case "Color [Lime]":
                    switch (!string.IsNullOrEmpty(custContainer[0x02]))   // on action - "Изменить" the timer launches an event - hint
                    {
                        case true:
                            return custContainer[0x02].Split(separator)[index];
                    }
                    return string.Empty;
                default:
                    return sender.ToString();
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
                    switch (sender.ToString() == "Color [Window]" || sender.ToString() == "Color [Lime]")
                    {
                        case true:
                            textBox1.Text = custContainer[0x02].Split(separator)[index];
                            return;
                        default:
                            textBox1.Text = sender.ToString();
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
            switch (container.ToString().Split(separator)[index].Length > byte.Parse(condition[index].ToString()))   // deal with length of GUI input fields
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
                        switch (sender[index].ToString().Length > byte.Parse(condition[index].ToString()))   // deal with length of GUI input fields
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
            var tabWarnPatient = new char[0x14] { (char)0x0411, (char)0x0435, (char)0x0437, (char)0x0020, (char)0x0434, (char)0x0430, (char)0x043D, (char)0x043D, (char)0x044B, (char)0x0445, (char)0x0020, (char)0x043F, (char)0x0430,
                                                  (char)0x0446, (char)0x0438, (char)0x0435, (char)0x043D, (char)0x0442, (char)0x0430, (char)0x003F };
            var tabWarnVaccine = new char[0x19] { (char)0x0411, (char)0x0435, (char)0x0437, (char)0x0020, (char)0x0438, (char)0x043D, (char)0x0444, (char)0x043E, (char)0x0440, (char)0x043C, (char)0x0430, (char)0x0446, (char)0x0438,
                                                  (char)0x0438, (char)0x0020, (char)0x043E, (char)0x0020, (char)0x0432, (char)0x0430, (char)0x043A, (char)0x0446, (char)0x0438, (char)0x043D, (char)0x0435, (char)0x003F };
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
                                    switch (GetMsg(GetTextFormatted(tabWarnVaccine, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            return ConfineFields(onset = (byte)name.Field.ФИО - 0x01, (byte)name.Field.Должность, sender, condition);   // determine GUI input fields within the sheet, in order
                                    }
                                    break;
                            }
                            switch (summary[0x02] > summary[0x01])   // concerning Вакцина
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(tabWarnPatient, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
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
                    switch (summary[default] == default)
                    {
                        case true:
                            SetFieldsColour(default, Color.Salmon);   // useless ConfineFields(onset = (byte)name.Field.ФИО - 0x01, ++summary[default], sender, condition) might be used to optimize the code
                            return default;
                        default:
                            return ConfineFields(onset = (byte)name.Field.ФИО - 0x01, summary[default], sender, condition);
                    }
                case 0x03:
                    switch (sender.Length == onset)   // does it equal to sent GUI input fields
                    {
                        case true:
                            switch (summary[0x01] > summary[0x02])   // concerning Пациент
                            {
                                case true:
                                    switch (GetMsg(GetTextFormatted(tabWarnVaccine, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
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
                                    switch (GetMsg(GetTextFormatted(tabWarnPatient, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
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
                                    switch (GetMsg(GetTextFormatted(tabWarnVaccine, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
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
                                    switch (GetMsg(GetTextFormatted(tabWarnPatient, nil.ToString().ToCharArray())) == string.Empty + (char)0x0059 + (char)0x0065 + (char)0x0073)
                                    {
                                        case true:
                                            onset = (byte)name.Field.Наименование - 0x01;
                                            break;
                                    }
                                    break;
                            }
                            return (summary[default] != 0x00) & true;   // do not let GUI input fields be empty at all
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
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagSlf, nil.ToString().ToCharArray());
        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(default, SystemColors.Window);
            SetFieldTextsColour(default, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagSlf, nil.ToString().ToCharArray());
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
            switch (text.Length == default)
            {
                case true:
                    return;
            }
            SetFieldsText(default, GetTextFormatted(text.ToCharArray(), (((char)0x0020).ToString() + (char)0x002D).ToString().ToCharArray()));
        }
        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x01, SystemColors.Window);
            SetFieldTextsColour(0x01, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagBirthYear, nil.ToString().ToCharArray());
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
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagHomeAddr, nil.ToString().ToCharArray());
        }
        private void textBox2_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x02, SystemColors.Window);
            SetFieldTextsColour(0x02, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagHomeAddr, nil.ToString().ToCharArray());
        }
        private void textBox2_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x02, GetTextFormatted(text.ToCharArray(), ((char)0x0020).ToString().ToCharArray()));
                    return;
            }
        }
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45, (char)47, (char)46);
        }
        private void textBox3_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagCompany, nil.ToString().ToCharArray());
        }
        private void textBox3_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x03, SystemColors.Window);
            SetFieldTextsColour(0x03, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagCompany, nil.ToString().ToCharArray());
        }
        private void textBox3_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x03, GetTextFormatted(text.ToCharArray(), ((char)0x0020).ToString().ToCharArray()));
                    return;
            }
        }
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45);
        }
        private void textBox4_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagOccupation, nil.ToString().ToCharArray());
        }
        private void textBox4_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x04, SystemColors.Window);
            SetFieldTextsColour(0x04, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagOccupation, nil.ToString().ToCharArray());
        }
        private void textBox4_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == ops.Action.Записать.ToString())
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x04, GetTextFormatted(text.ToCharArray(), ((char)0x002D).ToString().ToCharArray()));
                    return;
            }
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x04, GetTextFormatted(text.ToCharArray(), ((char)0x0020).ToString().ToCharArray()));
                    return;
            }
        }
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e, (char)45, (char)45);
        }
        private void textBox5_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagName, nil.ToString().ToCharArray());
        }
        private void textBox5_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x05, SystemColors.Window);
            SetFieldTextsColour(0x05, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagName, nil.ToString().ToCharArray());
        }
        private void textBox5_Leave(object sender, EventArgs e)
        {
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    var text = sender.ToString().Split((char)0x002C)[0x01].Split((char)0x003A)[0x01].Substring(0x01);   // cut off character subsequence along with (char)0x0020
                    switch (text.Length == default)
                    {
                        case true:
                            return;
                    }
                    SetFieldsText(0x05, GetTextFormatted(text.ToCharArray(), ((char)0x0020).ToString().ToCharArray()));
                    return;
            }
        }
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = GetKBRDSet(e);
        }
        private void textBox6_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagSerial, nil.ToString().ToCharArray());
        }
        private void textBox6_Enter(object sender, EventArgs e)
        {
            SetFieldsColour(0x06, SystemColors.Window);
            SetFieldTextsColour(0x06, SystemColors.WindowText);
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagSerial, nil.ToString().ToCharArray());
        }
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            SetFieldsColour(0x07, SystemColors.Window);
            SetFieldTextsColour(0x07, SystemColors.WindowText);
            label2.Text = comboBox1.Text;   // on init: to measure Text.Length of the further event
        }
        private void comboBox1_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagNumbAppoint, nil.ToString().ToCharArray());
            label2.Visible = default;
        }
        private void comboBox1_Leave(object sender, EventArgs e)
        {
            label2.Text = comboBox1.Text;
            label2.Visible = true;
            switch (button1.Text == ops.Action.Записать.ToString())
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
                                    label4.Text = $"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}";
                                    return;
                            }
                    }
            }
        }
        private void dateTimePicker2_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDateActual, nil.ToString().ToCharArray());
            switch (button1.Text == ops.Action.Записать.ToString())
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
            switch (button1.Text == ops.Action.Изменить.ToString())
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
            switch (button1.Text == ops.Action.Записать.ToString())
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
                            label4.Text = $"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}";
                            return;
                    }
            }
            switch (button1.Text == ops.Action.Изменить.ToString())
            {
                case true:
                    label3.Text = string.Empty;   // reserved for additional uniqueness(right after UNN)
                    return;
            }
        }
        private void dateTimePicker3_Enter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDatePlan, nil.ToString().ToCharArray());
            switch (button1.Text == ops.Action.Записать.ToString())
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
                            label4.Text = $"{dateTimePicker2.Value.AddDays(tmp).Day}.{dateTimePicker2.Value.AddDays(tmp).Month}.{dateTimePicker2.Value.AddDays(tmp).Year}";
                            return;
                    }
            }
            label4.Visible = default;
        }
        private void dateTimePicker3_Leave(object sender, EventArgs e)
        {
            SetFieldsColour(0x09, SystemColors.Window);
            SetFieldTextsColour(0x09, SystemColors.WindowText);
            switch (button1.Text == ops.Action.Записать.ToString())
            {
                case true:
                    return;
            }
            label4.Text = dateTimePicker3.Text;
            label4.Visible = true;
        }
        private void label1_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Patient.tagBirthYear, nil.ToString().ToCharArray());
        }
        private void label2_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagNumbAppoint, nil.ToString().ToCharArray());
        }
        private void label3_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDateActual, nil.ToString().ToCharArray());
        }
        private void label4_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = GetTextFormatted(reg.Tab.Vaccine.tagDatePlan, nil.ToString().ToCharArray());
        }
    }
}
