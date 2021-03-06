﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TelegramSouls.Server
{
    public class MessageHandler : IDisposable
    {
        private SessionStorage _sessions;
        private MessageSender _sender;
        private MessageQueue _queue;

        private bool _disposed = false;
        private CancellationTokenSource _cancellationTokenSource;

        public MessageHandler(MessageSender sender, MessageQueue queue,
            SessionStorage sessions)
        {
            _sender = sender;
            _queue = queue;
            _sessions = sessions;
        }

        private void Handle()
        {
            var message = _queue.Dequeue();
            if (message != null)
            {
                if (string.Equals(message.Text, "/start", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!_sessions.IsSessionActive(message.From.Id))
                    {
                        var context = _sessions.Create(message.From.Id, message.From.Username);
                        _sender.SendToRoom(context, string.Format("{0} материализовался из воздуха.", message.From.Username));
                        context.GetRoom().Look(context);
                    }

                    return;
                }

                if (!_sessions.IsSessionActive(message.From.Id))
                {
                    return;
                }

                var sessionContext = _sessions.Get(message);
                sessionContext.ReplyId = message.MessageId;

                if (string.Equals(message.Text, "/stop", System.StringComparison.OrdinalIgnoreCase))
                {
                    _sender.SendToRoom(sessionContext, string.Format("{0} медленно растворился в воздухе.", message.From.Username));
                    _sessions.Abandon(sessionContext.Id);
                    return;
                }

                if (string.Equals(message.Text, "/who", System.StringComparison.OrdinalIgnoreCase))
                {
                    var list = string.Join(", ", _sessions.GetSessions().Select(v => v.Username));
                    _sender.ReplyTo(sessionContext, list);
                    return;
                }

                if (string.Equals(message.Text, "|Север|", StringComparison.OrdinalIgnoreCase))
                {
                    sessionContext.GetRoom().GoNorth(sessionContext);
                    return;
                }

                if (string.Equals(message.Text, "|Юг|", StringComparison.OrdinalIgnoreCase))
                {
                    sessionContext.GetRoom().GoSouth(sessionContext);
                    return;
                }

                if (string.Equals(message.Text, "|Восток|", StringComparison.OrdinalIgnoreCase))
                {
                    sessionContext.GetRoom().GoEast(sessionContext);
                    return;
                }

                if (string.Equals(message.Text, "|Запад|", StringComparison.OrdinalIgnoreCase))
                {
                    sessionContext.GetRoom().GoWest(sessionContext);
                    return;
                }

                if (string.Equals(message.Text, "|Осмотреть|", StringComparison.OrdinalIgnoreCase))
                {
                    sessionContext.GetRoom().Look(sessionContext);
                    return;
                }

                if (sessionContext.GetRoom().ProcessContextAction(sessionContext, message.Text))
                {
                    return;
                }

                _sender.SendToRoom(sessionContext, string.Format("{0}: {1}", sessionContext.Username, message.Text));
            }
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Handle();
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // ...
                }

                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _disposed = true;
            }
        }

        ~MessageHandler()
        {
            Dispose(false);
        }
    }
}