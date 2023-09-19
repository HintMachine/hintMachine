using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            DockPanel messagePanel = new DockPanel();
            messagePanel.LastChildFill = true;

            Rectangle rectangle = new Rectangle();
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;
            rectangle.Width = 5;
            rectangle.Fill = new SolidColorBrush(Logger.GetColorForMessageType(logMessageType));

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
            textbox.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
            
            if (logMessageType == LogMessageType.ERROR)
                textbox.FontWeight = FontWeights.Bold;

            // If view was already at the bottom before adding the element, auto-scroll to prevent the user from
            // having to scroll manually each time there are new messages
            bool scrollToBottom = (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);

            messagePanel.Children.Add(rectangle);
            messagePanel.Children.Add(textbox);

            messagePanel.MouseEnter += (object sender, MouseEventArgs e) => {
                
            };
            messagePanel.MouseLeave += (object sender, MouseEventArgs e) => {
                
            };

            messagePanel.Margin = new Thickness(0,0,0,1);
            panel.Children.Add(messagePanel);
            
            if (scrollToBottom)
                scrollViewer.ScrollToBottom();
        }
    }
}
