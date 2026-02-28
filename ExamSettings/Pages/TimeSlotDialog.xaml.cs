using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;

namespace ExamSettings.Pages
{
    public partial class TimeSlotDialog : ContentDialog
    {
        public TimeSlot TimeSlot { get; set; }

        public TimeSlotDialog(TimeSlot timeSlot = null)
        {
            InitializeComponent();
            TimeSlot = timeSlot ?? new TimeSlot();
            DataContext = TimeSlot;
            
            // 手动设置控件的值
            txtName.Text = TimeSlot.Name;
            
            // 尝试解析日期字符串为DateTime
            if (!string.IsNullOrEmpty(TimeSlot.StartDate))
            {
                try
                {
                    dateStart.SelectedDate = DateTime.Parse(TimeSlot.StartDate);
                }
                catch { }
            }
            
            // 尝试解析时间字符串为DateTime
            if (!string.IsNullOrEmpty(TimeSlot.StartTime))
            {
                try
                {
                    timeStart.SelectedDateTime = DateTime.ParseExact(TimeSlot.StartTime, "HH:mm", null);
                }
                catch { }
            }
            
            // 尝试解析日期字符串为DateTime
            if (!string.IsNullOrEmpty(TimeSlot.EndDate))
            {
                try
                {
                    dateEnd.SelectedDate = DateTime.Parse(TimeSlot.EndDate);
                }
                catch { }
            }
            
            // 尝试解析时间字符串为DateTime
            if (!string.IsNullOrEmpty(TimeSlot.EndTime))
            {
                try
                {
                    timeEnd.SelectedDateTime = DateTime.ParseExact(TimeSlot.EndTime, "HH:mm", null);
                }
                catch { }
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (dateStart.SelectedDate == null)
            {
                args.Cancel = true;
                ShowError("请选择开始日期");
                return;
            }

            if (timeStart.SelectedDateTime == null)
            {
                args.Cancel = true;
                ShowError("请选择开始时间");
                return;
            }

            if (dateEnd.SelectedDate == null)
            {
                args.Cancel = true;
                ShowError("请选择结束日期");
                return;
            }

            if (timeEnd.SelectedDateTime == null)
            {
                args.Cancel = true;
                ShowError("请选择结束时间");
                return;
            }

            // 组合日期和时间进行比较
            DateTime startDateTime = dateStart.SelectedDate.Value.Date.Add(timeStart.SelectedDateTime.Value.TimeOfDay);
            DateTime endDateTime = dateEnd.SelectedDate.Value.Date.Add(timeEnd.SelectedDateTime.Value.TimeOfDay);

            if (startDateTime >= endDateTime)
            {
                args.Cancel = true;
                ShowError("开始时间不能晚于结束时间");
                return;
            }

            TimeSlot.Name = txtName.Text;
            TimeSlot.StartDate = dateStart.SelectedDate.Value.ToString("yyyy-MM-dd");
            TimeSlot.StartTime = timeStart.SelectedDateTime.Value.ToString("HH:mm");
            TimeSlot.EndDate = dateEnd.SelectedDate.Value.ToString("yyyy-MM-dd");
            TimeSlot.EndTime = timeEnd.SelectedDateTime.Value.ToString("HH:mm");
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ShowError(string message)
        {
            txtName.ToolTip = message;
            timeStart.ToolTip = message;
            timeEnd.ToolTip = message;
        }
    }
}
