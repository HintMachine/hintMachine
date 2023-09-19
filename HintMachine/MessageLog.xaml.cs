using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HintMachine
{
    public partial class MessageLog : UserControl
    {
        public MessageLog()
        {
            InitializeComponent();
        }

        public void AddMessage(string message, LogMessageType logMessageType)
        {
            // If view was already at the bottom before adding the element, auto-scroll to prevent the user from
            // having to scroll manually each time there are new messages
            bool scrollToBottom = (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);

            int rowID = grid.RowDefinitions.Count;
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = GridLength.Auto;
            grid.RowDefinitions.Add(rowDef);

            // Add a colored decoration rectangle to quickly see message type
            Rectangle rectangle = new Rectangle();
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;
            rectangle.Width = 5;
            rectangle.Fill = new SolidColorBrush(Logger.GetColorForMessageType(logMessageType));
            Grid.SetColumn(rectangle, 0);
            Grid.SetRow(rectangle, rowID);
            grid.Children.Add(rectangle);

            // Add a readonly TextBox containing the message text
            // (we use a TextBox instead of a TextBlock to enable text selection) 
            TextBox textbox = new TextBox
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Logger.GetColorForMessageType(logMessageType)),
                Padding = new Thickness(6, 4, 6, 4),
                FontSize = 14,
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            /*
            if (panel.Children.Count % 2 == 1)
                textbox.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            else */
            textbox.Background = Brushes.Transparent;
            //textbox.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));

            if (logMessageType == LogMessageType.ERROR)
                textbox.FontWeight = FontWeights.Bold;

            Grid.SetColumn(textbox, 1);
            Grid.SetRow(textbox, rowID);
            grid.Children.Add(textbox);

            if (scrollToBottom)
                scrollViewer.ScrollToBottom();
        }
    }
}
