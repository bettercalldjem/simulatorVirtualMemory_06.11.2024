using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace VirtualMemorySimulation
{
    public partial class MainWindow : Window
    {
        public class VirtualPage
        {
            public int PageID { get; set; }
            public string Data { get; set; }
            public string State { get; set; }
        }

        public class MemoryFrame
        {
            public int FrameID { get; set; }
            public VirtualPage Page { get; set; }
        }

        private List<VirtualPage> virtualDisk = new List<VirtualPage>();
        private List<MemoryFrame> memoryFrames = new List<MemoryFrame>();
        private Queue<VirtualPage> pageQueue = new Queue<VirtualPage>();
        private int pageFaults = 0;
        private int frameCount = 3;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnInitializeClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(FrameCountTextBox.Text, out frameCount) || frameCount <= 0)
            {
                MessageBox.Show("Введите корректное количество фреймов.");
                return;
            }

            virtualDisk.Clear();
            pageQueue.Clear();
            memoryFrames.Clear();
            pageFaults = 0;
            PageFaultText.Text = $"Пейдж фолтов: {pageFaults}";

            for (int i = 0; i < 10; i++)
            {
                virtualDisk.Add(new VirtualPage
                {
                    PageID = i,
                    Data = $"Данные страницы {i}",
                    State = "На диске"
                });
                pageQueue.Enqueue(virtualDisk[i]);
            }

            for (int i = 0; i < frameCount; i++)
            {
                memoryFrames.Add(new MemoryFrame
                {
                    FrameID = i,
                    Page = null
                });
            }

            UpdateMemoryGrid();
        }

        private void UpdateMemoryGrid()
        {
            var memoryDisplay = new List<object>();

            foreach (var frame in memoryFrames)
            {
                memoryDisplay.Add(new
                {
                    FrameID = frame.FrameID,
                    PageID = frame.Page?.PageID ?? -1,
                    State = frame.Page?.State ?? "Пусто"
                });
            }

            MemoryDataGrid.ItemsSource = memoryDisplay;
        }

        private void AccessMemory(int pageID)
        {
            var pageInMemory = memoryFrames.FirstOrDefault(f => f.Page?.PageID == pageID);

            if (pageInMemory == null)
            {
                PageFaultHandler(pageID);
            }
            else
            {
                pageInMemory.Page.State = "В памяти";
            }

            UpdateMemoryGrid();
        }

        private void PageFaultHandler(int pageID)
        {
            pageFaults++;
            PageFaultText.Text = $"Пейдж фолтов: {pageFaults}";

            var emptyFrame = memoryFrames.FirstOrDefault(f => f.Page == null);

            if (emptyFrame != null)
            {
                var page = virtualDisk.First(p => p.PageID == pageID);
                emptyFrame.Page = page;
                page.State = "В памяти";
            }
            else
            {
                var pageToReplace = pageQueue.Dequeue();
                pageQueue.Enqueue(pageToReplace); var frameToReplace = memoryFrames.First(f => f.Page?.PageID == pageToReplace.PageID);
                frameToReplace.Page = null;
                var newPage = virtualDisk.First(p => p.PageID == pageID);
                frameToReplace.Page = newPage;
                newPage.State = "В памяти";
            }
        }

        private void OnAccessButtonClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(((Button)sender).Tag.ToString(), out int pageID))
            {
                AccessMemory(pageID);
            }
        }
    }
}
