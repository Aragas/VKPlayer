using System;
using System.Drawing;
using System.Windows.Forms;

namespace NAudio.Utils
{
    /// <summary>
    ///     A thread-safe Progress Log Control
    /// </summary>
    public partial class ProgressLog : UserControl
    {
        /// <summary>
        ///     Creates a new progress log control
        /// </summary>
        public ProgressLog()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     The contents of the log as text
        /// </summary>
        public new string Text
        {
            get { return richTextBoxLog.Text; }
        }

        /// <summary>
        ///     Report progress from a progress event
        /// </summary>
        public void ReportProgress(ProgressEventArgs e)
        {
            Color color = Color.Black;
            if (e.MessageType == ProgressMessageType.Warning)
            {
                color = Color.Blue;
            }
            else if (e.MessageType == ProgressMessageType.Error)
            {
                color = Color.Red;
            }
            else if (e.MessageType == ProgressMessageType.Trace)
            {
                color = Color.Purple;
            }

            LogMessage(color, e.Message);
        }


        /// <summary>
        ///     Log a message
        /// </summary>
        public void LogMessage(Color color, string message)
        {
            if (richTextBoxLog.InvokeRequired)
            {
                Invoke(new LogMessageDelegate(LogMessage), new object[] {color, message});
            }
            else
            {
                richTextBoxLog.SelectionStart = richTextBoxLog.TextLength;
                richTextBoxLog.SelectionColor = color;
                richTextBoxLog.AppendText(message);
                richTextBoxLog.AppendText(Environment.NewLine);
            }
        }

        /// <summary>
        ///     Clear the log
        /// </summary>
        public void ClearLog()
        {
            if (richTextBoxLog.InvokeRequired)
            {
                Invoke(new ClearLogDelegate(ClearLog), new object[] {});
            }
            else
            {
                richTextBoxLog.Clear();
            }
        }

        private delegate void ClearLogDelegate();

        private delegate void LogMessageDelegate(Color color, string message);
    }
}