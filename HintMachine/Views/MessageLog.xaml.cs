using HintMachine.Helpers;
using HintMachine.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HintMachine.Views
{
    public partial class MessageLog : UserControl
    {
        private class Message : Border
        {
            public LogMessageType MessageType { get; private set; } = LogMessageType.RAW;

            public TextBox TextBox { get; private set; } = null;

            public Message(LogMessageType logMessageType, string message) : base()
            {
                MessageType = logMessageType;

                Color textColor = GetColorForMessageType(logMessageType);
                Color backgroundColor = textColor;
                backgroundColor.A = 20;
                Color borderColor = textColor;
                borderColor.A = 200;

                // Add a colored decoration rectangle to quickly see message type
                VerticalAlignment = VerticalAlignment.Stretch;
                BorderThickness = new Thickness(5, 0, 0, 0.5);
                BorderBrush = new SolidColorBrush(borderColor);
                
                TextBox = new TextBox {
                    // Add a readonly TextBox containing the message text
                    // (we use a TextBox instead of a TextBlock to enable text selection) 
                    Text = GetPrefixForMessageType(logMessageType) + message,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(textColor),
                    Padding = new Thickness(6, 4, 6, 4),
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true,
                    Background = new SolidColorBrush(backgroundColor)
                };

                Child = TextBox;
            }
        }

        // ----------------------------------------------------------------------------------

        private const int MAX_DISPLAYED_MESSAGES = 100;
        private const int MAX_KEPT_MESSAGES = 200;
        private List<Message> _messages = new List<Message>();

        // ----------------------------------------------------------------------------------

        public MessageLog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add the given message to the message log widget.
        /// <param name="message">the text contents of the message</param>
        /// <param name="logMessageType">the type of the message, which will influence its color and its decoration</param>
        /// </summary>
        public void AddMessage(string message, LogMessageType logMessageType)
        {
            // If view was already at the bottom before adding the element, auto-scroll to prevent the user from
            // having to scroll manually each time there are new messages
            bool scrollToBottom = (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight);

            Message lastMessage = (_messages.Count > 0) ? _messages[_messages.Count - 1] : null;
            if(lastMessage != null && lastMessage.MessageType == logMessageType && logMessageType == LogMessageType.SERVER_RESPONSE)
            {
                // If there already are other messages and the last message has the same type, we can "merge"
                // this message with the previous one for performance reasons. We only do this for SERVER_RESPONSE
                // messages since they are the most prone to flooding (e.g. with !missing)
                lastMessage.TextBox.Text += "\n" + GetPrefixForMessageType(logMessageType) + message;
            }
            else
            {
                // Usual case: just add a new message
                Message newMessage = new Message(logMessageType, message);

                if (logMessageType == LogMessageType.HINT && !Settings.DisplayFoundHintMessages && message.EndsWith("(found)"))
                    newMessage.Opacity = 0.6;
                _messages.Add(newMessage);

                UpdateMessagesVisibility();
                StackPanelMessages.Children.Add(newMessage);
            }
    
            if (scrollToBottom)
                ScrollViewer.ScrollToBottom();
        }

        /// <summary>
        /// Update the visibility of every message inside the message log according to the current rules.
        /// This takes chat filters and chat max size into account, filtering unwanted / old messages out.
        /// This method should to be called whenever chat filters are changed or when a message is added to
        /// potentially erase older messages.
        /// </summary>
        public void UpdateMessagesVisibility()
        {
            // Store scroll distance to bottom to put it back after hiding / showing new messages
            double distanceToBottom = ScrollViewer.ScrollableHeight - ScrollViewer.VerticalOffset;

            // Hide all filtered messages
            foreach (Message message in _messages)
            {
                bool visible = CanDisplayMessage(message.TextBox.Text, message.MessageType);
                message.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }

            // Truncate all message widgets above the safe-keeping limit
            while(_messages.Count > MAX_KEPT_MESSAGES) 
            {
                StackPanelMessages.Children.Remove(_messages[0]);
                _messages.Remove(_messages[0]);
            }

            // Count all visible messages starting from the bottom, and hide all those above the display limit
            int messagesCount = 0;
            for (int i = _messages.Count-1 ; i >= 0 ; --i)
            {
                if (messagesCount > MAX_DISPLAYED_MESSAGES)
                    _messages[i].Visibility = Visibility.Collapsed;
                else if (_messages[i].TextBox.Visibility == Visibility.Visible)
                    messagesCount++;
            }

            // Put back the same scroll distance as originally
            if (distanceToBottom > 0)
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ScrollableHeight - distanceToBottom);
        }

        public static Color GetColorForMessageType(LogMessageType logMessageType)
        {
            switch(logMessageType)
            {
                case LogMessageType.INFO:
                case LogMessageType.JOIN_LEAVE:
                case LogMessageType.SERVER_RESPONSE:
                case LogMessageType.STREAMER_SENSITIVE_INFO:
                    return Color.FromRgb(0, 150, 200);

                case LogMessageType.ITEM_SENT:
                case LogMessageType.ITEM_RECEIVED:
                    return Color.FromRgb(90, 30, 180);

                case LogMessageType.WARNING:
                    return Color.FromRgb(220, 200, 0);

                case LogMessageType.ERROR:
                    return Color.FromRgb(180, 40, 40);

                case LogMessageType.HINT:
                    return Color.FromRgb(20, 180, 20);

                case LogMessageType.CHAT:
                    return Color.FromRgb(140, 60, 30);

                case LogMessageType.GOAL:
                    return Color.FromRgb(220, 150, 0);
            }

            return Colors.Black;
        }

        public static string GetPrefixForMessageType(LogMessageType logMessageType)
        {
            if (logMessageType == LogMessageType.WARNING)
                return "⚠️ ";
            else if (logMessageType == LogMessageType.ERROR)
                return "❌ ";
            else if (logMessageType == LogMessageType.HINT)
                return "❓ ";
            else if (logMessageType == LogMessageType.CHAT)
                return "💬 ";
            else if (logMessageType == LogMessageType.ITEM_RECEIVED)
                return "📩 ";
            else if (logMessageType == LogMessageType.ITEM_SENT)
                return "📦 ";
            else if (logMessageType == LogMessageType.GOAL)
                return "👑 ";
            else if (logMessageType == LogMessageType.JOIN_LEAVE)
                return "👋 ";
            else if (logMessageType == LogMessageType.SERVER_RESPONSE)
                return "    ";

            return "";
        }

        public static bool CanDisplayMessage(string message, LogMessageType logMessageType) 
        {
            if (logMessageType == LogMessageType.HINT && !Settings.DisplayFoundHintMessages && message.EndsWith("(found)"))
                return false;
            if (logMessageType == LogMessageType.JOIN_LEAVE && !Settings.DisplayJoinLeaveMessages)
                return false;
            if (logMessageType == LogMessageType.CHAT && !Settings.DisplayChatMessages)
                return false;
            if (logMessageType == LogMessageType.ITEM_RECEIVED && !Settings.DisplayItemReceivedMessages)
                return false;
            if (logMessageType == LogMessageType.ITEM_SENT && !Settings.DisplayItemSentMessages)
                return false;
            if (logMessageType == LogMessageType.STREAMER_SENSITIVE_INFO && Settings.StreamerMode)
                return false;
            
            return true;
        }
    }
}
