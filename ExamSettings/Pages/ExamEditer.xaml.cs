using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace ExamSettings.Pages
{
    public class TimeSlot
    {
        public string Name { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }

    public partial class ExamEditer : Page
    {
        private List<TimeSlot>? timeSlots;
        private string? dataFilePath;

        public ExamEditer()
        {
            InitializeData();
            LoadData();
        }

        private void InitializeData()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataDir = Path.Combine(appDir, "Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            dataFilePath = Path.Combine(dataDir, "Default.json");
            timeSlots = new();
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    timeSlots = JsonConvert.DeserializeObject<List<TimeSlot>>(json) ?? new();
                }
                else
                {
                    // 初始化默认数据
                    string today = DateTime.Today.ToString("yyyy-MM-dd");
                    timeSlots = new()
                    {
                        new() { Name = "上午第一节课", StartDate = today, StartTime = "08:00", EndDate = today, EndTime = "08:45" },
                        new() { Name = "上午第二节课", StartDate = today, StartTime = "09:00", EndDate = today, EndTime = "09:45" },
                        new() { Name = "上午第三节课", StartDate = today, StartTime = "10:00", EndDate = today, EndTime = "10:45" },
                        new() { Name = "下午第一节课", StartDate = today, StartTime = "14:00", EndDate = today, EndTime = "14:45" },
                        new() { Name = "下午第二节课", StartDate = today, StartTime = "15:00", EndDate = today, EndTime = "15:45" }
                    };
                    SaveData();
                }
                UpdateDataGrid();
            }
            catch (Exception ex)
            {
                ShowStatus($"加载数据失败: {ex.Message}", false);
                timeSlots = new();
            }
        }

        private void SaveData()
        {
            try
            {
                if (timeSlots != null && dataFilePath != null)
                {
                    string json = JsonConvert.SerializeObject(timeSlots, Formatting.Indented);
                    File.WriteAllText(dataFilePath, json);
                    ShowStatus("数据保存成功", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"保存数据失败: {ex.Message}", false);
            }
        }

        private void UpdateDataGrid()
        {
            dgTimeSlots.ItemsSource = timeSlots;
        }

        private bool CheckTimeOverlap(TimeSlot newSlot, TimeSlot? existingSlot = null)
        {
            // 组合日期和时间进行比较
            DateTime newStartDateTime = DateTime.Parse(newSlot.StartDate).Date.Add(TimeSpan.Parse(newSlot.StartTime));
            DateTime newEndDateTime = DateTime.Parse(newSlot.EndDate).Date.Add(TimeSpan.Parse(newSlot.EndTime));

            if (timeSlots != null)
            {
                foreach (var slot in timeSlots)
                {
                    if (slot == existingSlot) continue;

                    DateTime slotStartDateTime = DateTime.Parse(slot.StartDate).Date.Add(TimeSpan.Parse(slot.StartTime));
                    DateTime slotEndDateTime = DateTime.Parse(slot.EndDate).Date.Add(TimeSpan.Parse(slot.EndTime));

                    if ((newStartDateTime >= slotStartDateTime && newStartDateTime < slotEndDateTime) ||
                        (newEndDateTime > slotStartDateTime && newEndDateTime <= slotEndDateTime) ||
                        (newStartDateTime <= slotStartDateTime && newEndDateTime >= slotEndDateTime))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            txtStatus.Text = message;
            txtStatus.Foreground = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TimeSlotDialog();
            var result = dialog.ShowAsync();
            result.ContinueWith(task =>
            {
                if (task.Result == ContentDialogResult.Primary && timeSlots != null && dialog.TimeSlot != null)
                {
                    if (CheckTimeOverlap(dialog.TimeSlot))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowStatus("时间段与现有时间段重叠", false);
                        });
                        return;
                    }

                    timeSlots.Add(dialog.TimeSlot);
                    Dispatcher.Invoke(() =>
                    {
                        UpdateDataGrid();
                        SaveData();
                        ShowStatus("添加时间段成功", true);
                    });
                }
            });
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgTimeSlots.SelectedItem is not TimeSlot selectedSlot)
            {
                ShowStatus("请选择要编辑的时间段", false);
                return;
            }

            var dialog = new TimeSlotDialog(selectedSlot);
            var result = dialog.ShowAsync();
            result.ContinueWith(task =>
            {
                if (task.Result == ContentDialogResult.Primary && dialog.TimeSlot != null)
                {
                    if (CheckTimeOverlap(dialog.TimeSlot, selectedSlot))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowStatus("时间段与现有时间段重叠", false);
                        });
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        UpdateDataGrid();
                        SaveData();
                        ShowStatus("编辑时间段成功", true);
                    });
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgTimeSlots.SelectedItem is not TimeSlot selectedSlot || string.IsNullOrEmpty(selectedSlot.Name))
            {
                ShowStatus("请选择要删除的时间段", false);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "确认删除",
                Content = $"确定要删除时间段 '{selectedSlot.Name}' 吗？",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消"
            };

            var result = dialog.ShowAsync();
            result.ContinueWith(task =>
            {
                if (task.Result == ContentDialogResult.Primary && timeSlots != null)
                {
                    timeSlots.Remove(selectedSlot);
                    Dispatcher.Invoke(() =>
                    {
                        UpdateDataGrid();
                        SaveData();
                        ShowStatus("删除时间段成功", true);
                    });
                }
            });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void AppBarButton_Click(object? sender, RoutedEventArgs e)
        {
            // 保留原有方法以避免编译错误
        }
    }
}